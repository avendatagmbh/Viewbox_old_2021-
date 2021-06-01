using System;
using System.Runtime.InteropServices;

namespace GhostscriptSharp.API
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int CreateAPIInstance(out IntPtr pinstance, IntPtr caller_handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int InitAPI(IntPtr instance, int argc, string[] argv);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int ExitAPI(IntPtr instance);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DeleteAPIInstance(IntPtr instance);

    public static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
    }
}