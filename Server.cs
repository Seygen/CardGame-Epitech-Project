using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace DOT_cardGames_2018
{
    class Server
    {
        List<Player> _players = new List<Player>();
        List<Tuple<Player, string>> _cmd = new List<Tuple<Player, string>>();
        Mutex _cmdMutex = new Mutex();
        Mutex _playerMutex = new Mutex();

        public void run()
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 8080);
            serverSocket.Start();
            Console.WriteLine("[LOG] : Server started");

            Thread newPlayerThread = new Thread(() => addPlayer(serverSocket));
            newPlayerThread.Start();


            while (_players.Count < 4)
            {
                broadcastToClients("Waiting for 4 players...\n");
                Thread.Sleep(2000);
            }
            GameManager g = new GameManager();
            Thread gameThread = new Thread(() => g.run(this));
            gameThread.Start();
            gameThread.Join();
        }

        void addPlayer(TcpListener serverSocket)
        {
            while (true)
            {
                _playerMutex.WaitOne();
                try
                {
                    _players.Add(new Player("player" + _players.Count, (_players.Count % 2 == 0 ? 1 : 2), serverSocket.AcceptTcpClient()));
                }
                finally
                {
                    _playerMutex.ReleaseMutex();
                }
                Console.WriteLine("[LOG] : New client connected");
                broadcastToClients("player" + (_players.Count - 1) + " joined the game!\n");
                Thread clientThread = new Thread(() => readClient(_players[_players.Count - 1]));
                clientThread.Start();
            }
        }

        void readClient(Player player)
        {
            while (true)
            {
                byte[] bytesFrom = new byte[player.getClient().ReceiveBufferSize];
                if (player.getClient().GetStream().Read(bytesFrom, 0, player.getClient().ReceiveBufferSize) == 0)
                {
                    player.getClient().Close();
                    return ;
                }
                string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                _cmdMutex.WaitOne();
                try
                {
                    _cmd.Add(new Tuple<Player, string>(player, dataFromClient));
                }
                finally
                {
                    _cmdMutex.ReleaseMutex();
                }
            }
        }

        public void sendDataToClient(TcpClient client, string data)
        {
            Byte[] sendBytes = Encoding.ASCII.GetBytes(data);
            client.GetStream().Write(sendBytes, 0, sendBytes.Length);
            client.GetStream().Flush();
        }

        public void broadcastToClients(string data)
        {
            foreach (Player p in _players)
            {
                sendDataToClient(p.getClient(), data);
            }
        }

        public void broadcastToClientsExceptSelf(Player self, string data)
        {
            foreach (Player p in _players)
            {
                if (self != p)
                {
                    sendDataToClient(p.getClient(), data);
                }
            }
        }

        public void broadcastToTeam(Player player, string data)
        {
            foreach (Player p in _players)
            {
                if (player.getTeam() == p.getTeam())
                {
                    sendDataToClient(p.getClient(), data);
                }
            }
        }

        public List<Player> getPlayers()
        {
            return (_players);
        }

        public Boolean cmdLeft()
        {
            bool b;

            _cmdMutex.WaitOne();
            try
            {
                b = (_cmd.Count > 0 ? true : false);
            }
            finally
            {
                _cmdMutex.ReleaseMutex();
            }
            return (b);
        }

        public Tuple<Player, string> getLastCmd()
        {
            Tuple<Player, string> cmd;

            _cmdMutex.WaitOne();
            try
            {
                cmd = _cmd[0];
                _cmd.RemoveAt(0);
            }
            finally
            {
                _cmdMutex.ReleaseMutex();
            }
            return (cmd);
        }
    }
}
