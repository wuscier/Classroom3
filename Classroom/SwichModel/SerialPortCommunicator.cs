using System;
using System.IO.Ports;
using System.Linq;
using Common.UiMessage;
using Serilog;

namespace Classroom.SwichModel
{
    public class SerialPortCommunicator
    {
        private static readonly object SyncRoot = new object();

        private static volatile SerialPortCommunicator _instance;

        public static SerialPortCommunicator Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (SyncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new SerialPortCommunicator();
                    }
                }

                return _instance;
            }
        }

        private readonly string _defaultPortName;

        private SerialPort DefaultSerialPort { get; set; }

        public SerialPortCommunicator()
        {
            var portNames = SerialPort.GetPortNames();
            if (portNames?.Length == 0)
            {
                Log.Logger.Error($"Log.SerialPortCommunicator,没有可用的串口");
                MessageQueueManager.Instance.AddInfo("没有可用的串口");
                return;
            }
            _defaultPortName = portNames.FirstOrDefault();
        }

        public void InitDefaultSerialPort()
        {
            try
            {
                if (DefaultSerialPort == null)
                {
                    DefaultSerialPort = new SerialPort(_defaultPortName, 9600, Parity.None, 8, StopBits.One);
                    DefaultSerialPort.DataReceived += DefaultSerialPort_DataReceived;
                }

                if (!DefaultSerialPort.IsOpen)
                {
                    Log.Logger.Information($"Log.SerialPortCommunicator, Open serial port：{DefaultSerialPort.PortName}");
                    DefaultSerialPort.Open();
                }
                Log.Logger.Information($"Log.SerialPortCommunicator, Open DefaultSerialPort");

            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Log.SerialPortCommunicator,InitDefaultSerialPort exception:{ex.Message}");
            }
        }

        public void CloseDefaultSerialPort()
        {
            try
            {
                if (DefaultSerialPort == null || !DefaultSerialPort.IsOpen) return;
                Log.Logger.Information($"Log.SerialPortCommunicator, Close serial port:{DefaultSerialPort.PortName}");
                DefaultSerialPort.Close();
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Log.SerialPortCommunicator,CloseDefaultSerialPort exception：:{ex.Message}");
            }
        }

        private void DefaultSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Log.Logger.Information($"Log.SerialPortCommunicator,DataReceived");
                var serialPort = sender as SerialPort;
                var inData = serialPort?.ReadExisting();
                Log.Logger.Information($"Log.SerialPortCommunicator,DataReceived:{inData}");
                App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    CommandController.Instance.ExecuteCommand(inData);
                }));
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"Log.SerialPortCommunicator,DefaultSerialPort_DataReceived exception：{ex.Message}");
            }
        }
    }
}
