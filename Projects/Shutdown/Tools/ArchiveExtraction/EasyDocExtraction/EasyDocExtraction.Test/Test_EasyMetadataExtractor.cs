using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyDocExtraction;
using System.Configuration;
using EasyDocExtraction.Test.Helper;
using System.IO;
using System.Text.RegularExpressions;

namespace EasyDocExtraction.Test
{
    [TestClass]
    public class Test_EasyMetadataExtractor
    {
        [TestMethod]
        public void EasyMetadataExtractor_GetMetadataHeaders_ShouldReturn_MetadataDefinitions()
        {
            var extractor = new EasyMetadataExtractor(EasyDocExtractionHelper.GetMetadataFiles()[0]);
            var metadataDef = extractor.GetMetadataHeaders();

            Assert.IsTrue(metadataDef.Count() > 0, "No headers information found with GetMetadataHeaders.");
            Assert.IsTrue(metadataDef.ToArray()[0].IndexOf("@FLDDSC") == 0, "@FLDDSC should be the first string in the line.");
        }
        [TestMethod]
        public void EasyMetadataExtractor_GetDatabaseInfo_ShouldReturn_NameOfFile_If_No_DB_Prefix()
        {
            var dbFilePath = EasyDocExtractionHelper.GetMetadataFiles()[0];
            var extractor = new EasyMetadataExtractor(dbFilePath);
            var _linesField = new PrivateObject(extractor);
            // change the private _lines field by removing the line that starts with @DB to force the execution of the else part of the statment
            _linesField.SetField("_lines", (_linesField.GetField("_lines") as string[]).Where(l => !l.StartsWith("@DB")).ToArray());
            var dbInfo = extractor.GetDatabaseInfo();

            Assert.IsTrue(dbInfo == Path.GetFileNameWithoutExtension(dbFilePath), "The DB file without extension should be equal to the GetDatabaseInfo output if no @DB prefix in the DB file found.");
        }
        [TestMethod]
        public void EasyMetadataExtractor_GetDatabaseInfo_ShouldReturn_DatabaseInfo()
        {
            var dbFilePath = EasyDocExtractionHelper.GetMetadataFiles()[0];
            var extractor = new EasyMetadataExtractor(dbFilePath);
            var _linesField = new PrivateObject(extractor);
            // change the private _lines field by removing the line that starts with @DB to force the execution of the else part of the statment
            var dbLine = (_linesField.GetField("_lines") as string[]).First();
            var dbInfo = extractor.GetDatabaseInfo();

            Assert.IsTrue(dbInfo == Regex.Replace(dbLine, @"[@,)(#$]", ""), "The DB file without extension should be equal to the GetDatabaseInfo output if no @DB prefix in the DB file found.");
        }
        [TestMethod]
        public void EasyMetadataExtractor_GetMetadataBodies_Should_Return_MetadataInformations()
        {

            var extractor = new EasyMetadataExtractor(EasyDocExtractionHelper.GetMetadataFiles()[0]);
            var metadataInfo = extractor.GetMetadataBodies();

            Assert.IsTrue(metadataInfo.Count() > 0, "No field informations found with GetMetadataBodies.");
            Assert.IsTrue(metadataInfo.ToArray()[0].IndexOf("@FOLDER") == 0, "@FOLDER should be the first string in the line.");
        }
    }
}
