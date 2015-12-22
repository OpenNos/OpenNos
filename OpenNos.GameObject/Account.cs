using AutoMapper;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Account : AccountDTO
    {
        public Account()
        {
            Mapper.CreateMap<AccountDTO, Account>();
            Mapper.CreateMap<Account, AccountDTO>();

        }
    }

}