using System;

namespace OpenNos.Data
{
    public abstract class SynchronizeableBaseDTO
    {
        #region Instantiation

        public SynchronizeableBaseDTO()
        {
            Id = Guid.NewGuid(); //make unique
        }

        #endregion

        #region Properties

        public Guid Id { get; set; }

        #endregion
    }
}