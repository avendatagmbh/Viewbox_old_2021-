// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-17
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Taxonomy.Interfaces;

namespace Taxonomy {

    public static class XbrlDisplayStyles {
        private static readonly Dictionary<string, XbrlDisplayStyle> Styles =
            new Dictionary<string, XbrlDisplayStyle>();

        static XbrlDisplayStyles() {
            foreach (FileInfo fi in new DirectoryInfo("Taxonomy\\styles").GetFiles("*.xml")) {
                var doc = new XmlDocument();
                doc.Load(fi.FullName);

                var xmlNodeList = doc.SelectNodes("/stylesheet/style");
                if (xmlNodeList != null)
                    foreach (XmlNode node in xmlNodeList) {
                        try {
                            string role = node.Attributes["role"].Value;

                            XmlAttribute ordinalAttr = node.Attributes["ordinal"];
                            double ordinal;
                            if (ordinalAttr != null && !string.IsNullOrEmpty(ordinalAttr.Value)) ordinal = Convert.ToDouble(ordinalAttr.Value);
                            else ordinal = 999;

                            string key = Path.GetFileNameWithoutExtension(fi.Name) + "." + role;
                            var style = new XbrlDisplayStyle {
                                Taxonomy = fi.Name,
                                Role = role,
                                Ordinal = ordinal
                            };

                            XmlAttribute styleClass = node.Attributes["style"];
                            //if (styleClass != null) {
                                //style.Class =
                                //    (XbrlDisplyStyleClasses) Enum.Parse(typeof (XbrlDisplyStyleClasses), styleClass.Value);
                            //} else style.Class = XbrlDisplyStyleClasses.Tree;

                            Styles[key] = style;
                        } catch (Exception ex) {
                            Debug.WriteLine("XblrDisplayStyles.Init: " + ex.Message);
                        }
                    }
            }
        }

        public static XbrlDisplayStyle GetStyle(ITaxonomy taxonomy, IRoleType role) {
            //int n = taxonomy.Path.LastIndexOf("\\");
            //string key = taxonomy.Path.Substring(n + 1, taxonomy.Path.Length - n - 1) + "." + role.Id;
            //if (_styles.ContainsKey(key)) return _styles[key];
            return null;
        }
    }
}