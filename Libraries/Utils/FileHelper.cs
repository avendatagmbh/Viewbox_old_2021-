using System;
using System.IO;

namespace Utils
{
    public class FileHelper
    {
        public static bool IsFileBeingUsed(string fileName, out Exception exception,
                                           FileShare fileShare = FileShare.None)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    //using (File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None)) {
                    using (File.Open(fileName, FileMode.Open, FileAccess.Read, fileShare))
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                return true;
            }
            exception = null;
            return false;
        }
    }
}