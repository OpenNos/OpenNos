using System;

namespace OpenNos.Core
{
    public class ImportAttribute : Attribute
    {
        #region Instantiation

        public ImportAttribute(string line, int index)
        {
            Line = line;
            Index = index;
        }

        #endregion

        #region Properties

        public int Index { get; set; }

        public string Line { get; set; }

        public override string ToString()
        {
            return $"Line: {Line}, Index: {Index}";
        }

        #endregion
    }
}