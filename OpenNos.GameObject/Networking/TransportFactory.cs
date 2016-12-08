using System;
using System.Collections.Concurrent;

namespace OpenNos.GameObject
{
    public class TransportFactory
    {
        #region Members

        private static TransportFactory instance;
        private long _lastTransportId = 1;
        private Random _random;
        private ConcurrentBag<long> _transportIds;

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

            if (_lastTransportId > 999999)
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