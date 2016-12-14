using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using System;

namespace OpenNos.GameObject
{
    public class BoxInstance : SpecialistInstance, IBoxInstance
    {
        #region Members

        private Random _random;

        #endregion

        #region Instantiation

        public BoxInstance()
        {
            _random = new Random();
        }

        public BoxInstance(Guid id)
        {
            Id = id;
            _random = new Random();
        }

        #endregion

        #region Properties

        public short HoldingVNum { get; set; }

        #endregion

    }
}
