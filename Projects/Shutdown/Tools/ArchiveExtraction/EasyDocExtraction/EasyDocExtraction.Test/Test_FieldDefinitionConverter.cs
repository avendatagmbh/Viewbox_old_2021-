using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyDocExtraction.Converter;
using EasyDocExtraction.Test.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace EasyDocExtraction.Test
{
    [TestClass]
    public class Test_FieldDefinitionConverter: Test_Base
    {

        public Test_FieldDefinitionConverter()
        {
            _fieldDefinitionConverter = new FieldDefinitionConverter();
        }

        [TestMethod]
        public void FieldDefinitionConverter_Should_Return_Valid_Field_Definitions_From_RawData()
        {

            Assert.IsTrue(_fieldDefinitions.Count > 0);
            _fieldDefinitions.ForEach(fd => Assert.IsTrue(fd.FieldDefinitionId > 0));
            _fieldDefinitions.ForEach(fd => Assert.IsTrue(Regex.Match(fd.FieldName, "\\w").Success , "FieldName should contain only characters."));
        }
        [TestMethod, ExpectedException(typeof(Exceptions.ConversionException))]
        public void GetCode_Call_With_Wrong_Argument_Should_Throw_An_Exception() 
        {
            new PrivateObject(_fieldDefinitionConverter).Invoke("GetCode", (object)new string[] { "BUG", "BUG", "BUG" });
        }
    }
}
