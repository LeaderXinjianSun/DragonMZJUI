using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DragonMZJUI.Model;
using System.Windows.Threading;
using BingLibrary.hjb;
using System.Data;
using System.IO;
using BingLibrary.hjb.file;

namespace DragonMZJUI.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static DispatcherTimer dispatcherTimer = new DispatcherTimer();
        string LastBanci;
        string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        public MainWindow()
        {
            InitializeComponent();
            LastBanci = Inifile.INIGetStringValue(iniParameterPath, "System", "Banci", "0");
            
            dispatcherTimer.Tick += new EventHandler(DispatcherTimerTickUpdateUi);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
        private void DispatcherTimerTickUpdateUi(Object sender, EventArgs e)
        {
            string banci = GlobalVar.GetBanci();
            if (banci != LastBanci)
            {
                LastBanci = banci;
                Inifile.INIWriteValue(iniParameterPath, "System", "Banci", LastBanci);
                //GlobalVar.AlarmRecordQueue.Clear();
                GlobalVar.AlarmRecord.Clear();
                GlobalVar.MESDataRecord.Clear();
                GlobalVar.AddMessage("换班数据清空" + LastBanci);
            }
            if (GlobalVar.AlarmRecordQueue.Count > 0)
            {
                foreach (AlarmTableItem item in GlobalVar.AlarmRecordQueue)
                {
                    GlobalVar.AlarmRecord.Add(item);
                }
                GlobalVar.AlarmRecordQueue.Clear();
            }
            if (GlobalVar.MESDataRecordQueue.Count > 0)
            {
                foreach (MESDataItem item in GlobalVar.MESDataRecordQueue)
                {
                    GlobalVar.MESDataRecord.Add(item);
                }
                GlobalVar.MESDataRecordQueue.Clear();
            }
        }
        private void ReadAlarmRecordfromCSV()
        {
            string filepath = "D:\\报警记录\\报警记录" + GlobalVar.GetBanci() + ".csv";
            DataTable dt = new DataTable();
            DataTable dt1;
            dt.Columns.Add("AlarmDate", typeof(string));
            dt.Columns.Add("MachineID", typeof(string));
            dt.Columns.Add("UserID", typeof(string));
            dt.Columns.Add("AlarmMessage", typeof(string));
            try
            {
                if (File.Exists(filepath))
                {
                    dt1 = Csvfile.GetFromCsv(filepath, 1, dt);
                    if (dt1.Rows.Count > 0)
                    {
                        foreach (DataRow item in dt1.Rows)
                        {
                            AlarmTableItem tr = new AlarmTableItem() { AlarmDate = item[0].ToString(), MachineID = item[1].ToString(), UserID = item[2].ToString(), AlarmMessage = item[3].ToString() };
                            lock (GlobalVar.obj)
                            {
                                //GlobalVar.AlarmRecord.Add(tr);
                                GlobalVar.AlarmRecordQueue.Enqueue(tr);
                            }
                        }
                        GlobalVar.AddMessage("读取报警记录完成");
                        
                    }
                }
                else
                {
                    GlobalVar.AddMessage("报警记录不存在");
                    
                }
            }
            catch (Exception ex)
            {
                GlobalVar.AddMessage(ex.Message);
                
            }
        }
        private void ReadBarcodeRecordfromCSV()
        {
            //"Date", "Barcode", "MachineID", "UserID", "ProductName", "MachineName", "FactoryArea", "FactorySeparation"
            string filepath = "D:\\生产记录\\条码" + GlobalVar.GetBanci() + ".csv";
            DataTable dt = new DataTable();
            DataTable dt1;
            dt.Columns.Add("Date", typeof(string));
            dt.Columns.Add("Barcode", typeof(string));
            dt.Columns.Add("MachineID", typeof(string));
            dt.Columns.Add("UserID", typeof(string));

            dt.Columns.Add("ProductName", typeof(string));
            dt.Columns.Add("MachineName", typeof(string));
            dt.Columns.Add("FactoryArea", typeof(string));
            dt.Columns.Add("FactorySeparation", typeof(string));
            dt.Columns.Add("ZhiJuClass", typeof(string));
            dt.Columns.Add("Barcodeproofing", typeof(string));
            dt.Columns.Add("scancodetype", typeof(string));
            dt.Columns.Add("CCD", typeof(string));
            dt.Columns.Add("NNNN", typeof(string));
            try
            {
                if (File.Exists(filepath))
                {
                    dt1 = Csvfile.GetFromCsv(filepath, 1, dt);
                    if (dt1.Rows.Count > 0)
                    {
                        foreach (DataRow item in dt1.Rows)
                        {
                            MESDataItem tr = new MESDataItem() { Date = item[0].ToString(),Index = item[1].ToString(), Barcode = item[2].ToString(), MachineID = item[3].ToString(), UserID = item[4].ToString(), ProductName = item[5].ToString(), MachineName = item[6].ToString(), FactoryArea = item[7].ToString(), FactorySeparation = item[8].ToString() };
                            lock (GlobalVar.obj1)
                            {
                                //GlobalVar.AlarmRecord.Add(tr);
                                GlobalVar.MESDataRecordQueue.Enqueue(tr);
                            }
                        }
                        GlobalVar.AddMessage("读取条码记录完成");

                    }
                }
                else
                {
                    GlobalVar.AddMessage("条码记录不存在");

                }
            }
            catch (Exception ex)
            {
                GlobalVar.AddMessage(ex.Message);

            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            GlobalVar.AddMessage("软件加载完成");
            try
            {
                SXJLibrary.Oracle oraDB = new SXJLibrary.Oracle("szdb10.eavarytech.com", "fksfcdb3", "ictdata", "ict*1");
                if (oraDB.isConnect())
                {
                    string oracletime = oraDB.OraclDateTime();
                    GlobalVar.AddMessage("同步数据库时间" + oracletime);
                }
                oraDB.disconnect();
            }
            catch (Exception ex)
            {

                GlobalVar.AddMessage(ex.Message);
            }
            GlobalVar.plc = new DeltaPLC();
            ReadAlarmRecordfromCSV();
            ReadBarcodeRecordfromCSV();
        }

        private void ControlGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CameraPageGrid.Visibility == Visibility.Collapsed)
            {
                CameraPageGrid.Visibility = Visibility.Visible;
            }
            else
            {
                CameraPageGrid.Visibility = Visibility.Collapsed;
            }
            
        }
    }
}
