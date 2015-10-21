using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class AccountDTO
    {
        public long AccountId { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        public AuthorityType Authortiy { get; set; }

        public int LastSession { get; set; }

        public bool LoggedIn { get; set; }
    }
}
