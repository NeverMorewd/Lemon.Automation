using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.GrpcWorkShop.GrpcDomains
{
    public interface IGrpcService
    {
        void Bind(ServiceBinderBase serviceBinder);
    }
}
