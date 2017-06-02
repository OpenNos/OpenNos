namespace OpenNos.PathFinder
{
    public class GridPos
    {
        #region Properties    

        public byte Value { get; set; }

        public short X { get; set; }

        public short Y { get; set; }

        #endregion

        #region Methods

        public bool IsWalkable()
        {
            return (Value == 0 || Value == 2 || Value >= 16 && Value <= 19);
        }

        #endregion
    }
}