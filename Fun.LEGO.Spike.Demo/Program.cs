using Fun.LEGO.Spike;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

using var services = new ServiceCollection()
    .AddLogging(builder => builder.AddSerilog(Log.Logger))
    .Configure<HubReplOptions>(options => options.PortName = "COM5")
    .AddSingleton<IHubRepl, HubRepl>()
    .BuildServiceProvider();

var hub = services.GetRequiredService<IHubRepl>();

await hub.Connect();

await hub.SendCode("""
    def add(x):\
        return x + 1
    """);

Console.WriteLine(await hub.SendCodeAndWaitResult("add(1)"));


await hub.InitMotorModule();

var motorA = hub.LinkMotorToPort(HubPort.A);

// await motorA.Run(100);
// await Task.Delay(1000);
// await motorA.Stop();


await motorA.RunForTime(3000, 100);

// await motorA.RunToAbsolutePosition(90, 100);
// await motorA.RunToRelativePosition(10, 1000);

// Console.WriteLine(await motorA.GetAbsolutePosition());
// Console.WriteLine(await motorA.GetRelativePosition());
// Console.WriteLine(await motorA.GetPWM());
// Console.WriteLine(await motorA.GetVelocity());


Console.ReadLine();
