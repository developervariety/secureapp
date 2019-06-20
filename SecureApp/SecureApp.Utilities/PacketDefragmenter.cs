using System;
using System.IO;

namespace SecureApp.Utilities
{
    internal class PacketDefragmenter 
    {
        public int BytesToReceive => CalculateBytesToReceive();

        private int CalculateBytesToReceive()
        {
            if (_receiveStream == null) {
                int read = 4 - PacketSizeCounter;
                if (read < 1)
                    throw new Exception("Error. Reading past packet header with no size."); //We are reading the header but header is alredy read?
                return read;
            }

            long neededBytes = CurrentPacketSize - _receiveStream.Length;
            if (neededBytes < 0)
                throw new IndexOutOfRangeException("Need negative amount of bytes to complete packet.");

            if (neededBytes > ReceiveBuffer.Length) {
                return ReceiveBuffer.Length;
            }

            return (int)neededBytes;
        }

        public int BufferIndex => PacketSizeCounter;
        
        private MemoryStream _receiveStream;
        
        private int CurrentPacketSize;

        private byte PacketSizeCounter;

        public byte[] ReceiveBuffer { get; set; }

        public PacketDefragmenter(int bufferSize)
        {
            if (bufferSize < 4)
                throw new Exception("Buffer size must be at least 4 bytes");
            ReceiveBuffer = new byte[bufferSize];
        }
        
        public byte[] Process(int bytes)
        {
            if (_receiveStream == null) {
                PacketSizeCounter += (byte)bytes;
                if (PacketSizeCounter > 4)
                    throw new Exception("received more than 4 bytes for packet header.");

                if (PacketSizeCounter != 4) return null;
                
                PacketSizeCounter = 0;
                CurrentPacketSize = BitConverter.ToInt32(ReceiveBuffer, 0);
                _receiveStream = new MemoryStream(CurrentPacketSize);

                return null;
            }

            _receiveStream.Write(ReceiveBuffer, 0, bytes);

            if (_receiveStream.Position != CurrentPacketSize) return null;
            
            byte[] packet = _receiveStream.ToArray();
            _receiveStream.Dispose();
            _receiveStream = null;
            
            return packet;
        }
    }
}
