using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyDocExtraction.Model;

namespace EasyDocExtraction.Converter
{
    public interface IFieldDefinitionConverter
    {
        List<FieldDefinition> ConvertRawDataToFieldsDefinition(IEnumerable<string> fieldsDefinitionRawData);
    }
    public interface IFieldValueConverter
    {
        List<FieldValue> GetFieldsValue();
    }
    public interface IArchivedDocumentConverter
    {
        List<ArchivedDocument> GetArchivedDocuments();
    }
    public interface IArchivedItemConverter
    {
        ArchivedItem GetArchivedItem();

        IFieldValueConverter FieldsValueConverter { get; }
        IFieldDefinitionConverter FieldDefinitionConverter { get; }
        IArchivedDocumentConverter ArchivedDocumentConverter { get; }
    }
}
