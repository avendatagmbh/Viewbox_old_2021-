using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Taxonomy;
using Taxonomy.Enums;
using Utils;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using System = eBalanceKitBusiness.Structures.DbMapping.System;

namespace eBalanceKitBusiness.Import
{
    internal class FullImporter {
        #region constructor
        public FullImporter(FileStream stream) {

            XbrlImportErrors = new List<XbrlImportError>();
            _fileName = stream.Name;

            try {
                stream.Position = 0;
                _xmlDoc.Load(stream);
            } catch (Exception ex) {
                XbrlImportErrors.Add(new XbrlImportError {
                    FilePath = _fileName,
                    ErrorDescription = ex.Message
                });
            }
        }
        #endregion constructor

        #region members

        private XmlDocument _xmlDoc = new XmlDocument();
        private string _fileName;
        public XbrlImporter XbrlImporter;

        public List<XbrlImportError> XbrlImportErrors { get; private set; }
        public bool HasErrors { get { return XbrlImportErrors.Count > 0; } }

        #endregion members

        public void Import(XmlNode report, XmlNode system, XmlNode company, string financialYear, FileStream stream) {

            Structures.DbMapping.System systemToAssign = SystemManager.Instance.Systems.FirstOrDefault(sys => system.InnerText.ToLower() == sys.Name.ToLower());
            
            string st13Import = null;
            XmlNode selectIdNo = company.SelectSingleNode("//*[name()='genInfo.company.id.idNo']");
            if (selectIdNo != null) {
                XmlNode st13ImportNode = selectIdNo.SelectSingleNode("//*[name()='genInfo.company.id.idNo.type.companyId.ST13']");
                if (st13ImportNode != null) {
                    st13Import = st13ImportNode.InnerText;
                }
            }
            string companyNameImport = null;
            XmlNode selectSingleNode = company.SelectSingleNode("//*[name()='genInfo.company.id.name']");
            if (selectSingleNode != null) {
                companyNameImport = selectSingleNode.InnerText;
            }

            Tuple<string, string> companyTuple = Tuple.Create(st13Import, companyNameImport);

            if (systemToAssign == null) {
                Structures.DbMapping.System newSystem = new Structures.DbMapping.System();
                newSystem.Name = system.InnerText;
                if (system.Attributes != null) {
                    newSystem.Comment = system.Attributes["comment"].Value;
                }
                SystemManager.Instance.AddSystem(newSystem);
                systemToAssign = newSystem;
            }

            string reportName = string.Empty;
            string reportComment = string.Empty;
            if (report.Attributes != null) {
                reportName = report.Attributes["name"].Value;
                if (report.Attributes["comment"] != null) {
                    reportComment = report.Attributes["comment"].Value;
                }
            }

            Tuple<string, string> reportTuple = Tuple.Create(reportName, reportComment);

            XbrlImporter = new XbrlImporter(stream);
            XbrlImporter.FullImport(reportTuple, systemToAssign, companyTuple, financialYear);
            XbrlImportErrors = XbrlImporter.XbrlImportErrors;
        }
    }
}
