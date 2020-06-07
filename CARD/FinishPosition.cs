using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CARD
{
    public class FinishPosition
    {
        private double startTop;//第一堆的上边距
        private double startLeft;//第一堆左边距
        private int finshNuml;//已完成牌堆数量
        public FinishPosition(double top, double left)
        {
            startTop = top;
            startLeft = left;
            finshNuml = 0;
        }
        public FinishPosition(double top, double left, int fn)
        {
            startTop = top;
            startLeft = left;
            finshNuml = fn;
        }
        public double getNextPositionLeft()
        {
            return startLeft + 15 * finshNuml;
        }
        public double getTop()
        {
            return startTop;
        }
        public double getLeft()
        {
            return startLeft;
        }
        public int getFinshNum1()
        {
            return finshNuml;

        }
        public void updateFinishNum()
        {
            finshNuml++;
        }
        public void clear()
        {
            finshNuml = 0;
        }

    }
}
