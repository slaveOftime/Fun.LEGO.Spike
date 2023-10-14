using Fun.LEGO.Spike;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.WriteTo.Console()
	.CreateLogger();

using var services = new ServiceCollection()
	.AddLogging(builder => builder.AddSerilog(Log.Logger))
	.Configure<HubReplOptions>(options => options.PortName = "COM5")
	.AddSingleton<IHubRepl, HubRepl>()
	.BuildServiceProvider();

var hub = services.GetRequiredService<IHubRepl>();

await hub.Connect();

var motorA = new Motor(hub, HubPort.A);

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
await using var motorPair1 = new MotorPair(hub, MotorPairs.PAIR_1);
await motorPair1.Pair(HubPort.A, HubPort.B);

Console.WriteLine("move for 2 sec");
await motorPair1.Move(0);
await Task.Delay(2000);

Console.WriteLine("stop for 1 sec");
await motorPair1.Stop();
await Task.Delay(1000);


Console.WriteLine("move 300 degree for 2 sec");
await motorPair1.MoveForDegree(300, 0);
await Task.Delay(2000);

Console.WriteLine("move for 2 sec");
await motorPair1.MoveForTime(2000, 10);
await Task.Delay(2000);


Console.WriteLine("move tank 100, 200 for 2 sec");
await motorPair1.MoveTank(100, 200);
await Task.Delay(2000);


Console.WriteLine("move tank -300 degree for 2 sec");
await motorPair1.MoveTankForDegree(-300, 100, -100);
await Task.Delay(2000);


Console.WriteLine("move tank for 2 sec");
await motorPair1.MoveTankForTime(2000, 1000, 2000);
await Task.Delay(2000);


Console.WriteLine("unpair");
await motorPair1.Unpair();

Console.WriteLine("=============================");

var colorSensor = new ColorSencor(hub, HubPort.C);

for (int i = 0; i < 10; i++) {
	Console.WriteLine("color = {0}", await colorSensor.GetColor());
	Console.WriteLine("rgbi = {0}", await colorSensor.GetRgbi());
	await Task.Delay(500);
}

for (int i = 0; i < 10; i++) {
	Console.WriteLine("reflection = {0}", await colorSensor.GetReflection());
	await Task.Delay(500);
}

Console.WriteLine("=============================");

var lightMatrix = new LightMatrix(hub);

Console.WriteLine("set left as top");
await lightMatrix.SetOrientation(LightMatrixOrientation.LEFT);

Console.WriteLine("get orientation");
Console.WriteLine(await lightMatrix.GetOrientation());

Console.WriteLine("show butterfly");
await lightMatrix.Show(LightMatrixImage.IMAGE_BUTTERFLY);
await Task.Delay(2000);

await lightMatrix.Clear();

Console.WriteLine("loop set pixel");
for (byte x = 0; x < 5; x++) {
	for (byte y = 0; y < 5; y++) {
		await lightMatrix.SetPixel(x, y, (byte)(100 - x * y * 100 / 25));
		await Task.Delay(200);
	}
}

Console.WriteLine("write text");
await lightMatrix.Write("hi you are ?");

Console.WriteLine("=============================");

Console.ReadLine();
