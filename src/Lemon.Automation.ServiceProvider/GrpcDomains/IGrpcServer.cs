using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.GrpcWorkShop.GrpcDomains
{
    public interface IGrpcServer
    {
        public Func<GrpcServerWorkShop, IGrpcServer> AddWorkShop { get; }
        public Func<ServiceBinderBase, IGrpcServer> AddServiceBinder { get; }
        public Func<IEnumerable<IGrpcService>, IGrpcServer> AddServices { get; }
        public Func<IGrpcServer> Start();
        public Func<IGrpcServer> Stop();
        public GrpcServerWorkShop WorkShop { get; }
        public ServiceBinderBase ServiceBinder { get; }
        public IEnumerable<IGrpcService>? Services { get; set; }
    }
}
