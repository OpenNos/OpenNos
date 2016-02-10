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
using OpenNos.DAL.Interface;
using OpenNos.Data;
using System;
using System.Linq;

namespace OpenNos.DAL.EF.MySQL
{
    public class ConfigDAO : IConfigDAO
    {
        #region Methods

        public ConfigDTO GetOption(short CharacterId, short configId)
        {
            using (var context = DataAccessHelper.CreateContext())
            {
                return Mapper.Map<ConfigDTO>(context.config.SingleOrDefault(c => c.ConfigId.Equals(configId)));
            }
        }

        public ConfigDTO SetOption(short CharacterId, short configId, short Value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}