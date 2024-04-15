using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lemon.Native.Winx64
{
    /// <summary>
    /// https://github.com/dotnet/runtime/issues/86573
    /// Currently aot of win-x86 is not supported!
    /// </summary>
    public static class Main
    {
        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = nameof(DllMain))]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static bool DllMain(nint hModule,
            [MarshalAs(UnmanagedType.U4)] EnumReasonForCall ul_reason_for_call,
            nint lpReserved)
        {
            //throw new NotImplementedException("DllMain");
            Windows.Win32.PInvoke.AttachConsole(Windows.Win32.PInvoke.ATTACH_PARENT_PROCESS);
            Console.WriteLine($"DllMain:{hModule};{lpReserved}");
            switch (ul_reason_for_call)
            {
                case EnumReasonForCall.DLL_PROCESS_ATTACH:
                    Console.WriteLine(nameof(EnumReasonForCall.DLL_PROCESS_ATTACH));
                    break;
                case EnumReasonForCall.DLL_PROCESS_DETACH:
                    Console.WriteLine(nameof(EnumReasonForCall.DLL_PROCESS_DETACH));
                    break;
                case EnumReasonForCall.DLL_THREAD_ATTACH:
                    Console.WriteLine(nameof(EnumReasonForCall.DLL_THREAD_ATTACH));
                    break;
                case EnumReasonForCall.DLL_THREAD_DETACH:
                    Console.WriteLine(nameof(EnumReasonForCall.DLL_THREAD_DETACH));
                    break;
            }
            return true;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = nameof(Int32Plus))]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static int Int32Plus(int anInt, int anotherInt)
        {
            Console.WriteLine($"Int32Plus:{anInt} + {anotherInt}");
            return anInt + anotherInt;
        }
    }
    public enum EnumReasonForCall : uint
    {
        DLL_PROCESS_ATTACH = 1,
        DLL_THREAD_ATTACH = 2,
        DLL_THREAD_DETACH = 3,
        DLL_PROCESS_DETACH = 0,
    }
}
