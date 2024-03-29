using Grpc.Core;
using GrpcDotNetNamedPipes;
using Lemon.Automation.Domains;
using Lemon.Automation.GrpcWorkShop;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;

namespace Lemon.Automation.UIProvider
{
    public class GrpcNamedPipeServer : INamedPipeServer, IGrpcServer
    {
        private readonly NamedPipeServer server;
        private GrpcServerWorkShop workShop;
        public GrpcNamedPipeServer(string connection)
        {
            server = new NamedPipeServer(connection);
            //WorkShop = new GrpcServerWorkShop();
            AddWorkShop = AddWorkShopExecute;
        }

        private IGrpcServer AddWorkShopExecute(GrpcServerWorkShop aWorkShop)
        {
            workShop = aWorkShop;
            return this;
        }

        public ServiceBinderBase ServiceBinder => server.ServiceBinder;

        public IEnumerable<IGrpcService>? Services 
        { 
            get; 
            set; 
        }

        public GrpcServerWorkShop WorkShop { get; }

        public Func<GrpcServerWorkShop, IGrpcServer> AddWorkShop { get; }

        public Func<ServiceBinderBase, IGrpcServer> AddServiceBinder => throw new NotImplementedException();

        public Func<IEnumerable<IGrpcService>, IGrpcServer> AddServices => throw new NotImplementedException();

        public void Start()
        {
            server.Start();
        }

        public void Stop()
        {
            server.Kill();
        }

        Func<IGrpcServer> IGrpcServer.Start()
        {
            throw new NotImplementedException();
        }

        Func<IGrpcServer> IGrpcServer.Stop()
        {
            throw new NotImplementedException();
        }
    }
}
