using System;

namespace OpenNos.Data
{
    public abstract class SynchronizableBaseDTO : MappingBaseDTO
    {
        #region Instantiation

        public SynchronizableBaseDTO()
        {
            Id = Guid.NewGuid(); //make unique
        }

        #endregion

        #region Properties

        public Guid Id { get; set; }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            return ((SynchronizableBaseDTO)obj).Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}