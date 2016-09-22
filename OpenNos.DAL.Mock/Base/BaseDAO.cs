using AutoMapper;
using System.Collections.Generic;

namespace OpenNos.DAL.Mock
{
    public abstract class BaseDAO<TDTO>
    {
        #region Members

        protected IMapper _mapper;

        #endregion

        #region Instantiation

        public BaseDAO()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TDTO, TDTO>();
                cfg.CreateMap<TDTO, TDTO>();
            });

            _mapper = config.CreateMapper();

            Container = new List<TDTO>();
        }

        #endregion

        #region Properties

        public IList<TDTO> Container { get; set; }

        #endregion

        #region Methods

        public void Insert(List<TDTO> dtos)
        {
            foreach (TDTO dto in dtos)
            {
                Insert(dto);
            }
        }

        public TDTO Insert(TDTO dto)
        {
            Container.Add(dto);
            return dto;
        }

        public IEnumerable<TDTO> LoadAll()
        {
            return Container;
        }

        #endregion
    }
}