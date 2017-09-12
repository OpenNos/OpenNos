using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Helpers
{
    public class RewardsHelper
    {
        #region Singleton

        private static RewardsHelper _instance;

        public static RewardsHelper Instance
        {
            get { return _instance ?? (_instance = new RewardsHelper()); }
        }

        #endregion

        #region Methods

        public int ArenaXpReward(byte characterLevel)
        {
            if (characterLevel <= 39)
            {
                // 25%
                return (int) (CharacterHelper.Instance.XPData[characterLevel] / 4);
            }
            if (characterLevel <= 55)
            {
                // 20%
                return (int) (CharacterHelper.Instance.XPData[characterLevel] / 5);
            }
            if (characterLevel <= 75)
            {
                // 10%
                return (int) (CharacterHelper.Instance.XPData[characterLevel] / 10);
            }
            if (characterLevel <= 79)
            {
                // 5%
                return (int) (CharacterHelper.Instance.XPData[characterLevel] / 20);
            }
            if (characterLevel <= 85)
            {
                // 2%
                return (int) (CharacterHelper.Instance.XPData[characterLevel] / 50);
            }
            if (characterLevel <= 90)
            {
                return (int) (CharacterHelper.Instance.XPData[characterLevel] / 80);
            }
            if (characterLevel <= 93)
            {
                return (int) (CharacterHelper.Instance.XPData[characterLevel] / 100);
            }
            if (characterLevel <= 99)
            {
                return (int) (CharacterHelper.Instance.XPData[characterLevel] / 1000);
            }
            return 0;
        }
        #endregion
    }
}