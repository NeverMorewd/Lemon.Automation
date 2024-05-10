using Lemon.Automation.BrowserNativeHost.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class BrowserContext : IBrowserContext
    {
        private IPlatformInformation PlatformInformation { get; init; }

        private IProcessService ProcessService { get; init; }

        public BrowserContext(IPlatformInformation platformInformationService, IProcessService processService)
        {
            PlatformInformation = platformInformationService;
            ProcessService = processService;
        }

        public BrowserName GetBrowserContextOfThisProcess()
        {
            Dictionary<string, BrowserName> browserNamePrefixes = GetRecognizedBrowserNamePrefixes();
            BrowserName foundBrowserName = BrowserName.CustomBrowser;
            ProcessService.IterateProcessAncestorNames(Process.GetCurrentProcess(), delegate (string parentProcessName)
            {
                if (IsKnownBrowserName(browserNamePrefixes, parentProcessName, out var out_browserName))
                {
                    foundBrowserName = out_browserName;
                    return LoopAction.Break;
                }
                return LoopAction.Continue;
            });
            return foundBrowserName;
        }

        public string BrowserNameAsString(BrowserName name)
        {
            return name.ToString();
        }

        public bool IsKnownBrowserName(Dictionary<string, BrowserName> browserNamePrefixes, string processName, out BrowserName out_browserName)
        {
            if (!string.IsNullOrEmpty(processName))
            {
                foreach (KeyValuePair<string, BrowserName> browserNamePrefix in browserNamePrefixes)
                {
                    string text = processName.Trim().ToLower();
                    string text2 = browserNamePrefix.Key.ToLower();
                    if (text.StartsWith(text2))
                    {
                        out_browserName = browserNamePrefix.Value;
                        return true;
                    }
                }
            }
            out_browserName = BrowserName.CustomBrowser;
            return false;
        }

        public Dictionary<string, BrowserName> GetRecognizedBrowserNamePrefixes()
        {
            Dictionary<string, BrowserName> dictionary = new Dictionary<string, BrowserName>();
            BrowserName[] values = Enum.GetValues<BrowserName>();
            for (int i = 0; i < values.Length; i++)
            {
                BrowserName browserName = values[i];
                dictionary.Add(browserName.ToString(), browserName);
            }
            if (PlatformInformation.IsOSX)
            {
                dictionary.Add("Google Chrome", BrowserName.Chrome);
            }
            dictionary.Add("chromium", BrowserName.Chrome);
            dictionary.Add("chromium-browser", BrowserName.Chrome);
            return dictionary;
        }
    }
}
