using System;

namespace EpPathFinding
{
    public class GridPos : SettlersEngine.IPathNode<Object>
    {
        public short x { get; set; }
        public short y { get; set; }
        public byte Value { get; set; }

        public bool IsWalkable(Object unused)
        {
            return (Value == 0 || Value == 2 || Value >= 16 && Value <= 19);
        }
    }
}