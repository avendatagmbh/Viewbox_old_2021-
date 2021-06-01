using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EasyDocExtraction.Exceptions;
using System.IO;
using System.Diagnostics.Contracts;
using EasyDocExtraction.Factory;
using EasyDocExtraction.Model;
using System.Reflection;
using EasyDocExtraction.Helper;
using System.Configuration;

namespace EasyDocExtraction.Converter
{
    public class ArchivedItemConverter<TFieldValueConverter, TFieldDefinitionConverter, TArchivedDocumentConverter> : IArchivedItemConverter
        where TFieldValueConverter : IFieldValueConverter
        where TFieldDefinitionConverter : IFieldDefinitionConverter, new()
        where TArchivedDocumentConverter : IArchivedDocumentConverter
    {

        string _metadataRawValue;
        List<FieldDefinition> _fieldDefinitionList;
        string _fieldsValueHeader;
        string _fieldsValue;

        string[] _fieldValueHeaderArray;
        string[] _fieldValueArray;

        public ArchivedItemConverter(string metadataRawValue, List<FieldDefinition> fieldDefinitions) 
        {
            char[] trailingChars = new[] {'\r', '\n'};

            _metadataRawValue = metadataRawValue;
            _fieldDefinitionList = fieldDefinitions;
            int headerIndex = _metadataRawValue.IndexOf("\r\n"); // the index where starts the fields value and where ends the field definition
            _fieldsValueHeader = _metadataRawValue.Substring(0, headerIndex); // is the first line that contains the order in which field values are set
            // splits and skips the first two elements who are @FOLDER and FT:xxx
            _fieldValueHeaderArray = _fieldsValueHeader.Split(',').Skip(2).ToArray();

            _fieldsValue = _metadataRawValue.Substring(headerIndex); // contains the lines with the raw values

            // removes the trailing chars at the begining end the end for the split to be clean
            _fieldsValue = _fieldsValue.Trim(trailingChars);
            if (_fieldsValue.IndexOf("^") == 0) _fieldsValue = _fieldsValue.Remove(0,1);
            if (_fieldsValue.LastIndexOf("^") == _fieldsValue.Length - 1) _fieldsValue = _fieldsValue.Remove(_fieldsValue.Length -1, 1);
            _fieldValueArray = _fieldsValue.Split(new[] { "^,^" }, StringSplitOptions.None);


        }
        public IFieldValueConverter FieldsValueConverter
        {
            get
            {
                return ReflectionHelper.CreateInstance<TFieldValueConverter>(_fieldValueArray, _fieldValueHeaderArray, _fieldDefinitionList);
            }
        }
        public IFieldDefinitionConverter FieldDefinitionConverter
        {
            get
            {
                return new TFieldDefinitionConverter();
            }
        }

        public IArchivedDocumentConverter ArchivedDocumentConverter
        {
            get
            {
                return ReflectionHelper.CreateInstance<TArchivedDocumentConverter>(_fieldValueArray, _fieldValueHeaderArray);
            }
        }

        public ArchivedItem GetArchivedItem()
        {
            bool corruptedData = _fieldValueArray.Length != _fieldValueHeaderArray.Length;
            return new ArchivedItem() { CorruptedData = corruptedData };
        }
    }

    public class FieldDefinitionConverter: IFieldDefinitionConverter
    {
        #region Public methods
        public List<FieldDefinition> ConvertRawDataToFieldsDefinition(IEnumerable<string> fieldsDefinitionRawData)
        {
            var fieldsDefinition = new List<FieldDefinition>();

            foreach (var definition in fieldsDefinitionRawData)
            {
                var defItems = Regex.Replace(definition, "\r|\n", "").Split(',');
                var fd = new FieldDefinition();
                fd.FieldDefinitionId = GetCode(defItems);
                fd.FieldFormat = GetFieldFormat(defItems);
                fd.FieldName = GetFieldName(defItems);
                fd.FieldType = GetFieldType(defItems);
                fieldsDefinition.Add(fd);
            }
            return fieldsDefinition;
        }
        #endregion

        #region Private Methods
        private FieldType GetFieldType(string[] items)
        {
            // if field name starts with a dot, we have a SystemField
            return (FieldType)(items[3].StartsWith(".") ? FieldType.SystemField : FieldType.CustomField);
        }

        private string GetFieldName(string[] items)
        {
            return items[3].Replace(".", "");
        }

        private FieldFormat GetFieldFormat(string[] items)
        {
            FieldFormat fieldFormat =  FieldFormat.None;
            try
            {
                // field formats start from the index 3 in the splitted string till the end 
                foreach (var value in items.Skip(4)) fieldFormat |= (FieldFormat)Enum.Parse(typeof(FieldFormat), value);
            }
            catch (Exception ex) 
            {
                Logger.WriteError("Could not convert to FieldFormat, unexpected value found in : " + string.Join(",", items), ex);
            }
            return fieldFormat;
        }

        private int GetCode(string[] items)
        {
            try
            {
                return int.Parse(items[2]);
            }
            catch(Exception ex)
            {
                throw new Exceptions.ConversionException("Code in items[2] cannot be converted as an int , value is : " + items[2], ex);
            }
        }
        #endregion
    }
    public class ArchivedDocumentConverter : IArchivedDocumentConverter
    {

        string[] _fieldValueHeaderArray;
        string[] _fieldValueArray;

        public ArchivedDocumentConverter(string[] fieldValueArray, string[] fieldValueHeaderArray) 
        {
            _fieldValueHeaderArray = fieldValueHeaderArray;
            _fieldValueArray = fieldValueArray;
        }
        public List<ArchivedDocument> GetArchivedDocuments()
        {
            // gets document path in string that should be found at the following match:
            // matches strings that starts with BI or FI:2001 (FI:2002, etc.)
            Regex regxRules = new Regex(@"BI:|FI:2\d{3}"); 
            int startIndex = _fieldValueHeaderArray.ToList().FindIndex(v => regxRules.Match(v).Success);
            var archivedDocuments = new List<ArchivedDocument>();

            // if startindex is -1 , we have no linked documents attached to this archived
            if (startIndex == -1) return archivedDocuments;

            string[] filePathArray = _fieldValueHeaderArray.Where(v => regxRules.Match(v).Success).ToArray();

            // loop though the documents linked to this archive, starting from the position of the first BI field definition (valueHeader) index 
            // and creates the archivedDocument object
            for (var i = startIndex; i < startIndex + filePathArray.Length; i++)
            {
                var archivedDocument = new ArchivedDocument();
                // originalDocumentPath is the path where the Easy software extractor exported the file
                string originalDocumentPath = string.Empty;
                
                if (_fieldValueArray.Length - 1 >= i)
                    originalDocumentPath = _fieldValueArray[i];

                // if we dont have something that looks like a valid path, raw data are probably corrupted, one other way
                // to gets the path is to take the end of the fieldvalueArray (which most likely contains file paths)
                if (originalDocumentPath.IndexOf("\\") == -1 || originalDocumentPath == string.Empty)
                {
                    // reset the index of the loop to te last elements in the value array minus the count of filePathArray
                    startIndex = _fieldValueArray.Length - filePathArray.Length;
                    i = startIndex;

                    originalDocumentPath = _fieldValueArray[i];
                }
                archivedDocument.DocumentPath = originalDocumentPath;

                // the location where the files are (based on the config root folder key)
                string newDocumentPath = GetCurrentFilePath(originalDocumentPath);

                var documentType = DocumentTypeFactory.GetDocumentTypeFromPath(newDocumentPath);
                archivedDocument.DocumentType = documentType;
                archivedDocument.DocumentTypeId = documentType.DocumentTypeId;

                if (!File.Exists(newDocumentPath))
                {
                    Logger.WriteError("\"{0}\" is not a valid file path. FieldValueHeader is : \"{1}\".",
                        newDocumentPath, string.Join(", ", _fieldValueHeaderArray));
                }
                else
                {
                    Logger.Write( "Reading document: (originalDocumentPath : {0})", originalDocumentPath);
                    byte[] bytes = new byte[0];
                    try
                    {
                        bytes = File.ReadAllBytes(newDocumentPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError(newDocumentPath + " file cannot be read.", ex);
                    }

                    archivedDocument.DocumentData = bytes;
                }

                archivedDocuments.Add(archivedDocument);
            }

            return archivedDocuments;

        }
        /// <summary>
        /// Returns the new path based on the filename and where the file resides
        /// </summary>
        /// <param name="originalFilePath"></param>
        /// <returns></returns>
        string GetCurrentFilePath(string originalFilePath) 
        {
            string[] folders = originalFilePath.Split(Path.DirectorySeparatorChar);
            string subFolder = folders[folders.Length - 2];
            try
            {
                return Path.Combine(Path.GetDirectoryName(EasyExtractionMain.CurrentDbFile), subFolder, Path.GetFileName(originalFilePath));
            }
            catch (Exception ex) {
                Logger.WriteError("GetCurrentFilePath method fails with argument \"" + originalFilePath + "\"", ex);
                throw;
            }
        }
    }

    public class FieldValueConverter : IFieldValueConverter
    {
        List<FieldDefinition> _fieldDefinitionList;
        string[] _fieldValueHeaderArray;
        string[] _fieldValueArray;

        public FieldValueConverter(string[] fieldValueArray, string[] fieldValueHeaderArray, List<FieldDefinition> fieldsDefinition) 
        {
            _fieldDefinitionList = fieldsDefinition;
            _fieldValueHeaderArray = fieldValueHeaderArray;

            _fieldValueArray = fieldValueArray;
        }
        /// <summary>
        /// Convert the raw Easy data containing the values to objects using the field definition in the raw data
        /// process in sequence to respect the given order of the raw file.
        /// </summary>
        /// <param name="_metadataRawValue"></param>
        /// <param name="_fieldsDefinition"></param>
        /// <returns></returns>
        public List<FieldValue> GetFieldsValue()
        {
            var fieldValues = new List<FieldValue>();
            
            // Select all field info from the header (containes the code of the field )
            //  the matching correspondence should be found in the fieldsDefinition
            var FIElements = _fieldValueHeaderArray.Where(v => v.StartsWith("FI"));
            int counter = -1;
            foreach (var FI in FIElements)
            {
                var fieldValue = new FieldValue();
                int code;
                // we have a blocking issue if the part after "FI:" is not an integer
                if (!int.TryParse(FI.Split(':')[1], out code)) throw new ConversionException(string.Format("The code \"{0}\" cannot be conveter to an integer.", FI.Split(':')[1]));

                var fieldDefinition = _fieldDefinitionList.Find(f => f.FieldDefinitionId == code);

                if (fieldDefinition == null) 
                { 
                    // creates an unrelated field definition
                    fieldDefinition = FieldDefinitionFactory.CreateUnknown(code, _fieldDefinitionList[0].EasyFolder);
                    // throw new ConversionException("No FieldDefinition found for code " + code);
                }
                counter++;
                string concreteValue = string.Empty;
                if (counter <= _fieldValueArray.Length - 1)
                {
                    concreteValue = _fieldValueArray[counter].Trim();
                }

                fieldValue.EasyFolderId = fieldDefinition.EasyFolderId;
                fieldValue.FieldDefinition = fieldDefinition;
                fieldValue.FieldDefinitionId = fieldDefinition.FieldDefinitionId;
                fieldValue.Value =  concreteValue;
                
                fieldValues.Add(fieldValue);
            }

            // handles case where data are corrupted: store all values in a field
            if (_fieldValueHeaderArray.Length != _fieldValueArray.Length)
            {
                var curruptedFieldDefinition = FieldDefinitionFactory.CreateUnknown(0, _fieldDefinitionList[0].EasyFolder);
                fieldValues.Add(new FieldValue()
                    {
                        EasyFolderId = _fieldDefinitionList[0].EasyFolder.EasyFolderId,
                        FieldDefinition = curruptedFieldDefinition,
                        FieldDefinitionId = curruptedFieldDefinition.FieldDefinitionId,
                        Value = String.Join("|", _fieldValueArray)
                    });
            }
            return fieldValues;
        }
    }

    class DocumentTypeConverter 
    {
        public static DocumentType GetDocumentTypeDocumentPath(string documentPath) { 
            Contract.Requires(!string.IsNullOrWhiteSpace(documentPath));

            return new DocumentType { Name = GetNameFromPath(documentPath) };
        }
        public static string GetNameFromPath(string documentPath)
        {
            return Path.HasExtension(documentPath) ? Path.GetExtension(documentPath).ToLower() : "none";
        }
    }
}
