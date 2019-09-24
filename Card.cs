using System;

namespace DOT_cardGames_2018
{
    class Card
    {
        Rank _rank;
        Type _type;
        public Card(Rank rank, Type type)
        {
            _rank = rank;
            _type = type;
        }

        public enum Rank
        {
            SEVEN,
            EIGHT,
            NINE,
            JACK = 20,
            QUEEN = 30,
            KING = 40,
            TEN = 100,
            AS = 110
        }

        public enum Type
        {
            SPADE,
            HEART,
            CLUB,
            DIAMOND
        }

        public Rank getRank()
        {
            return (_rank);
        }

        public Type getType()
        {
            return (_type);
        }

        public void setRank(Rank rank)
        {
            _rank = rank;
        }

        public void setType(Type type)
        {
            _type = type;
        }

        public string toString()
        {
            return (_rank + " " + _type);
        }
    }
}
