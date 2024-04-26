namespace Lemon.Automation.Domains
{
    public interface IElementCacheService<T>
    {
        public T Get(string elementKey);
        public void AddorUpdate(string elementKey, T element);
        public string AddorUpdate(T element);
        public void Clear();
        public bool TryRemove(string elementKey,out T? element);
        public bool Remove(T element);
    }
}
