using Lemon.Automation.BrowserNativeHost.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Services
{
    internal class EnvironmentProvider : IEnvironmentProvider
    {
        public string GetEnvironmentVariable(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName);
        }

        public string GetFolderPath(Environment.SpecialFolder folder)
        {
            return Environment.GetFolderPath(folder);
        }

        public string[] GetCommandLineArgs()
        {
            return Environment.GetCommandLineArgs();
        }

        public bool TryGetIntArgument(string argName, out int argValue)
        {
            string[] commandLineArgs = GetCommandLineArgs();
            argValue = 0;
            string[] array = commandLineArgs;
            foreach (string text in array)
            {
                if (text.StartsWith(argName + "=") && int.TryParse(text.Substring(argName.Length + 1), out argValue))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasArgument(string argName)
        {
            string[] commandLineArgs = GetCommandLineArgs();
            string[] array = commandLineArgs;
            foreach (string text in array)
            {
                if (text == argName || text.StartsWith(argName + "="))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
