using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Domains
{
    public interface IApplication
    {
        AssemblyName AssemblyName { get; }
        string AppName { get; }
        void Run(string[] runArgs);
        SynchronizationContext AppSynchronizationContext { get; }
        IAppHostedService ResolveHostService(IServiceProvider serviceProvider);
    }
}
