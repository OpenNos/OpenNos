using System;

namespace OpenNos.Pathfinding
{
    public class GridPos
    {
        public byte Value { get; set; }
        public short X { get; set; }
        public short Y { get; set; }

        public bool IsWalkable()
        {
            return (Value == 0 || Value == 2 || Value >= 16 && Value <= 19);
        }
    }
}