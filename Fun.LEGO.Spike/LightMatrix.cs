namespace Fun.LEGO.Spike;

public class LightMatrix {
	private readonly IHubRepl hubRepl;

	public LightMatrix(IHubRepl hubRepl) {
		this.hubRepl = hubRepl;
	}

	public Task Clear() =>
		hubRepl.SendCodeAndWaitResult("light_matrix.clear()");

	/// <summary>
	/// Retrieve the current orientation of the Light Matrix.
	/// </summary>
	public async Task<LightMatrixOrientation> GetOrientation() =>
		(LightMatrixOrientation)(int.Parse(await hubRepl.SendCodeAndWaitResult("light_matrix.get_orientation()")));

	/// <summary>
	/// Change the orientation of the Light Matrix. All subsequent calls will use the new orientation.
	/// </summary>
	public Task SetOrientation(LightMatrixOrientation top) =>
		hubRepl.SendCodeAndWaitResult($"light_matrix.set_orientation({(int)top})");

	/// <summary>
	/// Retrieve the intensity of a specific pixel on the Light Matrix.
	/// </summary>
	/// <param name="x">The X value, range (0 - 4)</param>
	/// <param name="y">The Y value, range (0 - 4)</param>
	public async Task<int> GetPixel(byte x, byte y) =>
		int.Parse(await hubRepl.SendCodeAndWaitResult($"light_matrix.get_pixel({x}, {y})"));

	/// <summary>
	/// Sets the brightness of one pixel (one of the 25 LEDs) on the Light Matrix.
	/// </summary>
	/// <param name="x">The X value, range (0 - 4)</param>
	/// <param name="y">The Y value, range (0 - 4)</param>
	/// <param name="intensity">How bright to light up the pixel</param>
	public Task SetPixel(byte x, byte y, byte intensity) =>
		hubRepl.SendCodeAndWaitResult($"light_matrix.set_pixel({x}, {y}, {intensity})");

	/// <summary>
	/// Change all the lights at the same time.
	/// </summary>
	/// <param name="pixels">A list containing light intensity values for all 25 pixels.</param>
	public async Task Show(IEnumerable<byte> pixels) {
		var value = string.Join(", ", pixels);
		await hubRepl.SendCodeAndWaitResult($"light_matrix.show([{value}])");
	}

	/// <summary>
	/// Display one of the built-in images on the display.
	/// </summary>
	public async Task Show(LightMatrixImage image) =>
		await hubRepl.SendCodeAndWaitResult($"light_matrix.show_image({(int)image})");

	/// <summary>
	/// Displays text on the Light Matrix, one letter at a time, scrolling from right to left, except if there is a single character to show (which will not scroll)
	/// </summary>
	public async Task Write(string text, byte intensity = 100, int timePerCharacterMs = 500) =>
		await hubRepl.SendCode($"light_matrix.write('{text}', {intensity}, {timePerCharacterMs})");
}


public enum LightMatrixOrientation {
	UP = 0,
	RIGHT = 1,
	LEFT = 3,
	DOWN = 2,
}

public enum LightMatrixImage {
	IMAGE_HEART = 1,
	IMAGE_HEART_SMALL = 2,
	IMAGE_HAPPY = 3,
	IMAGE_SMILE = 4,
	IMAGE_SAD = 5,
	IMAGE_CONFUSED = 6,
	IMAGE_ANGRY = 7,
	IMAGE_ASLEEP = 8,
	IMAGE_SURPRISED = 9,
	IMAGE_SILLY = 10,
	IMAGE_FABULOUS = 11,
	IMAGE_MEH = 12,
	IMAGE_YES = 13,
	IMAGE_NO = 14,
	IMAGE_CLOCK12 = 15,
	IMAGE_CLOCK1 = 16,
	IMAGE_CLOCK2 = 17,
	IMAGE_CLOCK3 = 18,
	IMAGE_CLOCK4 = 19,
	IMAGE_CLOCK5 = 20,
	IMAGE_CLOCK6 = 21,
	IMAGE_CLOCK7 = 22,
	IMAGE_CLOCK8 = 23,
	IMAGE_CLOCK9 = 24,
	IMAGE_CLOCK10 = 25,
	IMAGE_CLOCK11 = 26,
	IMAGE_ARROW_N = 27,
	IMAGE_ARROW_NE = 28,
	IMAGE_ARROW_E = 29,
	IMAGE_ARROW_SE = 30,
	IMAGE_ARROW_S = 31,
	IMAGE_ARROW_SW = 32,
	IMAGE_ARROW_W = 33,
	IMAGE_ARROW_NW = 34,
	IMAGE_GO_RIGHT = 35,
	IMAGE_GO_LEFT = 36,
	IMAGE_GO_UP = 37,
	IMAGE_GO_DOWN = 38,
	IMAGE_TRIANGLE = 39,
	IMAGE_TRIANGLE_LEFT = 40,
	IMAGE_CHESSBOARD = 41,
	IMAGE_DIAMOND = 42,
	IMAGE_DIAMOND_SMALL = 43,
	IMAGE_SQUARE = 44,
	IMAGE_SQUARE_SMALL = 45,
	IMAGE_RABBIT = 46,
	IMAGE_COW = 47,
	IMAGE_MUSIC_CROTCHET = 48,
	IMAGE_MUSIC_QUAVER = 49,
	IMAGE_MUSIC_QUAVERS = 50,
	IMAGE_PITCHFORK = 51,
	IMAGE_XMAS = 52,
	IMAGE_PACMAN = 53,
	IMAGE_TARGET = 54,
	IMAGE_TSHIRT = 55,
	IMAGE_ROLLERSKATE = 56,
	IMAGE_DUCK = 57,
	IMAGE_HOUSE = 58,
	IMAGE_TORTOISE = 59,
	IMAGE_BUTTERFLY = 60,
	IMAGE_STICKFIGURE = 61,
	IMAGE_GHOST = 62,
	IMAGE_SWORD = 63,
	IMAGE_GIRAFFE = 64,
	IMAGE_SKULL = 65,
	IMAGE_UMBRELLA = 66,
	IMAGE_SNAKE = 67,
}