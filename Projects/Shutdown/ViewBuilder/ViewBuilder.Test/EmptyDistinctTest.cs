using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AV.Log;
using DbAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViewBuilderBusiness.MetadataUpdate;
using ViewBuilderBusiness.Structures.Config;
using System.Diagnostics;
using log4net;

namespace ViewBuilder.Test
{
    /// <summary>
    /// Summary description for EmptyDistinctTest
    /// </summary>
    [TestClass]
    public class EmptyDistinctTest
    {
        #region [ Properties ]

        internal static ILog _log = LogHelper.GetLogger();
        private ProfileConfig _config;

        #endregion [ Properties ]

        #region [ Init, Cleanup ]

        [TestInitialize()]
        public void MyTestInitialize()
        {
            _config = ConfigLoadHelper.ConfigLoad();
        }


        [TestCleanup()]
        public void MyTestCleanup()
        {
            _config.Dispose();
        }

        #endregion [ Init, Cleanup ]

        #region [ TestMethods ]

        [TestMethod]
        public void EmptyDistinctGenerationTest() {
            string dbName = "sap_wesslingen_020";
            string tableName = "kalm";
            _log.InfoFormat("Empty distinct generation for table [{0}.{1}] started", dbName, tableName);
            Stopwatch sw = Stopwatch.StartNew();
            _config.ViewboxDb.CreateEmptyDistinctTable(null, dbName, tableName);
            sw.Stop();
            _log.InfoFormat("Empty distinct generation for table [{0}.{1}] took: {2} ms", dbName, tableName, sw.ElapsedMilliseconds);
            // With the original implementation
            // 4802 ms
            // 4708 ms
            // 5014 ms
            // With the modified implementation
            // 4608
            // 4709
            // 4805
        }

        #endregion [ TestMethods ]
    }
}
