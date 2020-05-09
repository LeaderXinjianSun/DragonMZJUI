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
        string iniParameterPath = System.Environment.CurrentDirectory + "//Parameter.ini";
        public BigDataPage()
        {
            InitializeComponent();
            AlarmDataGrid.ItemsSource = GlobalVar.AlarmRecord;
            MESDataRecord.ItemsSource = GlobalVar.MESDataRecord;
            MachineID_Text.Text = GlobalVar.MachineID = Inifile.INIGetStringValue(iniParameterPath, "System", "MachineID", "KF901");
            UserID_Text.Text = GlobalVar.UserID = Inifile.INIGetStringValue(iniParameterPath, "System", "UserID", "F0001");

            ProductName_Text.Text = GlobalVar.ProductName = Inifile.INIGetStringValue(iniParameterPath, "System", "ProductName", "KF9");
            MachineName_Text.Text = GlobalVar.MachineName = Inifile.INIGetStringValue(iniParameterPath, "System", "MachineName", "KF9-01");
            FactoryArea_Text.Text = GlobalVar.FactoryArea = Inifile.INIGetStringValue(iniParameterPath, "System", "FactoryArea", "SZ");
            FactorySeparation_Text.Text = GlobalVar.FactorySeparation = Inifile.INIGetStringValue(iniParameterPath, "System", "FactorySeparation", "A2-4F");
            ZhijuClass_Text.Text = GlobalVar.ZhijuClass = Inifile.INIGetStringValue(iniParameterPath, "System", "ZhijuClass", "ZX");
            MAC_Text.Text = GlobalVar.MAC = Inifile.INIGetStringValue(iniParameterPath, "System", "MAC", "C4-8E-8F-74-5D-89");
            Barcodeproofing_Text.Text = GlobalVar.Barcodeproofing = Inifile.INIGetStringValue(iniParameterPath, "System", "Barcodeproofing", "A2-4F");
            scancodetype_Text.Text = GlobalVar.scancodetype = Inifile.INIGetStringValue(iniParameterPath, "System", "scancodetype", "ZX,C46");
            CCD_Text.Text = GlobalVar.CCD = Inifile.INIGetStringValue(iniParameterPath, "System", "CCD", "CCD");
            NNNN_Text.Text = GlobalVar.NNNN = Inifile.INIGetStringValue(iniParameterPath, "System", "NNNN", "NNNN");
        }

        private void IDSaveButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalVar.MachineID = MachineID_Text.Text;
            GlobalVar.UserID = UserID_Text.Text;
            GlobalVar.ProductName = ProductName_Text.Text;
            GlobalVar.MachineName = MachineName_Text.Text;
            GlobalVar.FactoryArea = FactoryArea_Text.Text;
            GlobalVar.FactorySeparation = FactorySeparation_Text.Text;
            GlobalVar.ZhijuClass = ZhijuClass_Text.Text;
            GlobalVar.Barcodeproofing = Barcodeproofing_Text.Text;
            GlobalVar.scancodetype = scancodetype_Text.Text;
            GlobalVar.MAC = MAC_Text.Text;
            GlobalVar.CCD = CCD_Text.Text;
            GlobalVar.NNNN = NNNN_Text.Text;

            Inifile.INIWriteValue(iniParameterPath, "System", "MachineID", MachineID_Text.Text);
            Inifile.INIWriteValue(iniParameterPath, "System", "UserID", UserID_Text.Text);

            Inifile.INIWriteValue(iniParameterPath, "System", "ProductName", ProductName_Text.Text);
            Inifile.INIWriteValue(iniParameterPath, "System", "MachineName", MachineName_Text.Text);
            Inifile.INIWriteValue(iniParameterPath, "System", "FactoryArea", FactoryArea_Text.Text);
            Inifile.INIWriteValue(iniParameterPath, "System", "FactorySeparation", FactorySeparation_Text.Text);
            Inifile.INIWriteValue(iniParameterPath, "System", "ZhijuClass", ZhijuClass_Text.Text);
            Inifile.INIWriteValue(iniParameterPath, "System", "Barcodeproofing", Barcodeproofing_Text.Text);
            Inifile.INIWriteValue(iniParameterPath, "System", "scancodetype", scancodetype_Text.Text);
            Inifile.INIWriteValue(iniParameterPath, "System", "MAC", MAC_Text.Text);
            Inifile.INIWriteValue(iniParameterPath, "System", "CCD", CCD_Text.Text);
            Inifile.INIWriteValue(iniParameterPath, "System", "NNNN", NNNN_Text.Text);
        }

        private void MESDataRecord_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
