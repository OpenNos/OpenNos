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

using OpenNos.GameObject.Helpers;
using System.Collections.Generic;

namespace OpenNos.GameObject
{
    public class MapButton
    {
        #region Instantiation

        public MapButton(int id, short positionX, short positionY, short enabledVNum, short disabledVNum, List<EventContainer> disableEvents, List<EventContainer> enableEvents, List<EventContainer> firstEnableEvents)
        {
            MapButtonId = id;
            PositionX = positionX;
            PositionY = positionY;
            EnabledVNum = enabledVNum;
            DisabledVNum = disabledVNum;
            DisableEvents = disableEvents;
            EnableEvents = enableEvents;
            FirstEnableEvents = firstEnableEvents;
        }

        #endregion

        #region Properties

        public short DisabledVNum { get; set; }

        public List<EventContainer> DisableEvents { get; set; }

        public short EnabledVNum { get; set; }

        public List<EventContainer> EnableEvents { get; set; }

        public List<EventContainer> FirstEnableEvents { get; set; }

        public int MapButtonId { get; set; }

        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public bool State { get; set; }

        #endregion

        #region Methods

        public string GenerateIn()
        {
            return $"in 9 {(State ? EnabledVNum : DisabledVNum)} {MapButtonId} {PositionX} {PositionY} 1 0 0 0";
        }

        public string GenerateOut()
        {
            return $"out 9 {MapButtonId}";
        }

        public void RunAction()
        {
            State = !State;
            if (State)
            {
                EnableEvents.ForEach(e => EventHelper.Instance.RunEvent(e));
                FirstEnableEvents.ForEach(e => EventHelper.Instance.RunEvent(e));
                FirstEnableEvents.RemoveAll(s => s != null);
            }
            else
            {
                DisableEvents.ForEach(e => EventHelper.Instance.RunEvent(e));
            }
        }

        #endregion
    }
}