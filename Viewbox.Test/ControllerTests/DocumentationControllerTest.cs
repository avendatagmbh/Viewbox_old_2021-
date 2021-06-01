/* 
 * las - 2013.01.18
 * If you want to generate tests for a Controller, you should copy this class, uncomment the base code, and replace the % sign with your class name.
 * If it is ready, you can run the GenerateTests.GenerateAllTests test, and it will fill the missing TestMethods.
 * If you added some new methods to the Controller, the GenerateAllTests will handle it, and it only creates the missing methods' tests.
 * After generation, the new test methods will be commented out. If you specify the necessary parameters correctly, you can uncomment it.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;
namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class DocumentationControllerTest : BaseControllerTest<DocumentationController>
    {
        
		[TestMethod]
		public void IndexTest() {
            ActionResult result = Controller.Index();
            Assert.IsNotNull(result);
		}
// GenerateCode
    }
}
