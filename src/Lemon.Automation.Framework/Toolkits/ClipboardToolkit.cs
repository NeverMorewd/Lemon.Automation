using Polly;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Lemon.Automation.Framework.Toolkits
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/api/ole2/nf-ole2-olesetclipboard
    /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setclipboarddata
    /// </summary>
    public static class ClipboardToolkit
    {
        public static void SetText(string value)
        {
            try
            {
                unsafe
                {
                    byte[] textBytes = Encoding.Unicode.GetBytes(value);
                    nuint memorySize = new((uint)(textBytes.Length + 2));
                    nint hGlobalMemory = PInvoke.GlobalAlloc(Windows.Win32.System.Memory.GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, memorySize);
                    if (hGlobalMemory == 0)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    void* pGlobalMemory = PInvoke.GlobalLock(new GlobalFreeSafeHandle(hGlobalMemory));
                    if (pGlobalMemory == null)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    Marshal.Copy(textBytes, 0, (nint)pGlobalMemory, textBytes.Length);
                    Marshal.WriteInt16((nint)pGlobalMemory, textBytes.Length, 0);
                    PInvoke.GlobalUnlock(new GlobalFreeSafeHandle((nint)pGlobalMemory));
                    if (!PInvoke.OpenClipboard(default))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    if (!PInvoke.EmptyClipboard())
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    if (PInvoke.SetClipboardData(13u, new HANDLE(hGlobalMemory)) == nint.Zero)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    PInvoke.CloseClipboard();
                }
            }
            catch (Win32Exception ex)
            {
                throw new InvalidOperationException("Fail to set clipboard:" + ex.Message);
            }
        }

        public static bool TrySetText(string value)
        {
            SetText(value);
            return true;
        }

        public static string GetText()
        {
            if (ClipboardIdle())
            {
                return System.Windows.Forms.Clipboard.GetText(System.Windows.Forms.TextDataFormat.UnicodeText);
            }
            throw new InvalidOperationException("The clipboard is being occupied.");
        }

        public static bool TryGetText(out string text)
        {
            text = GetText();
            return true;
        }

        public static void SetImage(Image image)
        {
            if (ClipboardIdle())
            {
                System.Windows.Forms.Clipboard.SetImage(image);
                return;
            }
            throw new Exception("The clipboard is being occupied.");
        }

        public static bool TrySetImage(Image image)
        {
            SetImage(image);
            return true;
        }

        public static Image? GetImage()
        {
            if (ClipboardIdle())
            {
                return System.Windows.Forms.Clipboard.GetImage();
            }
            throw new Exception("The clipboard is being occupied.");
        }

        public static bool TryGetImage(out Image? image)
        {
            image = GetImage();
            return true;
        }

        public static void SetFile(IEnumerable<string> filePaths)
        {
            StringCollection filePathCollection = [.. filePaths];
            if (ClipboardIdle())
            {
                System.Windows.Forms.Clipboard.SetFileDropList(filePathCollection);
                return;
            }
            throw new Exception("The clipboard is being occupied.");
        }

        public static bool TrySetFile(IEnumerable<string> filePaths)
        {
            SetFile(filePaths);
            return true;
        }

        public static void SetDataObject(object data)
        {
            if (ClipboardIdle())
            {
                System.Windows.Forms.Clipboard.SetDataObject(data);
                return;
            }
            throw new Exception("The clipboard is being occupied.");
        }

        public static bool TrySetDataObject(object data)
        {
            SetDataObject(data);
            return true;
        }

        public static System.Windows.Forms.IDataObject? GetDataObject()
        {
            if (ClipboardIdle())
            {
                return System.Windows.Forms.Clipboard.GetDataObject();
            }
            throw new Exception("The clipboard is being occupied.");
        }

        public static bool TryGetDataObject(out System.Windows.Forms.IDataObject? data)
        {
            try
            {
                data = GetDataObject();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TryGetDataObject:{ex}");
                data = null;
                return false;
            }
        }

        public static void SetData(string format, object data)
        {
            if (ClipboardIdle())
            {
                System.Windows.Clipboard.SetData(format, data);
                return;
            }
            throw new Exception("The clipboard is being occupied.");
        }

        public static bool TrySetData(string format, object data)
        {
            SetData(format, data);
            return true;
        }

        public static object? GetData(string format)
        {
            if (ClipboardIdle())
            {
                return System.Windows.Forms.Clipboard.GetData(format);
            }
            throw new Exception("The clipboard is being occupied.");
        }

        public static bool TryGetData(string format, out object? obj)
        {
            obj = GetData(format);
            return true;
        }

        public static void Clear()
        {
            if (ClipboardIdle())
            {
                try
                {
                    if (!PInvoke.OpenClipboard(default))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    if (!PInvoke.EmptyClipboard())
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    PInvoke.CloseClipboard();
                    return;
                }
                catch (Win32Exception ex)
                {
                    throw new InvalidOperationException($"Fail to clear clipboard:{ex}");
                }
            }
            throw new Exception("The clipboard is being occupied.");
        }

        public static bool TryClear()
        {
            Clear();
            return true;
        }

        public static string[] GetFileDropList()
        {
            if (ClipboardIdle())
            {
                List<string> paths = [];
                if (System.Windows.Forms.Clipboard.ContainsFileDropList())
                {
                    StringEnumerator enumerator = System.Windows.Forms.Clipboard.GetFileDropList().GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current is not null)
                            {
                                string item = enumerator.Current;
                                paths.Add(item);
                            }
                        }
                    }
                    finally
                    {
                        if (enumerator is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                    return [.. paths];
                }
                return [];
            }
            throw new Exception("The clipboard is being occupied.");
        }

        public static bool TryGetFileDropList(out string[] paths)
        {
            paths = GetFileDropList();
            return true;
        }

        public static bool ClipboardIdle()
        {
           return Policy<bool>
                 .Handle<Exception>()
                 .OrResult(res => !res)
                 .Retry(3, (res, i, c) =>
                 {
                     Console.WriteLine($"Retry {i}th times, ex: {res.Exception?.Message}");
                 })
                 .Execute(() =>
                 {
                     return (nint)PInvoke.GetOpenClipboardWindow() == nint.Zero;
                 });
        }
    }
}
