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
using System.Data.Entity;
using System.Linq;
using OpenNos.Data.Enums;
using OpenNos.DAL.EF.DB;

namespace OpenNos.DAL.EF
{
    public class EquipmentOptionDAO : SynchronizableBaseDAO<EquipmentOption, EquipmentOptionDTO>, IEquipmentOptionDAO
    {
        #region Methods


        public SaveResult InsertOrUpdate(ref EquipmentOptionDTO equipmentOption)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {
                    Guid id = equipmentOption.Id;
                    EquipmentOption entity = context.EquipmentOption.FirstOrDefault(c => c.Id.Equals(id));

                    if (entity == null)
                    {
                        equipmentOption = Insert(equipmentOption, context);
                        return SaveResult.Inserted;
                    }
                    equipmentOption = Update(entity, equipmentOption, context);
                    return SaveResult.Updated;
                }
            }
            catch (Exception e)
            {
                return SaveResult.Error;
            }
        }

        public DeleteResult DeleteByWearableInstanceId(Guid wearableInstanceId)
        {
            try
            {
                using (OpenNosContext context = DataAccessHelper.CreateContext())
                {

                    foreach (EquipmentOption equipmentOption in context.EquipmentOption.Where(
                        i => i.WearableInstanceId.Equals(wearableInstanceId)))
                    {
                        if (equipmentOption != null)
                        {
                            context.EquipmentOption.Remove(equipmentOption);
                        }
                    }
                    context.SaveChanges();
                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                return DeleteResult.Error;
            }
        }


        protected override EquipmentOptionDTO Insert(EquipmentOptionDTO equipment, OpenNosContext context)
        {
            EquipmentOption entity = _mapper.Map<EquipmentOption>(equipment);
            context.EquipmentOption.Add(entity);
            context.SaveChanges();
            return _mapper.Map<EquipmentOptionDTO>(entity);
        }

        protected override EquipmentOptionDTO Update(EquipmentOption entity, EquipmentOptionDTO equipment,
            OpenNosContext context)
        {
            if (entity == null)
            {
                return _mapper.Map<EquipmentOptionDTO>(null);
            }
            entity.Level = equipment.Level;
            entity.Type = equipment.Type;
            entity.Value = equipment.Value;
            entity.WearableInstanceId = equipment.WearableInstanceId;
            context.Entry(entity).State = EntityState.Modified;
            context.SaveChanges();
            return _mapper.Map<EquipmentOptionDTO>(entity);
        }

        public IEnumerable<EquipmentOptionDTO> GetOptionsByWearableInstanceId(Guid wearableInstanceId)
        {
            using (OpenNosContext context = DataAccessHelper.CreateContext())
            {
                foreach (EquipmentOption cellonOptionobject in context.EquipmentOption.Where(i => i.WearableInstanceId.Equals(wearableInstanceId)))
                {
                    yield return _mapper.Map<EquipmentOptionDTO>(cellonOptionobject);
                }
            }
        }

        #endregion
    }
}