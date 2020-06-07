using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CARD
{
    public class CardGroup//一组牌
    {
        public List<Card>cards;
            public CardGroup()
        {
            cards = new List<Card>(10);
        }
    }
}
