using OpenNos.DAL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySQL = OpenNos.DAL.EF.MySQL;

namespace OpenNos.DAL
{
    public class DAOFactory
    {
        #region Members

        private static IAccountDAO _accountDAO;

        #endregion

        #region Properties

        public static IAccountDAO AccountDAO
        {
            get
            {
                if (_accountDAO == null)
                {
                    _accountDAO = new MySQL.AccountDAO();
                }

                return _accountDAO;
            }          
        }

        #endregion
    }
}
