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