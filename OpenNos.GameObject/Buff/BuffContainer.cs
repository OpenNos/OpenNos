using System;
using OpenNos.GameObject.Buff.Indicators;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;

namespace OpenNos.GameObject.Buff
{
    public class BuffContainer
    {
        private readonly ClientSession Session;
        private List<IndicatorBase> Indicators;
        public BuffContainer(ClientSession session)
        {
            Session = session;
            Indicators = new List<IndicatorBase>();
        }

        public void Add(IndicatorBase indicator)
        {
            foreach (IndicatorBase i in Indicators.Where(s => s.Id.Equals(indicator.Id)))
            {
                i.Disable(Session);
            }
            Indicators.Add(indicator);
            indicator.Enable(Session);
        }

        public int[] Get(BCard.Type type, BCard.SubType subType, bool pvp, bool affectingOpposite=false)
        {
            int value1 = 0;
            int value2 = 0;
            List<string> appliedBuffs = new List<string>();
            foreach (IndicatorBase buff in Indicators.Where(s => s.Start.AddMilliseconds(s.Duration * 100) > DateTime.Now && !s.Disabled))
            {
                List<BCardEntry> tmp = buff.DirectBuffs;
                if (buff.Delay != -1 && buff.Start.AddMilliseconds(buff.Delay * 100) < DateTime.Now)
                {
                    tmp.Concat(buff.DelayedBuffs);
                }
                if (!pvp)
                {
                    tmp = tmp.Where(s => !s.PVPOnly).ToList();
                }
                foreach (BCardEntry entry in tmp.Where(s => s.Type.Equals(type) && s.SubType.Equals(subType) && s.AffectingOpposite.Equals(affectingOpposite)))
                {
                    value1 += entry.Value1;
                    value2 += entry.Value2;
                }
            }


            //Not yet implemented, idek if I'll use this shit
            #region Equipment
            //switch (type)
            //{
            //    #region Damage
            //    case BCard.Type.Damage:
            //        switch (subType)
            //        {
            //            case BCard.SubType.Increase:

            //                break;
            //            case BCard.SubType.IncreaseMelee:

            //                break;
            //            case BCard.SubType.IncreaseDistance:

            //                break;
            //            case BCard.SubType.IncreaseMagic:

            //                break;
            //            case BCard.SubType.IncreaseLevel:

            //                break;
            //            case BCard.SubType.IncreasePercentage:

            //                break;
            //            case BCard.SubType.IncreaseMeleePercentage:

            //                break;
            //            case BCard.SubType.IncreaseDistancePercentage:

            //                break;
            //            case BCard.SubType.IncreaseMagicPercentage:

            //                break;
            //            case BCard.SubType.IncreasePercentageChance:

            //                break;
            //            case BCard.SubType.IncreaseMeleePercentageChance:

            //                break;
            //            case BCard.SubType.IncreaseDistancePercentageChance:

            //                break;
            //            case BCard.SubType.IncreaseMagicPercentageChance:

            //                break;
            //            default:
            //                Logger.Error(new NotImplementedException("BCard.SubType not implemented for this BCard.Type!"));
            //                break;
            //        }
            //        break;
            //    #endregion

            //    #region Defense
            //    case BCard.Type.Defense:
            //        switch (subType)
            //        {
            //            case BCard.SubType.Increase:

            //                break;
            //            case BCard.SubType.IncreaseMelee:

            //                break;
            //            case BCard.SubType.IncreaseDistance:

            //                break;
            //            case BCard.SubType.IncreaseMagic:

            //                break;
            //            case BCard.SubType.IncreaseLevel:

            //                break;
            //            case BCard.SubType.IncreasePercentage:

            //                break;
            //            case BCard.SubType.IncreaseMeleePercentage:

            //                break;
            //            case BCard.SubType.IncreaseDistancePercentage:

            //                break;
            //            case BCard.SubType.IncreaseMagicPercentage:

            //                break;
            //            case BCard.SubType.IncreasePercentageChance:

            //                break;
            //            case BCard.SubType.IncreaseMeleePercentageChance:

            //                break;
            //            case BCard.SubType.IncreaseDistancePercentageChance:

            //                break;
            //            case BCard.SubType.IncreaseMagicPercentageChance:

            //                break;
            //            default:
            //                Logger.Error(new NotImplementedException("BCard.SubType not implemented for this BCard.Type!"));
            //                break;
            //        }
            //        break;
            //    #endregion

            //    default:
            //        Logger.Error(new NotImplementedException("BCard.Type not implemented!"));
            //        break;
            //}
            #endregion

            return new int[] { value1, value2 };
        }
    }
}
