using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTrimmingTool
{
    public static class extensions
    {
        public static bool any(this string str, params string[] strings) {

            foreach (var x in strings)
            {
                if (str == x) return true;
            }

            return false;
        }

        public static int asInt(this string str) {
            return int.Parse(str);
        }
    }
}
