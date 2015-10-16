using OpenNos.DAL.EF.MySQL.DB;
using OpenNos.DAL.Interface;
using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.DAL.EF.MySQL
{
    public class AccountDAO : IAccountDAO
    {
        public bool CheckPasswordValiditiy(string name, string passwordHashed)
        {
            using (var context = DBHelper.CreateContext())
            {
                return context.Account.Any(a => a.Name.Equals(name) && a.Password.Equals(passwordHashed));
            }
        }

        public bool IsLoggedIn(string name)
        {
            using (var context = DBHelper.CreateContext())
            {
                return context.Account.Any(a => a.Name.Equals(name) && a.LoggedIn);
            }
        }

        public AuthorityType LoadAuthorityType(string name)
        {
            using (var context = DBHelper.CreateContext())
            {
                return (AuthorityType)context.Account.SingleOrDefault(a => a.Name.Equals(name)).Authority;
            }
        }

        public void LogIn(string name)
        {
            using (var context = DBHelper.CreateContext())
            {
                account account = context.Account.SingleOrDefault(a => a.Name.Equals(name));
                account.LoggedIn = true;
                account.LastConnect = DateTime.Now;
                context.SaveChanges();
            }
        }

        public void UpdateLastSession(string name, int session)
        {
            using (var context = DBHelper.CreateContext())
            {
                account account = context.Account.SingleOrDefault(a => a.Name.Equals(name));
                account.LastSession = session;
                context.SaveChanges();
            }
        }
    }
}
