using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fun.LEGO.Spike;

public interface IHubRepl : IDisposable {
    Task Connect();
    Task Disconnect();

    Task SendCode(params string[] codes);

    /// <summary>
    /// This will send a single line and wait for the related result, and only the first line of the actual result of be taken
    /// </summary>
    /// <param name="code"></param>
    /// <param name="cancellationInMs">Will use SendTimeoutMs from the HubOptions if not provided</param>
    /// <returns></returns>
    Task<string> SendCodeAndWaitResult(string code, int? cancellationInMs = null);

    Task InitMotorModule();
    Motor LinkMotorToPort(HubPort port);
}

public class HubReplOptions {
    public string PortName { get; set; } = "";
    public int SendTimeoutMs { get; set; } = 1000;
}

public partial class HubRepl : IHubRepl {

    public const string STOP_PROGRAM = "\x03";
    public const string START = "\x04";
    public const string NEWLINE = "\r\n";

    private const string SEND_PREFIX = "SEND: ";
    private const string RECEIVE_PREFIX = "RECV: ";

    private readonly HubReplOptions hubOption;
    private readonly ILogger<HubRepl> logger;

    private readonly SerialPort port;

    private readonly Thread readThread;
    private readonly CancellationTokenSource readCancellationTokenSource = new();

    private readonly object sendLocker = new();

    [GeneratedRegex(@"^ID[\d]*:")]
    private static partial Regex IdentifyResultRegex();

    private readonly Regex identifyResultRegex = IdentifyResultRegex();
    private readonly ConcurrentDictionary<int, TaskCompletionSource<string>> identifyResults = new();
    private readonly ConcurrentDictionary<HubPort, Motor> motors = new();

    public HubRepl(IOptions<HubReplOptions> option, ILogger<HubRepl> logger) {
        hubOption = option.Value;
        this.logger = logger;

        port = new SerialPort {
            NewLine = NEWLINE,
            BaudRate = 115200,
            PortName = hubOption.PortName,
            Encoding = Encoding.UTF8,
            DtrEnable = true,
            WriteTimeout = hubOption.SendTimeoutMs
        };

        readThread = new Thread(() => {
            while (!readCancellationTokenSource.IsCancellationRequested) {
                try {
                    var line = port.ReadLine();

                    logger.LogInformation($"{RECEIVE_PREFIX}{line}");

                    var match = identifyResultRegex.Match(line);
                    if (match.Success) {
                        if (int.TryParse(match.Value[2..^1], out var id) && identifyResults.TryRemove(id, out var task)) {
                            task.SetResult(line[match.Value.Length..]);
                        }
                    }
                }
                catch (Exception ex) {
                    logger.LogError($"RECEIVE failed: {ex.Message}", ex);
                }
            }
        });
    }

    public async Task Connect() {
        port.Open();
        readThread.Start();
        await SendCode(STOP_PROGRAM);
    }

    public Task Disconnect() {
        port.Close();
        readCancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    public async Task SendCode(params string[] codes) {
        foreach (var code in codes) {
            switch (code) {
                case START: logger.LogInformation(SEND_PREFIX + nameof(START)); break;
                case STOP_PROGRAM: logger.LogInformation(SEND_PREFIX + nameof(STOP_PROGRAM)); break;
                default: logger.LogInformation(SEND_PREFIX + code); break;
            }
            await Task.Run(() => {
                lock (sendLocker) {
                    port.WriteLine(code);
                    if (code.Contains(Environment.NewLine)) {
                        // To ensure we end multiple line input
                        port.WriteLine("""
                        
                        

                            """);
                    }
                }
            });
        }
    }

    public async Task<string> SendCodeAndWaitResult(string code, int? cancellationInMs = null) {
        var id = Random.Shared.Next();
        var result = new TaskCompletionSource<string>();
        identifyResults[id] = result;

        using var cts = new CancellationTokenSource(cancellationInMs ?? hubOption.SendTimeoutMs);
        cts.Token.Register(() => {
            identifyResults.TryRemove(id, out var _);
            result.SetCanceled();
        });

        await SendCode($$"""print("ID{{id}}:{0}".format({{code}}))""");
        return await result.Task;
    }


    public Task InitMotorModule() => SendCode("import motor, asyncio");

    public Motor LinkMotorToPort(HubPort port) =>
        motors.GetOrAdd(port, _ => new Motor(this, port));


    public void Dispose() {
        try {
            Disconnect();
        }
        catch (Exception) {
        }
    }
}