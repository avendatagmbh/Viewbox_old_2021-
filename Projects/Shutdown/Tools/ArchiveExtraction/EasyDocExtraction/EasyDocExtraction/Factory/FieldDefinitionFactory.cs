using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using EasyDocExtraction.Model;
using EasyDocExtraction.Converter;
using EasyDocExtraction.DataAccess;

namespace EasyDocExtraction.Factory
{
    public class FieldDefinitionFactory
    {
        static List<FieldDefinition> _fieldsDefinition = new List<FieldDefinition>();


        /// <summary>
        /// Creates and holds an array of fieldDefinitions (Metadata definitions of the field of an archived document) 
        /// (information about the data)
        /// </summary>
        /// <param name="fieldsDefinitionRawData"></param>
        public static List<FieldDefinition> CreateGetFieldsDefinition<TFieldDefinitionConverter>(IEnumerable<string> fieldsDefinitionRawData, EasyFolder easyDBFolder) where TFieldDefinitionConverter : IFieldDefinitionConverter, new()
        {
            var tempList = new List<FieldDefinition>();
            foreach (var fd in new TFieldDefinitionConverter().ConvertRawDataToFieldsDefinition(fieldsDefinitionRawData)) {
                if (!_fieldsDefinition.Exists(d =>
                        d.FieldDefinitionId == fd.FieldDefinitionId && d.EasyFolder.Name.Equals(easyDBFolder.Name)))
                {
                    fd.EasyFolder = easyDBFolder; // sets the 2nd PK and FK of the FieldDefinition
                    fd.EasyFolderId = easyDBFolder.EasyFolderId;
                    tempList.Add(fd);
                }
            }
            if (tempList.Count > 0) 
            {
                using (var repository = new EasyArchiveRepository())
                {
                    tempList.ForEach(fd => 
                    {
                        //repository.Entry<EasyFolder>(fd.EasyFolder).State = System.Data.EntityState.Unchanged;
                        repository.FieldDefinitions.Add(fd);
                    });
                    repository.SaveChanges();
                    // gets the field definition related to the current DB
                    _fieldsDefinition = repository.FieldDefinitions.Where(fd => fd.EasyFolderId == easyDBFolder.EasyFolderId).ToList();
                }
            }

            return _fieldsDefinition;
        }

        public static List<FieldDefinition> FieldsDefinition
        {
            get { return FieldDefinitionFactory._fieldsDefinition; }
        }

        internal static FieldDefinition CreateUnknown(int code, EasyFolder easyFolder)
        {
            return new FieldDefinition { FieldDefinitionId = code, EasyFolder = easyFolder, EasyFolderId = easyFolder.EasyFolderId, FieldName="UNKNOWN" };
        }
    }
}
