using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class TransportFactory
    {
        #region Members

        private static TransportFactory instance;
        private IList<long> _transportIds;
        private long lastTransportId = 1;

        #endregion

        #region Instantiation

        public TransportFactory()
        {
            _transportIds = new List<long>();
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
            lastTransportId = lastTransportId + 1;

            if (lastTransportId > 9999999)
            {
                lastTransportId = 1;
            }
            _transportIds.Add(lastTransportId);

            return lastTransportId;
        }

        #endregion
    }
}