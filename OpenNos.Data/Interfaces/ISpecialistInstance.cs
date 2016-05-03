namespace OpenNos.Data
{
    public interface ISpecialistInstance : IWearableInstance
    {
        #region Properties

        short SlDamage { get; set; }
        short SlDefence { get; set; }
        short SlElement { get; set; }
        short SlHP { get; set; }
        byte SpDamage { get; set; }
        byte SpDark { get; set; }
        byte SpDefence { get; set; }
        byte SpElement { get; set; }
        byte SpFire { get; set; }
        byte SpHP { get; set; }
        byte SpLevel { get; set; }
        byte SpLight { get; set; }
        byte SpStoneUpgrade { get; set; }
        byte SpWater { get; set; }
        long SpXp { get; set; }

        #endregion
    }
}