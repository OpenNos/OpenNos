namespace OpenNos.Data
{
    public interface IWearableInstanceDTO : IItemInstanceDTO
    {
        #region Properties

        byte Ammo { get; set; }
        byte Cellon { get; set; }
        short CloseDefence { get; set; }
        short Concentrate { get; set; }
        short CriticalDodge { get; set; }
        byte CriticalLuckRate { get; set; }
        short CriticalRate { get; set; }
        short DamageMaximum { get; set; }
        short DamageMinimum { get; set; }
        byte DarkElement { get; set; }
        byte DarkResistance { get; set; }
        short DefenceDodge { get; set; }
        short DistanceDefence { get; set; }
        short DistanceDefenceDodge { get; set; }
        short ElementRate { get; set; }
        byte FireElement { get; set; }
        byte FireResistance { get; set; }
        short HitRate { get; set; }
        short HP { get; set; }
        bool IsEmpty { get; set; }
        bool IsFixed { get; set; }
        byte LightElement { get; set; }
        byte LightResistance { get; set; }
        short MagicDefence { get; set; }
        short MP { get; set; }
        byte WaterElement { get; set; }
        byte WaterResistance { get; set; }

        #endregion
    }
}