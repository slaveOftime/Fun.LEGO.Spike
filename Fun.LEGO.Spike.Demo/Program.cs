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
await hub.InitMotorModule();

var motorA = hub.LinkMotorToPort(HubPort.A);

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

Console.ReadLine();
