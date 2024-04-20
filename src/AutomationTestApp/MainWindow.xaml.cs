using AutomationTestApp.Models;
using Microsoft.Web.WebView2.Core;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace AutomationTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? htmlText;
        private string? htmlPath;
        public MainWindow()
        {
            InitializeComponent();
            htmlText = System.IO.File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}Demos-main/pwa-to-do/index.html");
            htmlPath = $"file:///{AppDomain.CurrentDomain.BaseDirectory}Demos-main/pwa-to-do/index.html";
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            dataGrid.ItemsSource = Employee.GetEmployees();
            webView.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping("demo", $"{AppDomain.CurrentDomain.BaseDirectory}Demos-main/pwa-to-do", CoreWebView2HostResourceAccessKind.Allow);
            //webView.CoreWebView2.NavigateToString(htmlText);
            webView.CoreWebView2.Navigate("https://demo/index.html");
            webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = true;
            //webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
            //webView.CoreWebView2.ScriptDialogOpening += CoreWebView2_ScriptDialogOpening;
            //webView.Source = new Uri($"file:///{AppDomain.CurrentDomain.BaseDirectory}Demos-main/pwa-to-do/index.html");
            Native();
            void Native()
            {
                unsafe
                {
                    string txt = "1234567890";
                    nint handle = nint.Parse(txt);
                    if (TrySetClipboardText(handle))
                    {
                        var getHandle = TryGetClipboardText();
                        Span<char> charSpan = new(getHandle.ToPointer(), 10);
                        var getText = charSpan.ToString();
                        if (TryClearClipboardText())
                        {
                            
                        }
                    }
                }
            }


        }



        private void WebView2_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {

            }
            else
            {
                // 初始化失败，处理异常
            }
        }

        private void CoreWebView2_ScriptDialogOpening(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2ScriptDialogOpeningEventArgs e)
        {
            if (e.Kind == CoreWebView2ScriptDialogKind.Alert)
            {
                // 可以获取到提示框的消息内容
                TextBlock content = new()
                {
                    Text = e.Message,
                    Margin = new Thickness(5)
                };
                Grid grid = new();
                grid.Children.Add(content);

                // 在这里你可以自定义你的提示窗口，设置位置并显示消息内容
                Window alertWindow = new()
                {
                    Padding = new Thickness(5),
                    WindowState = WindowState.Normal,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    WindowStyle = WindowStyle.SingleBorderWindow,
                    MinHeight = 200,
                    MinWidth = 400,
                    Owner = this,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Content = grid
                };
                alertWindow.ShowDialog();

                e.Accept();
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            webView.Reload();
            await webView.CoreWebView2.ExecuteScriptAsync($"alert('{webView.Source} has been reloaded successfully')");
        }

        [DllImport("Lemon.Native.Winx64.dll")]
        public extern static bool TrySetClipboardText(nint textHandle);

        [DllImport("Lemon.Native.Winx64.dll")]
        public extern static nint TryGetClipboardText();

        [DllImport("Lemon.Native.Winx64.dll")]
        public extern static bool TryClearClipboardText();

    }
}