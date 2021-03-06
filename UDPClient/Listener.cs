﻿using System;
using System.Collections.Concurrent;
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
            if (content[offset] == 0)
            {
                for (int i = 0; i < size; i++)
                {
                    if (i + offset >= content.Length)
                        break;
                    content[i + offset] = bytes[i];
                }

                if (PacketReceived != null)
                    PacketReceived.Invoke(this, new AckEventArgs(offset));
            }
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
        private ConcurrentQueue<Packet> queue;
        private Dictionary<int, StateObject> states;
        Socket listener;
        //Mutex mtx;


        public EventHandler<string> Log
        {
            get;
            set;
        }
        public EventHandler<ObjectEventArgs> ObjectCreated { get; set; }

        public IEnumerable<StateObject> GetStates()
        {
            return states.Values;
        }

        public Listener(int port)
        {
            localEndPoint = new IPEndPoint(IPAddress.Any, port);
            //mtx = new Mutex();
            queue = new ConcurrentQueue<Packet>();
            Task.Factory.StartNew(Dequeue);
        }

        public void StartListening()
        {
            byte[] bytes = new Byte[RFBProtocol.NB_BYTE_PER_SECTION + RFBProtocol.HEADER_SIZE];

            states = new Dictionary<int, StateObject>();

            listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);
            listener.ReceiveBufferSize = 2000000;

                listener.Bind(localEndPoint);
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint endpoint = sender;
            while (true)
            {
                try
                {

                    int size = listener.ReceiveFrom(bytes, ref endpoint);
                    queue.Enqueue(new Packet(size, bytes, endpoint));
                }

                catch (Exception e)
                {
                    Log.Invoke(this, string.Format("Le client a fermé en sauvage"));
                }

            }
        }

        public void Dequeue()
        {
            while (true)
            {
                Packet pkt;
                if (queue.TryDequeue(out pkt))
                {
                    Task.Factory.StartNew(() => Listen(pkt));
                }
            }
        }

        private void Listen(Packet[] packets)
        {
            foreach (Packet packet in packets)
            {
                Listen(packet.bytes, packet.size, packet.endpoint);
            }
        }

        private void Listen(Packet packet)
        {

            Listen(packet.bytes, packet.size, packet.endpoint);
        }

        private void Listen(byte[] buffer, int size, EndPoint endpoint)
        {
            StateObject state;
            var protocol = RFBProtocol.Decode(buffer, size);

            List<byte> message = new List<byte>() { 1 };

            if (protocol.PacketHeader.ID == 0)
            {
                ++id;
                Log.Invoke(this, string.Format("Nouveau client numéro {0}", id));
                state = new StateObject()
                {
                    FileSize = BitConverter.ToInt32(protocol.Data, 0),
                    FileName = System.Text.Encoding.Default.GetString(protocol.Data, 4, protocol.Size - 4)
                };
                states[id] = state;

                if (ObjectCreated != null)
                    ObjectCreated.Invoke(this, new ObjectEventArgs(state));

                message.AddRange(BitConverter.GetBytes(id));
            }
            else
            {
                state = states[protocol.PacketHeader.ID];
                //Log.Invoke(this,string.Format("New packet from :{0}, offset: {1}",protocol.PacketHeader.ID,protocol.PacketHeader.Offset));
                message.AddRange(BitConverter.GetBytes(protocol.PacketHeader.ID));
                message.AddRange(BitConverter.GetBytes(protocol.PacketHeader.Offset));
                if (protocol.PacketHeader.IsAck)
                {
                    Log.Invoke(this, string.Format("Écrire un fichier pour numéro {0}", protocol.PacketHeader.ID));
                    File.WriteAllBytes(state.FileName,
                        state.Content);
                }
                else
                    state.UpdateContent(protocol.Data, protocol.PacketHeader.Offset, protocol.Size);
            }
            listener.SendTo(message.ToArray(), endpoint);
        }
    }
}