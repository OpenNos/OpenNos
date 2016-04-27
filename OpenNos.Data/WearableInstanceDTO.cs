namespace OpenNos.Data
{
    public class WearableInstanceDTO : ItemInstanceDTO, IWearableInstance
    {
        #region Properties

        public byte Ammo { get; set; }
        public byte Cellon { get; set; }
        public short CloseDefence { get; set; }
        public short Concentrate { get; set; }
        public short CriticalDodge { get; set; }
        public byte CriticalLuckRate { get; set; }
        public short CriticalRate { get; set; }
        public short DamageMaximum { get; set; }
        public short DamageMinimum { get; set; }
        public byte DarkElement { get; set; }
        public byte DarkResistance { get; set; }
        public short DefenceDodge { get; set; }
        public short Design { get; set; }
        public short DistanceDefence { get; set; }
        public short DistanceDefenceDodge { get; set; }
        public short ElementRate { get; set; }
        public byte FireElement { get; set; }
        public byte FireResistance { get; set; }
        public short HitRate { get; set; }
        public bool IsEmpty { get; set; }
        public bool IsFixed { get; set; }
        public byte LightElement { get; set; }
        public byte LightResistance { get; set; }
        public short MagicDefence { get; set; }
        public byte WaterElement { get; set; }
        public byte WaterResistance { get; set; }
        public short HP { get; set; }
        public short MP { get; set; }

        #endregion
    }
}