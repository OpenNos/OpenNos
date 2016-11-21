using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class TransportFactory
    {
        #region Members

        private static TransportFactory instance;
        private ConcurrentBag<long> _transportIds;
        private Random _random;
        private long _lastTransportId = 1;

        #endregion

        #region Instantiation

        public TransportFactory()
        {
            _transportIds = new ConcurrentBag<long>();
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

            _lastTransportId++;

            if(_lastTransportId > 999999)
            {
                _lastTransportId = 1;
            }

            _transportIds.Add(_lastTransportId);

            return _lastTransportId;
        }

        public bool RemoveTransportId(long id)
        {
            return _transportIds.TryTake(out id);
        }

        #endregion
    }
}