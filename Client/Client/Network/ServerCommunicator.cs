namespace FootballClient.Network
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    /// <summary>Represents a communicator between a server and a client via a Connected <see cref="TcpClient"/>.</summary>
    public class ServerCommunicator
    {
        #region Fields

        private readonly object syncObject = new object();

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
            lock (syncObject)
            {
                stream.WriteByte(packetToBeSent.Header.Command);
                stream.Write(BitConverter.GetBytes(packetToBeSent.Header.PackageSize), 0, 4);
                stream.WriteByte(packetToBeSent.Header.MessageType);

                packetToBeSent.Data.CopyTo(stream);
                stream.Flush(); 
            }
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
                PacketHeader header = ReadPackageHeader();
                if (header is null)
                {
                    throw new InvalidOperationException("The message has been received is invalid. The package's header is missing.");
                }

                Console.WriteLine(header);

                Stream receivedData = ReadPackageData(header);
                if (receivedData is null)
                {
                    throw new InvalidOperationException("The message has been received is invalid. The package's body is missing.");
                }

                Console.WriteLine($"Data received: {receivedData.Length} byte(s).");
                OnDataReceived(new DataReceivedEventArgs<Packet>(new Packet(header, receivedData)));
                //DiscardDataInStream(stream);
            }

            receiveInProgress = false;
        }

        private static void DiscardDataInStream(NetworkStream networkStream)
        {
            var buffer = new byte[8192];
            while (networkStream.DataAvailable)
            {
                networkStream.Read(buffer, 0, buffer.Length);
            }
        }

        private Stream ReadPackageData(PacketHeader header)
        {
            byte[] buffer = ReadDataWithSize(header.PackageSize);

            if (buffer is null)
            {
                return null;
            }

            return new MemoryStream(buffer);
        }

        private PacketHeader ReadPackageHeader()
        {
            int headerSize = PacketHeader.HeaderSize;
            byte[] buffer = ReadDataWithSize(headerSize);

            if (buffer is null)
            {
                return null;
            }

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

        #endregion
    }
}
