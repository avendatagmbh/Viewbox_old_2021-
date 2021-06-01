using System;
using System.Runtime.InteropServices;

namespace GhostscriptSharp.API
{
    internal class GhostScript32
    {
        /// <summary>
        ///   GS can only support a single instance, so we need to bottleneck any multi-threaded systems.
        /// </summary>
        private static readonly object ResourceLock = new object();

        /// <summary>
        ///   Calls the Ghostscript API with a collection of arguments to be passed to it
        /// </summary>
        public static void CallAPI(string[] args)
        {
            // Get a pointer to an instance of the Ghostscript API and run the API with the current arguments
            lock (ResourceLock)
            {
                IntPtr pDll =
                    NativeMethods.LoadLibrary(String.Format(@"{0}\gsdll32.dll", AppDomain.CurrentDomain.BaseDirectory));
                IntPtr addressCreateInstance = NativeMethods.GetProcAddress(pDll, "gsapi_new_instance");
                IntPtr addressInitInstance = NativeMethods.GetProcAddress(pDll, "gsapi_init_with_args");
                IntPtr addressExitInstance = NativeMethods.GetProcAddress(pDll, "gsapi_exit");
                IntPtr addressDeleteInstance = NativeMethods.GetProcAddress(pDll, "gsapi_delete_instance");
                var createAPIInstance =
                    (CreateAPIInstance)
                    Marshal.GetDelegateForFunctionPointer(addressCreateInstance, typeof (CreateAPIInstance));
                var initAPI = (InitAPI) Marshal.GetDelegateForFunctionPointer(addressInitInstance, typeof (InitAPI));
                var exitAPI = (ExitAPI) Marshal.GetDelegateForFunctionPointer(addressExitInstance, typeof (ExitAPI));
                var deleteAPIInstance =
                    (DeleteAPIInstance)
                    Marshal.GetDelegateForFunctionPointer(addressDeleteInstance, typeof (DeleteAPIInstance));
                IntPtr gsInstancePtr;
                createAPIInstance(out gsInstancePtr, IntPtr.Zero);
                try
                {
                    initAPI(gsInstancePtr, args.Length, args);
                }
                finally
                {
                    exitAPI(gsInstancePtr);
                    deleteAPIInstance(gsInstancePtr);
                }
            }
        }
    }
}