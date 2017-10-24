using FirmwareUpateTools.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace FirmwareUpateTools.ViewModel
{
    public class MainWindowViewModel:ViewModelBase
    {
        private STDFU sTDFU = new STDFU(); 

        public List<DFUDevice> DeviceList { get => sTDFU.Device; set => sTDFU.Device = value; }
        public int CurrentIndex { get => sTDFU.m_CurrentDevice; set => sTDFU.m_CurrentDevice = value; }
        public string PrintText { get => sTDFU.PrintText; set => sTDFU.PrintText = value; }
        public int Percent { get => sTDFU.ContextPercent; set => sTDFU.ContextPercent = value; }
        public string DownloadFilePath { get => sTDFU.m_DownFileName; set => sTDFU.m_DownFileName = value; }
        public string DownloadButtonEnable
        {
            get
            {
                if (DownloadFilePath != "")
                    return "True";
                else
                    return "False";
            }
        }


        private MainWindow window;
        public MainWindowViewModel()
        {
            sTDFU.Refresh();
            OnPropertyChanged("DeviceList");
            OnPropertyChanged("CurrentIndex");
        }
        public void RegisterWindow(MainWindow window)
        {
            this.window = window;
            WindowInteropHelper helper = new WindowInteropHelper(window);
            sTDFU.RegisterDeviceNotification(helper.Handle);
            sTDFU.DeviceNotificationFilter += STDFU_DeviceNotificationFilter;
            sTDFU.ContextPercentChanged += STDFU_ContextPercentChanged;
            sTDFU.PrintTextChanged += STDFU_PrintTextChanged;
        }

        private void STDFU_PrintTextChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("PrintText");
        }

        private void STDFU_ContextPercentChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("Percent");
        }

        private void STDFU_DeviceNotificationFilter(object sender, STDFU.DeviceNotificationEventArgs e)
        {
            if(e.IsArrived)
            {
                sTDFU.Refresh();
                OnPropertyChanged("DeviceList");
                OnPropertyChanged("CurrentIndex");
            }
            else
            {
                sTDFU.Refresh();
                OnPropertyChanged("DeviceList");
                OnPropertyChanged("CurrentIndex");
            }
        }




        private RelayCommand chooseDownloadFileClickCommand;

        private void _chooseDownloadFileClickCommand()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "固件文件(*.dfu)|*.dfu";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                DownloadFilePath = openFileDialog.FileName;
                OnPropertyChanged("DownloadFilePath");
                OnPropertyChanged("DownloadButtonEnable");
            }
        }

        public ICommand ChooseDownloadFileClickCommand
        {
            get
            {
                if (chooseDownloadFileClickCommand == null)
                {
                    chooseDownloadFileClickCommand =
                        new RelayCommand(x => this._chooseDownloadFileClickCommand());
                }
                return chooseDownloadFileClickCommand;
            }
        }

        private RelayCommand downloadFileClickCommand;

        private void _downloadFileClickCommand()
        {
            if(DownloadFilePath == "")
            {
                MessageBox.Show("请先选择固件文件");
            }
            else
            {
                sTDFU.UpgradeDFUFile(DownloadFilePath);
            }
        }

        public ICommand DownloadFileClickCommand
        {
            get
            {
                if (downloadFileClickCommand == null)
                {
                    downloadFileClickCommand =
                        new RelayCommand(x => _downloadFileClickCommand());
                }
                return downloadFileClickCommand;
            }
        }

        private RelayCommand uploadFileClickCommand;

        private void _uploadFileClickCommand()
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "固件文件(*.dfu)|*.dfu";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == true)
            {
                sTDFU.UploadDFUFile(saveFileDialog.FileName);
            }
        }

        public ICommand UploadFileClickCommand
        {
            get
            {
                if (uploadFileClickCommand == null)
                {
                    uploadFileClickCommand =
                        new RelayCommand(x => _uploadFileClickCommand());
                }
                return uploadFileClickCommand;
            }
        }


    }
}
