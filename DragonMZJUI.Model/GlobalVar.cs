using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewROI;

namespace DragonMZJUI.Model
{
    public class GlobalVar
    {
        public static HWndCtrl hWndCtrl;
        public static ROIController rOIController;
        public static string MessageStr = "";
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
        public static DeltaPLC plc;

    }
}
