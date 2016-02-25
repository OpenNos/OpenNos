using System;
using System.Collections.Generic;

namespace OpenNos.Core
{
    public class ParserCondition
    {
        #region Members

        private IList<ParserMapping> _mappings;

        #endregion

        #region Instantiation

        public ParserCondition(Type conditionType, int index, string value)
        {
            ConditionType = conditionType;
            Index = index;
            Value = value;
        }

        #endregion

        #region Properties

        public Type ConditionType { get; set; }

        public int Index { get; set; }

        public IList<ParserMapping> Mappings
        {
            get
            {
                if (_mappings == null)
                {
                    _mappings = new List<ParserMapping>();
                }

                return _mappings;
            }
            set
            {
                _mappings = value;
            }
        }

        public string Value { get; set; }

        #endregion

        #region Methods

        public bool UseCondition(string[] compares)
        {
            if (compares != null && compares.Length >= Index)
            {
                string value = compares[Index];

                if (value == Value)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}