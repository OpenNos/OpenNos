namespace OpenNos.GameObject.Mock
{
    public class GameObjectMockHelper
    {
        #region Members

        private static GameObjectMockHelper instance;
        private long nextClientId;

        #endregion

        #region Properties

        public static GameObjectMockHelper Instance
        {
            get
            {
                return instance ?? (instance = new GameObjectMockHelper());
            }
        }

        #endregion

        #region Methods

        public long GetNextClientId()
        {
            nextClientId = nextClientId + 1;
            return nextClientId;
        }

        #endregion
    }
}