namespace GameServer.Network
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    using GameServer.Controllers;

    public class Server
    {
        private bool stopRequest;
        private bool isListening;

        private readonly TcpListener tcpListener;
        
        public Server(IPEndPoint localEndPoint)
        {
            LocalEndPoint = localEndPoint ?? throw new ArgumentNullException(nameof(localEndPoint));
            tcpListener = new TcpListener(localEndPoint);
        }

        public IPEndPoint LocalEndPoint { get; }

        public void Start()
        {
            if (isListening)
            {
                throw new InvalidOperationException("The server is already in this state.");
            }

            isListening = true;
            tcpListener.Start(10);
            //Task.Factory.StartNew(ReceiveConnections);
            ReceiveConnections();
        }

        public void Stop()
        {
            tcpListener.Stop();
            stopRequest = true;
        }

        private void ReceiveConnections()
        {
            while (!stopRequest)
            {
                var controller = new ServerGameController();

                Console.WriteLine("Waiting for first Client.");

                TcpClient connectedClient1 = tcpListener.AcceptTcpClient();
                var communicator1 = new ServerCommunicator(connectedClient1);
                controller.SetHomeTeamCommunicator(communicator1);

                Console.WriteLine("First client has connected.");

                Console.WriteLine("Waiting for second Client.");

                TcpClient connectedClient2 = tcpListener.AcceptTcpClient();
                var communicator2 = new ServerCommunicator(connectedClient2);
                controller.SetAwayTeamCommunicator(communicator2);

                Console.WriteLine("Second client has connected.");
                Console.WriteLine("Match has started.");
            }

            stopRequest = false;
            isListening = false;
        }
    }
}
