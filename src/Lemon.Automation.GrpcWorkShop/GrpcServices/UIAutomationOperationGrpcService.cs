using Grpc.Core;
using Lemon.Automation.Domains;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.Protos;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Lemon.Automation.GrpcWorkShop.GrpcServices
{
    public class UIAutomationOperationGrpcService : UIAutomationOperationService.UIAutomationOperationServiceBase,IGrpcService
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<IUIAServiceFacade> _serviceFacades;
        private readonly IUIAServiceFacade _winServiceFacade;
        public UIAutomationOperationGrpcService(IEnumerable<IUIAServiceFacade> uiaServiceFacades, 
            ILogger<UIAutomationOperationGrpcService> logger)
        {
            _logger = logger;
            _serviceFacades = uiaServiceFacades;
            _winServiceFacade = _serviceFacades.First();
        }

        public override Task<GetDesktopResponse> GetDesktop(GetDesktopRequest request, ServerCallContext context)
        {
            return base.GetDesktop(request, context);
        }

        public override Task<GetAllChildResponse> GetAllChild(GetAllChildRequest request, ServerCallContext context)
        {
            return base.GetAllChild(request, context);
        }

        public void Bind(ServiceBinderBase serviceBinder)
        {
            UIAutomationOperationService.BindService(serviceBinder,this);
        }
    }
}
