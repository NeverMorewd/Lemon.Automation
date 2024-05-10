using Lemon.Automation.BrowserNativeHost.Contracts;
using Microsoft.Win32;

namespace Lemon.Automation.BrowserNativeHost.Services
{
    internal class Win32Registry : IWin32Registry
    {
        public int? ReadIntValue(Win32RegistryHive hive, string keyPath, string valueName)
        {
            try
            {
                using RegistryKey registryKey = OpenKey(hive, keyPath);
                object value = registryKey.GetValue(valueName);
                if (value == null)
                {
                    return null;
                }
                RegistryValueKind valueKind = registryKey.GetValueKind(valueName);
                if (valueKind == RegistryValueKind.DWord)
                {
                    return value as int?;
                }
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString(), "C:\\dr-ag-54982b01-we\\_work\\1\\s\\UiPath\\Portable\\UiPath.BrowserBridge.Portable\\Services\\Impl\\Win32Registry.cs", 36, "ReadIntValue");
            }
            return null;
        }

        private RegistryKey OpenKey(Win32RegistryHive hive, string keyPath)
        {
            RegistryKey rootKey = GetRootKey(hive);
            return rootKey.OpenSubKey(keyPath);
        }

        private RegistryKey GetRootKey(Win32RegistryHive hive)
        {
            return (hive == Win32RegistryHive.CurrentUser) ? Registry.CurrentUser : Registry.LocalMachine;
        }
    }
}
