using Lemon.Automation.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Bootstrapper.Apps
{
    internal class AppUIProvider : ApplicationContext, IBackgroundApplication
    {
        public string AppName => nameof(AppUIProvider);

        public void Run(string[] runArgs)
        {
            Application.Run(this);
        }
    }
}
