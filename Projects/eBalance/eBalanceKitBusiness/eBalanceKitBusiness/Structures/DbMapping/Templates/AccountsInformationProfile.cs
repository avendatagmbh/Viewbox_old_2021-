// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-04-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using eBalanceKitBusiness.Import;

namespace eBalanceKitBusiness.Structures.DbMapping.Templates {

    /// <summary>
    /// class to store information about the import settings when you try to import csv file. AccountsProfileManager handles the items from this class.
    /// </summary>
    public partial class AccountsInformationProfile : TemplateBase {

        public Dictionary<string, int> ColumnsDictionary { get; set; }

        public char Separator { get; set; }

        public char Delimiter { get; set; }

        public Encoding Encoding { get; set; }

        public BalanceListImportType BalanceListImportType { get; set; }

        public bool WithComment { get; set; }

        public bool WithIndex { get; set; }

        public bool WithTaxonomyColumn { get; set; }

        public bool WithHeadLine { get; set; }

        internal override void WriteTemplateXml() {

            // add relevant data to that element if you would like to export it
            // from Model.Importer.CsvReader or Model.Importer.Config from BalListImportAssistant
            // don't forget to write some code to import it in ReadTemplateXml
            XElement element = new XElement("root",
                                            from keyValue in ColumnsDictionary
                                            select new XElement(keyValue.Key, keyValue.Value));
            element.Add(new XElement("encoding", Encoding.BodyName));
            element.Add(new XElement("separator", Separator));
            element.Add(new XElement("delimiter", Delimiter));
            element.Add(new XElement("balance_list_import_type", BalanceListImportType.ToString()));
            element.Add(new XElement("with_comment", WithComment ? "t": "f"));
            element.Add(new XElement("with_index", WithIndex ? "t": "f"));
            element.Add(new XElement("with_taxonomy_column", WithTaxonomyColumn ? "t": "f"));
            element.Add(new XElement("with_head_line", WithHeadLine ? "t": "f"));
            Template = element.ToString();
        }

        internal override void ReadTemplateXml() {

            // load the element into the class. Values are used in 
            // Model.Importer.CsvReader or Model.Importer.Config from BalListImportAssistant
            ColumnsDictionary = new Dictionary<string, int>();
            XElement element = XElement.Load(new StringReader(Template));
            foreach (XElement xElement in element.Elements()) {
                switch (xElement.Name.LocalName) {
                    case "encoding":
                        Encoding = Encoding.GetEncoding(xElement.Value);
                        break;
                    case "separator":
                        Separator = xElement.Value.ToCharArray().First();
                        break;
                    case "delimiter":
                        Delimiter = xElement.Value.ToCharArray().First();
                        break;
                    case "with_comment":
                        WithComment = xElement.Value == "t";
                        break;
                    case "with_index":
                        WithIndex = xElement.Value == "t";
                        break;
                    case "with_taxonomy_column":
                        WithTaxonomyColumn = xElement.Value == "t";
                        break;
                    case "with_head_line":
                        WithHeadLine = xElement.Value == "t";
                        break;
                    case "balance_list_import_type":
                        BalanceListImportType value;
                        Enum.TryParse(xElement.Value, true, out value);
                        BalanceListImportType = value;
                        break;
                    default:
                        ColumnsDictionary[xElement.Name.LocalName] = Int32.Parse(xElement.Value);
                        break;
                }
            }
        }

        /// <summary>
        /// in the combobox the name will be shown.
        /// </summary>
        /// <returns>Name tag</returns>
        public override string ToString() { return Name; }

        public void Save() {
            WriteTemplateXml();
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.DbMapping.Save(this);
            }
 
        }
    }
}
