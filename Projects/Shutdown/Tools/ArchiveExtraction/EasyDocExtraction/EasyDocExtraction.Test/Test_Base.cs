using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyDocExtraction.Test.Helper;
using EasyDocExtraction.Converter;
using EasyDocExtraction.Model;
using EasyDocExtraction.Factory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyDocExtraction.Helper;
using Moq;

namespace EasyDocExtraction.Test
{
    public abstract class Test_Base
    {

        protected EasyMetadataExtractor _extractor;
        protected FieldDefinitionConverter _fieldDefinitionConverter;
        protected FieldValueConverter _fValueconverter;
        protected ArchivedDocumentConverter _arcDocConverter;
        protected List<FieldDefinition> _fieldDefinitions;
        protected string _currentFile;
        protected string[] _archiveRawValues;
        protected string[] _fieldDefinitionRawValues;
        protected ArchivedItemConverter<FieldValueConverter, FieldDefinitionConverter, ArchivedDocumentConverter> _aic;
        protected Factory.ArchivedItemFactory<ArchivedItemConverter<FieldValueConverter, FieldDefinitionConverter, ArchivedDocumentConverter>> _archFactory;

        [ClassInitialize]
        public static void AssemblyInit()
        {   
        }
        protected Test_Base(bool buildDefault = true)
        {

            if (buildDefault)
            {
                BuildObjects(EasyDocExtractionHelper.GetMetadataFiles()[0]);
            }
        }

        protected List<ArchivedItem> GetArchivesFromEasyFolder(string easyFolderPath) 
        {
            BuildObjects(easyFolderPath);

            return  ArchivedItemFactory<ArchivedItemConverter<FieldValueConverter, FieldDefinitionConverter, ArchivedDocumentConverter>>.GetArchivedItems(
                _aic,
                _extractor, null);
        }

        protected void BuildObjects(string easyFolderPath)
        {
            _currentFile = easyFolderPath;
            EasyExtractionMain.CurrentDbFile = _currentFile;
            _extractor = new EasyMetadataExtractor(_currentFile);

            _fieldDefinitionConverter = new FieldDefinitionConverter();

            _archiveRawValues = _extractor.GetMetadataBodies().ToArray();
            _fieldDefinitionRawValues = _extractor.GetMetadataHeaders().ToArray();
            _fieldDefinitions = _fieldDefinitionConverter.ConvertRawDataToFieldsDefinition(_fieldDefinitionRawValues);

            _fValueconverter = new FieldValueConverter(_archiveRawValues, _fieldDefinitionRawValues, _fieldDefinitions);

            _arcDocConverter = new ArchivedDocumentConverter(_archiveRawValues, _fieldDefinitionRawValues);

            LoadArchive(0);
        }
        protected void LoadArchive(int archiveIndex) { 
            _aic =GetArchiveItemConverter(_archiveRawValues[archiveIndex], 
                _fieldDefinitions);
        }
        protected ArchivedItemConverter<FieldValueConverter, FieldDefinitionConverter, ArchivedDocumentConverter> GetArchiveItemConverter(string rawArchiveValue, List<FieldDefinition> fieldDefinition) {
            return new ArchivedItemConverter<FieldValueConverter, FieldDefinitionConverter, ArchivedDocumentConverter>(
                rawArchiveValue,
                _fieldDefinitions); 
        }
    }
}
