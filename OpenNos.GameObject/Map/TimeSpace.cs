using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.DAL.EF;
using System;

namespace OpenNos.GameObject
{
    public class TimeSpace : TimeSpaceDTO
    {
        public Guid MapInstanceId { get;  set; }
        public TimeSpaceType Type { get; set; }

        public Guid SourceMapInstanceId
        {
            get
            {
                if (MapInstanceId == default(Guid))
                {
                    MapInstanceId = ServerManager.Instance.GetBaseMapInstanceIdByMapId(MapId);
                }
                return MapInstanceId;
            }
            set { MapInstanceId = value; }
        }

    }
}