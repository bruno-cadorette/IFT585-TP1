﻿using System;
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
    public class ObjectEventArgs : EventArgs
    {
        public StateObject sObject;

        public ObjectEventArgs(StateObject o)
        {
            sObject = o;
        }
    }

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

        public string FileName { get; set; }

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

    public class Packet
    {
        public byte[] bytes;
        public int size;
        public EndPoint endpoint;

        public Packet(int size, byte[] bytes, EndPoint endpoint)
        {
            this.size = size;
            this.bytes = bytes;
            this.endpoint = endpoint;
        }

    }

    public class Listener
    {
        private int id = 0;
        IPEndPoint localEndPoint;
        private const int NB_BYTE_PER_SECTION = 2064;
        private const int FILE_LENGTH = 5;
        private const int HEADER_SIZE = 9;
        private Queue<Packet> queue;
        private Dictionary<int, StateObject> states;
        Socket listener;

        public EventHandler<ObjectEventArgs> ObjectCreated { get; set; }

        public IEnumerable<StateObject> GetStates()
        {
            return states.Values;
        }

        public Listener(int port)
        {
            localEndPoint = new IPEndPoint(IPAddress.Any, port);
            queue = new Queue<Packet>();
            Task.Factory.StartNew(Dequeue);
        }

        public void StartListening()
        {
            byte[] bytes = new Byte[NB_BYTE_PER_SECTION];

            states = new Dictionary<int, StateObject>();

            listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);

            try
            {
                listener.Bind(localEndPoint);

                while (true)
                {
                    EndPoint endpoint = localEndPoint;
                    int size = listener.ReceiveFrom(bytes, ref endpoint);

                    queue.Enqueue(new Packet(size, bytes, endpoint));
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Dequeue()
        {
            while (true)
            {
                if (queue.Count > 0)
                {
                    Packet packet = queue.Dequeue();
                    Task.Factory.StartNew(() => Listen(packet.bytes, packet.size, packet.endpoint));
                }
            }
        }

        private void Listen(byte[] buffer, int size, EndPoint endpoint)
        {

            StateObject state;
            var protocol = RFBProtocol.Decode(buffer, size);

            List<byte> message = new List<byte>() { 1 };

            if (protocol.PacketHeader.ID == 0)
            {
                ++id;
                state = new StateObject()
                {
                    FileSize = BitConverter.ToInt32(protocol.Data, RFBProtocol.FILE_LENGTH),
                    FileName = System.Text.Encoding.Default.GetString(protocol.Data,RFBProtocol.HEADER_SIZE,protocol.Size)
                };
                states[id] = state;

                if (ObjectCreated != null)
                    ObjectCreated.Invoke(this, new ObjectEventArgs(state));

                message.AddRange(BitConverter.GetBytes(id));
            }
            else
            {
                state = states[protocol.PacketHeader.ID];
                message.AddRange(BitConverter.GetBytes(protocol.PacketHeader.ID));
                if (protocol.PacketHeader.IsAck)
                    File.WriteAllBytes("C:\\Users\\Bruno\\Documents\\TEST" + id + ".txt", state.Content);
                else
                    state.UpdateContent(protocol.Data, protocol.PacketHeader.Offset, protocol.Size);
            }

            listener.SendTo(message.ToArray(), endpoint);
        }
    }
}