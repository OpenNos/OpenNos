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

        public void Clear()
        {
            IndicatorBase[] items = new IndicatorBase[Indicators.Count];
            Indicators.CopyTo(items);
            foreach (IndicatorBase i in items.Where(s => !s.StaticBuff).ToList())
            {
                i.Disable(Session);
            }
        }

        public void DisableEffects(bool good, bool bad, int level)
        {
            if (good)
            {
                lock (Indicators)
                {
                    IndicatorBase[] items = new IndicatorBase[Indicators.Count];
                    Indicators.CopyTo(items);
                    foreach (IndicatorBase i in items.Where(s => !s.BadBuff && s._buffLevel < level).ToList())
                    {
                        i.Disable(Session);
                    }
                }
            }
            if (bad)
            {
                lock (Indicators)
                {
                    IndicatorBase[] items = new IndicatorBase[Indicators.Count];
                    Indicators.CopyTo(items);
                    foreach (IndicatorBase i in items.Where(s => s.BadBuff && s._buffLevel < level).ToList())
                    {
                        i.Disable(Session);
                    }
                }
            }
        }

        public void Add(IndicatorBase indicator)
        {
            lock (Indicators)
            {
                IndicatorBase[] items = new IndicatorBase[Indicators.Count];
                Indicators.CopyTo(items);

                foreach (IndicatorBase i in items.Where(s => s.Id.Equals(indicator.Id) && !s.Disabled))
                {
                    i.Disable(Session);
                }
            }
            Indicators.Add(indicator);
            indicator.Enable(Session);
        }

        public int[] Get(BCard.Type type, BCard.SubType subType, bool pvp, bool affectingOpposite = false)
        {
            int value1 = 0;
            int value2 = 0;
            List<string> appliedBuffs = new List<string>();
            lock (Indicators)
            {
                IndicatorBase[] items = new IndicatorBase[Indicators.Count];
                Indicators.CopyTo(items);
                foreach (IndicatorBase buff in items.Where(s => s.Start.AddMilliseconds(s.Duration * 100) > DateTime.Now && !s.Disabled))
                {
                    List<BCardEntry> tmp = buff.DirectBuffs;
                    if (buff.Delay != -1 && buff.Start.AddMilliseconds(buff.Delay * 100) < DateTime.Now)
                    {
                        tmp = tmp.Concat(buff.DelayedBuffs).ToList();
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
