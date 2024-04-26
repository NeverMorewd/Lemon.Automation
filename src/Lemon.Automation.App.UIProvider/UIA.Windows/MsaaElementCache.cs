using Lemon.Automation.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.App.UIProvider.UIA.Windows
{
    public class MsaaElementCache : IElementCacheService<Flaui3Element>
    {
        public void AddorUpdate(string elementKey, Flaui3Element element)
        {
            throw new NotImplementedException();
        }

        public string AddorUpdate(Flaui3Element element)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public Flaui3Element Get(string elementKey)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Flaui3Element element)
        {
            throw new NotImplementedException();
        }

        public bool TryRemove(string elementKey, out Flaui3Element? element)
        {
            throw new NotImplementedException();
        }
    }
}
