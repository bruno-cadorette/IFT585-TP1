using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPClient;

namespace IFT585_TP1
{
    class ProgressViewModel : BindableBase
    {
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

        private UDPClientSender sender;

        public ProgressViewModel(UDPClientSender sender)
        {
            this.sender = sender;
            this.sender.ACKReceived += (o, e) => ProgressBarPercent++;
        }
    }
}
