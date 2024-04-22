using System.Reflection;

namespace Lemon.Automation.Domains
{
    public interface IApplication
    {
        AssemblyName AssemblyName { get; }
        string? AppName { get; }
        void Run(string[]? runArgs);
        SynchronizationContext? AppSynchronizationContext { get; }
        IAppHostedService ResolveHostService(IServiceProvider serviceProvider);
    }
}
