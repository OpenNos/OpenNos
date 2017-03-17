using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject
{
    public class MapButton
    {
        public bool State { get; set; }
        public MapInstance MapInstance { get; set; }
        public int MapButtonId { get; set; }
        public short PositionX { get; set; }
        public short PositionY { get; set; }
        public short EnabledVNum { get; set; }
        public short DisabledVNum { get; set; }
        public List<EventContainer> FirstEnableEvents { get; set; }
        public List<EventContainer> DisableEvents { get; set; }
        public List<EventContainer> EnableEvents { get; set; }

        public MapButton(MapInstance mapInstance, int id, short positionX, short positionY, short enabledVNum, short disabledVNum, List<EventContainer> disableEvents, List<EventContainer> enableEvents, List<EventContainer> firstEnableEvents)
        {
            MapInstance = mapInstance;
            MapButtonId = id;
            PositionX = positionX;
            PositionY = positionY;
            EnabledVNum = enabledVNum;
            DisabledVNum = disabledVNum;
            DisableEvents = disableEvents;
            EnableEvents = enableEvents;
            FirstEnableEvents = firstEnableEvents;
        }

        public void RunAction()
        {
            MapInstance.Broadcast(GenerateOut());
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

            MapInstance.Broadcast(GenerateIn());
        }
        public string GenerateOut()
        {
            return $"out 9 {MapButtonId}";
        }
        public string GenerateIn()
        {
            return $"in 9 {(State ? EnabledVNum: DisabledVNum)} {MapButtonId} {PositionX} {PositionY} 1 0 0 0";
        }
    }
}
