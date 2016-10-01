namespace OpenNos.GameObject.Mock
{
    public class GameObjectMockHelper
    {
        #region Members

        private static GameObjectMockHelper instance;
        private long nextClientId = 0;

        #endregion

        #region Properties

        public static GameObjectMockHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameObjectMockHelper();
                }
                return instance;
            }
        }

        #endregion

        public long GetNextClientId()
        {
            nextClientId = nextClientId + 1;
            return nextClientId;
        }
    }
}