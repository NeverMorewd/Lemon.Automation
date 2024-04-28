using Lemon.Automation.Domains;
using System.Collections.Concurrent;
using Lemon.Automation.Framework.Extensions;

namespace Lemon.Automation.App.UIProvider.UIA.Windows
{
    public class Flaui3ElementCache : IElementCacheService<Flaui3Element>
    {
        private static readonly ConcurrentDictionary<string, Lazy<Flaui3Element>> _caches = new();
        public Flaui3ElementCache()
        {

        }

        public void AddorUpdate(string elementKey, Flaui3Element element)
        {
            Lazy<Flaui3Element> lazy = new(element);

            _caches.AddOrUpdate(elementKey,
                addValueFactory: key => element,
                updateValueFactory: (key, oldelement) => element);
        }

        public string AddorUpdate(Flaui3Element element)
        {
            string elementKey = $"{nameof(Flaui3Element)}:{element.GetHashCode()}";

            Lazy<Flaui3Element> lazy = new(element);

            _caches.AddOrUpdate(elementKey,
                addValueFactory: key => element,
                updateValueFactory: (key, oldelement) => element);

            return elementKey;
        }

        public void Clear()
        {
            _caches.Clear();
        }

        public Flaui3Element Get(string elementKey)
        {
            if (!string.IsNullOrEmpty(elementKey))
            {
                if (_caches.TryGetValue(elementKey, out var element))
                {
                    return element.Value;
                }
            }
            throw new InvalidOperationException();

        }

        public bool Remove(Flaui3Element element)
        {
            string elementKey = $"{nameof(Flaui3Element)}:{element.GetHashCode()}";
            return _caches.Remove(elementKey, out _);
        }

        public bool TryRemove(string elementKey, out Flaui3Element? element)
        {
            if (_caches.TryRemove(elementKey, out Lazy<Flaui3Element>? lazyElement))
            {
                element = lazyElement.Value;
                return true;
            }
            element = default;
            return false;
        }
    }
}
