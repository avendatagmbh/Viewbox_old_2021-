using System;
using System.Collections.Generic;
using System.IO;


namespace ViewboxAdminBusiness.Manager
{
    /// <summary>
    /// interface for file manipulations... this helps mocking file manipulations
    /// </summary>
    public interface IFileManager {
        bool DirectoryExist(string path);
        void CreateDirectory(string path);
        bool FileExist(string path);
        string GetFolderPath(Environment.SpecialFolder specialfolder );
        void Delete(string filename);
        IEnumerable<FileInfo> GetFileInfo(string folder);
    }
}
