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
using SxjLibrary;

namespace DragonMZJUI.Model
{
    public class DeltaPLC
    {
        string COM, STATE;
        string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        Delta_ModbusASCII plc;
        public Vision vision;
        public Scan ScanC;
        public bool Connect = false;
        int ReUpdateHour;
        public DeltaPLC()
        {
            
            COM = Inifile.INIGetStringValue(iniParameterPath, "System", "COM", "COM1");
            STATE = Inifile.INIGetStringValue(iniParameterPath, "System", "STATE", "01");
            plc = new Delta_ModbusASCII(COM, 9600, System.IO.Ports.Parity.Even, 7, System.IO.Ports.StopBits.One);
            ScanC = new Scan();
            string ScanCom = Inifile.INIGetStringValue(iniParameterPath, "System", "扫码枪串口", "COM1");
            ReUpdateHour = int.Parse(Inifile.INIGetStringValue(iniParameterPath, "System", "ReUpdateHour", "0"));
            //ScanC.ini(ScanCom);
            //ScanC.Connect();
            PlcRun();
            vision = new Vision();

            //plc.PLCWriteBit(STATE, "M3111", "0006", "3E");
           // GlobalVar.AddMessage("蚀刻信息已写入PLC");
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
                if (DateTime.Now.Hour % 2 == 0)
                {
                    if (DateTime.Now.Hour != ReUpdateHour)
                    {
                        ReUpdateHour = DateTime.Now.Hour;
                        Inifile.INIWriteValue(iniParameterPath, "System", "ReUpdateHour", ReUpdateHour.ToString());
                        GlobalVar.plc.vision.ReUploadAction();
                    }
                }
                Task task = Task.Run(() =>
                {
                    try
                    {
                        state1 = plc.ReadM(STATE, "M1000");                         
                        if (state1)
                        {
                            //拍照1
                            System.Threading.Thread.Sleep(20);                            
                            if (plc.ReadM(STATE, "M3000"))
                            {
                                plc.PLCWrite(STATE, "M3100", "0000");
                                GlobalVar.AddMessage("触发拍照1");
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWrite(STATE, "M3000", "0000");
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWrite(STATE, "M3002", "0000");
                                GlobalVar.plc.vision.GetImage1();
                                plc.PLCWrite(STATE, "M3002", "FF00");
                            }
                            //拍照2
                            System.Threading.Thread.Sleep(20);
                            if (plc.ReadM(STATE, "M3001"))
                            {
                                GlobalVar.AddMessage("触发拍照2");
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWrite(STATE, "M3001", "0000");
                                System.Threading.Thread.Sleep(20);
                                
                                plc.PLCWrite(STATE, "M3003", "0000");
                                GlobalVar.plc.vision.GetImage2();
                                plc.PLCWrite(STATE, "M3003", "FF00");
                                GlobalVar.plc.vision.ProcessImage();
                                string Str_Result_etch = GetCoilStr(GlobalVar.plc.vision.Result_etch);
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWriteBit(STATE, "M3101", "0006", Str_Result_etch);
                                GlobalVar.AddMessage("蚀刻信息已写入PLC");
                                //string Str_Result_blue = GetCoilStr(GlobalVar.plc.vision.Result_blue);
                                //System.Threading.Thread.Sleep(20);
                                //plc.PLCWriteBit(STATE, "M121", "000A", Str_Result_blue);


                                string Str_Result_barcode = GetCoilStr(GlobalVar.plc.vision.Result_barcode);
                                System.Threading.Thread.Sleep(20);
                                plc.PLCWriteBit(STATE, "M3111", "0006", Str_Result_barcode);
                                GlobalVar.AddMessage("扫码信息已写入PLC");





                                plc.PLCWrite(STATE, "M3100", "FF00");
                                GlobalVar.AddMessage("本次处理完成");
                            }
                            //if (plc.ReadM(STATE, "M260"))
                            //{
                            //    GlobalVar.AddMessage("触发扫码");
                            //    System.Threading.Thread.Sleep(20);
                            //    plc.PLCWrite(STATE, "M260", "0000");
                            //    System.Threading.Thread.Sleep(20);
                            //    plc.PLCWrite(STATE, "M262", "0000");
                            //    System.Threading.Thread.Sleep(20);
                            //    plc.PLCWrite(STATE, "M264", "0000");
                            //    ScanC.GetBarCode(PLCScanBCallback);
                            //}
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
        //void PLCScanBCallback(string bar)
        //{
        //    GlobalVar.AddMessage(bar);
        //    if (bar != "Error")
        //    {
        //        plc.PLCWrite(STATE, "M262", "FF00");
                
        //    }
        //    else
        //    {
        //        plc.PLCWrite(STATE, "M264", "FF00");
        //    }
        //    SaveCSVfileBarcode(bar);
        //}
        //private void SaveCSVfileBarcode(string bar)
        //{
        //    string filepath = "D:\\生产记录\\条码" + GlobalVar.GetBanci() + ".csv";
        //    if (!Directory.Exists("D:\\生产记录"))
        //    {
        //        Directory.CreateDirectory("D:\\生产记录");
        //    }
        //    try
        //    {
        //        if (!File.Exists(filepath))
        //        {
        //            string[] heads = { "Date", "Barcode"};
        //            Csvfile.AddNewLine(filepath, heads);
        //        }
        //        string[] conte = { System.DateTime.Now.ToString(), bar };
        //        Csvfile.AddNewLine(filepath, conte);
        //    }
        //    catch (Exception ex)
        //    {
        //        GlobalVar.AddMessage(ex.Message);
        //    }
        //}

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
