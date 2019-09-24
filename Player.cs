using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace DOT_cardGames_2018
{
    class Player
    {
        string _name;
        int _team;
        TcpClient _client;
        List<Card> _hand = new List<Card>();
        List<Card> _fold = new List<Card>();

        public Player(string name, int team, TcpClient client)
        {
            _name = name;
            _team = team;
            _client = client;
        }

        public string getName()
        {
            return (_name);
        }

        public int getTeam()
        {
            return (_team);
        }

        public TcpClient getClient()
        {
            return (_client);
        }

        public List<Card> getHand()
        {
            return (_hand);
        }

        public List<Card> getFold()
        {
            return (_fold);
        }

        public void setName(string name)
        {
            _name = name;
        }

        public void setTeam(int team)
        {
            _team = team;
        }

        public void addCard(Card card)
        {
            _hand.Add(card);
        }

        public void addCardToFold(Card card)
        {
            _fold.Add(card);
        }

        public void removeCardFromHand(Card card)
        {
            _hand.RemoveAt(_hand.IndexOf(card));
        }

        public string handToString()
        {
            string hand = "Your hand is : ";

            foreach (Card card in _hand)
            {
                hand += card.toString() + (_hand.IndexOf(card) != _hand.Count - 1 ? ", " : ".\n");
            }
            return (hand);
        }
    }
}
