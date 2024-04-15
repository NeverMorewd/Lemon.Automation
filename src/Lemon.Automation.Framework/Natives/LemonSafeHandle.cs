using System.Runtime.InteropServices;
using Windows.Win32;

namespace Lemon.Automation.Framework.Win32Native
{
    public class LemonSafeHandle : SafeHandle
    {
        public LemonSafeHandle(IntPtr preexistingHandle, bool ownsHandle) 
            : base(preexistingHandle, ownsHandle)
        {
            handle = preexistingHandle;
        }
        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
            {
                return PInvoke.CloseHandle(new Windows.Win32.Foundation.HANDLE(handle));
            }
            return false;
        }
    }
}
