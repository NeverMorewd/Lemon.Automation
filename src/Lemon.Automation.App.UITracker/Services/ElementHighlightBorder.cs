using Lemon.Automation.Framework.AutomationCore.Models;
using Lemon.Automation.Framework.Extensions;
using Lemon.Automation.Framework.Natives;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace Lemon.Automation.App.UITracker.Services
{
    internal class ElementHighlightBorder : Window
    {
        private Rectangle m_OriginalRect;
        private bool _isWindowTransparent;
        private readonly Panel _innerPane;

        public ElementHighlightBorder(Rectangle rectangle,
            SolidColorBrush aBorderColor,
            bool anIsTransParent,
            bool anIsBackgroundTansparent,
            int durationInMs = 0)
        {
            _isWindowTransparent = anIsTransParent;
            m_OriginalRect = new Rectangle(rectangle.Location, rectangle.Size);
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            Topmost = true;
            ShowActivated = false;
            ShowInTaskbar = false;
            IsHitTestVisible = false;
            IsEnabled = false;
            Background = System.Windows.Media.Brushes.Transparent;
            WindowStartupLocation = WindowStartupLocation.Manual;
            SolidColorBrush solidColorBrushBorder = aBorderColor;

            _innerPane = new DockPanel
            {
                Background = System.Windows.Media.Brushes.Transparent
            };
            base.Content = new Border
            {
                BorderThickness = new Thickness(1.0),
                BorderBrush = System.Windows.Media.Brushes.White,
                CornerRadius = new CornerRadius(4.0),
                Child = new Border
                {
                    BorderThickness = new Thickness(3.0),
                    BorderBrush = solidColorBrushBorder,
                    CornerRadius = new CornerRadius(2.0),
                    Child = _innerPane
                }
            };
            RePositionAndResize(m_OriginalRect, TransformToPixels(this).X / 96.0);
            Loaded += OverlayRectangleWindow_Loaded;
            Closed += ElementHighlighterWindow_Closed;
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }
        public Action ActionOverEvent
        {
            private get;
            set;
        }
        public string InputText
        {
            get;
            private set;
        }
        public bool IsMouseInActionPopup
        {
            get;
            private set;
        } = false;
        private void ElementHighlighterWindow_Closed(object sender, EventArgs e)
        {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
            Resources.MergedDictionaries.Clear();
            Resources.Clear();
            this.Closed -= ElementHighlighterWindow_Closed;
        }
        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            RePositionAndResize(m_OriginalRect, TransformToPixels(this).X / 96.0);
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            SetWindowTransparent(_isWindowTransparent);
        }
        private void OverlayRectangleWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #region DPI适配

        private void RePositionAndResize(Rectangle rect, double factor = 1.0)
        {
            Rectangle rectangle2 = rect.Downscale(factor);
            if ((double)(rectangle2.Width + 4) <= SystemParameters.WorkArea.Width)
            {
                rectangle2.Inflate(4, 0);
            }
            if ((double)(rectangle2.Height + 4) <= SystemParameters.WorkArea.Height)
            {
                rectangle2.Inflate(0, 4);
            }
            base.Left = rectangle2.Left;
            base.Top = rectangle2.Top;
            base.Width = rectangle2.Width;
            base.Height = rectangle2.Height;
        }
        public Dpi TransformToPixels(Visual aVisual)
        {
            Matrix matrix;
            var source = PresentationSource.FromVisual(aVisual);
            if (source != null)
            {
                matrix = source.CompositionTarget.TransformToDevice;
            }
            else
            {
                using (var src = new HwndSource(new HwndSourceParameters()))
                {
                    matrix = src.CompositionTarget.TransformToDevice;
                }
            }

            double pixelX = (int)(matrix.M11 * 96.0);
            double pixelY = (int)(matrix.M22 * 96.0);
            return new Dpi(pixelX, pixelY);
        }
        #endregion
        public void SetTagText(string aText)
        {
            InputText = aText;
        }
        public void SetWindowTransparent(bool aTransparent)
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            if (aTransparent)
            {
                _innerPane.Background = System.Windows.Media.Brushes.Transparent;
                NativeInvoke.EnableWindowTransparent(hwnd);
            }
            else
            {
                _innerPane.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 0, 0, 139));
                NativeInvoke.DisableWindowTransparent(hwnd);
            }
        }
        public void SetBackgroundTransparent(bool aTransparent)
        {
            if (aTransparent)
            {
                _innerPane.Background = System.Windows.Media.Brushes.Transparent;
            }
            else
            {
                _innerPane.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 0, 0, 139));
            }
        }
        public void SetCurrentElementRectBorder(Rectangle rectangle)
        {
            m_OriginalRect = new Rectangle(rectangle.Location, rectangle.Size);
            //logger.LogDebug($"Border m_OriginalRect:{rectangle.Location.Y},{rectangle.Location.X},{rectangle.Size.Height},{rectangle.Size.Width}");
            Topmost = true;
            Background = System.Windows.Media.Brushes.Transparent;
            _innerPane.Background = System.Windows.Media.Brushes.Transparent;
            RePositionAndResize(m_OriginalRect, TransformToPixels(this).X / 96.0);
        }
    }
}
