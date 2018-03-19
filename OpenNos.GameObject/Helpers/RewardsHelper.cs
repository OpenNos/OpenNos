using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;

namespace OpenNos.GameObject.Helpers
{
    public class RewardsHelper : Singleton<RewardsHelper>
    {
        #region Methods

        public int ArenaXpReward(byte characterLevel)
        {
            if (characterLevel <= 39)
            {
                // 25%
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 4);
            }
            if (characterLevel <= 55)
            {
                // 20%
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 5);
            }
            if (characterLevel <= 75)
            {
                // 10%
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 10);
            }
            if (characterLevel <= 79)
            {
                // 5%
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 20);
            }
            if (characterLevel <= 85)
            {
                // 2%
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 50);
            }
            if (characterLevel <= 90)
            {
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 80);
            }
            if (characterLevel <= 93)
            {
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 100);
            }
            if (characterLevel <= 99)
            {
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 1000);
            }
            return 0;
        }
        #endregion
    }
}