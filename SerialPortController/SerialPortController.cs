using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Threading.Channels;

namespace Developer
{
    public class SerialPortController
    {
        #region Class Properties
        private SerialPort? _serialPort;
        private Stream? _bufferStream;

        private static int BUFFER_SIZE = 128; // Max okunacak byte sayisi

        private static int PACKET_SIZE = 2; // Her 2 byte okundugunda asyncallback tetiklenir.

        public volatile bool READ_FLAG = false;


        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public int DataBits { get; set; }
        public string ComPort { get; set; }

        public List<byte> Buffer = new();

        public delegate void WriteAndRead(string message);
        #endregion


        #region Constructors
        public SerialPortController(string comPort, int baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            this.BaudRate = baudRate;
            this.Parity = parity;
            this.StopBits = stopBits;
            this.ComPort = comPort;

            //Open serial port
            OpenPort();

            _serialPort.WriteTimeout = 500; // defualt 500 ms WriteTimeOut
            _serialPort.ReadTimeout = 500; // defualt 500 ms WriteTimeOut
        }
        public SerialPortController(string comPort, int baudRate, int dataBits, Parity parity, StopBits stopBits, int writeTimeOut, int readTimeOut)
        {
            this.BaudRate = baudRate;
            this.Parity = parity;
            this.StopBits = stopBits;
            this.ComPort = comPort;

            OpenPort();

            _serialPort.WriteTimeout = writeTimeOut; // defualt 500 ms WriteTimeOut
            _serialPort.ReadTimeout = readTimeOut; // defualt 500 ms WriteTimeOut

        }

        #endregion


        #region Serial Port Controller Class Helper Methods

        public static string[] ListAvaliablePorts() => SerialPort.GetPortNames();

        private void OpenPort()
        {

            try
            {
                _serialPort = new SerialPort(this.ComPort);
                _serialPort.BaudRate = this.BaudRate;
                _serialPort.Parity = this.Parity;
                _serialPort.StopBits = this.StopBits;
                _serialPort.Handshake = Handshake.None;
                _serialPort.ReadBufferSize = 4096;
                _serialPort.WriteBufferSize = 4096;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

                _serialPort.Open();

                // Initialize the buffer stream
                _bufferStream = _serialPort.BaseStream;

            }

            catch (IOException ex)
            {
                throw new IOException("Could not open port: " + ComPort, ex);

            }

            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException("Null port parameter provided. ", ex);
            }

            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException(ComPort + " is already open !", ex);
            }

        }

        public void WriteData(string message) => _serialPort.WriteLine(message);

        public async Task<string> AsyncReadBuffer(int timeout)
        {

            var sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeout)
            {

                if (!READ_FLAG)
                {
                    continue;
                }


                var message = Encoding.ASCII.GetString(Buffer.ToArray());

                Console.WriteLine($"Buffer data: {message} ");

                Buffer.Clear();

                Console.WriteLine("Enter a value to continue (Y/N)");
                Console.ReadLine();

            }

            sw.Stop();

            READ_FLAG = false;

            return Encoding.ASCII.GetString(Buffer.ToArray());
        } 

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
  
            Buffer.AddRange(Encoding.ASCII.GetBytes(_serialPort.ReadExisting()));

            //line feed char geldiginde flag kontrol edilerek veri okunur
            if (Buffer.Any(val => val == 0x0A))
            {
                READ_FLAG = true;
            }
        }

        public async Task<string> Drive(string message , int timeout)
        {
            //reset flag
            READ_FLAG = false;
            WriteData(message);
            return await AsyncReadBuffer(timeout);
        }
        #endregion




    }
}