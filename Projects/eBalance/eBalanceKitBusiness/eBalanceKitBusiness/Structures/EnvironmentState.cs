// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-05-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace eBalanceKitBusiness.Structures {
    /// <summary>
    /// Stores the environment settings susch as selected system, selected navigation tree node, etc.
    /// </summary>
    public class EnvironmentState : IXmlSerializable {

        #region [ Constructors ]

        public EnvironmentState()
            : this(-1) { }

        public EnvironmentState(int userId)
            : this(userId, -1, -1, -1, -1, null, -1) { }

        public EnvironmentState(int userId, int selectedSystemId, int selectedCompanyId, int selectedFinancialYearId, int selectedDocumentId, string selectedNavigationTreeEntry, int selectedMenuIndex) {
            this.UserId = userId;
            this.SelectedSystemId = selectedSystemId;
            this.SelectedCompanyId = selectedCompanyId;
            this.SelectedFinancialYearId = selectedFinancialYearId;
            this.SelectedDocumentId = selectedDocumentId;
            this.SelectedNavigationTreeEntry = selectedNavigationTreeEntry;
            this.ExpandedNavigationTreeEntries = new List<string>();
            this.SelectedMenuIndex = selectedMenuIndex;
        }

        #endregion [ Constructors ]

        #region [ Properties ]

        public int UserId { get; set; }
        public int SelectedSystemId { get; set; }
        public int SelectedCompanyId { get; set; }
        public int SelectedFinancialYearId { get; set; }
        public int SelectedDocumentId { get; set; }
        public string SelectedNavigationTreeEntry { get; set; }
        public List<string> ExpandedNavigationTreeEntries { get; set; }
        public int SelectedMenuIndex { get; set; }

        #endregion [ Properties ]

        #region [ Methods ]

        public void AddExpandedNavigationTreeEntry(string navigationTreeEntry) {
            this.ExpandedNavigationTreeEntries.Add(navigationTreeEntry);
        }

        #endregion [ Methods ]

        #region [ IXmlSerializable members ]

        public XmlSchema GetSchema() { return null; }

        public void ReadXml(XmlReader reader) {
            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "EnvironmentState") {
                UserId = Int32.Parse(reader["UserId"]);
                SelectedSystemId = Int32.Parse(reader["SelectedSystemId"]);
                SelectedCompanyId = Int32.Parse(reader["SelectedCompanyId"]);
                SelectedFinancialYearId = Int32.Parse(reader["SelectedFinancialYearId"]);
                SelectedDocumentId = Int32.Parse(reader["SelectedDocumentId"]);
                SelectedNavigationTreeEntry = reader["SelectedNavigationTreeEntry"];
                // SelectedMenuIndex should be NULL because it is added later then the others
                SelectedMenuIndex = Int32.Parse(reader["SelectedMenuIndex"] ?? "0");
                reader.Read();
                if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "ExpandedNavigationTreeEntries") {
                    while (reader.MoveToContent() == XmlNodeType.Element &&
                           reader.LocalName == "ExpandedNavigationTreeEntries") {
                        reader.Read();
                        if (reader.MoveToContent() == XmlNodeType.Element &&
                            reader.LocalName == "ExpandedNavigationTreeEntry") {
                            while (reader.MoveToContent() == XmlNodeType.Element &&
                                   reader.LocalName == "ExpandedNavigationTreeEntry") {
                                this.ExpandedNavigationTreeEntries.Add(reader["Id"]);
                                reader.Read();
                            }
                            reader.Read();
                        }
                    }
                    reader.Read();
                }
            }
        }

        public void WriteXml(XmlWriter writer) {
            writer.WriteStartElement("EnvironmentState");
            writer.WriteAttributeString("UserId", this.UserId.ToString());
            writer.WriteAttributeString("SelectedSystemId", this.SelectedSystemId.ToString());
            writer.WriteAttributeString("SelectedCompanyId", this.SelectedCompanyId.ToString());
            writer.WriteAttributeString("SelectedFinancialYearId", this.SelectedFinancialYearId.ToString());
            writer.WriteAttributeString("SelectedDocumentId", this.SelectedDocumentId.ToString());
            writer.WriteAttributeString("SelectedNavigationTreeEntry", this.SelectedNavigationTreeEntry ?? string.Empty);
            writer.WriteAttributeString("SelectedMenuIndex", this.SelectedMenuIndex.ToString());
            writer.WriteStartElement("ExpandedNavigationTreeEntries");
            foreach (string expandedNavigationTreeEntry in ExpandedNavigationTreeEntries) {
                writer.WriteStartElement("ExpandedNavigationTreeEntry");
                writer.WriteAttributeString("Id", expandedNavigationTreeEntry);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        #endregion [ IXmlSerializable members ]
    }

    /// <summary>
    /// List of environment settings (per user), to store it in the user config
    /// </summary>
    public class EnvironmentStateList : List<EnvironmentState>, IXmlSerializable {

        #region [ IXmlSerializable members ]

        public XmlSchema GetSchema() { return null; }

        public void ReadXml(XmlReader reader) {
            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "EnvironmentStates") {
                if (reader.ReadToDescendant("EnvironmentState")) {
                    while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "EnvironmentState") {
                        EnvironmentState state = new EnvironmentState();
                        state.ReadXml(reader);
                        this.Add(state);
                    }
                }
                reader.Read();
            }
        }

        public void WriteXml(XmlWriter writer) {
            writer.WriteStartDocument();
            writer.WriteStartElement("EnvironmentStates");
            foreach (EnvironmentState environmentState in this) {
                environmentState.WriteXml(writer);
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        #endregion [ IXmlSerializable members ]
    }
}
