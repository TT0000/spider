using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CARD 
{
    public class Card
    {
        private int id;//扑克牌大小
        private int type;//扑克牌花色
        public bool  hide;//扑克牌是否显示
        public int sequence;//扑克牌在牌堆中的位置
        public int stackNum;//标记扑克牌所属的堆
        public Card(int i, int t)
        {
            id = i;
            type = t;
            hide = true;
            sequence = 0;
        }
        public int getID()
        {
            return id;
        }
        public int getType()
        {
            return type;
        }
        public bool isHide()
        {
            return hide;
        }
    }
}
