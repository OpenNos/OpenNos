using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.DAL.Interface
{
    public interface IAccountDAO
    {
        #region Methods

        bool CheckPasswordValiditiy(string name, string passwordHashed);

        AuthorityType LoadAuthorityType(string name);

        void UpdateLastSession(string name, int session);

        #endregion
    }
}
