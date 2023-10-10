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

await hub.SendCode("import motor, device");
await hub.SendCode("from hub import port");

Console.WriteLine(await hub.SendCodeAndWaitResult("motor.velocity(port.A)"));
Console.WriteLine(await hub.SendCodeAndWaitResult("device.data(port.A)"));

Console.WriteLine("Press Enter to exit ...");
Console.ReadLine();
