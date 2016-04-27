namespace OpenNos.DAL.EF.MySQL
{
    using Domain;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Account
    {
        #region Instantiation

        public Account()
        {
            Character = new HashSet<Character>();
            GeneralLog = new HashSet<GeneralLog>();
        }

        #endregion

        #region Properties

        public long AccountId { get; set; }

        public AuthorityType Authority { get; set; }

        public virtual ICollection<Character> Character { get; set; }

        public virtual ICollection<GeneralLog> GeneralLog { get; set; }

        public DateTime LastCompliment { get; set; }

        public int LastSession { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Password { get; set; }

        #endregion
    }
}