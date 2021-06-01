using System.Collections.Generic;
using AV.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using log4net;

namespace AV.Log.Test
{
    /// <summary>
    ///This is a test class for LogHelperTest and is intended
    ///to contain all LogHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LogHelperTest
    {

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
        
        /// <summary>
        ///A test for GetLogger
        ///</summary>
        [TestMethod]
        public void ExtensionMethodTest()
        {
            ILog log = LogHelper.GetLogger();
            log.Debug("test entry");

            ILog log2 = LogHelper.GetLogger("testLogger");
            log2.Debug("test entry2");

            int count = LogHelper.LogEntries.Count;

            log.DebugWithCheck("debug test with check");
            log.ErrorWithCheck("error test with check");
            log.InfoWithCheck("info test with check");
            log.FatalWithCheck("fatal test with check");
            log.WarnWithCheck("warning test with check");

            log.DebugFormatWithCheck("debug test with check {0}", "param");
            log.ErrorFormatWithCheck("error test with check {0}", "param");
            log.InfoFormatWithCheck("info test with check {0}", "param");
            log.FatalFormatWithCheck("fatal test with check {0}", "param");
            log.WarnFormatWithCheck("warning test with check {0}", "param");

            Assert.IsTrue(LogHelper.LogEntries.Count == count);
        }

        /// <summary>
        ///A test for testing an exception to be logged
        ///</summary>
        [TestMethod]
        public void GetLoggerExceptionTest()
        {
            ILog log = LogHelper.GetLogger();
            int count = LogHelper.LogEntries.Count;
            try
            {
                throw new Exception("this should be written to the log");
            }
            catch (Exception ex)
            {
                log.Debug("test exception occurred", ex);
            }
            using (NDC.Push("myTestCase"))
            {
                NDC.Push("last");
                log = LogHelper.GetLogger();
                log.Debug("test afer exception");
            }

            Assert.IsTrue(LogHelper.LogEntries.Count == count);
        }

        /// <summary>
        ///A test for testing an exception to be logged
        ///</summary>
        [TestMethod]
        public void LogMethodsTest() {
            ILog log = LogHelper.GetLogger("not collect");
            ILog logWithCollectEnabled = LogHelper.GetLogger("collect");

            int count = LogHelper.LogEntries.Count;

            log.Log(LogLevelEnum.Debug, "log message", false);
            log.Log(LogLevelEnum.Error, "log message", false);
            log.Log(LogLevelEnum.Fatal, "log message", false);
            log.Log(LogLevelEnum.Info, "log message", false);
            log.Log(LogLevelEnum.Warn, "log message", false);

            logWithCollectEnabled.Log(LogLevelEnum.Debug, "log message", true);
            logWithCollectEnabled.Log(LogLevelEnum.Error, "log message", true);
            logWithCollectEnabled.Log(LogLevelEnum.Fatal, "log message", true);
            logWithCollectEnabled.Log(LogLevelEnum.Info, "log message", true);
            logWithCollectEnabled.Log(LogLevelEnum.Warn, "log message", true);

            Assert.IsTrue(LogHelper.LogEntries.Count == count + 5);

            log.Log(LogLevelEnum.Debug, "log message {0}", new ArgumentException("param"), false, "text");
            log.Log(LogLevelEnum.Error, "log message {0}", new ArgumentException("param"), false, "text");
            log.Log(LogLevelEnum.Fatal, "log message {0}", new ArgumentException("param"), false, "text");
            log.Log(LogLevelEnum.Info, "log message {0}", new ArgumentException("param"), false, "text");
            log.Log(LogLevelEnum.Warn, "log message {0}", new ArgumentException("param"), false, "text");

            logWithCollectEnabled.Log(LogLevelEnum.Debug, "log message {0}", new ArgumentException("param"), true, "text");
            logWithCollectEnabled.Log(LogLevelEnum.Error, "log message {0}", new ArgumentException("param"), true, "text");
            logWithCollectEnabled.Log(LogLevelEnum.Fatal, "log message {0}", new ArgumentException("param"), true, "text");
            logWithCollectEnabled.Log(LogLevelEnum.Info, "log message {0}", new ArgumentException("param"), true, "text");
            logWithCollectEnabled.Log(LogLevelEnum.Warn, "log message {0}", new ArgumentException("param"), true, "text");

            Assert.IsTrue(LogHelper.LogEntries.Count == count + 10);
        }
    }
}
