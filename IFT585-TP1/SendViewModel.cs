using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Net;
using UDPClient;
using System.ComponentModel;

namespace IFT585_TP1
{
    class SendViewModel : BindableBase
    {
        public ICommand SelectFile { get; private set; }

        public ICommand SendFile { get; private set; }

        private int port;
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                SetProperty(ref port, value);
            }
        }

        private int percent = 0;
        public int ProgressBarPercent
        {
            get
            {
                return percent;
            }
            set
            {
                SetProperty(ref percent, value);
            }
        }

        public int FileSize 
        { 
            get
            {
                return sender.FileSize;
            }
        }
        



        IPAddress ipAdress;
        public string IPAdress {
            get
            {
                return ipAdress.ToString();
            }
            set
            {
                SetProperty(ref ipAdress, IPAddress.Parse(value));
            }
        }

        private string filePath;
        private UDPClientSender sender;

        private readonly BackgroundWorker worker;

        private void SelectFileImpl()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            var result = dlg.ShowDialog();
            
            if(result.HasValue && result.Value)
            {
                filePath = dlg.FileName;
            }
        }

        private void SendFileImpl()
        {
            sender = new UDPClientSender(ipAdress, port, filePath);
            sender.ACKReceived += (o, e) => ProgressBarPercent++;
            sender.SendFile();
        }

        public SendViewModel()
        {
            SelectFile = new ActionCommand(SelectFileImpl);
            SendFile = new ActionCommand(() => Task.Factory.StartNew(SendFileImpl));
        }
    }
}
