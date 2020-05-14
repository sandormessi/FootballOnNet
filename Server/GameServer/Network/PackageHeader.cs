namespace GameServer.Network
{
    public class PacketHeader
    {
        public PacketHeader(byte command, int packageSize, byte messageType)
        {
            Command = command;
            PackageSize = packageSize;
            MessageType = messageType;
        }

        /// <summary>The size of the header itself in bytes.</summary>
        public static int HeaderSize
        {
            get
            {
                return sizeof(byte) + sizeof(int) + sizeof(byte);
            }
        }

        /// <summary>Gets the command this packet encapsulates.</summary>
        public byte Command { get; }
        /// <summary>Gets the size of this packet in bytes.</summary>
        public int PackageSize { get; }
        /// <summary>Gets the type of the message.</summary>
        public byte MessageType { get; }

        public override string ToString()
        {
            return $"Command: {Command}, PackageSize: {PackageSize}, MessageType: {MessageType}";
        }
    }
}
