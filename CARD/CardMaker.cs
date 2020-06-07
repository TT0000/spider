using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using CARD;

namespace CARD
{
    public class CardMaker
    {
        List<Card> allCard;
        int groupOffset;
        public void clear()
        {
            allCard.Clear();
            groupOffset = 0;
        }
        private void makeCards(int type)
        {
            allCard = new List<Card>(104);
            Card[] sequence = getSequence(type);
            for (int i = 0; i < 104; i++)
            {
                allCard.Add(sequence[i]);
            }
        }
        public CardMaker(int type)
        {
            makeCards(type);
        }
        public CardGroup getCardGroup(int GroupNum)
        {
            int offset = groupOffset * 10;
            if (GroupNum > 0 && GroupNum < 11)
            {
                CardGroup stack = new CardGroup();
                GroupNum--;
                for (int i = 0; i < 10; i++)
                {
                    stack.cards.Add(allCard.ElementAt(i + 10 * GroupNum - offset));
                }
                return stack;
            }
            return null;
        }
        public List<Card> GetlastCards()
        {
            List<Card> cards = new List<Card>(4);
            for (int i = 0; i < 4; i++)
            {
                cards.Add(allCard.ElementAt(100 + i));
                allCard.ElementAt(100 + i).stackNum = (int)(i + 1);
            }
            return cards;
        }
        private Card[] getSequence(int type)//洗牌
        {
            Random rd = new Random();
            Card[] sequence = new Card[104];
            switch (type)
            {
                case 1:
                    for (int i = 0; i< 104; i++)
                    {
                        sequence[i] = new Card((int)(i % 13 + 1), 1);
                    }
                    break;
                case 2:
                    for (int i = 0; i < 52; i++)
                    {
                        sequence[i] = new Card((int)(i % 13 + 1), 1);
                    }
                    break;
                case 3:
                    for (int j = 52; j < 104; j++)
                    {
                        sequence[j] = new Card((int)(j % 13 + 1), 2);
                    }
                    break;
            }
            Card tep;
            int dert;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 4; j <= 10; j++)
                {
                    tep = sequence[i * 10 + j];
                    dert = rd.Next(4, 11);
                    sequence[i * 10 + j] = sequence[(i + 5) * 10 + dert];
                    sequence[(i + 5) * 10 + dert] = tep;
                }
           
            }
            return sequence;
        }
        public void setGroupOffset(int offset)
        {
            groupOffset = offset;
        }
        public void addCard(Card card)
        {
            allCard.Add(card);
        }            
    }
}
