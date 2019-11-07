using System;
using System.Text;
using System.Windows;
using Funbit.Ets.Telemetry.Server.Data.Reader;

namespace Funbit.Ets.Telemetry.Server.Data
{
    public class Ets2TelemetryDataReader : IDisposable
    {
        /// <summary>
        /// ETS2 telemetry plugin maps the data to this mapped file name.
        /// </summary>
        const string Ets2TelemetryMappedFileName = "Local\\Ets2TelemetryServer";

        readonly SharedProcessMemory<Ets2TelemetryStructure> _sharedMemory =
            new SharedProcessMemory<Ets2TelemetryStructure>(Ets2TelemetryMappedFileName);

        readonly Ets2TelemetryData _data = new Ets2TelemetryData();

        readonly object _lock = new object();

        // ReSharper disable once InconsistentNaming
        static readonly Lazy<Ets2TelemetryDataReader> instance = new Lazy<Ets2TelemetryDataReader>(
            () => new Ets2TelemetryDataReader());
        public static Ets2TelemetryDataReader Instance => instance.Value;

        public bool IsConnected => _sharedMemory.IsConnected;

        public IEts2TelemetryData Read()
        {
            lock (_lock)
            {
                _sharedMemory.Data = default(Ets2TelemetryStructure);
                _sharedMemory.Read();
                _data.Update(_sharedMemory.Data);
                return _data;
            }
        }
        /// <summary>
        /// Returns A String Array Of Some Deserialized Data.
        /// </summary>
        /// <returns></returns>
        public String[] getDeserializedTelemetryData()
        {
            return generateDeserializedTelemetryData(_data);
        }
 
        /// <summary>
        ///  Simple Deserialization Method For The "EtsTelemetryStructure"
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private String[] generateDeserializedTelemetryData(Ets2TelemetryData data)
        {
            /// Here I'm Only Using A Few Values Just For Example Purposes But Essentially You Need To Take
            /// Every Property You're Interested In From The Struct "Ets2TelemetryStructure" And Place It Into The String Array.
            /// Essentially You Can Repeat This Process For ALL The Properties Listed In "EtsTelemetryStructure"
            String[] deserializedData = new string[] { data.Truck.Acceleration.ToString(),data.Truck.AirPressure.ToString(),
            data.Truck.OilPressure.ToString(),data.Truck.Fuel.ToString(),data.Truck.FuelCapacity.ToString(),data.Truck.EngineRpm.ToString()};
            

            return deserializedData;
        }

        public void serialTelemetryDataWrite()
        {
            var _SerialPort1 = serialPortInitialisation();
            _SerialPort1.Open();
            System.Threading.Thread.Sleep(100);
            _SerialPort1.Write(_data.ToString());
        }

        public void SerialNativeRead()
        {
            int blockLimit = 4096;
            byte[] buffer = new byte[blockLimit];
            String Output = "";
            String _ReceiveBufferString = "";
            int BytesRead = 0;
            var _SerialPort1 = serialPortInitialisation();
            _SerialPort1.Open();
            System.Threading.Thread.Sleep(500);
            _SerialPort1.WriteLine("All Your Base Are Belong To Us");
            System.Threading.Thread.Sleep(500);




            //while (!_ReceiveBufferString.Contains("\n"))
            //{
            //    BytesRead = _SerialPort1.BaseStream.Read(buffer, 0, 1);
            //    _ReceiveBufferString = _ReceiveBufferString + Encoding.ASCII.GetString(buffer, 0, BytesRead);
            //    Console.WriteLine("SerialNativeRead: Data Is = " + _ReceiveBufferString);
            //}

            //_ReceiveBufferString = _ReceiveBufferString.Remove(_ReceiveBufferString.Length - 1);


            //if (BytesRead >= 4)
            //{
            //    Output = Encoding.ASCII.GetString(buffer, 0, BytesRead);

            //}

            Console.WriteLine("SerialNativeRead: Data Is = " + Output);

        }

        private System.IO.Ports.SerialPort serialPortInitialisation()
        {
            var _SerialPort1 = new System.IO.Ports.SerialPort();
            try
            {
                _SerialPort1.PortName = "COM4";
                _SerialPort1.BaudRate = 9600;
                _SerialPort1.NewLine = "\n";
                
                //var gameData = _data;
                //System.IO.Ports.SerialPort Port = new System.IO.Ports.SerialPort("COM4");
                //Port.BaudRate = 9600;
                //Port.Open();
                //Port.WriteTimeout = 8000;
                //Port.Write("Craig");
            }
            catch (Exception ex)
            {
                // handle exception
                if (ex is UnauthorizedAccessException)
                {
                    System.Windows.Forms.MessageBox.Show("Unauthorized Access", "Unauthorized attempt detected");
                    System.Diagnostics.Debug.WriteLine("COMS ERROR: Failed To Initialise Port(Unauthorized Access)");
                    System.Diagnostics.Debug.WriteLine("Unauthorized Access Source is " + ex.Source);
                    return null;
                }

                if (ex is System.IO.IOException)
                {
                    System.Windows.Forms.MessageBox.Show("Please Check That A COMS Device Exists At That Port", "Cannot Establish COMS Link");
                    System.Diagnostics.Debug.WriteLine("COMS ERROR: Failed To Initialise Port(Cannot Establish COMS Link)");
                    return null;
                }
            }
            return _SerialPort1;

        }
        public void connectToArduino()
        {

            try
            {
                var _SerialPort1 = new System.IO.Ports.SerialPort();
                _SerialPort1.PortName = "COM11";
                _SerialPort1.BaudRate = 9600;
                _SerialPort1.NewLine = "\n";
                _SerialPort1.Open();
                String[] deserializedData = getDeserializedTelemetryData();
                /// The Returned Deserialized Data Is Now Written From The String Array
                /// And Sent Via The COM Port.
                for(int x=0; x<deserializedData.Length; x++)
                {
                    _SerialPort1.WriteLine(deserializedData[x]);
                }
                //var gameData = _data;
                //System.IO.Ports.SerialPort Port = new System.IO.Ports.SerialPort("COM4");
                //Port.BaudRate = 9600;
                //Port.Open();
                //Port.WriteTimeout = 8000;
                //Port.Write("Craig");
            }
            catch (Exception ex)
            {
                // handle exception
                if (ex is UnauthorizedAccessException)
                {
                    System.Windows.Forms.MessageBox.Show("Unauthorized Access", "Unauthorized attempt detected");
                    System.Diagnostics.Debug.WriteLine("COMS ERROR: Failed To Initialise Port(Unauthorized Access)");
                    System.Diagnostics.Debug.WriteLine("Unauthorized Access Source is " + ex.Source);
                    return;
                }

                if (ex is System.IO.IOException)
                {
                    System.Windows.Forms.MessageBox.Show("Please Check That A COMS Device Exists At That Port", "Cannot Establish COMS Link");
                    System.Diagnostics.Debug.WriteLine("COMS ERROR: Failed To Initialise Port(Cannot Establish COMS Link)");
                    return;
                }
            }

        }

        public void Dispose()
        {
            _sharedMemory?.Dispose();
        }
    }
}