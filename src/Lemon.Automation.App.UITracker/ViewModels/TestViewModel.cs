using Lemon.Automation.Framework.Rx;
using Lemon.Automation.Framework.Toolkits;
using R3;
using System.Diagnostics;

namespace Lemon.Automation.App.UITracker.ViewModels
{
    public class TestViewModel : RxViewModel
    {
        public TestViewModel()
        {
            BrowseExeAndRunCommand = new ReactiveCommand<Unit>(async _ =>
            {
                Microsoft.Win32.OpenFileDialog dlg = new()
                {
                    Filter = "exe files (*.exe)|*.exe"
                };
                if (dlg.ShowDialog() == true)
                {
                    var filePath = dlg.FileName;
                    if (filePath.EndsWith(".exe") || filePath.EndsWith(".EXE"))
                    {
                        try
                        {
                            var process = Process.Start(filePath);
                            process.EnableRaisingEvents = true;
                            process.Exited += Process_Exited;
                            if (process != null)
                            {
                                var uiMessageBox = new Wpf.Ui.Controls.MessageBox
                                {
                                    Title = "Info",
                                    Content = $"{filePath} has been started!"
                                };
                                await uiMessageBox.ShowDialogAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            var uiMessageBox = new Wpf.Ui.Controls.MessageBox
                            {
                                Title = "Error",
                                Content = $"{ex}"
                            };
                            await uiMessageBox.ShowDialogAsync();
                        }
                    }
                }
            });

            TestClipboardCommand = new ReactiveCommand<string>(async txt =>
            {
                if (string.IsNullOrEmpty(txt))
                {
                    var uiMessageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "Warning",
                        Content = "Empty content is not allowed!"
                    };

                    await uiMessageBox.ShowDialogAsync();
                }
                else
                {
                    var idle = ClipboardToolkit.ClipboardIdle();
                    ClipboardToolkit.SetText(txt);
                    var uiMessageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "Info",
                        Content = ClipboardToolkit.GetText()
                    };

                    await uiMessageBox.ShowDialogAsync();
                }
            });

            TestCommand = new ReactiveCommand<Unit>(_ =>
            {
                //InfinityRecursion();
                var process = Process.GetProcessesByName("II.RPA.Core.Server").First();
                process.EnableRaisingEvents = true;
                process.Exited += (s,e)=> { Console.WriteLine($"II.RPA.Core.Server exited"); };
            });
        }

        private void Process_Exited1(object? sender, EventArgs e)
        {
            Console.WriteLine($"II.RPA.Core.Server exited");
        }

        private void Process_Exited(object? sender, EventArgs e)
        {
            Console.WriteLine($"{e}");
        }

        public ReactiveCommand<Unit> BrowseExeAndRunCommand
        {
            get;
        }
        public ReactiveCommand<string> TestClipboardCommand
        {
            get;
        }

        public ReactiveCommand<Unit> TestCommand
        {
            get;
        }

        public override void Dispose()
        {
            Disposable.Combine(BrowseExeAndRunCommand).Dispose();
        }

        /// <summary>
        /// stack overflow
        /// https://learn.microsoft.com/en-us/dotnet/api/system.stackoverflowexception?view=net-8.0&redirectedfrom=MSDN
        /// </summary>
        /// <returns></returns>
        public static string InfinityRecursion()
        {
            return InfinityRecursion();
        }
    }
}
