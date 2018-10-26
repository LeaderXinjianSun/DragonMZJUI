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
using System.Windows.Forms;

namespace DragonMZJUI.View
{
    /// <summary>
    /// OperatePage.xaml 的交互逻辑
    /// </summary>
    public partial class OperatePage : System.Windows.Controls.UserControl
    {
        bool first = true;
        public DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public OperatePage()
        {
            InitializeComponent();
            dispatcherTimer.Tick += new EventHandler(DispatcherTimerTickUpdateUi);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();   
        }
        private void MsgTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MsgTextBox.ScrollToEnd();
        }
        private void DispatcherTimerTickUpdateUi(Object sender, EventArgs e)
        {
            PlcConnect.Fill =  GlobalVar.plc.Connect ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
            MsgTextBox.Text = GlobalVar.MessageStr;
            OperatePageGrid.IsEnabled = GlobalVar.plc.vision.CCDStatus;
            if (first && GlobalVar.plc.vision.CCDStatus)
            {
                first = false;
                GlobalVar.plc.vision.window1 = GlobalVar.hWndCtrl.viewPort.HalconWindow;
            }
        }

        private void GrapButtonClick(object sender, RoutedEventArgs e)
        {
            GlobalVar.plc.vision.GetImage1();
        }
        private void GrapButtonClick2(object sender, RoutedEventArgs e)
        {
            GlobalVar.plc.vision.GetImage2();
        }
        
        private void ReOpenCameraButtonClick(object sender, RoutedEventArgs e)
        {
            GlobalVar.plc.vision.CloseCamera();
            GlobalVar.plc.vision.OpenCameraAsync();
        }
        private void ProcessButtonClick(object sender, RoutedEventArgs e)
        {
            GlobalVar.plc.vision.ProcessImage();
        }
        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog svf = new SaveFileDialog();
            svf.Title = "保存图片";
            svf.InitialDirectory = @"E:\images";
            svf.Filter = "图片文件(*.bmp)|*.bmp|所有文件(*.*)|*.*";
            svf.FileName = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            if (svf.ShowDialog() == DialogResult.OK)
            {
                GlobalVar.plc.vision.SaveImage(svf.FileName);
            }
        }
        private void OpenButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Title = "打开图片";
            opf.Filter = "图片文件(*.bmp)|*.bmp|所有文件(*.*)|*.*";
            opf.InitialDirectory = @"E:\images";
            if (opf.ShowDialog() == DialogResult.OK)
            {
                GlobalVar.plc.vision.OpenImage(opf.FileName);
            }
        }
    }
}
