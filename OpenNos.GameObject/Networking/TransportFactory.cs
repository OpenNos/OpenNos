using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class TransportFactory
    {
        #region Members

        private static TransportFactory instance;
        private List<long> _transportIds;
        private Random _random;

        #endregion

        #region Instantiation

        public TransportFactory()
        {
            _transportIds = new List<long>();
            _random = new Random();
        }

        #endregion

        #region Properties

        public static TransportFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TransportFactory();
                }
                return instance;
            }
        }

        #endregion

        #region Methods

        public long GenerateTransportId()
        {

            long newTransportId = _random.Next(1, Int32.MaxValue);
            while(_transportIds.Contains(newTransportId))
            {
                newTransportId = _random.Next(1, Int32.MaxValue);
            }

            _transportIds.Add(newTransportId);

            return newTransportId;
        }

        public bool RemoveTransportId(long id)
        {
            return _transportIds.Remove(id);
        }

        #endregion
    }
}