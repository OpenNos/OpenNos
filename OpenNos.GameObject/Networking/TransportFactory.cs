using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class TransportFactory
    {
        #region Members

        private IList<long> _transportIds;
        private static TransportFactory instance;
        private Random _random;

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

        public long GenerateTransportId()
        {
            long newTransportId = _random.Next(1, 999999);

            while(_transportIds.Contains(newTransportId))
            {
                newTransportId = _random.Next(1, 999999);
            }

            _transportIds.Add(newTransportId);

            return newTransportId;
        }
    }
}