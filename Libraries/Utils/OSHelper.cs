using System;
using System.Runtime.InteropServices;

namespace Utils
{
    public static class OSHelper
    {
        #region Platform enum

        public enum Platform
        {
            X86,
            X64,
            Unknown
        }

        #endregion

        internal const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;
        internal const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
        internal const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
        internal const ushort PROCESSOR_ARCHITECTURE_UNKNOWN = 0xFFFF;

        [DllImport("kernel32.dll")]
        internal static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        public static string GetOSName()
        {
            var osv = Environment.OSVersion;
            switch (osv.Platform)
            {
                case PlatformID.Win32NT:
                    switch (osv.Version.Major)
                    {
                        case 4:
                            switch (osv.Version.Minor)
                            {
                                case 0:
                                    return "Windows NT 4.0";
                            }
                            break;
                        case 5:
                            switch (osv.Version.Minor)
                            {
                                case 0:
                                    return "Windows 2000";
                                case 1:
                                    return "Windows XP";
                                case 2:
                                    return "Windows Server 2003";
                            }
                            break;
                        case 6:
                            switch (osv.Version.Minor)
                            {
                                case 0:
                                    return "Windows Vista / Windows Server 2008";
                                case 1:
                                    return "Windows 7";
                            }
                            break;
                    }
                    break;
            }
            return "Unbekannt";
        }

        public static Platform GetPlatform()
        {
            SYSTEM_INFO sysInfo = new SYSTEM_INFO();
            GetNativeSystemInfo(ref sysInfo);
            switch (sysInfo.wProcessorArchitecture)
            {
                case PROCESSOR_ARCHITECTURE_AMD64:
                    return Platform.X64;
                case PROCESSOR_ARCHITECTURE_INTEL:
                    return Platform.X86;
                default:
                    return Platform.Unknown;
            }
        }

        #region Nested type: SYSTEM_INFO

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };

        #endregion
    }
}