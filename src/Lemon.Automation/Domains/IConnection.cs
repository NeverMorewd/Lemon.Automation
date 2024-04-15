namespace Lemon.Automation.Domains
{
    public interface IConnection
    {
        string? ConnectionKey { get; }
        int? ConnectTimeout { get; }
    }
}
