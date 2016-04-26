using System;

namespace OpenNos.Data
{
    public interface IItemInstanceDTO
    {
        #region Properties

        int Amount { get; set; }
        byte Design { get; set; }
        bool IsUsed { get; set; }
        DateTime? ItemDeleteTime { get; set; }
        long ItemInstanceId { get; set; }
        short ItemVNum { get; set; }
        byte Rare { get; set; }
        byte Upgrade { get; set; }

        #endregion
    }
}