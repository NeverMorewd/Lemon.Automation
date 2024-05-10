using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost.Contracts
{
    internal interface IEnvironmentProvider
    {
        string GetEnvironmentVariable(string variableName);

        string GetFolderPath(Environment.SpecialFolder folder);

        string[] GetCommandLineArgs();

        bool TryGetIntArgument(string argName, out int argValue);

        bool HasArgument(string argName);
    }
}
