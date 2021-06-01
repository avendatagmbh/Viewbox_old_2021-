using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using EasyDocExtraction.Helper;

namespace EasyDocExtraction.Test.Helper
{
    public class EasyDocExtractionHelper
    {
        EasyExportedFoldersConfigurationSection _easySection;
        EasyDocExtractionHelper() 
        {
            _easySection = ConfigurationManager.GetSection("easySection") as EasyExportedFoldersConfigurationSection;
        }
        public static string[] GetMetadataFiles()
        {
            string[] files = new string[0];
            Exception error= null;
            try
            {
                files = Directory.GetFiles(new EasyDocExtractionHelper()._easySection.Items[0].Value);
            }
            catch(Exception ex)
            {
                error = ex;
            }
            if(error != null || files.Length == 0)
                throw new Exception("Files cannot be pulled from the folder in the easySection ", error);

            return files;
        }
    }
}
