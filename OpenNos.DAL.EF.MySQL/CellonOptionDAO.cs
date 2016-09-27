/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using AutoMapper;
using OpenNos.DAL.EF.MySQL.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class CellonOptionDAO : ICellonOptionDAO
    {
        #region Members

        private IMapper _mapper;

        #endregion

        #region Instantiation

        public CellonOptionDAO()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CellonOption, CellonOptionDTO>();
                cfg.CreateMap<CellonOptionDTO, CellonOption>();
            });

            _mapper = config.CreateMapper();
        }

        #endregion

        #region Methods

        public IEnumerable<CellonOptionDTO> GetOptionsByWearableInstanceId(long wearableInstanceId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (CellonOption CellonOptionobject in context.CellonOption.Where(i => i.WearableInstanceId.Equals(wearableInstanceId)))
                {
                    yield return _mapper.Map<CellonOptionDTO>(CellonOptionobject);
                }
            }
        }

        #endregion
    }
}