using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Account
    {
        public long AccountId { get; set; }

        public string Name { get; set; }

        public byte Authority { get; set; }

        public string Password { get; set; }
    }
}
