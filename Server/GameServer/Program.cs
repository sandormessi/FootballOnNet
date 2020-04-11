namespace GameServer
{
    using System;
    using System.Net;

    public class Program
    {
        private static readonly EndPoint EndPoint = new IPEndPoint(IPAddress.Loopback, 55555);

        static void Main()
        {
            Console.ReadLine();
        }
    }
}