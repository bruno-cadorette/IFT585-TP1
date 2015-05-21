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
    public class StateObject : IUDP
    {   
        private byte[] content;
        private int fileSize;

        public EventHandler<AckEventArgs> PacketReceived { get; set; }
        public EventHandler<AckEventArgs> Resended { get; set; }

        public void UpdateContent(byte[] bytes, int offset, int size)
        {
            for (int i = 0; i < size; i++)
                content[i + offset] = bytes[i];

            if (PacketReceived != null)
                PacketReceived.Invoke(this, new AckEventArgs(offset));
        }

        public byte[] Content
        {
            get { return content; }
        }

        public int FileSize
        {
            get { return fileSize; }
            set 
            {
                fileSize = value;
                content = new byte[fileSize];
            }
        }
}

    public class Listener
    {
        private int id = 0;
        IPEndPoint localEndPoint;
        private const int NB_BYTE_PER_SECTION = 2064;
        private const int HEADER_SIZE = 9;
        private Dictionary<int, StateObject> states;
        Socket listener;

        public Listener(int port)
        {
            localEndPoint = new IPEndPoint(IPAddress.Any, port);
        }

        public void StartListening()
        {
            byte[] bytes = new Byte[NB_BYTE_PER_SECTION];

            states = new Dictionary<int, StateObject>();

            listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram , ProtocolType.Udp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    EndPoint endpoint = localEndPoint;
                    int size = listener.ReceiveFrom(bytes, ref endpoint);

                    Task.Factory.StartNew(() => Listen(bytes, size, endpoint));
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Listen(byte[] buffer, int size, EndPoint endpoint)
        {
            if (size < HEADER_SIZE)
                return;

            StateObject state;
            List<byte> message = new List<byte>() {1};

            int currentId = BitConverter.ToInt32(buffer, 1);
            if (currentId == 0)
            {
                state = new StateObject();
                currentId = ++id;

                state.FileSize = BitConverter.ToInt32(buffer, HEADER_SIZE);
                states[currentId] = state;
                message.AddRange(BitConverter.GetBytes(id));
            }
            else
            {
                state = states[currentId];
                if (buffer[0] == 1)
                    File.WriteAllBytes("TEST" + id + ".txt", state.Content);
                else
                    state.UpdateContent(buffer, BitConverter.ToInt32(buffer, 5), size - HEADER_SIZE);
            }
            listener.SendTo(message.ToArray(), endpoint);
        }
    }
}