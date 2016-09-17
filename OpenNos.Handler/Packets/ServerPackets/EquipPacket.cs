using OpenNos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    [Header("equip")]
    public class EquipPacket : PacketBase
    {
        [PacketIndex(0)]
        public byte WeaponArmourUpgrade { get; set; }

        [PacketIndex(1)]
        public byte Design { get; set; }

        [PacketIndex(2)]
        public List<EquipSubPacket> EquipEntries { get; set; }
    }

    public class EquipSubPacket : PacketBase
    {
        [PacketIndex(0)]
        public byte Index { get; set; }

        [PacketIndex(1)]
        public int ItemVNum { get; set; }

        [PacketIndex(2)]
        public byte Rare { get; set; }

        [PacketIndex(3)]
        public byte Upgrade { get; set; }
    }
}
