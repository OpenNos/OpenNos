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

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject.Buff.Indicators
{
    public class IndicatorBase
    {
        #region Members

        public const int _buffLevel = 1;
        public int _level;
        public const bool BadBuff = false;
        public int Delay = -1;
        public readonly List<BCardEntry> DelayedBuffs = new List<BCardEntry>();
        public readonly List<BCardEntry> DirectBuffs = new List<BCardEntry>();
        public int Duration;
        public int Id;
        public int Interval = -1;
        public string Name;
        public DateTime Start = DateTime.Now;
        public const bool StaticBuff = false;

        #endregion

        #region Properties

        public bool Disabled { get; private set; }

        #endregion

        #region Methods

        public virtual void Disable(ClientSession session)
        {
            if (!Disabled && session != null && session.HasSelectedCharacter)
            {
                if (StaticBuff) // always true for now !ignore!
                {
                    session.SendPacket($"vb {Id} 0 {Duration}");
                    session.SendPacket(session.Character.GenerateSay($"You are no longer under the effect {Name}.", 11));
                }
                else
                {
                    session.SendPacket($"bf 1 {session.Character.CharacterId} 0.{Id}.0 {_level}");
                    session.SendPacket(session.Character.GenerateSay($"You are no longer under the effect {Name}.", 20));
                }
                Disabled = true;
                if (DirectBuffs.Concat(DelayedBuffs).Any(s => s.Type == BCard.Type.Speed))
                {
                    session.Character.LastSpeedChange = DateTime.Now;
                    session.SendPacket(session.Character.GenerateCond());
                }
            }
        }

        public virtual void Enable(ClientSession session)
        {
            if (StaticBuff) // always true for now !ignore!
            {
                session.SendPacket($"vb {Id} 1 {Duration}");
                session.SendPacket(session.Character.GenerateSay($"You are under the effect {Name}.", 12));
            }
            else
            {
                session.SendPacket($"bf 1 {session.Character.CharacterId} 0.{Id}.{Duration} {_level}");
                session.SendPacket(session.Character.GenerateSay($"You are under the effect {Name}.", 20));
            }
            if (DirectBuffs.Any(s => s.Type == BCard.Type.Speed))
            {
                session.Character.LastSpeedChange = DateTime.Now;
                session.SendPacket(session.Character.GenerateCond());
            }
            if (Delay != -1)
            {
                System.Reactive.Linq.Observable.Timer(TimeSpan.FromMilliseconds(Duration * 100))
           .Subscribe(
           o =>
           {
               if (DelayedBuffs.Any(s => s.Type == BCard.Type.Speed))
               {
                   if (!Disabled && session.HasSelectedCharacter)
                   {
                       session.Character.LastSpeedChange = DateTime.Now;
                       session.SendPacket(session.Character.GenerateCond());
                   }
               }
           });
            }
            System.Reactive.Linq.Observable.Timer(TimeSpan.FromMilliseconds(Duration * 100))
                       .Subscribe(
                       o =>
                       {
                           Disable(session);
                       });
        }

        #endregion
    }
}