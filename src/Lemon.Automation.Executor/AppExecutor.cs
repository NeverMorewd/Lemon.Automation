using Lemon.Automation.Domains;
using System.Reflection;

namespace Lemon.Automation.Executor
{
    public class AppExecutor : IWinformApplication
    {
        public string AppName => nameof(AppExecutor);
        public AssemblyName AssemblyName { get; private set; }
        public SynchronizationContext AppSynchronizationContext { get; }
        public void Run(string[] runArgs)
        {
            throw new NotImplementedException();
        }

        public IAppHostedService ResolveHostService(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }
    }
}
