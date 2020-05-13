namespace Client.Network
{
    public class PacketHeader
    {
        public PacketHeader(byte command, int packageSize, byte messageType)
        {
            Command = command;
            PackageSize = packageSize;
            MessageType = messageType;
        }

        public static int HeaderSize
        {
            get
            {
                return sizeof(byte) + sizeof(int) + sizeof(byte);
            }
        }

        public byte Command { get; }
        public int PackageSize { get; }
        public byte MessageType { get; }

        
        public override string ToString()
        {
            return $"Command: {Command}, PackageSize: {PackageSize}, MessageType: {MessageType}";
        }
    }
}
