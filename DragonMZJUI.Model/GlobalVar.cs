using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewROI;
using System.Collections.ObjectModel;

namespace DragonMZJUI.Model
{
    public class AlarmTableItem
    {
        public string AlarmDate { set; get; }
        public string MachineID { set; get; }
        public string UserID { set; get; }
        public string AlarmMessage { set; get; }
    }
    public class GlobalVar
    {
        public static object obj = new object();
        public static HWndCtrl hWndCtrl;
        public static ROIController rOIController;
        public static string MessageStr = "";
        public static ObservableCollection<AlarmTableItem> AlarmRecord = new ObservableCollection<AlarmTableItem>();
        public static Queue<AlarmTableItem> AlarmRecordQueue = new Queue<AlarmTableItem>();
        public static string MachineID;
        public static string UserID;
        public static void AddMessage(string str)
        {
            string[] s = MessageStr.Split('\n');
            if (s.Length > 1000)
            {
                MessageStr = "";
            }
            if (MessageStr != "")
            {
                MessageStr += "\n";
            }
            MessageStr += System.DateTime.Now.ToString("HH:mm:ss") + " " + str;
        }
        public static string GetBanci()
        {
            string rs = "";
            if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20)
            {
                rs += DateTime.Now.ToString("yyyyMMdd") + "Day";
            }
            else
            {
                if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour < 8)
                {
                    rs += DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + "Night";
                }
                else
                {
                    rs += DateTime.Now.ToString("yyyyMMdd") + "Night";
                }
            }
            return rs;
        }
        public static DeltaPLC plc;

    }
}
