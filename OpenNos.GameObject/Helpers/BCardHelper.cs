using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Helpers
{
    public class BCardHelper
    {
        #region Singleton

        private static BCardHelper _instance;

        public static BCardHelper Instance
        {
            get { return _instance ?? (_instance = new BCardHelper()); }
        }

        #endregion

        public int GetEffectByCardId(int CardId)
        {
            switch (CardId)
            {
                // Saignement // Brulure
                case 1:
                case 21:
                case 42:
                case 64:
                case 65:
                case 82:
                case 185:
                case 186:
                case 187:
                case 188:
                case 189:
                case 190:
                case 191:
                case 192:
                case 342:
                    return 6004;

                // Syncope
                case 7:
                case 66:
                case 100:
                case 195:
                case 196:
                case 197:
                case 198:
                    return 6003;

                //Gel
                case 27:
                case 135:
                case 199:
                case 200:
                case 201:
                case 202:
                case 372:
                    return 35;

                default:
                    return 0;
            }
        }
    }
}
