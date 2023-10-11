namespace Fun.LEGO.Spike;

public class Motor {
    private readonly IHubRepl hubRepl;
    private readonly HubPort hubPort;

    public Motor(IHubRepl hubRepl, HubPort hubPort) {
        this.hubRepl = hubRepl;
        this.hubPort = hubPort;
    }

    public async Task<int> GetAbsolutePosition() =>
        int.Parse(await hubRepl.SendCodeAndWaitResult($"motor.absolute_position({(int)hubPort})"));

    public async Task<int> GetRelativePosition() =>
        int.Parse(await hubRepl.SendCodeAndWaitResult($"motor.relative_position({(int)hubPort})"));

    /// <summary>
    /// Change the position used as the offset 
    /// </summary>
    /// <param name="position">The degree of the motor</param>
    public async Task ResetRelativePosition(int position) =>
        await hubRepl.SendCodeAndWaitResult($"motor.reset_relative_position({(int)hubPort}, {position})");


    /// <summary>
    /// degree/sec
    /// </summary>
    public async Task<int> GetVelocity() =>
        int.Parse(await hubRepl.SendCodeAndWaitResult($"motor.velocity({(int)hubPort})"));


    public async Task<int> GetPWM() =>
        int.Parse(await hubRepl.SendCodeAndWaitResult($"motor.get_duty_cycle({(int)hubPort})"));

    public async Task<int> SetPWM(int value) =>
        int.Parse(await hubRepl.SendCodeAndWaitResult($"motor.set_duty_cycle({(int)hubPort}, {value})"));


    public Task Stop() => hubRepl.SendCode($"motor.stop({(int)hubPort})");

    /// <summary>
    /// Start a motor at a constant speed
    /// </summary>
    /// <param name="velocity">
    /// The velocity in degrees/sec
    /// Value ranges depends on motor type.
    /// Small motor (essential): -660 to 660
    /// Medium motor: -1110 to 1110
    /// Large motor: -1050 to 1050
    /// </param>
    /// <param name="acceleration">degree/sec^2 0-10000</param>
    public Task Run(int velocity, int acceleration = 1000) => hubRepl.SendCode($"motor.run({(int)hubPort}, {velocity}, acceleration = {acceleration})");

    /// <summary>
    /// Turn a motor for a specific number of degrees
    /// </summary>
    /// <param name="degrees"></param>
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
    public Task RunForDegree(int degrees, int velocity, MotorStop stop = MotorStop.BREAK, int acceleration = 1000, int deceleration = 1000) =>
        hubRepl.SendCode($"motor.run_for_degrees({(int)hubPort}, {degrees}, {velocity}, stop = {(int)stop}, acceleration = {acceleration}, deceleration = {deceleration})");

    /// <summary>
    /// Turn a motor for a specific number of ms
    /// </summary>
    /// <param name="duration">The duration in milliseconds</param>
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
    public Task RunForTime(int duration, int velocity, MotorStop stop = MotorStop.BREAK, int acceleration = 1000, int deceleration = 1000) =>
        hubRepl.SendCode($"motor.run_for_time({(int)hubPort}, {duration}, {velocity}, stop = {(int)stop}, acceleration = {acceleration}, deceleration = {deceleration})");

    /// <summary>
    /// Turn a motor to an absolute position
    /// </summary>
    /// <param name="position">The degree of the motor</param>
    /// <param name="velocity">
    /// The velocity in degrees/sec
    /// Value ranges depends on motor type.
    /// Small motor (essential): -660 to 660
    /// Medium motor: -1110 to 1110
    /// Large motor: -1050 to 1050
    /// </param>
    /// <param name="direction">The direction to turn</param>
    /// <param name="stop">The behavior of the motor after it has stopped</param>
    /// <param name="acceleration">The acceleration (deg/sec²) (0 - 10000)</param>
    /// <param name="deceleration">The acceleration (deg/sec²) (0 - 10000)</param>
    public Task RunToAbsolutePosition(int position, int velocity, MotorDirection direction = MotorDirection.CLOCKWISE, MotorStop stop = MotorStop.BREAK, int acceleration = 1000, int deceleration = 1000) =>
        hubRepl.SendCode($"motor.run_to_absolute_position({(int)hubPort}, {position}, {velocity}, direction = {(int)direction}, stop = {(int)stop}, acceleration = {acceleration}, deceleration = {deceleration})");

    /// <summary>
    /// Turn a motor to an relative position
    /// </summary>
    /// <param name="position">The degree of the motor</param>
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
    public Task RunToRelativePosition(int position, int velocity, MotorStop stop = MotorStop.BREAK, int acceleration = 1000, int deceleration = 1000) =>
        hubRepl.SendCode($"motor.run_to_relative_position({(int)hubPort}, {position}, {velocity}, stop = {(int)stop}, acceleration = {acceleration}, deceleration = {deceleration})");
}

public enum MotorStop {
    /// <summary>
    /// to make the motor coast until a stop
    /// </summary>
    COAST = 0,
    /// <summary>
    /// to brake and continue to brake after stop
    /// </summary>
    BREAK = 1,
    /// <summary>
    /// to tell the motor to hold its position
    /// </summary>
    HOLD = 2,
    /// <summary>
    /// to tell the motor to keep running at whatever velocity it's running at until it gets another command
    /// </summary>
    CONTINUE = 3,
    /// <summary>
    /// to make the motor brake until stop and then coast and compensate for inaccuracies in the next command
    /// </summary>
    SMART_COAST = 4,
    /// <summary>
    /// to make the motor brake and continue to brake after stop and compensate for inaccuracies in the next command
    /// </summary>
    SMART_BRAKE = 5
}

public enum MotorDirection {
    CLOCKWISE = 0,
    COUNTERCLOCKWISE = 1,
    SHORTEST_PATH = 2,
    LONGEST_PATH = 3,
}

public enum MotorState {
    READY = 0,
    RUNNING = 1,
    STALLED = 2,
    ERROR = 3,
    DISCONNECTED = 4,
}