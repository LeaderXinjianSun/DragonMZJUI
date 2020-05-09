using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using BingLibrary.hjb;

namespace SxjLibrary
{
    public class Scan
    {
        public event EventHandler StateChanged;
        public bool mState;
        private string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        bool Scaner = false;
        public bool State
        {
            get { return mState; }
            set
            {
                if (mState != value)
                {
                    mState = value;
                    if (StateChanged != null)
                        StateChanged(null, null);
                }
            }
        }
        public void ini(string Com)
        {
            try
            {
                Scaner = bool.Parse(Inifile.INIGetStringValue(iniParameterPath, "System", "Scaner", "False"));
            }
            catch 
            {

               
            }
            if (Scaner)
            {
                mSerialPort = new SerialPort(Com, 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                START_DECODE = new byte[] { 0x16, 0x54, 0x0D };//{0x03,0x53,0x80,0xFF,0x2A };
                STOP_DECODE = new byte[] { 0x16, 0x55, 0x0D };
                MODE_DECODE = new byte[] { 0x16, 0x4D, 0x0D, 0x30, 0x34, 0x30, 0x31, 0x44, 0x30, 0x35, 0x2E };
            }
            else
            {
                mSerialPort = new SerialPort(Com, 115200, System.IO.Ports.Parity.Even, 8, System.IO.Ports.StopBits.One);
            }
            
            mSerialPort.ReadTimeout = 1000;
            mSerialPort.WriteTimeout = 1000;
        }
        public void Connect()
        {
            try
            {
                mSerialPort.Open();
                State = true;
            }
            catch (Exception ex) { Trace.WriteLine(ex.Message, "扫码连接"); }
        }

        public bool DoBarcode = true;

        public SerialPort mSerialPort;
        //public static bool DoBarcode=true;
        public string BarCode;
        static byte[] START_DECODE = new byte[] { 0x4C, 0x4F, 0x4E, 0x0D, 0x0A };//{0x03,0x53,0x80,0xFF,0x2A };
        static byte[] STOP_DECODE = new byte[] { 0x4C, 0x4F, 0x46, 0x46, 0x0D, 0x0A };
        static byte[] MODE_DECODE = new byte[] { 0x16, 0x4D, 0x0D, 0x30, 0x34, 0x30, 0x31, 0x44, 0x30, 0x35, 0x2E };
        public delegate void ProcessDelegate(string barcode);
        public async void GetBarCode(ProcessDelegate CallBack)
        {
            BarCode = "Error";
            Func<System.Threading.Tasks.Task> taskFunc = () =>
            {
                return System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        if (DoBarcode)
                        {
                            if (!mSerialPort.IsOpen)
                                Connect();
                            mSerialPort.ReadExisting();
                            mSerialPort.Write(START_DECODE, 0, START_DECODE.Length);
                            BarCode = mSerialPort.ReadLine();
                            string[] ss = BarCode.Split(new string[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
                            BarCode = ss[0];
                        }
                        State = true;
                    }
                    catch (Exception ex)
                    {
                        State = false;
                        Trace.WriteLine(ex.Message, "GetBarCode");
                    }
                    try
                    {
                        if (DoBarcode)
                        {
                            if (!mSerialPort.IsOpen)
                                Connect();
                            mSerialPort.ReadExisting();
                            mSerialPort.Write(STOP_DECODE, 0, STOP_DECODE.Length);
                        }
                        State = true;
                    }
                    catch (Exception ex)
                    {
                        State = false;
                        Trace.WriteLine(ex.Message, "StopBarCode");
                    }
                });
            };
            await taskFunc();
            CallBack(BarCode);
        }
        public void SetMode()
        {
            try
            {
                if (DoBarcode)
                {
                    if (!mSerialPort.IsOpen)
                        Connect();
                    mSerialPort.ReadExisting();
                    mSerialPort.Write(MODE_DECODE, 0, MODE_DECODE.Length);
                }
                State = true;
            }
            catch (Exception ex)
            {
                State = false;
                Trace.WriteLine(ex.Message, "SetMode");
            }
        }
    }
    public class HoneywellSerial
    {
        public event EventHandler StateChanged;
        public bool mState;
        public bool State
        {
            get { return mState; }
            set
            {
                if (mState != value)
                {
                    mState = value;
                    if (StateChanged != null)
                        StateChanged(null, null);
                }
            }
        }
        public void ini(string port)
        {
            mSerialPort = new SerialPort(port, 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
            mSerialPort.ReadTimeout = 1000;
            mSerialPort.WriteTimeout = 1000;
            //mSerialPort.RtsEnable = true;
            //mSerialPort.DtrEnable = false;
        }
        public void Connect()
        {
            try
            {
                mSerialPort.Open();
                State = true;
            }
            catch (Exception ex) { Trace.WriteLine(ex.Message, "扫码连接"); }
        }
        public SerialPort mSerialPort;
        //public static bool DoBarcode=true;
        public string BarCode { get; set; }
        byte[] START_DECODE = new byte[] { 0x16, 0x54, 0x0D };
        byte[] STOP_DECODE = new byte[] { 0x16, 0x55, 0x0D };
        private System.Object SerialLock = new System.Object();
        public delegate void ProcessDelegate(string barcode);

        bool HasStart = false;
        public async void GetBarCode(ProcessDelegate CallBack)
        {
            if (!HasStart)
                HasStart = true;
            else
                return;

            BarCode = "Error";
            Func<System.Threading.Tasks.Task> taskFunc = () =>
            {
                return System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
         
                            if (!mSerialPort.IsOpen)
                                Connect();
                            lock (SerialLock)
                            {
                                mSerialPort.ReadExisting();
                                mSerialPort.Write(START_DECODE, 0, START_DECODE.Length);

                            BarCode = mSerialPort.ReadTo("\r");

                            int mCount = mSerialPort.BytesToRead;
                                int mtiemout = 0;
                                while (mCount == 0)
                                {
                                    mCount = mSerialPort.BytesToRead;
                                    System.Threading.Thread.Sleep(60);
                                mtiemout++;
                                if (mtiemout > 50)
                                {
                                    mSerialPort.ReadExisting();
                                    Debug.Print("GetBarCode:" + "没有回应！");
                                    break;
                                }
                            }
                            if (mCount != 0)
                            {
                                BarCode = mSerialPort.ReadExisting();
                                string[] ss = BarCode.Split(new string[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
                                BarCode = ss[0];
                            }
                        }
                        Console.WriteLine("BarCode:" + BarCode);

                        State = true;
                    }
                    catch (Exception ex)
                    {
                        State = false;
                        Trace.WriteLine(ex.Message, "GetBarCode");
                    }
                    try
                    {

                        if (!mSerialPort.IsOpen)
                            Connect();
                        mSerialPort.ReadExisting();
                        mSerialPort.Write(STOP_DECODE, 0, STOP_DECODE.Length);
                        //mSerialPort.WriteLine("LOFF");

                        State = true;
                    }
                    catch (Exception ex)
                    {
                        State = false;
                        Trace.WriteLine(ex.Message, "StopBarCode");
                    }
                });
            };
            await taskFunc();
            HasStart = false;
            if (CallBack != null)
                CallBack(BarCode);
        }

        public string GetBarcode()
        {
            if (!HasStart)
                HasStart = true;
            else
                return "Error";

            BarCode = "";

            try
            {
      
                    if (!mSerialPort.IsOpen)
                        Connect();
                    lock (SerialLock)
                    {
                        mSerialPort.ReadExisting();
                        mSerialPort.Write(START_DECODE, 0, START_DECODE.Length);

                        //BarCode = mSerialPort.ReadTo("\r");

                        int mCount = mSerialPort.BytesToRead;
                        int mtiemout = 0;
                        while (mCount == 0)
                        {
                            mCount = mSerialPort.BytesToRead;
                            System.Threading.Thread.Sleep(60);
                            mtiemout++;
                            if (mtiemout > 50)
                            {
                                mSerialPort.ReadExisting();
                                Debug.Print("GetBarCode:" + "没有回应！");
                                break;
                            }
                        }
                        if (mCount != 0)
                            BarCode = mSerialPort.ReadExisting();
                        else
                            BarCode = "TimeOut";
                    }
                    Console.WriteLine("BarCode:" + BarCode);

                State = true;
            }
            catch (Exception ex)
            {
                State = false;
                Trace.WriteLine(ex.Message, "GetBarCode");
            }
            try
            {

                if (!mSerialPort.IsOpen)
                    Connect();
                mSerialPort.ReadExisting();
                mSerialPort.Write(STOP_DECODE, 0, STOP_DECODE.Length);
                //mSerialPort.WriteLine("LOFF");

                State = true;
            }
            catch (Exception ex)
            {
                State = false;
                Trace.WriteLine(ex.Message, "StopBarCode");
            }

            HasStart = false;
            return BarCode;
        }
    }
}
