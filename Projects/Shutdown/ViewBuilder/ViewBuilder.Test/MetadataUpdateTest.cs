using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using SystemDb.Internal;
using DbAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViewBuilderBusiness.Manager;
using ViewBuilderBusiness.MetadataUpdate;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilder.Test
{
    /// <summary>
    /// Summary description for MetadataUpdateTest
    /// </summary>
    [TestClass]
    public class MetadataUpdateTest
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
        public void UpdateMetadataFromSapTest()
        {
            using (IDatabase conn = _config.ViewboxDb.ConnectionManager.GetConnection()) {
                UpdateSapMetadata.UpdateDescriptions(_config, conn);
            }
        }

        [TestMethod]
        public void UpdateLanguagesTest()
        {
            using (IDatabase conn = _config.ViewboxDb.ConnectionManager.GetConnection())
            {
                UpdateViewBoxMetadata.UpdateLanguages(_config, conn);
            }
        }

        [TestMethod]
        public void UpdateMetadataPropertyTextsTest()
        {
            using (IDatabase conn = _config.ViewboxDb.ConnectionManager.GetConnection()) {
                UpdateViewBoxMetadata.UpdatePropertyTexts(_config, conn);
            }
        }

        [TestMethod]
        public void UpdateMetadataOptimizationTextsTest()
        {
            using (IDatabase conn = _config.ViewboxDb.ConnectionManager.GetConnection()) {
                UpdateViewBoxMetadata.UpdateOptimizationTexts(_config, conn);
            }
        }

        [TestMethod]
        public void UpdateIssueAndParameterLanguageTextsTest()
        {
            using (IDatabase conn = _config.ViewboxDb.ConnectionManager.GetConnection()) {
                UpdateViewBoxMetadata.UpdateIssueAndParameterLanguageTexts(_config, conn);
            }
        }

        [TestMethod]
        public void UpdateCategoryTextsTest()
        {
            using (IDatabase conn = _config.ViewboxDb.ConnectionManager.GetConnection()) {
                UpdateViewBoxMetadata.UpdateCategoryTexts(_config, conn);
            }
        }

        [TestMethod]
        public void UpdateOptimizationTextsTest()
        {
            using (IDatabase conn = _config.ViewboxDb.ConnectionManager.GetConnection())
            {
                UpdateViewBoxMetadata.UpdateOptimizationTexts(_config, conn);
            }
        }

        [TestMethod]
        public void UpdateObjectTypesTextsTest()
        {
            using (IDatabase conn = _config.ViewboxDb.ConnectionManager.GetConnection())
            {
                UpdateViewBoxMetadata.UpdateObjectTypesTexts(_config, conn);
            }
        }

        #endregion
    }
}
