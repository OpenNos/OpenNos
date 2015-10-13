using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LoginServer
{
    public class AntiSqlInjection
    {

        private static readonly Regex RegexId
        = new Regex(@"(^[_@\#a-zA-Z][_@\#\$a-zA-Z0-9]*(?:\.[_@\#\$a-zA-Z0-9]+)?$)|(^\[{1}[_@\#\$\.a-z A-Z0-9]{0,128}\]{1}$)", RegexOptions.Compiled);
        private static readonly Regex RegexInteger = new Regex(@"^[\d]+$", RegexOptions.Compiled);
        private static readonly Regex RegexValue = new Regex(@"(^(([']{1}([^'])*[']{1})|([^'])*)$)", RegexOptions.Compiled);

        public static string ValidateSqlId(string identifierName, bool allowEmptyStrings = false)
        {
            if (allowEmptyStrings && string.IsNullOrEmpty(identifierName))
            {
                return identifierName;
            }
            if (!RegexId.IsMatch(identifierName))
            {
                throw new ApplicationException(string.Format("'{0}' is not a valid SQL identifier name.", identifierName));
            }
            return identifierName;
        }

        public static string ValidateInteger(string oid, bool allowEmptyStrings = false)
        {
            if (!IsOidValid(oid, allowEmptyStrings))
            {
                throw new ApplicationException("Provided OID is not valid. The value should be integer.");
            }
            return oid;
        }

        private static bool IsOidValid(string oid, bool allowEmptyStrings)
        {
            return (allowEmptyStrings && string.IsNullOrEmpty(oid)) || RegexInteger.IsMatch(oid);
        }

        public static IEnumerable<string> ValidateIntegerList(IEnumerable<string> oidList, bool allowEmptyStrings = false)
        {
            if (!oidList.All(oid => IsOidValid(oid, allowEmptyStrings)))
            {
                throw new ApplicationException("Not a valid Integer List.");
            }
            return oidList;
        }

        public static string ValidateSqlValue(string value)
        {
            // Is it null?
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            // Is it a BOOLEAN?
            bool throwawayBool;
            if (bool.TryParse(value, out throwawayBool))
            {
                return value;
            }
            // Is it a number?
            double throwawayNumber;
            if (double.TryParse(value, out throwawayNumber))
            {
                return value;
            }
            if (RegexValue.IsMatch(value.Replace("''", string.Empty)))
            {
                return value;
            }
            throw new ApplicationException(string.Format("ValidateSqlValue: SQL value is not legitimate. Field: {0}", value));
        }

        public static string ValidateIntId(string id)
        {
            int result;
            return int.TryParse(id, out result) ? result.ToString(CultureInfo.InvariantCulture) : "";
        }

        public static int ValidateIntIdForSqlWhere(string id)
        {
            int result;
            return int.TryParse(id, out result) ? result : -1;
        }

    }
}
