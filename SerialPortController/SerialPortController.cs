using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using System.Threading;

namespace Developer
{
    public class SerialPortController
    {
        #region Class Properties
        private SerialPort? _serialPort;
        private Stream? _bufferStream;

        private static int BUFFER_SIZE = 240;

        private static int PACKET_SIZE = 12;

        public CancellationTokenSource _cancellationTokenSource;

        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public int DataBits { get; set; }
        public string ComPort { get; set; }

        public byte[] buffer;

        public List<byte> bufferAsync;

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
                _serialPort.Open();

                // Initialize array and list buffers 
                buffer = new byte[BUFFER_SIZE];
                bufferAsync = new List<byte>();

                // Initialize the buffer stream
                _bufferStream = _serialPort.BaseStream;

                // Initialize the CancellationTokenSource
                _cancellationTokenSource = new CancellationTokenSource();
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

        public void StartAsyncRead()
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            _bufferStream.BeginRead(buffer, 0, PACKET_SIZE, new AsyncCallback(ReadCallback), null);
        }

        public void StopAsyncRead() => _cancellationTokenSource.Cancel();

        private async void ReadCallback(IAsyncResult ar)
        {
            //Calculate offset value to write buffer index

            try
            {
                var result = await Task.Run(() => _bufferStream.ReadAsync(buffer, offset: 0, count: PACKET_SIZE));

                //Add byte list asynch buffer
                bufferAsync.AddRange(Array.FindAll(buffer, val => val is not 0));


                //Check bufferAsync List
                if (bufferAsync.Count >= BUFFER_SIZE)
                {
                    await _bufferStream.FlushAsync();

                    bufferAsync.Clear();

                    Console.WriteLine("Buffer limit exceeded.");


                    //Fill buffer with zeros again
                    for (int i = 0; i < buffer.Length; ++i) buffer[i] = 0;

                }

                //Start async read again
                StartAsyncRead();

            }
            catch (TimeoutException)
            {
                StartAsyncRead();
            }


        }

        #endregion




    }
}