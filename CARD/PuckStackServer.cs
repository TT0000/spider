using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CARD
{
  public  class PuckStackServer
    {
        List<CardStack> gameStacks;
        public void clear()
        {
            gameStacks.Clear();
        }
        public List<CardStack> getAllStack()
        {
            return gameStacks;
        }
        public PuckStackServer()
        {
            gameStacks = new List<CardStack>(10);
            for (int i = 0; i < 10; i++)
            {
                gameStacks.Add(new CardStack((int)(i + 1)));
            }
        }
        public CardStack GetStack(int i)
        {
            return gameStacks.ElementAt(i - 1);
        }
        public void removeAllStacks()
        {
            for (int i = 0; i < 10; i++)
            {
                gameStacks.RemoveAt(i);
            }
        }
        public bool canMove(Card mover)
        {
            int stackNum = mover.stackNum;
            return gameStacks.ElementAt(stackNum - 1).canMove(mover);
        }
        public bool canReceive(Card arriver, int targetStackNum)
        {
            return gameStacks.ElementAt(targetStackNum - 1).canReceive(arriver);
        }
        public void Receive(int targetStackNum, List<Card> arriver)
        {
            gameStacks.ElementAt(targetStackNum - 1).Receive(arriver);
        }
    }
}
