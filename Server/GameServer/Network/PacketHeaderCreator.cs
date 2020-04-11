namespace GameServer.Network
{
    public static class PacketHeaderCreator
    {
        public static PacketHeader Create(CommandType commandType, MessageType messageType, int packetSize)
        {
            return new PacketHeader((byte)commandType, packetSize, (byte)messageType);
        }
        
        public static PacketHeader Create(CommandType commandType, MessageType messageType, long packetSize)
        {
            return new PacketHeader((byte)commandType, (int)packetSize, (byte)messageType);
        }
    }
}
