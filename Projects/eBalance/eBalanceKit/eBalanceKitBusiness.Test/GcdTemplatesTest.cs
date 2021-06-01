using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.MappingTemplate;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKitBusiness.Test
{
    [TestClass]
    public class GcdTemplatesTest
    {
        [TestMethod]
        public void TestGcdTemplates()
        {
            User adminUser = TestHelper.LogonAsAdmin();

            Document document = null;
            MappingTemplateHeadGCD newTemplate = null;

            try {

                #region [ Prepare ]

                Structures.DbMapping.System system = SystemManager.Instance.Systems.FirstOrDefault();
                document = TestHelper.CreateDocument();

                const string strvalue1 = "teszt1";
                const string strvalue2 = "teszt2";
                const string strvalue3 = "teszt3";

                #endregion [ Prepare ]

                TestHelper.InitDocument(document);
                TemplateManager.InitGcd();

                DocumentManager.Instance.CurrentDocument = document;
                IValueTreeEntry value1 = document.ValueTreeGcd.GetValue("de-gcd_genInfo.report.audit.certificateText");
                IValueTreeEntry value2 = document.ValueTreeGcd.GetValue("de-gcd_genInfo.report.audit.certificateText.pre");
                IValueTreeEntry value3 = document.ValueTreeGcd.GetValue("de-gcd_genInfo.report.audit.certificateText.desc");

                value1.Value = strvalue1;
                value2.Value = strvalue2;
                value3.Value = strvalue3;

                //save current status to a template
                newTemplate = TemplateManager.CreateTemplateGcd(document);
                var createmodel = new CreateTemplateModel(document, newTemplate);
                TemplateManager.InitTemplateGcd(createmodel);

                //changing original values
                value1.Value = "teszt4";
                value2.Value = "teszt5";
                value3.Value = "teszt6";
                
                //apply template
                ApplyGCDTemplateModel model = new ApplyGCDTemplateModel(document, newTemplate);
                object[] paramArray = {new ProgressInfo(), model};
                TemplateManager.ApplyTemplateGcd(paramArray);

                //checking values, they must contain the original values
                if (value1.Value.ToString() != strvalue1 || value2.Value.ToString() != strvalue2 || value3.Value.ToString() != strvalue3) {
                    Assert.Fail("The values are incorrect!");
                }


            }
            catch (Exception ex) {
                Assert.Fail(ex.Message);
            }

            #region [ Cleanup ]

            finally {
                try {
                    if (document != null) {
                        TestHelper.DeleteDocument(document);
                    }
                }
                catch {
                    Assert.Fail("Error during document deleting!");
                }

                try {
                    if (newTemplate != null) {
                        TemplateManager.DeleteTemplate(newTemplate);
                    }
                }
                catch {
                    Assert.Fail("Error during template deleting!");
                }
            }

            #endregion [ Cleanup ]

        }
    }
}
