using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CARD
{
   public class CardStack
    {
        private int newestCardId;//某一列中最前的扑克牌下标
        public List<Card> cards;//记录游戏中的一列里面的扑克牌
        private int stackId;//堆标记
        private int unshowedNum;//这一堆牌里尚未显示的牌的数量——计算坐标时将使用到该变量
        public CardStack(int id)
        {
            newestCardId = -1;
            stackId = id;
            cards = new List<Card>();
        }
        public List<Card> getAllcard()
        {
            return cards;
        }
        public int getStackId()
        {
            return stackId;
        }
        public void setStackId(int id)
        {
            stackId = id;
        }
        public int getcurrentNum()
        {
            return cards.Count;
        }
        public bool canReceive(Card arriver)
        {
            if (newestCardId < 0)
            {
                return true;
            }
            return cards.ElementAt(newestCardId).getID() - arriver.getID() == 1;
        }
        public void Receive(List<Card> arriver)
        {
            cards.AddRange(arriver);
            int count = cards.Count;
            for (int i = newestCardId + 1; i < count; i++)
            {
                cards.ElementAt(i).sequence = (int)(i + 1);
                cards.ElementAt(i).stackNum = stackId;
            }
            newestCardId = (int)(count - 1);
        }
        public bool canMove(Card leaver)
        {
            if (leaver.Equals(cards.ElementAt(newestCardId)))
            {
                return true;
            }
            else
            {
                if (leaver.hide)
                {
                    return false;
                }
                else
                {
                    for (int i = leaver.sequence - 1; i < newestCardId; i++)//检查牌堆是否连续
                    {
                        if ((cards.ElementAt(i).getID() - cards.ElementAt(i + 1).getID()) != 1 || cards.ElementAt(i).getType() != cards.ElementAt(i + 1).getType())//类型不同或者不连续都不能移动
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        public void move(int start,int count)
        {
            cards.RemoveRange(start, count);
            newestCardId -= (int)count;
        }
        public int getUnshowedNum()
        {
            return unshowedNum;
        }
        public void updateUnshowedNum()
        {
            unshowedNum--;
        }
        public void addCard(Card card, bool isHide)
        {
            addCard(card);
            card.hide = isHide;
            if (isHide)
            {
                unshowedNum++;
            }
        }
        private void addCard(Card card)
        {
            cards.Add(card);
            card.sequence = (int)cards.Count;
            card.stackNum = stackId;
            newestCardId = (int)(cards.Count - 1);
        }
        public List<Card> GetCards(int start,int end)
        {
            List<Card> mover = new List<Card>(end - start + 1);
            for (int i = start - 1; i < end; i++)
            {
                mover.Add(cards.ElementAt(i));
            }
            mover.TrimExcess();
            return mover;
        }
    }
    
}
