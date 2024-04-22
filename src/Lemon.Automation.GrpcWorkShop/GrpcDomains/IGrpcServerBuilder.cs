using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.GrpcWorkShop.GrpcDomains
{
    public interface IGrpcServerBuilder
    {
        public Func<GrpcServerWorkShop, IGrpcServerBuilder> AddWorkShop { get; }
        public Func<ServiceBinderBase, IGrpcServerBuilder> AddServiceBinder { get; }
        public Func<IEnumerable<IGrpcService>, IGrpcServerBuilder> AddServices { get; }
        public IGrpcServer Build();
    }
}
