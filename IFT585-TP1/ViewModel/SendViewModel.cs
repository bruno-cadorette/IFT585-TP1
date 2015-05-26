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

        private int port = 50020;
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

        public ProgressViewModel ProgressViewModel { get; set; }

        private string log;
        public string Log
        {
            get
            {
                return log;
            }
            set
            {
                SetProperty(ref log, value);
            }
        }


        IPAddress ipAdress = IPAddress.Parse("25.44.91.16");
        public string IPAdress
        {
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


        private void SelectFileImpl()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            var result = dlg.ShowDialog();
            
            if (result.HasValue && result.Value)
            {
                filePath = dlg.FileName;
            }
        }

        private void SendFileImpl()
        {
            var sender = new UDPClientSender(ipAdress, port, filePath);
            ProgressViewModel = new ProgressViewModel(sender);
            sender.Resended += (o, e) => Log += String.Format("{0} : Le packet no {1} a été renvoyé\n", DateTime.Now, e.OffSet);
            sender.SendFile();
        }



        public SendViewModel()
        {
            SelectFile = new ActionCommand(SelectFileImpl);
            SendFile = new ActionCommand(() => Task.Factory.StartNew(SendFileImpl));
        }
    }
}
