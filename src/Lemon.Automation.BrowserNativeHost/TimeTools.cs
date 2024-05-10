using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class TimeTools
    {
        public static void DoTimed(string label, Action action)
        {
            DateTime now = DateTime.Now;
            action();
            double totalMilliseconds = (DateTime.Now - now).TotalMilliseconds;
            //Log.Verbose($"[{label}] Execution took: {totalMilliseconds}ms", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\TimeTools.cs", 13, "DoTimed");
        }

        public static TReturn DoTimed<TReturn>(string label, Func<TReturn> action)
        {
            DateTime now = DateTime.Now;
            TReturn result = action();
            double totalMilliseconds = (DateTime.Now - now).TotalMilliseconds;
            //Log.Verbose($"[{label}] Execution took: {totalMilliseconds}ms", "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.Portable.Shared\\TimeTools.cs", 21, "DoTimed");
            return result;
        }
    }
}
