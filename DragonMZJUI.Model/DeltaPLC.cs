using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DXH.PLC;
using BingLibrary.hjb;
using System.IO;
using OfficeOpenXml;
using BingLibrary.hjb.file;

namespace DragonMZJUI.Model
{
    public class DeltaPLC
    {
        string COM, STATE;
        string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        Delta_ModbusASCII plc;
        public Vision vision;
        
        public bool Connect = false;
        public DeltaPLC()
        {
            
            COM = Inifile.INIGetStringValue(iniParameterPath, "System", "COM", "COM1");
            STATE = Inifile.INIGetStringValue(iniParameterPath, "System", "STATE", "01");
            plc = new Delta_ModbusASCII(COM, 19200, System.IO.Ports.Parity.Even, 7, System.IO.Ports.StopBits.One);
            PlcRun();
            vision = new Vision();
            

        }
        async void PlcRun()
        {
            short QuestCycle = 100;
            bool state1 = false;
            bool first = true;

            AlarmTuple[] AlarmTupleArray = new AlarmTuple[200];
            int alramItemsCount = 0;
            string alarmconfigfile = System.Environment.CurrentDirectory + "\\CA9报警.xlsx";

            try
            {
                alramItemsCount = UpdateAlarmFromExcel(alarmconfigfile, AlarmTupleArray);
                GlobalVar.AddMessage("加载报警项：" + alramItemsCount.ToString());
            }
            catch (Exception ex)
            {
                GlobalVar.AddMessage(ex.Message);
            }


            while (true)
            {
                await Task.Delay(QuestCycle);
                Task task = Task.Run(() =>
                {
                    try
                    {
                        state1 = plc.ReadM(STATE, "M1000");                         
                        if (state1)
                        {
                            //拍照1
                            System.Threading.Thread.Sleep(20);                            
                            if (plc.ReadM(STATE, "M120"))
                            {
                                GlobalVar.AddMessage("触发拍照1");
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWrite(STATE, "M120", "0000");
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWrite(STATE, "M980", "0000");
                                GlobalVar.plc.vision.GetImage1();
                                plc.PLCWrite(STATE, "M980", "FF00");
                            }
                            //拍照2
                            System.Threading.Thread.Sleep(20);
                            if (plc.ReadM(STATE, "M982"))
                            {
                                GlobalVar.AddMessage("触发拍照2");
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWrite(STATE, "M982", "0000");
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWrite(STATE, "M129", "0000");
                                GlobalVar.plc.vision.GetImage2();
                                GlobalVar.plc.vision.ProcessImage();
                                string Str_Result_etch = GetCoilStr(GlobalVar.plc.vision.Result_etch);
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWriteBit(STATE, "M2213", "000A", Str_Result_etch);
                                string Str_Result_blue = GetCoilStr(GlobalVar.plc.vision.Result_blue);
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWriteBit(STATE, "M2201", "000A", Str_Result_blue);
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWrite(STATE, "M129", "FF00");
                                GlobalVar.AddMessage("拍照结果写入PLC");
                            }
                            //报警
                            for (int i = 0; i < alramItemsCount; i++)
                            {
                                AlarmTupleArray[i].CoilStatus = plc.ReadM(STATE, AlarmTupleArray[i].CoilName);
                                if (AlarmTupleArray[i].LastCoilStatus != AlarmTupleArray[i].CoilStatus)
                                {
                                    AlarmTupleArray[i].LastCoilStatus = AlarmTupleArray[i].CoilStatus;
                                    if (AlarmTupleArray[i].CoilStatus && !first)
                                    {
                                        AlarmTableItem _alarmTableItem = new AlarmTableItem();
                                        _alarmTableItem.AlarmDate = DateTime.Now.ToString();
                                        _alarmTableItem.AlarmMessage = AlarmTupleArray[i].AlarmContent;
                                        _alarmTableItem.MachineID = GlobalVar.MachineID;
                                        _alarmTableItem.UserID = GlobalVar.UserID;

                                        lock (GlobalVar.obj)
                                            GlobalVar.AlarmRecordQueue.Enqueue(_alarmTableItem);
                                      
                                        
                                        SaveCSVfileAlarm(_alarmTableItem.AlarmMessage);

                                        //记录报警
                                    }
                                }

                            }
                            first = false;
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
        private void SaveCSVfileAlarm(string alrstr)
        {
            string filepath = "D:\\报警记录\\报警记录" + GlobalVar.GetBanci() + ".csv";
            if (!Directory.Exists("D:\\报警记录"))
            {
                Directory.CreateDirectory("D:\\报警记录");
            }
            try
            {
                if (!File.Exists(filepath))
                {
                    string[] heads = { "AlarmDate", "MachineID", "UserID", "AlarmMessage" };
                    Csvfile.AddNewLine(filepath, heads);
                }
                string[] conte = { System.DateTime.Now.ToString(), GlobalVar.MachineID, GlobalVar.UserID, alrstr };
                Csvfile.AddNewLine(filepath, conte);
            }
            catch (Exception ex)
            {
                GlobalVar.AddMessage(ex.Message);
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
        private int UpdateAlarmFromExcel(string filename, AlarmTuple[] alarmTupleArray)
        {
            int itemsCount = 0;
            FileInfo existingFile = new FileInfo(filename);
            using (ExcelPackage package = new ExcelPackage(existingFile))
            {
                // get the first worksheet in the workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                int rowCount = worksheet.Dimension.End.Row;
                int colCount = worksheet.Dimension.End.Column;
                for (int i = 1; i <= rowCount; i++)
                {
                    if (worksheet.Cells[i, 1] != null && worksheet.Cells[i, 1].Value != null && worksheet.Cells[i, 2] != null && worksheet.Cells[i, 2].Value != null)
                    {
                        alarmTupleArray[itemsCount].CoilName = worksheet.Cells[i, 1].Value.ToString();
                        alarmTupleArray[itemsCount].AlarmContent = worksheet.Cells[i, 2].Value.ToString();
                        alarmTupleArray[itemsCount].CoilStatus = false;
                        alarmTupleArray[itemsCount].LastCoilStatus = false;
                        itemsCount++;
                    }

                }

            }

            return itemsCount;
        }
        
    }
    public struct AlarmTuple
    {
        public string CoilName;
        public bool CoilStatus;
        public bool LastCoilStatus;
        public string AlarmContent;
    }
}
