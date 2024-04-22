using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace Lemon.Automation.Framework.Mvvm
{
    /// <summary>
    /// https://stackoverflow.com/questions/71706287/how-to-bind-a-custom-routed-event-to-a-command-in-the-view-model
    /// </summary>
    public class EventToCommandBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty RoutedEventProperty = DependencyProperty.Register(
            nameof(RoutedEvent),
            typeof(RoutedEvent),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(null));

        private readonly RoutedEventHandler handler;

        public RoutedEvent RoutedEvent
        {
            get => (RoutedEvent)GetValue(RoutedEventProperty);
            set => SetValue(RoutedEventProperty, value);
        }

        public static readonly DependencyProperty WithCommandProperty = DependencyProperty.Register(
            nameof(WithCommand),
            typeof(ICommand),
            typeof(EventToCommandBehavior),
            new PropertyMetadata(null));

        public ICommand WithCommand
        {
            get => (ICommand)GetValue(WithCommandProperty);
            set => SetValue(WithCommandProperty, value);
        }

        public EventToCommandBehavior()
        {
            handler = (s, e) =>
            {
                var args = e;

                if (WithCommand.CanExecute(args))
                {
                    WithCommand.Execute(args);
                }
            };
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AddHandler(RoutedEvent, handler);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.RemoveHandler(RoutedEvent, handler);
        }
    }
}
