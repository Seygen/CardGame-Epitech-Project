using System;
using System.Threading;

namespace DOT_cardGames_2018
{
    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server();
            Thread serverThread = new Thread(() => s.run());
            serverThread.Start();
            serverThread.Join();
        }
    }
}
