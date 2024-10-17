using Developer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

var ports = SerialPortController.ListAvaliablePorts();

//Reader port
var serialPort = new SerialPortController(
    comPort: "COM1",
    baudRate: 115200,
    parity: Parity.None,
    dataBits: 8,
    stopBits: StopBits.One,
    readTimeOut: 250,
    writeTimeOut: 250
    );

//Writer port
//var serialPort2 = new SerialPortController(
//    comPort: "COM2",
//    baudRate: 115200,
//    parity: Parity.None,
//    dataBits: 8,
//    stopBits: StopBits.One,
//    readTimeOut: 250,
//    writeTimeOut: 250
//);


var result = Task.Run(async() =>
{
     var response = await serialPort.SendAndGetResponse(message: "TEST TEST TEST BABY YASUO \r\n", timeout: 5000);
     Console.WriteLine(response);
});

while (true)
{
    Thread.Sleep(500);
    Console.WriteLine("Code running...");

    
}
