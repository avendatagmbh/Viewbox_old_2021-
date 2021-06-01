using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Utils
{
    public static class MiscUtils
    {
        /// <summary>
        ///   The product is neither advertised or installed.
        /// </summary>
        private const int INSTALLSTATE_UNKNOWN = -1;

        /// <summary>
        ///   An invalid parameter was passed to the function.
        /// </summary>
        private const int INSTALLSTATE_INVALIDARG = -2;

        /// <summary>
        ///   The product is advertised but not installed
        /// </summary>
        private const int INSTALLSTATE_ADVERTISED = 1;

        /// <summary>
        ///   The product is installed for a different user.
        /// </summary>
        private const int INSTALLSTATE_ABSENT = 2;

        /// <summary>
        ///   The product is installed for the current user.
        /// </summary>
        private const int INSTALLSTATE_DEFAULT = 5;

        [DllImport("msi.dll")]
        public static extern Int32 MsiQueryProductState(string szProduct);

        public static bool IsVS2010RTLInstalled()
        {
            RegistryKey key;
            RegistryKey hklm = Registry.LocalMachine;
            string[] Keys = new[]
                                {
                                    "SOFTWARE\\Microsoft\\VisualStudio\\10.0\\VC\\VCRedist\\x86",
                                    "SOFTWARE\\Microsoft\\VisualStudio\\10.0\\VC\\Runtimes\\x86",
                                    "SOFTWARE\\Microsoft\\VisualStudio\\10.0\\VC\\VCRedist\\x64",
                                    "SOFTWARE\\Microsoft\\VisualStudio\\10.0\\VC\\Runtimes\\x64"
                                };
            foreach (string skey in Keys)
            {
                key = hklm.OpenSubKey(skey);
                if (key != null)
                {
                    Object value = key.GetValue("Installed");
                    if (Convert.ToInt32(value) == 1) return true;
                }
            }
            return false;
        }

        public static bool IsVS2008RTLInstalled()
        {
            string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\uninstall";
            var displayName = "Microsoft Visual C++ 2008 Redistributable";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                foreach (var subKeyName in rk.GetSubKeyNames())
                {
                    using (RegistryKey registryKey = rk.OpenSubKey(subKeyName))
                    {
                        if (registryKey.GetValue("DisplayName") != null)
                        {
                            if (registryKey.GetValue("DisplayName").ToString().Contains(displayName)) return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}