using OpenNos.Domain;
using System;

namespace OpenNos.Data
{
    public class AccountDTO
    {
        #region Properties

        public long AccountId { get; set; }

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

        public DateTime LastCompliment { get; set; }

        public DateTime LastLogin { get; set; }

        public int LastSession { get; set; }

        public bool LoggedIn { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }

        #endregion
    }
}