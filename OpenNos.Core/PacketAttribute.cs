using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class Packet : System.Attribute
    {
        private string _header;

        public Packet(string header)
        {
            this._header = header;
        }

        #region Properties

        public string Header
        {
            get
            {
                return _header;
            }
        }

        #endregion
    }
}
