using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPClient
{
    public interface ILogable
    {
        EventHandler<string> Log { get; set; }
    }
}
