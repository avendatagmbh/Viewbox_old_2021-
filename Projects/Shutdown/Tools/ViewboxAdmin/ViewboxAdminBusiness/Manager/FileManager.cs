/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Attila Papp        2012-09-03      this class does the actual file manipulations, should be as simple as possible
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ViewboxAdminBusiness.Manager
{
    /// <summary>
    /// do the actual file IO operations.
    /// </summary>
    public class FileManager :IFileManager
    {
        public bool DirectoryExist(string path) {
            return Directory.Exists(path);
        }

        public void CreateDirectory(string path) {
            Directory.CreateDirectory(path);
        }

        public bool FileExist(string path) {
            return File.Exists(path);
        }


        public string GetFolderPath(Environment.SpecialFolder specialfolder) {
            return Environment.GetFolderPath(specialfolder);
        }


        public void Delete(string filename) { File.Delete(filename); }


        public IEnumerable<FileInfo> GetFileInfo(string folder) {
            return new DirectoryInfo(folder).EnumerateFiles();
        }
    }
}
