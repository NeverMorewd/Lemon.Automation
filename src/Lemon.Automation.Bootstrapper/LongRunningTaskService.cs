using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.Bootstrapper
{
    public class LongRunningTaskService : IHostedService
    {
        public LongRunningTaskService()
        { }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Start the work
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //Graceful shutdown from the Host
            throw new NotImplementedException();
        }
    }
}
