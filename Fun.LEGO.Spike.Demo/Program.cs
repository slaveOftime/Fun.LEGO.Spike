using Fun.LEGO.Spike;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.WriteTo.Console()
	.CreateLogger();

using var services = new ServiceCollection()
	.AddLogging(builder => builder.AddSerilog(Log.Logger))
	.Configure<HubReplOptions>(options => options.PortName = "COM5")
	.AddSingleton<IHubRepl, HubRepl>()
	.BuildServiceProvider();

var hub = services.GetRequiredService<IHubRepl>();

await hub.Connect();

var motorA = await hub.LinkMotorToPort(HubPort.A);

Console.WriteLine("run in 100 degree/sec for 1 sec");
await motorA.Run(100);
await Task.Delay(1000);

Console.WriteLine("stop for 2 sec");
await motorA.Stop();
await Task.Delay(2000);

Console.WriteLine("run 100 degree");
await motorA.RunForDegree(100, 100);
await Task.Delay(2000);

Console.WriteLine("run for 3 sec");
await motorA.RunForTime(3000, 100);
await Task.Delay(3000);

Console.WriteLine("run for 3 sec -500 degree/sec");
await motorA.RunForTime(3000, -500);
await Task.Delay(3000);

Console.WriteLine("set relative position");
await motorA.ResetRelativePosition(0);
await motorA.RunToRelativePosition(-90, 1000);
await Task.Delay(1000);

Console.WriteLine("run to 90 degree absolute");
await motorA.RunToAbsolutePosition(90, 100);
while (true) {
	var currentPosition = await motorA.GetAbsolutePosition();

	Console.WriteLine("velocity = " + await motorA.GetVelocity());
	Console.WriteLine("position = " + currentPosition);

	if (Math.Abs(currentPosition - 90) < 3) break;
	await Task.Delay(100);
}


Console.WriteLine("=============================");

Console.WriteLine("pair motor A B");
var motorAB = await hub.PairMotor(HubPort.A, HubPort.B);

Console.WriteLine("move for 2 sec");
await motorAB.Move(50);
await Task.Delay(2000);

Console.WriteLine("stop for 1 sec");
await motorAB.Stop();
await Task.Delay(1000);


Console.WriteLine("move 300 degree for 2 sec");
await motorAB.MoveForDegree(300, 50);
await Task.Delay(2000);

Console.WriteLine("move for 2 sec");
await motorAB.MoveForTime(2000, 50);
await Task.Delay(2000);


Console.WriteLine("move tank 100, 200 for 2 sec");
await motorAB.MoveTank(100, 200);
await Task.Delay(2000);


Console.WriteLine("move tank -300 degree for 2 sec");
await motorAB.MoveTankForDegree(-300, 100, -100);
await Task.Delay(2000);


Console.WriteLine("move tank for 2 sec");
await motorAB.MoveTankForTime(2000, 100, -100);
await Task.Delay(2000);


Console.WriteLine("unpair");
await motorAB.Unpair();

Console.WriteLine("=============================");

Console.ReadLine();
