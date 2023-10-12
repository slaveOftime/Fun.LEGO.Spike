namespace Fun.LEGO.Spike;

public class ColorSencor {
	private readonly IHubRepl hubRepl;
	private readonly HubPort hubPort;

	public ColorSencor(IHubRepl hubRepl, HubPort hubPort) {
		this.hubRepl = hubRepl;
		this.hubPort = hubPort;
	}

	/// <summary>
	/// Returns the colour value of the detected colour. Use the color module to map the colour value to a specific colour.
	/// </summary>
	public async Task<Color> GetColor() =>
		(Color)int.Parse(await hubRepl.SendCodeAndWaitResult($"color_sensor.color({(int)hubPort})"));

	/// <summary>
	/// Retrieves the intensity of the reflected light (0-100%).
	/// </summary>
	public async Task<byte> GetReflection() =>
		byte.Parse(await hubRepl.SendCodeAndWaitResult($"color_sensor.reflection({(int)hubPort})"));

	/// <summary>
	/// Retrieves the overall colour intensity and intensity of red, green and blue.
	/// </summary>
	public async Task<(byte red, byte blue, byte gree, ushort indensity)> GetRgbi() {
		var result = await hubRepl.SendCodeAndWaitResult($"color_sensor.rgbi({(int)hubPort})");

		var index1 = result.IndexOf(",");
		var index2 = result.IndexOf(",", index1 + 1);
		var index3 = result.IndexOf(",", index2 + 1);

		var red = byte.Parse(result.AsSpan().Slice(1, index1 - 1));
		var green = byte.Parse(result.AsSpan().Slice(index1 + 2, index2 - index1 - 2));
		var blue = byte.Parse(result.AsSpan().Slice(index2 + 2, index3 - index2 - 2));
		var indensity = ushort.Parse(result.AsSpan().Slice(index3 + 2, result.Length - index3 - 3));

		return (red, blue, green, indensity);
	}
}

public enum Color {
	Red = 0,
	Green = 1,
	Blue = 2,
	Magenta = 3,
	Yellow = 4,
	Orange = 5,
	Azure = 6,
	Black = 7,
	White = 8,
}