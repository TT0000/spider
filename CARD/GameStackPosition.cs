using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CARD
{
    public class GameStackPosition
    {
        double horizontalInterval;
        double cardWidth;
        double firstVI;
        double secondVI;
        public GameStackPosition(int h, int usv, double p, double sv)
        {
            horizontalInterval = h;
            cardWidth = p;
            firstVI = usv;
            secondVI = sv;
        }
        public double getMoveVertical()
        {
            return secondVI;
        }
        public int getStackNumber(double position)
        {
            double total = cardWidth + horizontalInterval;
            for (int i = 0; i < 11; i++)
            {
                if (cardWidth * (i - 1) + horizontalInterval * i < position && total * i > position)
                {
                    return (int)i;
                }
            }
            if (position < horizontalInterval)
            {
                return 1;
            }
            else
            {
                return 10;
            }
        }
        public double getNextUnShowedTop(int cardNum)
        {
            return firstVI * cardNum + 5;
        }
        public double getNextshowedTop(int showCardNum, int unShowNum)
        {
            return unShowNum * firstVI + showCardNum * secondVI + 5;
        }
        public double getStackLeft(int stackNum)
        {
            return horizontalInterval * stackNum + (stackNum - 1) * cardWidth;
        }
    }
}
