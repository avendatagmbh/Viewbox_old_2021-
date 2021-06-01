using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViewBuilderBusiness.Manager;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilder.Test
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ParameterTest
    {
        #region Properties

        private ProfileConfig _config;

        #endregion

        #region Init, Cleanup

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

        #endregion

        #region TestMethods

        [TestMethod]
        public void TestReferencedObjectsExists()
        {
            string errors = _config.IndexDb.TestReferencedObjectsExists();
            Assert.IsTrue(errors.Length == 0, errors);
        }

        [TestMethod]
        public void TestCreateIndexData()
        {
            using (IDatabase conn = _config.ViewboxDb.ConnectionManager.GetConnection())
            {
                Parameter parameter = conn.DbMapping.Load<Parameter>().FirstOrDefault();
                Assert.IsNotNull(parameter);
                IndexDb.IndexDb.DoJob(_config.ViewboxDb, new Utils.ProgressCalculator(), 4, 0, parameter.Id);
            }
        }

        [TestMethod]
        public void TestCreateIndexDataForSelectedParameters() {
            List<int> parameterIds = new List<int>() {
                519,
                520,
                521,
                522,
                21,
                23,
            };
            using (IDatabase conn = _config.ViewboxDb.ConnectionManager.GetConnection())
            {
                List<Parameter> parameters = conn.DbMapping.Load<Parameter>();
                foreach (int parameterId in parameterIds) {
                    Parameter parameter = parameters.FirstOrDefault(p => p.Id == parameterId);
                    Assert.IsNotNull(parameter);
                    IndexDb.IndexDb.DoJob(_config.ViewboxDb, new Utils.ProgressCalculator(), 4, 0, parameter.Id);
                }
            }
        }

        #endregion



    }
}
