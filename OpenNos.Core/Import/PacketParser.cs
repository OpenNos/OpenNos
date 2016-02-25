using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenNos.Core
{
    public class PacketParser
    {
        #region Members

        private IList<ParserCondition> _conditions;

        #endregion

        #region Properties

        private IList<ParserCondition> Conditions
        {
            get
            {
                if (_conditions == null)
                {
                    _conditions = new List<ParserCondition>();
                }

                return _conditions;
            }
            set
            {
                _conditions = value;
            }
        }

        #endregion

        #region Methods

        public PacketParser Condition<TDTO>(int index, string value)
        {
            //create condition type
            ParserCondition condition = new ParserCondition(typeof(TDTO), index, value);
            Conditions.Add(condition);

            return this;
        }

        public IEnumerable<TDTO> Deserialize<TDTO>(string path)
        {
            IEnumerable<ParserCondition> accordingConditions = Conditions.Where(c => c.ConditionType.Equals(typeof(TDTO)));

            if (!accordingConditions.Any())
                throw new System.Exception($"No condition exists for given type {typeof(TDTO)}!");

            IList<TDTO> results = new List<TDTO>();

            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);

                foreach (string line in lines)
                {
                    string[] splittedLine = line.Split(' ');

                    ParserCondition conditionToUse = accordingConditions.FirstOrDefault(c => c.UseCondition(splittedLine));

                    if (conditionToUse != null)
                    {
                        TDTO newParsedObject = Activator.CreateInstance<TDTO>();

                        foreach (ParserMapping mapping in conditionToUse.Mappings)
                        {
                            if (splittedLine.Length >= mapping.Index)
                            {
                                mapping.Property.SetValue(newParsedObject, Convert.ChangeType(splittedLine[mapping.Index], mapping.Property.PropertyType));
                            }
                            else
                                throw new Exception($"The specified line contains less elements than the index for this mapping was set!");
                        }

                        results.Add(newParsedObject);
                    }
                }
            }

            return results;
        }

        public PacketParser Map(string propertyName, int index)
        {
            ParserCondition lastCondition = Conditions.LastOrDefault();

            if (lastCondition == null)
                throw new System.Exception("No mapping possible without having a condition!");

            PropertyInfo property = lastCondition.ConditionType.GetProperty(propertyName);

            if (property != null)
            {
                lastCondition.Mappings.Add(new ParserMapping(property, index));
            }
            else
                throw new Exception($"Property {propertyName} not found on type {lastCondition.ConditionType}!");

            return this;
        }

        #endregion
    }
}