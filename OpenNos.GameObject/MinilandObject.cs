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

namespace OpenNos.GameObject
{
    public class MinilandObject : MinilandObjectDTO
    {
        #region Members

        public ItemInstance ItemInstance;

        #endregion

        #region Methods

        public string GenerateMinilandEffect(bool removed)
        {
            return $"eff_g  {ItemInstance.Item.EffectValue} {MapX.ToString("00")}{MapY.ToString("00")} {MapX} {MapY} {(removed ? 1 : 0)}";
        }

        public string GenerateMinilandObject(bool deleted)
        {
            return $"mlobj {(deleted ? 0 : 1)} {ItemInstance.Slot} {MapX} {MapY} {ItemInstance.Item.Width} {ItemInstance.Item.Height} 0 {ItemInstance.DurabilityPoint} 0 {(ItemInstance.Item.IsMinilandObject ? 1 : 0)}";
        }

        #endregion
    }
}