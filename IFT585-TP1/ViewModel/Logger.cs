using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPClient;

namespace IFT585_TP1.ViewModel
{
    class Logger
    {
        int messageID = 0;
        public Logger(ILogable logableEntity)
        {
            logableEntity.Log += (o, e) => Log(o, LogFormat(e));
        }
        private string LogFormat(string log)
        {
            return (String.Format("{0} - {1} : {2}\n", ++messageID, DateTime.Now.ToString("HH:mm:ss.ffff"), log));
        }

        public EventHandler<string> Log { get; set; }
    }
}
