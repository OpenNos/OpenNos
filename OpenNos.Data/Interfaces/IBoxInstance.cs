namespace OpenNos.Data
{
    public interface IBoxInstance : ISpecialistInstance
    {
        #region Properties

        short HoldingVNum { get; set; }

        #endregion
    }
}