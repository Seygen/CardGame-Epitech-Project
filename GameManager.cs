using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace DOT_cardGames_2018
{
    class GameManager
    {
        int _fold = 0;
        int _turn = 0;
        Tuple<Player, string> _cmd = null;
        List<Tuple<Player, Card>> _table = new List<Tuple<Player, Card>>();

        public void run(Server s)
        {
            Console.WriteLine("[LOG] : Game started");
            Deck deck = new Deck();
            Console.WriteLine("[DEBUG] : deck : " + deck.toString());
            deck.shuffle();
            distributeCards(s, deck);
            s.broadcastToClients("Game is starting.\n");
            foreach (Player player in s.getPlayers())
                s.sendDataToClient(player.getClient(), player.handToString());
            s.sendDataToClient(s.getPlayers()[0].getClient(), "Your turn.\n");
            while (_fold < 8)
            {
                if (s.cmdLeft())
                {
                    _cmd = s.getLastCmd();
                    if (string.Compare(_cmd.Item2, "HELP") == 0)
                    {
                        s.sendDataToClient(_cmd.Item1.getClient(), @"- HELP
    Display this command
- HAND
    Display the cards in your hand
- MSGA
    Send a message to all the clients
- MSGT
    Send a message to player in your team
- CARD
    Play a card
- TABLE
    Display the card on the table
- EXIT
    Exit the game
");
                    }
                    else if (string.Compare(_cmd.Item2, "HAND") == 0)
                    {
                        s.sendDataToClient(_cmd.Item1.getClient(), _cmd.Item1.handToString());
                    }
                    else if (string.Compare(_cmd.Item2.Substring(0, 4), "MSGA") == 0)
                    {
                        s.broadcastToClientsExceptSelf(_cmd.Item1, _cmd.Item1.getName() + " : " + _cmd.Item2.Substring(5));
                    }
                    else if (string.Compare(_cmd.Item2.Substring(0, 4), "MSGT") == 0)
                    {
                        s.broadcastToTeam(_cmd.Item1, "Team" + _cmd.Item1.getTeam() + " : " + _cmd.Item1.getName() + " : " + _cmd.Item2.Substring(5));
                    }
                    else if (string.Compare(_cmd.Item2.Substring(0, 4), "CARD") == 0)
                    {
                        if (_cmd.Item1 == s.getPlayers()[_turn % 4])
                        {
                            string[] card = _cmd.Item2.Split(new Char[] { ' ', '\n' });
                            bool cardValid = false;

                            foreach (Card.Type type in Enum.GetValues(typeof(Card.Type)))
                                foreach (Card.Rank rank in Enum.GetValues(typeof(Card.Rank)))
                                    if (string.Compare(card[1], rank.ToString("g")) == 0 && string.Compare(card[2], type.ToString("g")) == 0)
                                        cardValid = true;
                            if (cardValid)
                            {
                                Card cardFound = null;

                                foreach (Card c in _cmd.Item1.getHand())
                                    if (string.Compare(card[1], c.getRank().ToString("g")) == 0 && string.Compare(card[2], c.getType().ToString("g")) == 0)
                                        cardFound = c;
                                if (cardFound != null)
                                {
                                    Console.WriteLine("[LOG] : Card found in hand");
                                    _cmd.Item1.removeCardFromHand(cardFound);
                                    _table.Add(new Tuple<Player, Card>(_cmd.Item1, cardFound));
                                    s.sendDataToClient(_cmd.Item1.getClient(), "Turn completed.\n");
                                    if (_table.Count != 4)
                                    {
                                        _turn++;
                                    }
                                    else
                                    {
                                        _turn = s.getPlayers().IndexOf(foldWinner());
                                        _fold++;
                                    }
                                    s.sendDataToClient(s.getPlayers()[_turn % 4].getClient(), "Your turn.\n");
                                }
                                else
                                {
                                    s.sendDataToClient(_cmd.Item1.getClient(), "You don't have this card.\n");
                                }
                            }
                            else
                            {
                                s.sendDataToClient(_cmd.Item1.getClient(), "This Card doesen't exist.\n");
                            }
                        }
                        else
                        {
                            s.sendDataToClient(_cmd.Item1.getClient(), "Not your turn\n");
                        }
                    }
                    else if (string.Compare(_cmd.Item2, "TABLE") == 0)
                    {
                        s.sendDataToClient(_cmd.Item1.getClient(), tableToString());
                    }
                    else if (string.Compare(_cmd.Item2, "EXIT\n") == 0)
                    {
                        s.broadcastToClients("The game is closing after a player left\n");
                        return ;
                    }
                    else
                    {
                        s.sendDataToClient(_cmd.Item1.getClient(), "Invalid command\n");
                    }
                }
            }
            s.broadcastToClients("Team" + teamWinner(s) + " win!\n");
            s.broadcastToClients("Game finish\n");
        }

        void distributeCards(Server s, Deck deck)
        {
            foreach (Player p in s.getPlayers())
                for (int i = 0; i < 3; i++)
                    p.addCard(deck.distributeCard());
            foreach (Player p in s.getPlayers())
                for (int i = 0; i < 2; i++)
                    p.addCard(deck.distributeCard());
            foreach (Player p in s.getPlayers())
                for (int i = 0; i < 3; i++)
                    p.addCard(deck.distributeCard());
            Console.WriteLine("[LOG] : Deck distributed");
        }

        string tableToString()
        {
            if (_table.Count == 0)
                return ("There is no card on the table\n");
            string table = "Card on the table are : ";

            foreach (Tuple<Player, Card> t in _table)
            {
                table += t.Item2.toString() + (_table.IndexOf(t) != _table.Count - 1 ? ", " : "\n");
            }
            return (table);
        }

        Player foldWinner()
        {
            Player player = _table[0].Item1;
            Card cardFirst = _table[0].Item2;

            foreach (Tuple<Player, Card> t in _table)
            {
                if (t.Item2.getType() == cardFirst.getType() && t.Item2.getRank() > cardFirst.getRank())
                {
                    player = t.Item1;
                }
            }
            while (_table.Count > 0)
            {
                player.addCardToFold(_table[0].Item2);
                _table.RemoveAt(0);
            }
            return (player);
        }

        int teamWinner(Server s)
        {
            int team1 = 0;
            int team2 = 0;

            foreach (Player p in s.getPlayers())
            {
                foreach (Card c in p.getFold())
                {
                    if (p.getTeam() == 1)
                    {
                        team1 += ((int)c.getRank()) / 10;
                    }
                    else
                    {
                        team2 += ((int)c.getRank()) / 10;
                    }
                }
            }
            return ((team1 > team2 ? 1 : 2));
        }
    }
}