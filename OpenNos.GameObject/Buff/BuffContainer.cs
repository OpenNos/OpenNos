/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.GameObject.Buff.BCard;
using OpenNos.GameObject.Buff.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using Type = OpenNos.GameObject.Buff.BCard.Type;

namespace OpenNos.GameObject.Buff
{
    public class BuffContainer
    {
        #region Members

        private readonly List<IndicatorBase> Indicators;
        private readonly ClientSession Session;

        #endregion

        #region Instantiation

        public BuffContainer(ClientSession session)
        {
            Session = session;
            Indicators = new List<IndicatorBase>();
        }

        #endregion

        #region Methods

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
                Indicators.Add(indicator);
            }
            indicator.Enable(Session);
        }

        public void Clear()
        {
            lock (Indicators)
            {
                IndicatorBase[] items = new IndicatorBase[Indicators.Count];
                Indicators.CopyTo(items);
                foreach (IndicatorBase i in items.Where(s => !IndicatorBase.StaticBuff && !s.Disabled).ToList())
                {
                    i.Disable(Session);
                }
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
                    foreach (IndicatorBase i in items.Where(s => !IndicatorBase.BadBuff && IndicatorBase._buffLevel < level).ToList())
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
                    foreach (IndicatorBase i in items.Where(s => IndicatorBase.BadBuff && IndicatorBase._buffLevel < level).ToList())
                    {
                        i.Disable(Session);
                    }
                }
            }
        }

        public int[] Get(Type type, SubType subType, bool pvp, bool affectingOpposite = false)
        {
            int value1 = 0;
            int value2 = 0;
            lock (Indicators)
            {
                IndicatorBase[] items = new IndicatorBase[Indicators.Count + 5];
                Indicators.CopyTo(items);
                foreach (IndicatorBase buff in items.Where(s => s != null && s.Start.AddMilliseconds(s.Duration * 100) > DateTime.Now && !s.Disabled))
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

            // Not yet implemented

            #region Equipment

            //switch (type)
            //{
            //    #region Damage
            //    case BCard.Type.Damage:
            //        switch (subType)
            //        {
            //            case BCard.SubType.Increase:

            // break; case BCard.SubType.IncreaseMelee:

            // break; case BCard.SubType.IncreaseDistance:

            // break; case BCard.SubType.IncreaseMagic:

            // break; case BCard.SubType.IncreaseLevel:

            // break; case BCard.SubType.IncreasePercentage:

            // break; case BCard.SubType.IncreaseMeleePercentage:

            // break; case BCard.SubType.IncreaseDistancePercentage:

            // break; case BCard.SubType.IncreaseMagicPercentage:

            // break; case BCard.SubType.IncreasePercentageChance:

            // break; case BCard.SubType.IncreaseMeleePercentageChance:

            // break; case BCard.SubType.IncreaseDistancePercentageChance:

            // break; case BCard.SubType.IncreaseMagicPercentageChance:

            // break; default: Logger.Error(new NotImplementedException("BCard.SubType not
            // implemented for this BCard.Type!")); break; } break; #endregion

            // #region Defense case BCard.Type.Defense: switch (subType) { case BCard.SubType.Increase:

            // break; case BCard.SubType.IncreaseMelee:

            // break; case BCard.SubType.IncreaseDistance:

            // break; case BCard.SubType.IncreaseMagic:

            // break; case BCard.SubType.IncreaseLevel:

            // break; case BCard.SubType.IncreasePercentage:

            // break; case BCard.SubType.IncreaseMeleePercentage:

            // break; case BCard.SubType.IncreaseDistancePercentage:

            // break; case BCard.SubType.IncreaseMagicPercentage:

            // break; case BCard.SubType.IncreasePercentageChance:

            // break; case BCard.SubType.IncreaseMeleePercentageChance:

            // break; case BCard.SubType.IncreaseDistancePercentageChance:

            // break; case BCard.SubType.IncreaseMagicPercentageChance:

            // break; default: Logger.Error(new NotImplementedException("BCard.SubType not
            // implemented for this BCard.Type!")); break; } break; #endregion

            //    default:
            //        Logger.Error(new NotImplementedException("BCard.Type not implemented!"));
            //        break;
            //}

            #endregion

            return new[] { value1, value2 };
        }

        public string GetAllActiveBuffs()
        {
            var str = string.Empty;
            lock (Indicators)
            {
                IndicatorBase[] items = new IndicatorBase[Indicators.Count + 5];
                Indicators.CopyTo(items);
                str = items.Where(s => s != null && s.Start.AddMilliseconds(s.Duration * 100) > DateTime.Now && !s.Disabled).Aggregate(str, (current, buff) => current + $" {buff.Id}");
            }
            return str;
        }

        #endregion
    }
}