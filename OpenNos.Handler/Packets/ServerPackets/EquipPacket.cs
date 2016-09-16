using OpenNos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class EquipPacket : PacketBase
    {
        [Index(0)]
        public byte WeaponUpgrade { get; set; }

        [Index(1)]
        public byte ArmorUpgrade { get; set; }

        [Index(2)]
        public List<EquipSubPacket> EquipEntries { get; set; }
    }

    public class EquipSubPacket : PacketBase
    {
        [Index(0)]
        public byte Index { get; set; }

        [Index(1)]
        public int ItemVNum { get; set; }

        [Index(2)]
        public byte Rare { get; set; }

        [Index(3)]
        public byte Upgrade { get; set; }
    }
}
