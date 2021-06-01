using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Viewbox.Test
{
    [TestClass]
    public class GenerateTests
    {
        public static string GenerateCodeString = "// GenerateCode";
        public static string[] IgnoredMethods = new[] {"ToString", "GetType", "Equals"};

        [TestMethod]
        public void GenerateAllTest()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "../../../";
            foreach (
                Type t in
                    Assembly.GetExecutingAssembly().GetTypes().Where(
                        w => typeof (IGenerateTest).IsAssignableFrom(w) && !w.ContainsGenericParameters && !w.IsAbstract)
                )
            {
                ConstructorInfo ctor = t.GetConstructor(new Type[] {});
                if (ctor == null)
                    continue;
                object instance = ctor.Invoke(new object[] {});
                string fileName = path + t.Namespace.Replace("Viewbox.Test.", "") + "/" + t.Name + ".cs";
                string oldFileContent = File.ReadAllText(fileName);
                string newFileContent = ((IGenerateTest) instance).GetFileContent(oldFileContent);
                if (oldFileContent != newFileContent)
                    File.WriteAllText(fileName, oldFileContent.Replace(GenerateCodeString, newFileContent));
            }
        }
    }
}