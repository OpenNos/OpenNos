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

        public byte Authority { get; set; }

        public AuthorityType AuthorityEnum
        {
            get
            {
                return (AuthorityType)Authority;
            }
            set
            {
                Authority = (byte)value;
            }
        }

        public int LastSession { get; set; }

        public bool LoggedIn { get; set; }
    }
}
