namespace GameServer.Network
{
    using System;
    using System.IO;

    public class Packet
    {
        public Packet(PacketHeader header, Stream data)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Data = data ?? throw new ArgumentNullException(nameof(data)); 
            data.Seek(0, SeekOrigin.Begin);
        }

        public PacketHeader Header { get; }
        public Stream Data { get; }
    }
}
