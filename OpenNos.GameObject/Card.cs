using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Card : CardDTO
    {
        public bool BadBuff { get; set; }

        public Card()
        {

        }

        #region Methods

        public override void Initialize()
        {
            // no custom stuff done for Card
        }

        #endregion
    }
}
