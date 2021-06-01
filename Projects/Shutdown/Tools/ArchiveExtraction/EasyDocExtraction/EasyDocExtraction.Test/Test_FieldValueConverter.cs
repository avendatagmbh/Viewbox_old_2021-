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
    public class Test_FieldValueConverter: Test_Base
    {
        [TestMethod, ExpectedException(typeof(Exceptions.ConversionException))]
        public void GetCode_Call_With_Wrong_Argument_Should_Throw_An_Exception() 
        {
            new PrivateObject(base._fieldDefinitionConverter).Invoke("GetCode", (object)new string[] { "BUG", "BUG", "BUG" });
        }
    }
}
