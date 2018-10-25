using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DXH.PLC;
using BingLibrary.hjb;

namespace DragonMZJUI.Model
{
    public class DeltaPLC
    {
        string COM, STATE;
        string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        Delta_ModbusASCII plc;
        public Vision vision;
        
        bool[] Plc1In = new bool[32];
        bool[] Plc1Out;
        public bool Connect = false;
        public DeltaPLC()
        {
            vision = new Vision();
            COM = Inifile.INIGetStringValue(iniParameterPath, "System", "COM", "COM1");
            STATE = Inifile.INIGetStringValue(iniParameterPath, "System", "STATE", "01");
            plc = new Delta_ModbusASCII(COM, 19200, System.IO.Ports.Parity.Even, 7, System.IO.Ports.StopBits.One);
            PlcRun();
        }
        async void PlcRun()
        {
            short QuestCycle = 100;
            bool state1 = false;
            while (true)
            {
                await Task.Delay(QuestCycle);
                Task task = Task.Run(() =>
                {
                    try
                    {
                        string M1000 = plc.PLCRead(STATE, "M1000");
  
                        int intM1000 = Convert.ToInt32(M1000, 16);
                        state1 = (intM1000 & 1) == 1;
                        if (state1)
                        {
                            //拍照1
                            System.Threading.Thread.Sleep(20);
                            string M2225 = plc.PLCRead(STATE, "M2225");
                            
                            int intM2225 = Convert.ToInt32(M2225, 16);
                            if ((intM2225 & 1) == 1)
                            {
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWrite(STATE, "M2225", "0000");
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWrite(STATE, "M2231", "0000");
                                GlobalVar.plc.vision.GetImage1();
                                plc.PLCWrite(STATE, "M2231", "FF00");
                            }
                            //拍照2
                            System.Threading.Thread.Sleep(20);
                            string M2226 = plc.PLCRead(STATE, "M2226");

                            int intM2226 = Convert.ToInt32(M2226, 16);
                            if ((intM2226 & 1) == 1)
                            {
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWrite(STATE, "M2226", "0000");
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWrite(STATE, "M2231", "0000");
                                GlobalVar.plc.vision.GetImage2();
                                plc.PLCWrite(STATE, "M2231", "FF00");
                            }
                            //System.Threading.Thread.Sleep(20);
                            //Plc1Out = GetCoilArray(Plc1OutStr, 10);
                            //string Plc1InStr = GetCoilStr(Plc1In);
                            //plc.PLCWriteBit(STATE, "M910", "000A", Plc1InStr);
                        }
                        QuestCycle = 100;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        state1 = false;
                        QuestCycle = 1000;
                    }
                });
                await task;
                Connect = state1;
            }
        }
        private bool[] GetCoilArray(string s, int length)
        {
            bool[] boolArray = new bool[length];
            for (int i = 0; i < s.Length; i++)
            {
                string temp = s.Substring(i, 2);
                int curtemp = Convert.ToInt32(temp, 16);
                for (int j = 0; j < 8; j++)
                {
                    if (((curtemp >> j) & 1) == 1)
                    {
                        if ((j + i * 4) < length)
                            boolArray[j + i * 4] = true;
                    }
                    else
                    {
                        if ((j + i * 4) < length)
                            boolArray[j + i * 4] = false;
                    }
                }
                i++;
            }
            return boolArray;
        }
        private string GetCoilStr(bool[] b)
        {
            string s1 = "";
            for (int i = 0; i < b.Length; i++)
            {
                s1 += b[b.Length - 1 - i] ? "1" : "0";
            }
            string NewData = "";
            for (int i = 0; i < 8 - s1.Length % 8 && s1.Length % 8 != 0; i++)
            {
                NewData += "0";
            }
            NewData += s1;
            string modbusStr = "";
            for (int i = 0; i < NewData.Length / 8; i++)
            {
                string Coils8 = NewData.Substring(NewData.Length - 8 * (i + 1), 8);
                int tempV1 = Convert.ToInt32(Coils8, 2);
                string coliValue = tempV1.ToString("X2");
                modbusStr += coliValue;
            }
            return modbusStr;
        }
    }
}
