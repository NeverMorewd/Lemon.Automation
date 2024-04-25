using Lemon.Automation.Framework.Extensions;
using System.Collections.Concurrent;

namespace Lemon.Automation.App.UIProvider.UIA.Windows
{
    internal class Win32ElementCache
    {
        private readonly ConcurrentDictionary<string,Lazy<Win32Element>> _caches = new();
        public Win32ElementCache() 
        {
            
        }
        public string Add(Win32Element element)
        {
            string elementKey = $"{nameof(Win32Element)}:{element.GetHashCode()}";
            Lazy<Win32Element> lazy = new(element);
            _caches.TryAdd(elementKey, lazy);

           // _caches.GetOrAdd(elementKey, _ => element
           return elementKey ;
        }
    }
}
