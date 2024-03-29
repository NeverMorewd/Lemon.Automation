using Lemon.Automation.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Bootstrapper.Apps
{
    internal class AppExecutor : IBackgroundApplication
    {
        public string AppName => nameof(AppExecutor);

        public void Run(string[] runArgs)
        {
            throw new NotImplementedException();
        }
    }
}
