using Polly;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;

namespace Lemon.Native.Winx64.Natives
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/windows/win32/api/ole2/nf-ole2-olesetclipboard
    /// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setclipboarddata
    /// </summary>
    public static class Clipboard
    {
        public static void SetText(string value)
        {
            try
            {
                if (ClipboardIdle())
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
                        // https://learn.microsoft.com/zh-cn/windows/win32/dataxchg/standard-clipboard-formats
                        if (PInvoke.SetClipboardData(13u, new Windows.Win32.Foundation.HANDLE(hGlobalMemory)) == nint.Zero)
                        {
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        }
                        PInvoke.CloseClipboard();
                    }
                }
                else
                {
                    Console.WriteLine("The clipboard is being occupied");
                    throw new InvalidOperationException("The clipboard is being occupied.");
                }
            }
            catch (Win32Exception wex)
            {
                Console.WriteLine(wex);
                throw new InvalidOperationException("Fail to set clipboard:" + wex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static bool TrySetText(string value)
        {
            try
            {
                SetText(value);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
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
            try
            {
                Clear();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
        public static nint TryGetText()
        {
            if (ClipboardIdle())
            {
                try
                {
                    if (!PInvoke.OpenClipboard(default))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    var dataSafeHandle = PInvoke.GetClipboardData_SafeHandle(13u);
                    if (!dataSafeHandle.IsInvalid)
                    {
                        var dataHandle = dataSafeHandle.DangerousGetHandle();
                        return dataHandle;
                    }

                }
                catch (Win32Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    PInvoke.CloseClipboard();
                }
            }
            return nint.Zero;
        }
        public static string GetText()
        {
            if (ClipboardIdle())
            {
                try
                {
                    if (!PInvoke.OpenClipboard(default))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                    var dataSafeHandle = PInvoke.GetClipboardData_SafeHandle(13u);
                    unsafe
                    {
                        if (!dataSafeHandle.IsInvalid)
                        {
                            var dataHandle = dataSafeHandle.DangerousGetHandle();
                            char* dataPointer = (char*)dataHandle.ToPointer();

                            //https://stackoverflow.com/questions/2213871/marshal-ptrtostringuni-vs-new-string
                            //var dataString = Marshal.PtrToStringUni(dataHandle);
                            var dataString = new string(dataPointer);

                            Marshal.FreeHGlobal(dataHandle);
                            return dataString;
                            //if (!string.IsNullOrEmpty(dataString))
                            //{
                            //    Span<char> charSpan = new(dataPointer, dataString.Length);
                            //    return charSpan.ToString();
                            //}
                        }
                    }

                }
                catch (Win32Exception ex)
                {
                    throw new InvalidOperationException($"Fail to clear clipboard:{ex}");
                }
                finally 
                {
                    PInvoke.CloseClipboard();
                }
            }
            throw new Exception("The clipboard is being occupied.");
        }

        public static bool ClipboardIdle()
        {
            return Policy<bool>
                  .Handle<Exception>()
                  .OrResult(res => !res)
                  .WaitAndRetry(3, retryTimes => TimeSpan.FromSeconds(2*retryTimes),
                    (res, delay, times, context) =>
                    {
                        Console.WriteLine($"retry {times}th times, sleep: {delay.TotalSeconds}s, ex: {res.Exception?.Message}");
                    })
                  .Execute(() =>
                  {
                      return (nint)PInvoke.GetOpenClipboardWindow() == nint.Zero;
                  });
        }
    }
}
