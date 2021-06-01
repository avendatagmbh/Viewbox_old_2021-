using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyDocExtraction.Converter;
using System.Reflection;
using EasyDocExtraction.Model;
using EasyDocExtraction.Helper;

namespace EasyDocExtraction.Factory
{
    public class ArchivedItemFactory<TArchivedItemConverter> where TArchivedItemConverter : IArchivedItemConverter
    {
        /// <summary>
        /// Build all ArchivedItem from the rawdata pulled from Easy (contained in the IEasyMetadataExtractor)
        /// </summary>
        /// <param name="extractor"></param>
        /// <returns></returns>
        public static List<ArchivedItem> GetArchivedItems(TArchivedItemConverter Tconverter, IEasyMetadataExtractor extractor, Action<List<ArchivedItem>> saveByChunkAction) 
        {
            EasyFolder easyContainer = EasyFolderFactory.CreateGetEasyFolder(extractor.GetDatabaseInfo());

            List<FieldDefinition> fieldDefinitions = FieldDefinitionFactory.CreateGetFieldsDefinition<FieldDefinitionConverter>(extractor.GetMetadataHeaders(), easyContainer);

            // Easy has one container (folder) for a bunch of archive item

            List<ArchivedItem> archivedItems = new List<ArchivedItem>();
            IEnumerable<string> metadataRawValues = extractor.GetMetadataBodies();

            // gets the chunk size the data should be and notify when available
            int chunkSize = ConfigurationHelper.GetDataChunckSize();
            // iterates per raw archive item data
            foreach (var metadataRawValue in metadataRawValues)
            {
                var archivedItem = GetArchivedItem(Tconverter, metadataRawValue, fieldDefinitions);

                archivedItem.EasyFolder = easyContainer;
                archivedItem.EasyFolderId = easyContainer.EasyFolderId;

                archivedItems.Add(archivedItem);
                chunkSize--;

                // notify when chunk is available
                if (saveByChunkAction != null && chunkSize == 0)
                {
                    saveByChunkAction(archivedItems);
                    archivedItems = new List<ArchivedItem>();
                    chunkSize = ConfigurationHelper.GetDataChunckSize();
                }
            }
            return archivedItems;
        }
        public static List<ArchivedItem> GetArchivedItems(IEasyMetadataExtractor extractor, Action<List<ArchivedItem>> saveByChunkAction)
        {
            return GetArchivedItems(default(TArchivedItemConverter), extractor, saveByChunkAction);
        }
        /// <summary>
        /// Build the ArchiveItem from the raw data with the help of the fieldDefinitions.
        /// </summary>
        /// <param name="mainConverter">can be null, argument provided for test purposes</param>
        /// <param name="metadataRawValue"></param>
        /// <param name="fieldDefinitions"></param>
        /// <returns></returns>
        private static ArchivedItem GetArchivedItem(IArchivedItemConverter mainConverter, string metadataRawValue, List<FieldDefinition> fieldDefinitions)
        {
            if (mainConverter == null) 
                mainConverter = ReflectionHelper.CreateInstance<TArchivedItemConverter>(metadataRawValue, fieldDefinitions);

            ArchivedItem archivedItem = mainConverter.GetArchivedItem();
            archivedItem.FieldValues = mainConverter.FieldsValueConverter.GetFieldsValue();
            archivedItem.ArchivedDocuments = mainConverter.ArchivedDocumentConverter.GetArchivedDocuments();

            return archivedItem;
        }
    }
}
