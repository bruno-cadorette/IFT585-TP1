using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace UDPClient
{
    interface IUDP
    {
        int FileSize { get; set; }

        EventHandler<AckEventArgs> PacketReceived { get; set; }
        EventHandler<AckEventArgs> Resended{ get; set; }
    }
}
