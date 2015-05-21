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
    public class StateObject
    {   
        public byte[] content;
    }

    public class Listener
    {
        private int id = 1;
        IPEndPoint localEndPoint;
        private const int NB_BYTE_PER_SECTION = 2064;
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
                SocketType.Stream, ProtocolType.Udp);

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

        public void Listen(byte[] buffer, int size, EndPoint endpoint)
        {
            if (size < 9)
                return;

            StateObject state;
            List<byte> message = new List<byte>() {1};

            int currentId = BitConverter.ToInt32(buffer, 1);
            if (currentId == 0)
            {
                state = new StateObject();
                currentId = id;
                id++;

                state.content = new byte[BitConverter.ToInt32(buffer, 9)];
                states[currentId] = state;

                message.AddRange(BitConverter.GetBytes(id));
            }
            else
            {
                state = states[currentId];

                if (buffer[0] == 1)
                    File.WriteAllBytes("TEST" + id + ".txt", state.content);
                else
                {
                    int offset = BitConverter.ToInt32(buffer, 5);

                    for (int i = 0; i < size - 9; i++)
                        state.content[i + offset] = buffer[i];
                }
            }

            listener.SendTo(message.ToArray(), endpoint);
        }
    }
}