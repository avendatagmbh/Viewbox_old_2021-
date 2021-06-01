using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using EasyDocExtraction.Converter;
using EasyDocExtraction.Model;
using EasyDocExtraction.Factory;
using EasyDocExtraction.DataAccess;
using System.Data.Entity;
using EasyDocExtraction.Helper;

namespace EasyDocExtraction
{
    public class EasyExtractionMain
    {

        /// <summary>
        /// Holds a reference to the current DB being processed.
        /// </summary>
        public static string CurrentDbFile { get; set; }

        public EasyExtractionMain(string dbFile) 
        {
            CurrentDbFile = dbFile;
        }
        /// <summary>
        /// Creates archives base on the database raw easy file
        /// </summary>
        /// <returns></returns>
        public List<ArchivedItem> GetArchives() 
        {
            return  ArchivedItemFactory<ArchivedItemConverter<FieldValueConverter, FieldDefinitionConverter, ArchivedDocumentConverter>>
                    .GetArchivedItems(new EasyMetadataExtractor(CurrentDbFile), SaveByChunkAction);
        }

        public Action<List<ArchivedItem>> SaveByChunkAction { get; set; }
    }
}
