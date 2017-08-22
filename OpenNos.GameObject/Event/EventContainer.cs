using OpenNos.Domain;
using System;

namespace OpenNos.GameObject
{
    public class EventContainer
    {
        #region Instantiation

        public EventContainer(MapInstance mapInstance, EventActionType eventActionType, object param)
        {
            MapInstance = mapInstance;
            EventActionType = eventActionType;
            Parameter = param;
        }

        #endregion

        #region Properties

        public EventActionType EventActionType { get; set; }

        public MapInstance MapInstance { get; set; }

        public object Parameter { get; set; }

        #endregion
    }
}