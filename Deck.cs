using System;
using System.Collections.Generic;

namespace DOT_cardGames_2018
{
    class Deck
    {
        List<Card> _deck = new List<Card>();

        public Deck()
        {
            foreach (Card.Type type in Enum.GetValues(typeof(Card.Type)))
            {
                foreach (Card.Rank rank in Enum.GetValues(typeof(Card.Rank)))
                {
                    _deck.Add(new Card(rank, type));
                }
            }
            Console.WriteLine("[LOG] : Deck created");
        }

        public void shuffle()
        {
            Random rng = new Random();

            for (int n = _deck.Count; n > 1; )
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = _deck[k];
                _deck[k] = _deck[n];
                _deck[n] = value;
            }
            Console.WriteLine("[LOG] : Deck shuffled");
        }

        public Card distributeCard()
        {
            Card card;

            card = _deck[0];
            _deck.RemoveAt(0);
            return (card);
        }

        public string toString()
        {
            string deck = "";

            foreach (Card card in _deck)
            {
                deck += card.toString() + (_deck.IndexOf(card) != _deck.Count - 1 ? ", " : "\n");
            }
            return (deck);
        }
    }
}
