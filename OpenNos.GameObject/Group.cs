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

using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class Group
    {
        public int GroupId { get; set; }
        public List<long> Characters { get; set; }
        public byte SharingMode { get; set; }

        public Group()
        {
            Characters = new List<long>();
        }
        public List<string> GeneratePst()
        {
            int i = 0;
            List<string> str = new List<string>();
            foreach (long id in Characters)
            {
                str.Add($"pst 1 {ClientLinkManager.Instance.GetProperty<long>(id, "CharacterId")} {++i} { ClientLinkManager.Instance.GetProperty<int>(id, "Hp") / ClientLinkManager.Instance.GetUserMethod<double>(id, "HPLoad") * 100 } {(int)(ClientLinkManager.Instance.GetProperty<int>(id, "Mp") / ClientLinkManager.Instance.GetUserMethod<double>(id, "MPLoad") * 100) } 0 0 {ClientLinkManager.Instance.GetProperty<byte>(id, "Class")} {ClientLinkManager.Instance.GetProperty<byte>(id, "Gender")} {(ClientLinkManager.Instance.GetProperty<bool>(id, "UseSp") ? ClientLinkManager.Instance.GetProperty<int>(id, "Morph") : 0)}");
            }
            return str;
        }
    }
}