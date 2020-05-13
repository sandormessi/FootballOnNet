namespace Client.Network
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Represents a communicator between a server and a client via a Connected <see cref="TcpClient"/>.</summary>
    public class ServerCommunicator
    {
        #region Fields

        private readonly SemaphoreSlim syncSemaphoreSlim = new SemaphoreSlim(1, 1);

        private bool receiveInProgress;
        private readonly TcpClient tcpClient;
        private readonly NetworkStream stream;
        private bool stopSignal;

        #endregion

        #region Constructors

        public ServerCommunicator(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));
            stream = tcpClient.GetStream();
        }

        #endregion

        #region Public Methods

        public void Stop()
        {
            stopSignal = true;
        }
        public void StartCommunication()
        {
            Task.Factory.StartNew(ReceiveData).ConfigureAwait(false);
        }

        public void SendDataAsPacket(Packet packetToBeSent)
        {
           syncSemaphoreSlim.Wait();

           stream.WriteByte(packetToBeSent.Header.Command);
           stream.Write(BitConverter.GetBytes(packetToBeSent.Header.PackageSize), 0, 4);
           stream.WriteByte(packetToBeSent.Header.MessageType);

           packetToBeSent.Data.CopyTo(stream);
           stream.Flush();

           syncSemaphoreSlim.Release();
        }

        #endregion

        #region Public Events

        public event EventHandler<DataReceivedEventArgs<Packet>> DataReceived;

        #endregion

        #region Event Invocator Methods

        protected virtual void OnDataReceived(DataReceivedEventArgs<Packet> e)
        {
            DataReceived?.Invoke(this, e);
        }

        #endregion

        #region Private Methods

        private void ReceiveData()
        {
            if (receiveInProgress)
            {
                throw new InvalidOperationException("This operation is already in progress.");
            }

            receiveInProgress = true;

            while (!stopSignal)
            {
               PacketHeader header = ReadPacket(out Stream receivedData);
               OnDataReceived(new DataReceivedEventArgs<Packet>(new Packet(header, receivedData)));
            }

            receiveInProgress = false;
        }

        private Stream ReadPackageData(PacketHeader header)
        {
            byte[] buffer = ReadDataWithSize(header.PackageSize);

            return new MemoryStream(buffer);
        }
        private PacketHeader ReadPackageHeader()
        {
            int headerSize = PacketHeader.HeaderSize;
            byte[] buffer = ReadDataWithSize(headerSize);

            byte command = buffer[0];
            var packageSize = BitConverter.ToInt32(buffer, 1);
            byte messageType = buffer.Last();

            return new PacketHeader(command, packageSize, messageType);
        }
        private byte[] ReadDataWithSize(int packageSize)
        {
            var receivedBytes = 0;
            var buffer = new byte[packageSize];
            int bytesWaitingFor = packageSize;

            while ((receivedBytes != packageSize) && !stopSignal)
            {
                if (tcpClient.Available > 0)
                {
                    int currentlyReceivedBytes = stream.Read(buffer, receivedBytes, bytesWaitingFor);
                    bytesWaitingFor -= currentlyReceivedBytes;
                    receivedBytes += currentlyReceivedBytes;
                }
            }

            if (receivedBytes != packageSize)
            {
                return null;
            }

            return buffer;
        }

        private PacketHeader ReadPacket(out Stream receivedData)
        {
           PacketHeader header = ReadPackageHeader();

           Console.WriteLine(header);

           receivedData = ReadPackageData(header);

           Console.WriteLine($"Data received: {receivedData.Length} byte(s).");
           return header;
        }

      #endregion
   }
}
