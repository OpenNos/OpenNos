using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public static class StringHelper
    {
        public static string Truncate(string source, int length)
        {
            return (source.Length > length ? source.Substring(0, length) : source);
        }
    }
}
