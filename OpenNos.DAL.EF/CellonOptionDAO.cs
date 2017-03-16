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

using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.EF
{
    public class CellonOptionDAO : SynchronizableBaseDAO<CellonOption, CellonOptionDTO>, ICellonOptionDAO
    {
        #region Methods

        public IEnumerable<CellonOptionDTO> GetOptionsByWearableInstanceId(Guid wearableInstanceId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                foreach (CellonOption cellonOptionobject in context.CellonOption.Where(i => i.WearableInstanceId.Equals(wearableInstanceId)))
                {
                    yield return _mapper.Map<CellonOptionDTO>(cellonOptionobject);
                }
            }
        }

        #endregion
    }
}