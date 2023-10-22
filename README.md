# Fun.LEGO.Spike [![Nuget](https://img.shields.io/nuget/vpre/Fun.LEGO.Spike)](https://www.nuget.org/packages/Fun.LEGO.Spike)

Currently this library will use USB to connect to LEGO Spike hub 3 to interact it's python REPL. And it is just a dumb csharp wrapper to send python code to REPL and wait printed result from serial port.

Supported wrapper:

- LightMatrix
- ColorSensor
- Motor
- MotorPair

## How to use it

```csharp
using var services = new ServiceCollection()
	.AddLogging()
	.Configure<HubReplOptions>(options => options.PortName = "COM5")
	.AddSingleton<IHubRepl, HubRepl>()
	.BuildServiceProvider();

var hub = services.GetRequiredService<IHubRepl>();
await hub.Connect(autoRetry: true);

var lightMatrix = new LightMatrix(hub);
await lightMatrix.SetOrientation(LightMatrixOrientation.LEFT);
await lightMatrix.Show(LightMatrixImage.IMAGE_BUTTERFLY);
```
