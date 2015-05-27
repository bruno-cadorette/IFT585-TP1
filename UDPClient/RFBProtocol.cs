using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPClient
{
    /// <summary>
    /// RFB Protocol
    /// 1 byte => 1 == ACK, 0 == DATA
    /// 4 byte => file number
    /// 4 byte => offset
    /// 
    /// First Transmission
    /// 1 byte => DATA
    /// 4 byte => FileID
    /// 4 byte => File Size
    /// X Byte => File Name
    /// 
    /// 
    /// 
    /// </summary>
    class RFBProtocol
    {
        public class Header
        {
            public bool IsAck { get; private set; }
            public int ID { get; private set; }
            public int Offset { get; private set; }

            public Header(byte[] buffer, int size)
            {
                if (size < HEADER_SIZE)
                {
                    throw new Exception("La taille du paquet est trop petit!");
                }
                IsAck = buffer[0] == 1;
                ID = BitConverter.ToInt32(buffer, 1);

                Offset = (ID == 0) ? 0 : BitConverter.ToInt32(buffer, 5);
            }
        }
        public static const int NB_BYTE_PER_SECTION = 2064;
        public static const int FILE_LENGTH = 5;
        public static const int HEADER_SIZE = 9;

        public byte[] Data { get; private set; }
        public Header PacketHeader { get; private set; }
        public int Size { get; private set; }
        public static RFBProtocol Decode(byte[] buffer, int size)
        {
            var header = new Header(buffer, size);
            var skipValue = (header.ID == 0) ? FILE_LENGTH : HEADER_SIZE;
            var s = size - skipValue;
            return new RFBProtocol()
            {
                PacketHeader = header,
                Size = s,
                Data = buffer.Skip(skipValue).Take(s).ToArray()
            };
        }

    }
}
