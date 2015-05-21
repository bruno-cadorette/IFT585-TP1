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
    /// <summary>
    /// RFB Protocol
    /// 1 byte => 1 == ACK, 0 == DATA
    /// 4 byte => file number
    /// 4 byte => offset
    /// </summary>
    public class UDPClientSender
    {
        //Properties
        public int FileSize
        {
            get {return  m_file.Length; }
        }

        //Members
        private readonly byte[] ackBytes = {1};
        private readonly byte[] dataBytes = { 0 };
        private const int NB_BYTE_PER_SECTION = 2048;
        private const long TIMEOUT = 5000;
        private Socket m_socket;
        private Socket listeningSocket;
        private IPEndPoint m_endpoint;
        private IPEndPoint listeninEndPoint;
        private UdpClient udpClient;
        private byte[] m_file;
        private int fileID;
        private Dictionary<int, Timer> m_timers; 


        //Events
        public EventHandler ACKReceived;

        public UDPClientSender(IPAddress addr, int port,string path)
        {
            m_timers = new Dictionary<int, Timer>();
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,ProtocolType.Udp);
            m_endpoint = new IPEndPoint(addr,port);
            listeninEndPoint = new IPEndPoint(IPAddress.Any,port);
            udpClient = new UdpClient(port);
            m_file = File.ReadAllBytes(path);        
            fileID = 0;
        }

        /// <summary>
        ///     Send the specified file to host
        /// </summary>
        /// <param name="path">path to send</param>
        /// <returns>succesful</returns>
        public bool SendFile(string path)
        {
            //Get byte from file
            StartTransfer(path);                                    //Start Connection, ask for FileID
            int nbSection = m_file.Length / NB_BYTE_PER_SECTION;    
            var task = Task.Factory.StartNew(Listen);                          //Listen for ACK
            for (int i = 0; i < nbSection; i++)
            {
                try
                {
                    SendSectionAsync(i*NB_BYTE_PER_SECTION);
                }
                catch
                {
                    return false;
                }
            }
                
            return true;
        }

        private void StartTransfer(string path)
        {
            string fileName = Path.GetFileName(path);
            byte[] data = dataBytes.Concat(BitConverter.GetBytes(fileID)).ToArray();        //ConCat DATA + fileID (0)
            data = data.Concat(Encoding.ASCII.GetBytes(fileName)).ToArray();                //Concat FileName
            m_socket.SendTo(data, m_endpoint);                                              //Send

            byte[] receivedBytes = udpClient.Receive(ref listeninEndPoint); // Wait for Server to give a File ID
            fileID = BitConverter.ToInt32(receivedBytes, 1);                //FileID for this file is known

        }


        /// <summary>
        /// Send a section over the network
        /// </summary>
        /// <param name="file">Byte of the file</param>
        /// <param name="offSet"></param>
        private async void SendSectionAsync(int offSet)
        {
            var section = m_file.Skip(offSet).Take(NB_BYTE_PER_SECTION).ToList(); // Data
            var byteOffset = BitConverter.GetBytes(offSet);
            section.InsertRange(0, byteOffset) ;                                // OFFSET
            section = BitConverter.GetBytes(fileID).Concat(section).ToList();   // File ID
            section = dataBytes.Concat(section).ToList();                       // ACK or DATA
            m_socket.SendTo(section.ToArray(),m_endpoint);
            Timer timer = new Timer(Resend,offSet,TIMEOUT,Timeout.Infinite);
            m_timers[offSet] = timer;
        }

        private void Resend(object state)
        {
            SendSectionAsync((int)state);
        }

        private void Listen()
        {
            while (true)
            {
                byte[] data = udpClient.Receive(ref listeninEndPoint); //Sync Call
                if (data[0] == 1)
                {
                    int off = BitConverter.ToInt32(data, 1);
                    if (m_timers.ContainsKey(off))
                    {
                        if(ACKReceived != null)
                            ACKReceived.Invoke(this,new EventArgs());
                        m_timers[off].Dispose();
                        m_timers.Remove(off);
                    }
                }

            }

        }
    }
}
