using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Framework.Extensions
{
    public static class StringExtension
    {
        public static string? JParse(this string o)
        {
            if (o == null) return null;
            string result = JsonConvert.ToString(o);
            result = result[1..^1];
            return result;
        }
    }
}
