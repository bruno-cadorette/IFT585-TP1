using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UDPClient
{
    public class UDPClient
    {
        private const int NB_BYTE_PER_SECTION = 2048;
        private const long TIMEOUT = 5000;
        private Socket m_socket;
        private IPEndPoint m_endpoint;
        private byte[] m_file;
        private Dictionary<int, Timer> m_timers; 

        public UDPClient(IPAddress addr, int port)
        {
            m_timers = new Dictionary<int, Timer>();
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,ProtocolType.Udp);
            m_endpoint = new IPEndPoint(addr,port);
        }

        /// <summary>
        ///     Send the specified file to host
        /// </summary>
        /// <param name="path">path to send</param>
        /// <returns>succesful</returns>
        public bool SendFile(string path)
        {
            if (File.Exists(path))
            {
                m_file= File.ReadAllBytes(path);
                int nbSection = m_file.Length / NB_BYTE_PER_SECTION;
                for (int i = 0; i < nbSection; i++)
                {
                    try
                    {
                        SendSectionAsync(nbSection*NB_BYTE_PER_SECTION);
                    }
                    catch
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }


        /// <summary>
        /// Send a section over the network
        /// </summary>
        /// <param name="file">Byte of the file</param>
        /// <param name="offSet"></param>
        private async void SendSectionAsync(int offSet)
        {
            var section = m_file.Skip(offSet).Take(NB_BYTE_PER_SECTION).ToList();
            var byteOffset = BitConverter.GetBytes(offSet);
            section.InsertRange(0, byteOffset) ;
            m_socket.SendTo(section.ToArray(),m_endpoint);
            Timer timer = new Timer(Resend,offSet,TIMEOUT,Timeout.Infinite);
            m_timers[offSet] = timer;
        }

        private void Resend(object state)
        {
            SendSectionAsync((int)state);
        }
    }
}
