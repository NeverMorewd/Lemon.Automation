using Lemon.Automation.Domains;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Bootstrapper.Apps
{
    public class AppExecutor : IBackgroundApplication
    {
        public string AppName => nameof(AppExecutor);
        public AssemblyName AssemblyName { get; private set; }
        public SynchronizationContext AppSynchronizationContext { get; }
        public void Run(string[] runArgs)
        {
            throw new NotImplementedException();
        }

        public T ResolveHostService<T>(IServiceProvider serviceProvider) where T : IHostedService
        {
            throw new NotImplementedException();
        }
    }
}
