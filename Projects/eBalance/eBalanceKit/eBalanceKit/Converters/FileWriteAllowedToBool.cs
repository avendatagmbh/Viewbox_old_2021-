// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-03-12
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.IO;
using System.Security.AccessControl;

namespace eBalanceKit.Converters {
    public class FileWriteAllowedToBool {
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || parameter == null) return false;

            if (string.IsNullOrWhiteSpace(value.ToString())) {
                return false;
            }

            string path = value.ToString();
            path = Path.GetDirectoryName(path);
            
            if (!Directory.Exists(path)) {
                    return false;
            }
            
            return CanWrite(value.ToString());
            

        }

        private static bool IsDirectory(string path) {
            System.IO.FileAttributes fa = System.IO.File.GetAttributes(path);
            bool isDirectory = (fa & FileAttributes.Directory) != 0;
            return isDirectory;
        }

        private static bool CanWrite(string directoryPath) {
            bool isWriteAccess = false;
            try {
                AuthorizationRuleCollection collection = Directory.GetAccessControl(directoryPath).GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                foreach (FileSystemAccessRule rule in collection) {
                    if (rule.AccessControlType == AccessControlType.Allow) {
                        isWriteAccess = true;
                        break;
                    }
                }
            }
            catch (UnauthorizedAccessException) {
                isWriteAccess = false;
            }
            catch (Exception) {
                isWriteAccess = false;
            }
            return isWriteAccess;
        }

    }
}