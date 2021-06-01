using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyDocExtraction.Test.Helper;
using EasyDocExtraction.Factory;
using EasyDocExtraction.Converter;
using EasyDocExtraction.Model;

namespace EasyDocExtraction.Test
{
    [TestClass]
    public class Test_EasyRepository: Test_Base
    {
        public Test_EasyRepository() : base(false) { }

        [TestMethod]
        public void EasyFolder_Belonging_To_Archives_Sould_Also_Belong_To_Definitions_Of_Values() 
        {
            foreach (var easyFolder in EasyDocExtractionHelper.GetMetadataFiles()) 
            {
                EasyExtractionMain.CurrentDbFile = easyFolder;

                List<ArchivedItem> archives = ArchivedItemFactory<ArchivedItemConverter<FieldValueConverter, FieldDefinitionConverter, ArchivedDocumentConverter>>
                    .GetArchivedItems(new EasyMetadataExtractor(easyFolder), null);
                var pooler = archives.GetEnumerator();
                while (pooler.MoveNext() && !pooler.Current.CorruptedData) 
                {
                    var archive = pooler.Current;
                    Assert.IsTrue(archive.FieldValues.ToList().TrueForAll(f => f.FieldDefinition.EasyFolder.Name.Equals(archive.EasyFolder.Name)));
                    
                }
                //foreach (var archive in archives)
                //{
                //    Assert.IsTrue(archive.FieldValues.ToList().TrueForAll(f => f.FieldDefinition.EasyFolder.Name.Equals(archive.EasyFolder.Name)));
                //}
            }
        }
    }
}
