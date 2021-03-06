﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Net;
using UDPClient;
using System.ComponentModel;
using IFT585_TP1.ViewModel;
using System.IO;

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

        ProgressViewModel progressViewModel;
        public ProgressViewModel ProgressViewModel
        {
            get
            {
                return progressViewModel;
            }
            set
            {
                SetProperty(ref progressViewModel, value);
            }
        }

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
        private void LogAction(string log)
        {
            Log = (String.Format("{0} : {1}\n", DateTime.Now.ToString("HH:mm:ss ffff"), log)) + Log;
        }

        private void SendFileImpl()
        {
            var sender = new UDPClientSender(ipAdress, port, filePath);
            ProgressViewModel = new ProgressViewModel(sender);
            LogAction("Vous envoyez un fichier");
            //sender.Resended += (o, e) => LogAction(String.Format("Le packet avec l'offset {0} a été renvoyé", e.OffSet));
           // sender.PacketReceived += (o, e) => LogAction(String.Format("Le ACK avec l'offset {0} a été reçu", e.OffSet));
            sender.Log += (o, message) => LogAction(message);
            sender.SendFile();
        }



        public SendViewModel()
        {
            SelectFile = new ActionCommand(SelectFileImpl);
            SendFile = new ActionCommand(() => Task.Factory.StartNew(SendFileImpl));
        }
    }
}
