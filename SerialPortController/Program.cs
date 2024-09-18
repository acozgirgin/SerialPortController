using Developer;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

var ports = SerialPortController.ListAvaliablePorts();

var serialPort = new SerialPortController(
    comPort: "COM1",
    baudRate: 115200,
    parity: Parity.None,
    dataBits: 8,
    stopBits: StopBits.One,
    readTimeOut: 250,
    writeTimeOut: 250
    );

//Start Async Read
serialPort.StartAsyncRead();

int secondCounter = 1;
while (true)
{
    Thread.Sleep(1000);
    Console.WriteLine("Buffer data: ");

    foreach (var data in serialPort.bufferAsync)

        ++secondCounter;

    if (secondCounter >= 20)
    {
        serialPort.StopAsyncRead();
    }
}