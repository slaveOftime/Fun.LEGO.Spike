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

	/// <summary>
	/// Send multiple code block to hub repl
	/// </summary>
	/// <param name="codes"></param>
	/// <returns></returns>
	Task SendCode(params string[] codes);

	/// <summary>
	/// This will send a single line code and wait for the related result, and only the first line of the actual result of be taken.
	/// </summary>
	/// <param name="code"></param>
	/// <param name="cancellationInMs"></param>
	/// <returns></returns>
	Task<string> SendCodeAndWaitResult(string code, int? cancellationInMs = null);
}

public class HubReplOptions {
	public string PortName { get; set; } = "";
	public int? SendTimeoutMs { get; set; }
}

public partial class HubRepl : IHubRepl {

	public const string CTRL_A = "\x01";
	public const string CTRL_B = "\x02";
	public const string CTRL_C = "\x03";
	public const string CTRL_D = "\x04";
	public const string CTRL_E = "\x05";

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
		};

		if (hubOption.SendTimeoutMs.HasValue)
			port.WriteTimeout = hubOption.SendTimeoutMs.Value;

		readThread = new Thread(() => {
			while (!readCancellationTokenSource.IsCancellationRequested) {
				try {
					var line = port.ReadLine();

					logger.LogDebug("{RECEIVE_PREFIX}{line}", RECEIVE_PREFIX, line);

					var match = identifyResultRegex.Match(line);
					if (match.Success) {
						if (int.TryParse(match.Value[2..^1], out var id) && identifyResults.TryRemove(id, out var task)) {
							task.SetResult(line[match.Value.Length..]);
						}
					}
				}
				catch (Exception ex) {
					logger.LogError(ex, "RECEIVE failed: {Message}", ex.Message);
				}
			}
		});
	}

	public async Task Connect() {
		port.Open();
		readThread.Start();
		await SendCode(CTRL_C);

		await SendCode("from hub import light_matrix");
		await SendCode("import color_sensor");
		await SendCode("import motor, motor_pair");
	}

	public Task Disconnect() {
		port.Close();
		readCancellationTokenSource.Cancel();
		return Task.CompletedTask;
	}

	public async Task SendCode(params string[] codes) {
		foreach (var code in codes) {
			switch (code) {
				case CTRL_A: logger.LogDebug(SEND_PREFIX + nameof(CTRL_A)); break;
				case CTRL_B: logger.LogDebug(SEND_PREFIX + nameof(CTRL_B)); break;
				case CTRL_C: logger.LogDebug(SEND_PREFIX + nameof(CTRL_C)); break;
				case CTRL_D: logger.LogDebug(SEND_PREFIX + nameof(CTRL_D)); break;
				default: logger.LogDebug("{SEND_PREFIX}{code}", SEND_PREFIX, code); break;
			}
			await Task.Run(() => {
				var hasMultipleLines = code.Contains(Environment.NewLine);
				lock (sendLocker) {
					if (hasMultipleLines) port.WriteLine(CTRL_A);
					port.WriteLine(code);
					if (hasMultipleLines) port.WriteLine(CTRL_B);
				}
			});
		}
	}

	public async Task<string> SendCodeAndWaitResult(string code, int? cancellationInMs = null) {
		var id = Random.Shared.Next();
		var result = new TaskCompletionSource<string>();
		identifyResults[id] = result;

		using var cts = new CancellationTokenSource(cancellationInMs ?? -1);
		cts.Token.Register(() => {
			identifyResults.TryRemove(id, out var _);
			result.SetCanceled();
		});

		await SendCode($$"""print("ID{0}:{1}".format({{id}}, {{code}}))""");
		return await result.Task;
	}


	public void Dispose() {
		GC.SuppressFinalize(this);

		try {
			Disconnect();
		}
		catch (Exception) {
		}
	}
}