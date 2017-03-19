namespace OpenNos.GameObject
{
    public class TransportFactory
    {
        #region Members

        private static TransportFactory instance;
        private long _lastTransportId = 100000;

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
                _lastTransportId = 0;
            }

            return _lastTransportId;
        }

        #endregion
    }
}