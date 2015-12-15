using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class CharacterDTO
    {
        public long CharacterId { get; set; }

        public long AccountId { get; set; }

        public string Name { get; set; }

        public byte Slot { get; set; }

        public byte Gender { get; set; }

        public byte Class { get; set; }

        public ClassType ClassEnum
        {
            get
            {
                return (ClassType)Class;
            }
            set
            {
                Class = (byte)value;
            }
        }

        public byte HairStyle { get; set; }

        public byte HairColor { get; set; }

        public short MapId { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        public int Hp { get; set; }

        public int Mp { get; set; }

        public long Gold { get; set; }

        public byte JobLevel { get; set; }

        public long JobLevelXp { get; set; }

        public byte Level { get; set; }

        public long LevelXp { get; set; }

        public int Reput { get; set; }

        public int Dignite { get; set; }
    }
}
