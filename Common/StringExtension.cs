using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class StringExtension
    {
        public static string CutController(this string input)
        {
            return input.Replace("Controller", "");
        }
    }
}
