using System.Collections.Concurrent;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fun.LEGO.Spike;

interface IHubRepl : IDisposable {
    Task Connect();
    Task Disconnect();
    
    Task SendCode(params string[] codes);

    /// <summary>
    /// This will send a single line and wait for the related result, and only the first line of the actual result of be taken
    /// </summary>
    /// <param name="code"></param>
    /// <param name="cancellationInMs"></param>
    /// <returns></returns>
    Task<string> SendCodeAndWaitResult(string code, int cancellationInMs = 1000);
}

class HubReplOptions {
    public string PortName { get; set; } = "";
}

partial class HubRepl : IHubRepl {

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

    public HubRepl(IOptions<HubReplOptions> option, ILogger<HubRepl> logger) {
        hubOption = option.Value;
        this.logger = logger;

        port = new SerialPort {
            NewLine = NEWLINE,
            BaudRate = 115200,
            PortName = hubOption.PortName,
            Encoding = Encoding.UTF8,
            DtrEnable = true,
            WriteTimeout = 1000
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
        foreach (var codesStr in codes) {
            foreach (var code in codesStr.Split(Environment.NewLine).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x))) {
                switch (code) {
                    case START: logger.LogInformation(SEND_PREFIX + nameof(START)); break;
                    case STOP_PROGRAM: logger.LogInformation(SEND_PREFIX + nameof(STOP_PROGRAM)); break;
                    default: logger.LogInformation(SEND_PREFIX + code); break;
                }
                await Task.Run(() => {
                    lock (sendLocker) {
                        port.WriteLine(code);
                    }
                });
            }
        }
    }
    
    public async Task<string> SendCodeAndWaitResult(string code, int cancellationInMs = 1000) {
        var id = Random.Shared.Next();
        var result = new TaskCompletionSource<string>();
        identifyResults[id] = result;

        using var cts = new CancellationTokenSource(cancellationInMs);
        cts.Token.Register(() => {
            identifyResults.TryRemove(id, out var _);
            result.SetCanceled();
        });

        await SendCode($$"""print("ID{{id}}:{0}".format({{code}}))""");
        return await result.Task;
    }

    public void Dispose() {
        try {
            Disconnect();
        }
        catch (Exception) {
        }
    }
}