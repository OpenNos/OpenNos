using System;
using System.Collections.Concurrent;

namespace OpenNos.GameObject
{
    public class TransportFactory
    {
        #region Members

        private static TransportFactory instance;
        private long _lastTransportId = 1;

        #endregion

        #region Instantiation

        private TransportFactory()
        {
        }

        #endregion

        #region Properties

        public static TransportFactory Instance => instance ?? (instance = new TransportFactory());

        #endregion

        #region Methods

        public long GenerateTransportId()
        {
            _lastTransportId++;

            if (_lastTransportId >= long.MaxValue)
            {
                _lastTransportId = 1;
            }

            return _lastTransportId;
        }
        #endregion
    }
}