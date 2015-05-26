﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UDPClient;

namespace IFT585_TP1.ViewModel
{
    class ReceiverViewModel : BindableBase
    {
        public ObservableCollection<ProgressViewModel> DownloadingFiles { get; set; }

        private class Udp : IUDP
        {
            public int FileSize
            {
                get
                {
                    return 100;
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public EventHandler<AckEventArgs> PacketReceived
            {
                get;
                set;
            }

            public EventHandler<AckEventArgs> Resended
            {
                get;
                set;
            }
        }

        public string OwnIPAdress
        {
            get
            {
                return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();

            }
        }

        public ICommand Listen { get; private set; }

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

        private Listener listener;

        public ReceiverViewModel()
        {
            Listen = new ActionCommand(() =>
            {
                Task.Factory.StartNew(() =>
                    {
                        listener = new Listener(port);
                        listener.StartListening();
                        listener.ObjectCreated += (o, e) => DownloadingFiles.Add(new ProgressViewModel(e.sObject));
                    });
            });
            DownloadingFiles = new ObservableCollection<ProgressViewModel>();
        }

    }
}
