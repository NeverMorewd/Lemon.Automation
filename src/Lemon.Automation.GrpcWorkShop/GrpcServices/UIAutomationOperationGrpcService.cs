using Grpc.Core;
using Lemon.Automation.Domains;
using Lemon.Automation.GrpcWorkShop.GrpcDomains;
using Lemon.Automation.Protos;
using Microsoft.Extensions.Logging;
using System.Windows;

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

        public async override Task<GetDesktopResponse> GetDesktop(GetDesktopRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogDebug("GetDesktop");
                var uiElement = _winServiceFacade.GetDesktop();
                GetDesktopResponse response = new()
                {
                    Element = new Element
                    {
                        Id = uiElement.Id,
                        Name = uiElement.Name,
                        Left = uiElement.RegionRectangle.Left,
                        Top = uiElement.RegionRectangle.Top,
                        Height = uiElement.RegionRectangle.Height,
                        With = uiElement.RegionRectangle.Width,
                    }
                };
                return await Task.FromResult(response);
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async override Task<GetAllChildResponse> GetAllChild(GetAllChildRequest request, ServerCallContext context)
        {
            try
            {
                _logger.LogDebug("GetAllChild");
                var id = request.Element.Id;
                var children = _winServiceFacade.GetAllChildren(id);
                var elements = children.Select(uiElement => new Element
                {
                    Id = uiElement.Id,
                    Name = uiElement.Name,
                    Left = uiElement.RegionRectangle.Left,
                    Top = uiElement.RegionRectangle.Top,
                    Height = uiElement.RegionRectangle.Height,
                    With = uiElement.RegionRectangle.Width,
                });

                GetAllChildResponse response = new();
                response.Element.AddRange(elements);
                return await Task.FromResult(response);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public void Bind(ServiceBinderBase serviceBinder)
        {
            UIAutomationOperationService.BindService(serviceBinder,this);
        }
    }
}
