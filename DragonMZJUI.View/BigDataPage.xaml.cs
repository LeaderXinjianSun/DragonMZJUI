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
using BingLibrary.hjb;

namespace DragonMZJUI.View
{
    /// <summary>
    /// BigDataPage.xaml 的交互逻辑
    /// </summary>
    public partial class BigDataPage : UserControl
    {
        string iniParameterPath = System.Environment.CurrentDirectory + "\\Parameter.ini";
        public BigDataPage()
        {
            InitializeComponent();
            AlarmDataGrid.ItemsSource = GlobalVar.AlarmRecord;
            MachineID_Text.Text = GlobalVar.MachineID = Inifile.INIGetStringValue(iniParameterPath, "System", "MachineID", "HS901");
            UserID_Text.Text = GlobalVar.UserID = Inifile.INIGetStringValue(iniParameterPath, "System", "UserID", "F0001");
        }

        private void IDSaveButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalVar.MachineID = MachineID_Text.Text;
            GlobalVar.UserID = UserID_Text.Text;
            Inifile.INIWriteValue(iniParameterPath, "System", "MachineID", MachineID_Text.Text);
            Inifile.INIWriteValue(iniParameterPath, "System", "UserID", UserID_Text.Text);
        }
    }
}
