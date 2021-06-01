using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;

namespace Viewbox.Test
{
    public class BaseControllerTest<T> : BaseTest, IGenerateTest where T : ControllerBase
    {
        protected T Controller;

        #region IGenerateTest Members

        public string GetFileContent(string oldContent)
        {
            StringBuilder result = new StringBuilder();
            StringBuilder builder = new StringBuilder();

            foreach (
                MethodInfo info in
                    typeof (T).GetMethods().Where(
                        info =>
                        GenerateTests.IgnoredMethods.All(w => w != info.Name) && info.DeclaringType == typeof (T)))
            {
                builder.Clear();
                string methodName = builder.Append(info.Name).Append("Test").ToString();
                if (!oldContent.Contains(methodName))
                {
                    result.AppendLine();

                    result.Append("\t\t").AppendLine("[TestMethod]");
                    result.Append("\t\t").Append("public void ").Append(methodName).AppendLine("() {");
                    result.Append("\t\t\t").Append("// ActionResult result = Controller.").Append(info.Name).Append("(");
                    result.AppendLine(");");
                    result.Append("\t\t\t").AppendLine("// Assert.IsNotNull(result);");
                    result.Append("\t\t\t").AppendLine("Assert.IsNotNull(null);");
                    result.Append("\t\t").AppendLine("}").AppendLine();
                }
            }
            result.Append(GenerateTests.GenerateCodeString);
            return result.ToString();
        }

        #endregion

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            Controller = Activator.CreateInstance<T>();
            Assert.IsNotNull(Controller);
            Controller.ControllerContext =
                new ControllerContext(new RequestContext(HttpContextFactory.Current, new RouteData()), Controller);
            var baseController = Controller as BaseController;
            if (baseController == null) return;
            baseController.BeforeInitialize(null);
            baseController.AfterInitialize(null);
        }
    }
}