using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyDocExtraction.Model;
using EasyDocExtraction.DataAccess;

namespace EasyDocExtraction.Factory
{
    public class EasyFolderFactory
    {
        
        static List<EasyFolder> _easyFolders = new List<EasyFolder>();

        public static EasyFolder CreateGetEasyFolder(string dbInfo) 
        {
            EasyFolder easyFolder = _easyFolders.Find(d => d.Name == dbInfo.ToUpper());
            if (easyFolder == null)
            {
                easyFolder = new EasyFolder { Name = dbInfo.ToUpper() };

                using(var dbContext =  new EasyArchiveRepository())
                {
                    dbContext.EasyFolders.Add(easyFolder);
                    dbContext.SaveChanges();
                    _easyFolders = dbContext.EasyFolders.ToList();
                }
            }
            return easyFolder;
        }
    }

}
