using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyDocExtraction.Converter;
using EasyDocExtraction.Test.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using System.IO;
using EasyDocExtraction.Factory;
using System.Reflection;
using EasyDocExtraction.Model;

namespace EasyDocExtraction.Test
{
    [TestClass]
    public class Test_ArchivedDocumentConverter : Test_Base
    {
        
        public Test_ArchivedDocumentConverter():base() 
        { 
        
        }
        /// <summary>
        /// Helper method
        /// </summary>
        /// <param name="metadataRawValue"></param>
        /// <param name="fieldDefinitions"></param>
        /// <returns></returns>
        private ArchivedItem GetArchivedItem(string metadataRawValue, List<FieldDefinition> fieldDefinitions)
        {
            return  (ArchivedItem)typeof(ArchivedItemFactory<ArchivedItemConverter<FieldValueConverter, FieldDefinitionConverter, ArchivedDocumentConverter>>)
                .GetMethod("GetArchivedItem", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] {null, metadataRawValue, fieldDefinitions });
            //var archiveItem = ArchivedItemFactory<ArchivedItemConverter<FieldValueConverter, FieldDefinitionConverter, ArchivedDocumentConverter>>.GetArchivedItem(rawdata, _fDefList);
        }
        [TestMethod]
        public void GetCurrentFilePath_Should_Return_The_Path_To_Valid_Folder()
        {
            foreach (var rawdata in _extractor.GetMetadataBodies())
            {
                var archiveItem = GetArchivedItem(rawdata, _fieldDefinitions);

                archiveItem.ArchivedDocuments.ToList().ForEach(a =>
                {
                    Assert.IsTrue(File.Exists(new PrivateObject(_arcDocConverter).Invoke("GetCurrentFilePath", a.DocumentPath) as string),
                    "GetCurrentFilePath should return a valid document path.");

                    Assert.IsTrue((new PrivateObject(_arcDocConverter).Invoke("GetCurrentFilePath", a.DocumentPath) as string).Contains(
                        Path.GetFileNameWithoutExtension(a.DocumentPath)),
                                    "GetCurrentFilePath should return a path that contains the document path name.");
                });
            }
        }
        [TestMethod]
        public void Archive_Were_Document_File_Is_Missing_Should_Have_An_Empty_DocumentData_Property() 
        { 
            //Set a wrong path for the current DB file
            EasyExtractionMain.CurrentDbFile = Assembly.GetExecutingAssembly().Location;
            foreach (var rawdata in _extractor.GetMetadataBodies())
            {
                var archiveItem = GetArchivedItem(rawdata, _fieldDefinitions);

                archiveItem.ArchivedDocuments.ToList().ForEach(a =>
                {
                    Assert.IsNull(a.DocumentData , "DocumentData should be null when file not found.");

                });
                break; // no need for more 
            }
        }
        /// <summary>
        /// documents should still be found if unmaching fielddefinition by searching at the end of the field value
        /// without the help of the field definition.
        /// </summary>
        [TestMethod]
        public void FieldValues_With_Unmatching_FieldDefinitions_Should_Handle_Document_Extraction_Smoothly() {
            // build the converters with a corrupted file
            BuildObjects((EasyDocExtractionHelper.GetMetadataFiles().Single(f => f.Contains("CORRUPTED"))));
            LoadArchive(0);

            string[] fieldValues = new PrivateObject(_aic).GetField("_fieldValueArray") as string[];
            string[] fieldValueHeaders = new PrivateObject(_aic).GetField("_fieldValueHeaderArray") as string[];

            if (fieldValues.Length != fieldValueHeaders.Length)
            {
                foreach (var doc in new ArchivedDocumentConverter(fieldValues, fieldValueHeaders).GetArchivedDocuments())
                { 
                    Assert.IsTrue(doc.DocumentData != new byte[0], "DocumentData should not be null.");
                }
            }


        }
    }
}
