using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CARD
{
    public class UnfinishPosition
    {
        public double top;
        public double left;
        int number;

        public UnfinishPosition(double _top, double _left)
        {
            top = _top;
            left = _left;
            number = 5;
        }
        public double getTop()
        {
            return top;
        }
        public double getLeft()
        {
            return left + (5 - number) * 15;
        }
        public void updateNum()
        {
            number--;
        }
    }

}
