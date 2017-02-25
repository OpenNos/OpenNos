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

namespace OpenNos.Domain
{
    public enum InventoryType : byte
    {
        Equipment = 0,
        Main = 1,
        Etc = 2,
        Miniland = 3,
        Specialist = 6,
        Costume = 7,
        Wear = 8,
        Bazaar = 9,
        Warehouse = 10,
        FamilyWareHouse = 11,
        PetWarehouse = 12,
        FirstPartnerInventory = 13,
        SecondPartnerInventory = 14,
        ThirdPartnerInventory = 15,
    }
}