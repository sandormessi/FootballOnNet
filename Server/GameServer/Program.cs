namespace GameServer
{
    using System;
    using System.Net;

    using GameServer.Network;

    public static class Program
    {
        private static readonly IPEndPoint EndPoint = new IPEndPoint(IPAddress.Loopback, 55555);

        private static void Main()
        {
            var server2 = new Server(EndPoint);
            server2.Start();

            Console.ReadKey();
        }
    }
}