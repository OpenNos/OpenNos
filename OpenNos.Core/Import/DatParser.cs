using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OpenNos.Core
{
    public static class DatParser
    {
        #region Members

        private const string datEntryOffset = "END";

        #endregion

        #region Methods

        public static IEnumerable<TDTO> Parse<TDTO>(string filePath)
        {
            IList<TDTO> parsedResults = new List<TDTO>();

            string[] plainEntries = Regex.Split(File.ReadAllText(filePath).Replace("\r", "\n"), datEntryOffset);

            //load all qualified import properties
            IEnumerable<PropertyInfo> importProperties = typeof(TDTO).GetProperties()
                .Where(prop => prop.IsDefined(typeof(ImportAttribute), false));

            //iterate thru all dat entries
            foreach (string plainEntry in plainEntries)
            {
                var parsedEntry = Activator.CreateInstance<TDTO>();

                //iterate thru all qualified import properties
                foreach (PropertyInfo importProperty in importProperties)
                {
                    ImportAttribute attribute = importProperty.GetCustomAttribute<ImportAttribute>();
                    var value = ParseLineIndex(plainEntry, attribute);

                    if(value != null && value != "-1")
                    {
                        importProperty.SetValue(parsedEntry, Convert.ChangeType(value, importProperty.PropertyType));
                    }
                }

                parsedResults.Add(parsedEntry);
            }

            return parsedResults;
        }

        private static string ParseIndex(string line, ImportAttribute attribute)
        {
            return Regex.Split(line, @"\s")[attribute.Index];
        }

        private static string ParseLine(string entry, ImportAttribute attribute)
        {
            return Regex.Match(entry, $"^.*{attribute.Line}.*$", RegexOptions.Multiline).Value;
        }

        private static string ParseLineIndex(string entry, ImportAttribute attribute)
        {
            string line = ParseLine(entry, attribute);

            //check if line has been found
            if (!String.IsNullOrEmpty(line))
            {
                return ParseIndex(line, attribute);
            }
            else
            {
                Console.WriteLine($"{attribute} not found.");
                Debug.WriteLine($"{attribute} not found in {entry}");
            }

            return null;
        }

        #endregion
    }
}