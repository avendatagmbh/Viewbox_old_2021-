using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Logging {
    public class XMLLoggingColumn {

        public string Name { get; set; }
        public string Comment { get; set; }
        public string TypeName { get; set; }
        public string DefaultValue { get; set; }
        public string Type { get; set; }
        public string DbType { get; set; }
        public int MaxLength { get; set; }
        public int NumericScale { get; set; }
        public int OrdinalPosition { get; set; }
        public bool AllowDBNull { get; set; }
        public bool AutoIncrement { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnsigned { get; set; }
        public bool IsIdentity { get; set; }

        public void WriteToXML(XmlWriter writer) {
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("Comment", Comment);
            writer.WriteElementString("Type", Type);
            writer.WriteElementString("TypeName", TypeName);
            writer.WriteElementString("DbType", DbType);
            writer.WriteElementString("MaxLength", MaxLength.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("DefaultValue", DefaultValue);
            writer.WriteElementString("AllowDBNull",
                                      AllowDBNull.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("AutoIncrement",
                                      AutoIncrement.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("NumericScale",
                                      NumericScale.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("IsPrimaryKey",
                                      IsPrimaryKey.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("IsUnsigned", IsUnsigned.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("IsIdentity", IsIdentity.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("OrdinalPosition",
                                      OrdinalPosition.ToString(CultureInfo.InvariantCulture));
        }

        public void Load(XmlNode node) {
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "Name":
                        Name = childNode.InnerText;
                        break;

                    case "Comment":
                        Comment = childNode.InnerText;
                        break;

                    case "Type":
                        Type = childNode.InnerText;
                        break;

                    case "TypeName":
                        TypeName = childNode.InnerText;
                        break;

                    case "DbType":
                        DbType = childNode.InnerText;
                        break;

                    case "DefaultValue":
                        DefaultValue = childNode.InnerText;
                        break;

                    case "MaxLength":
                        MaxLength = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
                        break;

                    case "NumericScale":
                        NumericScale = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
                        break;

                    case "OrdinalPosition":
                        OrdinalPosition = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
                        break;

                    case "AllowDBNull":
                        AllowDBNull = bool.Parse(childNode.InnerText);
                        break;

                    case "AutoIncrement":
                        AutoIncrement = bool.Parse(childNode.InnerText);
                        break;

                    case "IsPrimaryKey":
                        IsPrimaryKey = bool.Parse(childNode.InnerText);
                        break;

                    case "IsUnsigned":
                        IsUnsigned = bool.Parse(childNode.InnerText);
                        break;

                    case "IsIdentity":
                        IsIdentity = bool.Parse(childNode.InnerText);
                        break;
                }
            }
        }
    }
}
