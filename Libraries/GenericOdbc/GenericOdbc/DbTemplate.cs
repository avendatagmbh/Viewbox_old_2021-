// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-17
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using DbAccess;

namespace GenericOdbc {
    internal class DbTemplate : IDbTemplate {
        #region constructors
        /// <summary>
        /// Creates default template for unknown database types.
        /// </summary>
        internal DbTemplate() {
            IsUserDefined = true;
        }

        /// <summary>
        /// Creates a new template from the specified template xml file.
        /// </summary>
        /// <param name="templateFile"></param>
        internal DbTemplate(string templateFile) {

            Filename = templateFile;

            var doc = new XmlDocument();
            doc.Load(templateFile);
            try {
                XmlElement root = doc.DocumentElement;

                Debug.Assert(root != null, "root != null");
                foreach (XmlAttribute attribute in root.Attributes) {
                    switch (attribute.Name) {
                        case "ServerName":
                            ServerName = attribute.Value;
                            break;
                    }
                }

                foreach (XmlNode node in root.ChildNodes) {
                    switch (node.Name) {
                        case "Enquote":
                            foreach (XmlNode quoteNode in node.ChildNodes) {
                                switch (quoteNode.Name) {
                                    case "Left":
                                        QuoteLeft = quoteNode.InnerText;
                                        break;

                                    case "Right":
                                        QuoteRight = quoteNode.InnerText;
                                        break;
                                }
                            }
                            break;

                        case "ConnectionStringParameter":
                            foreach (XmlNode paramNode in node.ChildNodes) {
                                string caption = string.Empty;
                                string captionDe = string.Empty;
                                string name = string.Empty;
                                ConnectionStringParamType type = ConnectionStringParamType.String;

                                Debug.Assert(paramNode.Attributes != null, "paramNode.Attributes != null");
                                foreach (XmlAttribute attribute in paramNode.Attributes) {
                                    switch (attribute.Name) {
                                        case "Caption":
                                            caption = attribute.Value;
                                            break;

                                        case "Caption_de":
                                            captionDe = attribute.Value;
                                            break;

                                        case "Name":
                                            name = attribute.Value;
                                            break;

                                        case "Type":
                                            switch (attribute.Value) {
                                                case "Integer":
                                                    type = ConnectionStringParamType.Integer;
                                                    break;

                                                case "Boolean":
                                                    type = ConnectionStringParamType.Integer;
                                                    break;

                                                case "Password":
                                                    type = ConnectionStringParamType.Password;
                                                    break;
                                            }
                                            break;
                                    }
                                }


                                if (IsGermanUICulture && !string.IsNullOrEmpty(captionDe)) {
                                    caption = captionDe;
                                }

                                // parameters pwd and password should allways be handled as password parameters
                                if (name.ToLower() == "pwd" || name.ToLower() == "password")
                                    type = ConnectionStringParamType.Password;

                                _csb.AddParam(name, caption, type);
                            }
                            break;
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }
        }
        #endregion constructors

        #region members
        private readonly IConnectionStringBuilder _csb =
            ConnectionStringBuilderFactory.GetConnectionStringBuilder("GenericODBC");
        #endregion members

        #region properties
        private static bool IsGermanUICulture {
            get { return Thread.CurrentThread.CurrentUICulture.Name == "de-DE"; }
        }

        public string Filename { get; private set; }

        public bool IsUserDefined { get; private set; }

        #region ServerName
        private string _serverName;

        public string ServerName {
            get { return _serverName ?? (IsGermanUICulture ? "benutzerdefiniert" : "user defined"); }
            private set { _serverName = value; }
        }
        #endregion

        public string QuoteLeft { get; private set; }
        public string QuoteRight { get; private set; }

        public IEnumerable<IConnectionStringParam> Params {
            get { return _csb.Params; }
        }
        #endregion properties
    }
}