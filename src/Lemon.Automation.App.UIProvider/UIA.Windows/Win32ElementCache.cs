using Lemon.Automation.Domains;
using Lemon.Automation.Framework.Extensions;
using System.Collections.Concurrent;

namespace Lemon.Automation.App.UIProvider.UIA.Windows
{
    public class Win32ElementCache: IElementCacheService<Win32Element>
    {
        private static readonly ConcurrentDictionary<string,Lazy<Win32Element>> _caches = new();
        public Win32ElementCache() 
        {
            
        }

        public void AddorUpdate(string elementKey, Win32Element element)
        {
            Lazy<Win32Element> lazy = new(element);

            _caches.AddOrUpdate(elementKey,
                addValueFactory: key => element,
                updateValueFactory: (key, oldelement) => element);
        }

        public string AddorUpdate(Win32Element element)
        {
            string elementKey = $"{nameof(Win32Element)}:{element.GetHashCode()}";

            Lazy<Win32Element> lazy = new(element);
           
            _caches.AddOrUpdate(elementKey,
                addValueFactory: key => element,
                updateValueFactory: (key, oldelement) => element);

            return elementKey;
        }

        public void Clear()
        {
            _caches.Clear();
        }

        public Win32Element Get(string elementKey)
        {
            if (!string.IsNullOrEmpty(elementKey))
            {
                if(_caches.TryGetValue(elementKey, out var element))
                {
                    return element.Value;
                }
            }
            throw new InvalidOperationException();

        }

        public bool Remove(Win32Element element)
        {
            string elementKey = $"{nameof(Win32Element)}:{element.GetHashCode()}";
            return _caches.Remove(elementKey, out _);
        }

        public bool TryRemove(string elementKey, out Win32Element? element)
        {
            if (_caches.TryRemove(elementKey, out Lazy<Win32Element>? lazyElement))
            {
                element = lazyElement.Value;
                return true;
            }
            element = default;
            return false;
        }
    }
}
