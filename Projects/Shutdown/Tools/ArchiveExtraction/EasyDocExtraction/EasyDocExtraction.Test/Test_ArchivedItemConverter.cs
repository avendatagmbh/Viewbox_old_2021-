using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyDocExtraction.Converter;

namespace EasyDocExtraction.Test
{
    [TestClass]
    public class Test_ArchivedItemConverter: Test_Base
    {
        /// <summary>
        /// FieldValueConverter constructor Needs testing As the instance is built through reflection, 
        /// thus not compile time support in case the constructor definition changes.
        /// </summary>
        [TestMethod]
        public void Call_To_FieldValueConverter_Property_Should_Return_An_Instance_Sucessfully()
        {
            Assert.IsNotNull(_aic.FieldsValueConverter);
        }
        [TestMethod]
        public void Call_To_FieldDefinitionConverter_Property_Should_Return_An_Instance_Sucessfully()
        {
            Assert.IsNotNull(_aic.FieldDefinitionConverter);
        }
        [TestMethod]
        public void Call_To_GetArchivedItem_Method_Should_Return_An_Instance_Sucessfully()
        {
            Assert.IsNotNull(_aic.ArchivedDocumentConverter);
        }
        [TestMethod]
        public void Call_To_ArchivedDocumentConverter_Property_Should_Return_An_Instance_Sucessfully()
        {
            Assert.IsNotNull(_aic.GetArchivedItem());
        }
    }
}
