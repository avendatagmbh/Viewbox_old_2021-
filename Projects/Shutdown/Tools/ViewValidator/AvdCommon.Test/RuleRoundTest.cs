using AvdCommon.Rules.ExecuteRules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AvdCommon.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "RuleRoundTest" und soll
    ///alle RuleRoundTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class RuleRoundTest {


        private TestContext testContextInstance;

        /// <summary>
        ///Ruft den Testkontext auf, der Informationen
        ///über und Funktionalität für den aktuellen Testlauf bietet, oder legt diesen fest.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Zusätzliche Testattribute
        // 
        //Sie können beim Verfassen Ihrer Tests die folgenden zusätzlichen Attribute verwenden:
        //
        //Mit ClassInitialize führen Sie Code aus, bevor Sie den ersten Test in der Klasse ausführen.
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Mit ClassCleanup führen Sie Code aus, nachdem alle Tests in einer Klasse ausgeführt wurden.
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen.
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod()]
        public void ExecuteTest() {
            RuleRound target = new RuleRound(){RoundToDecimal = 2};
            string value = "10,1553";
            string expected = "10,16";
            
            string actual = target.Execute(value);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ExecuteTest_ThreeDigits() {
            RuleRound target = new RuleRound() { RoundToDecimal = 3 };
            string value = "10,1553";
            string expected = "10,155";

            string actual = target.Execute(value);
            Assert.AreEqual(expected, actual);
        }
    }
}
