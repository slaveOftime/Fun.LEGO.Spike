namespace Fun.LEGO.Spike;

public class MotorPair {
	private readonly IHubRepl hubRepl;
	private readonly MotorPairs pair;

	public MotorPair(IHubRepl hubRepl, MotorPairs pair) {
		this.hubRepl = hubRepl;
		this.pair = pair;
	}

	public MotorPairs Pair => pair;
	public bool IsPaired { get; internal set; }


	public Task Stop(MotorStop stop = MotorStop.BREAK) =>
		hubRepl.SendCodeAndWaitResult($"motor_pair.stop({(int)pair}, stop = {(int)stop})");

	public async Task Unpair() {
		await hubRepl.SendCodeAndWaitResult($"motor_pair.unpair({(int)pair})");
		IsPaired = false;
	}


	/// <summary>
	/// Move a Motor Pair at a constant speed until a new command is given.
	/// </summary>
	/// <param name="steering">The steering (-100 to 100)</param>
	/// <param name="velocity">
	/// The velocity in degrees/sec
	/// Value ranges depends on motor type.
	/// Small motor (essential): -660 to 660
	/// Medium motor: -1110 to 1110
	/// Large motor: -1050 to 1050
	/// </param>
	/// <param name="acceleration">degree/sec^2 0-10000</param>
	public Task Move(byte steering, short velocity = 360, ushort acceleration = 1000) =>
		hubRepl.SendCode($"motor_pair.move({(int)pair}, {steering}, velocity = {velocity}, acceleration = {acceleration})");

	/// <summary>
	/// Move a Motor Pair at a constant speed for a specific number of degrees.
	/// </summary>
	/// <param name="degrees"></param>
	/// <param name="steering">The steering (-100 to 100)</param>
	/// <param name="velocity">
	/// The velocity in degrees/sec
	/// Value ranges depends on motor type.
	/// Small motor (essential): -660 to 660
	/// Medium motor: -1110 to 1110
	/// Large motor: -1050 to 1050
	/// </param>
	/// <param name="stop">The behavior of the motor after it has stopped</param>
	/// <param name="acceleration">The acceleration (deg/sec²) (0 - 10000)</param>
	/// <param name="deceleration">The acceleration (deg/sec²) (0 - 10000)</param>
	public Task MoveForDegree(int degrees, byte steering, short velocity = 360, MotorStop stop = MotorStop.BREAK, ushort acceleration = 1000, ushort deceleration = 1000) =>
		hubRepl.SendCode($"motor_pair.move_for_degrees({(int)pair}, {degrees},{steering}, velocity = {velocity}, stop = {(int)stop}, acceleration = {acceleration}, deceleration = {deceleration})");

	/// <summary>
	/// Move a Motor Pair at a constant speed for a specific duration.
	/// </summary>
	/// <param name="duration">The duration in milliseconds</param>
	/// <param name="steering">The steering (-100 to 100)</param>
	/// <param name="velocity">
	/// The velocity in degrees/sec
	/// Value ranges depends on motor type.
	/// Small motor (essential): -660 to 660
	/// Medium motor: -1110 to 1110
	/// Large motor: -1050 to 1050
	/// </param>
	/// <param name="stop">The behavior of the motor after it has stopped</param>
	/// <param name="acceleration">The acceleration (deg/sec²) (0 - 10000)</param>
	/// <param name="deceleration">The acceleration (deg/sec²) (0 - 10000)</param>
	public Task MoveForTime(int duration, byte steering, short velocity = 360, MotorStop stop = MotorStop.BREAK, ushort acceleration = 1000, ushort deceleration = 1000) =>
		hubRepl.SendCode($"motor_pair.move_for_time({(int)pair}, {duration}, {steering}, velocity = {velocity}, stop = {(int)stop}, acceleration = {acceleration}, deceleration = {deceleration})");

	/// <summary>
	/// Perform a tank move on a Motor Pair at a constant speed until a new command is given.
	/// </summary>
	/// <param name="leftVelocity">The velocity (deg/sec) of the left motor.</param>
	/// <param name="rightVelocity">The velocity (deg/sec) of the right motor.</param>
	/// <param name="acceleration">The acceleration (deg/sec²) (0 - 10000)</param>
	public Task MoveTank(int leftVelocity, int rightVelocity, ushort acceleration = 1000) =>
		hubRepl.SendCode($"motor_pair.move_tank({(int)pair}, {leftVelocity}, {rightVelocity}, acceleration = {acceleration})");

	/// <summary>
	/// Move a Motor Pair at a constant speed for a specific number of degrees.
	/// </summary>
	/// <param name="degrees"></param>
	/// <param name="leftVelocity">The velocity (deg/sec) of the left motor.</param>
	/// <param name="rightVelocity">The velocity (deg/sec) of the right motor.</param>
	/// <param name="stop">The behavior of the motor after it has stopped</param>
	/// <param name="acceleration">The acceleration (deg/sec²) (0 - 10000)</param>
	/// <param name="deceleration">The acceleration (deg/sec²) (0 - 10000)</param>
	public Task MoveTankForDegree(int degrees, int leftVelocity, int rightVelocity, MotorStop stop = MotorStop.BREAK, ushort acceleration = 1000, ushort deceleration = 1000) =>
		hubRepl.SendCode($"motor_pair.move_tank_for_degrees({(int)pair}, {degrees}, {leftVelocity}, {rightVelocity}, stop = {(int)stop}, acceleration = {acceleration}, deceleration = {deceleration})");

	/// <summary>
	/// Perform a tank move on a Motor Pair at a constant speed for a specific amount of time.
	/// </summary>
	/// <param name="duration">The duration in milliseconds</param>
	/// <param name="leftVelocity">The velocity (deg/sec) of the left motor.</param>
	/// <param name="rightVelocity">The velocity (deg/sec) of the right motor.</param>
	/// <param name="stop">The behavior of the motor after it has stopped</param>
	/// <param name="acceleration">The acceleration (deg/sec²) (0 - 10000)</param>
	/// <param name="deceleration">The acceleration (deg/sec²) (0 - 10000)</param>
	public Task MoveTankForTime(int duration, int leftVelocity, int rightVelocity, MotorStop stop = MotorStop.BREAK, ushort acceleration = 1000, ushort deceleration = 1000) =>
		hubRepl.SendCode($"motor_pair.move_tank_for_time({(int)pair}, {duration}, {leftVelocity}, {rightVelocity}, stop = {(int)stop}, acceleration = {acceleration}, deceleration = {deceleration})");
}

public enum MotorPairs {
	PAIR_1 = 0,
	PAIR_2 = 1,
	PAIR_3 = 2,
}
