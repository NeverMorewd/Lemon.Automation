using FlaUI.Core.AutomationElements;
using Lemon.Automation.App.UITracker.Models;
using Lemon.Automation.App.UITracker.Services;
using Lemon.Automation.Framework.Rx;
using Lemon.Automation.GrpcProvider.GrpcClients;
using Lemon.Automation.Protos;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using System.Windows;
using System.Windows.Data;

namespace Lemon.Automation.App.UITracker.ViewModels
{
    public class InspectViewModel: RxViewModel
    {
        private readonly ObservableList<ElementProxyModel> _elements;
        private readonly ILogger _logger;
        private readonly ElementInspectService _elementInspectService;
        public InspectViewModel(ElementInspectService elementInspectService,
            ElementHighlightService elementHighlighter,
            ILogger<InspectViewModel> logger) 
        {
            _elements = [];
           // _elements.AddRange(BuildDummyData(100));

            _elementInspectService = elementInspectService;
            _logger = logger;

            //Observable.Interval(TimeSpan.FromSeconds(3))
            //    .ObserveOnThreadPool()
            //    .Subscribe(_ =>
            //    {
            //        _elements.Clear();
            //        _elements.AddRange(BuildDummyData(new Random().Next(9,100)));
            //    });
            //Task.Run(async() => 
            //{
            //    var desktop = await _elementInspectService.GetDesktop();
            //    var allchildren = await _elementInspectService.GetAllChildren(desktop);
            //});
            CurrentElement = new BindableReactiveProperty<ElementProxyModel>();
            ElementsView = _elements.CreateView(x => x).ToNotifyCollectionChanged();
            BindingOperations.EnableCollectionSynchronization(ElementsView, new object());

            LoadedCommand = new ReactiveCommand<RoutedEventArgs>(_ => 
            {
                Task.Run(async() => 
                {
                    var desktop = await _elementInspectService.GetDesktop();
                    var allchildren = await _elementInspectService.GetAllChildren(desktop);
                    foreach (var child in allchildren)
                    {
                        _elements.Add(new ElementProxyModel(child.Id, child.Name, child.ElementType.ToString(), null));
                    }
                });

               
            });

            LoadChildrenCommand = new ReactiveCommand<RoutedEventArgs>(async obj => 
            {
                var element = CurrentElement.Value;
                var children = await _elementInspectService.GetAllChildren(new Protos.Element { Id = element.AutomationId });
                foreach (var child in children)
                {
                    element.AddChild(new ElementProxyModel(child.Id, child.Name, child.ElementType.ToString(), null));
                }
            });

            LoadElementDetailCommand = new ReactiveCommand<ElementProxyModel>(param => 
            {
                CurrentElement.Value = param;
            });
        }

        public INotifyCollectionChangedSynchronizedView<ElementProxyModel> ElementsView 
        { 
            get; 
            private set; 
        }
        public ReactiveCommand<RoutedEventArgs> LoadedCommand
        {
            get;
            set;
        }
        public ReactiveCommand<RoutedEventArgs> LoadChildrenCommand
        {
            get;
        }
        public ReactiveCommand<ElementProxyModel> LoadElementDetailCommand
        {
            get;
        }
        public BindableReactiveProperty<ElementProxyModel> CurrentElement
        {
            get;
        }
        private IEnumerable<ElementProxyModel> BuildDummyData(int count)
        {
            return Enumerable.Range(1, count).Select(i => new ElementProxyModel($"id-{i}",
                $"name-{i}",
                $"controtype-{i}",
                Enumerable.Range(1, count).Select(i => new ElementProxyModel($"id-{i}",
                $"name-{i}",
                $"controtype-{i}",
                null))));
        }
    }
}
