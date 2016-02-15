using OpenNos.Domain;

namespace OpenNos.Data
{
    public class CharacterDTO
    {
        #region Properties

        public long AccountId { get; set; }

        public int Backpack { get; set; }

        public long CharacterId { get; set; }

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

        public short Compliment { get; set; }

        public bool ExchangeBlocked { get; set; }

        public bool FriendRequestBlocked { get; set; }

        public bool WhisperBlocked { get; set; }

        public bool FamilyRequestBlocked { get; set; }


        public bool GroupRequestBlocked { get; set; }

        public bool MouseAimLock { get; set; }

        public bool HeroChatBlocked { get; set; }

        public bool EmoticonsBlocked { get; set; }

        public bool QuickGetUp { get; set; }

        public bool HpBlocked { get; set; }

        public bool BuffBlocked { get; set; }

        public bool MinilandInviteBlocked { get; set; }

        public int Dignite { get; set; }

        public int Faction { get; set; }

        public byte Gender { get; set; }

        public long Gold { get; set; }

        public byte HairColor { get; set; }

        public byte HairStyle { get; set; }

        public int Hp { get; set; }

        public byte JobLevel { get; set; }

        public long JobLevelXp { get; set; }

        public byte Level { get; set; }

        public long LevelXp { get; set; }

        public short MapId { get; set; }

        public short MapX { get; set; }

        public short MapY { get; set; }

        public int Mp { get; set; }

        public string Name { get; set; }

        public long Reput { get; set; }

        public byte Slot { get; set; }

        public int SpAdditionPoint { get; set; }

        public int SpPoint { get; set; }

        public byte State { get; set; }

        public CharacterState StateEnum
        {
            get
            {
                return (CharacterState)State;
            }
            set
            {
                State = (byte)value;
            }
        }

        #endregion
    }
}