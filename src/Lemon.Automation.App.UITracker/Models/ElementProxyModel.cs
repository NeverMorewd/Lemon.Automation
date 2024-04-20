using CommunityToolkit.Mvvm.ComponentModel;
using ObservableCollections;
using System.Windows.Data;

namespace Lemon.Automation.App.UITracker.Models
{
    public class ElementProxyModel : ObservableObject
    {
        private ObservableList<ElementProxyModel> _children;
        public ElementProxyModel(string automationId, 
            string name, 
            string controlType, 
            IEnumerable<ElementProxyModel>? children) 
        {
            AutomationId = automationId;
            Name = name;
            ControlType = controlType;
            if (children != null)
            {
                _children = new ObservableList<ElementProxyModel>(children);
                ChildrenView = _children.CreateView(x => x).ToNotifyCollectionChanged();
                BindingOperations.EnableCollectionSynchronization(ChildrenView, new object());
            }
        }
        public string AutomationId
        {
            get;
        }
        public string Name
        {
            get;
        }
        public string ControlType
        {
            get;
        }
        public INotifyCollectionChangedSynchronizedView<ElementProxyModel> ChildrenView 
        { 
            get; 
            set; 
        }
    }
}
