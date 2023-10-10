using System.IO.Ports;
using System.Text;

const string STOP_PROGRAM = "\x03";
const string START = "\x04";
const string NEWLINE = "\r\n";

using var port = new SerialPort {
    NewLine = NEWLINE,
    BaudRate = 115200,
    PortName = "COM5",
    Encoding = Encoding.UTF8
};

port.Open();
port.WriteTimeout = 5000;
port.RtsEnable = true;
port.DtrEnable = true;

var sendCode = (string code) => {
    var codes = code.Split(Environment.NewLine).Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));
    foreach (var item in codes) {
        if (item == STOP_PROGRAM) Console.WriteLine("SEND: " + nameof(STOP_PROGRAM));
        else if (item == START) Console.WriteLine("SEND: " + nameof(START));
        else Console.WriteLine("SEND: " + item);

        port.WriteLine(item);
    }
};

_ = Task.Run(() => {
    while (true) {
        try {
            Console.WriteLine($"RECEIVE: {port.ReadLine()}");
        }
        catch (Exception ex) {
            Console.WriteLine(ex);
        }
    }
});


sendCode(STOP_PROGRAM);

sendCode("import motor, time");
sendCode("from hub import port");

while (true) {
    if (Console.ReadKey().KeyChar == 'w') {
        sendCode("motor.run(port.A, 1000)");
        await Task.Delay(1000);
        sendCode("motor.velocity(port.A)");
    }
    else if (Console.ReadKey().KeyChar == 's') {
        sendCode("motor.stop(port.A)");
    }
}

// sendCommand("""{"i":"abc","m":"scratch.display_text","p":{"text":"hi"}\r\n}"""u8);


// Console.WriteLine("Press Enter to exit ...");
// Console.ReadLine();