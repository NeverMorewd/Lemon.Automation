using Lemon.Automation.Framework.Rx;
using Lemon.Automation.Framework.Toolkits;
using Lemon.Native.Winx64.Natives;
using R3;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using NativeWindow = Lemon.Native.Winx64.Natives.NativeWindow;

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

                var handles = NativeWindow.EnumWindowsSafe((handle) => true);
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "handles.txt");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Create(filePath).Dispose();
                foreach (var handle in handles)
                {
                    try
                    {
                        var className = NativeWindow.GetWindowClassName(handle);
                        var processId = NativeWindow.GetWindowProcessId(handle);
                        var title = NativeWindow.GetWindowTitle(handle);
                        var isVisible = NativeWindow.IsWindowVisible(handle);
                        var placement = NativeWindow.GetWindowPlacement(handle);
                        var rect = NativeWindow.GetWindowRect(handle);
                        //NativeWindow.GetRootWindow(handle);
                       var windowline = $"{handle}:{className}; {processId}; {title}; {isVisible}; {placement}; {rect}";
                        File.AppendAllLines(filePath, [windowline]);

                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                }


                
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
