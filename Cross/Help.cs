using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cross
{
    public static class Help
    {
        public static string Cross() {
            return "" +
            "cross [num] [value] [neutral] -- cross-validation\n" +
            "       [num] checks lexema\n" +
            "       [value] default probability value\n" +
            "       [neutral] threshold value\n";
        }
    }
}