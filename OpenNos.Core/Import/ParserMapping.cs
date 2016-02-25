using System.Reflection;

namespace OpenNos.Core
{
    public class ParserMapping
    {
        #region Instantiation

        public ParserMapping(PropertyInfo property, int index)
        {
            Property = property;
            Index = index;
        }

        #endregion

        #region Properties

        public int Index { get; set; }

        public PropertyInfo Property { get; set; }

        #endregion
    }
}