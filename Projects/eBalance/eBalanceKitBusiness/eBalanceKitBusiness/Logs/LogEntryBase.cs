using System;
using DbAccess;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Manager;
using Taxonomy;
using System.Xml;
using System.IO;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Logs {
    public abstract class LogEntryBase {
        public LogEntryBase() {
        }

        public abstract DateTime Timestamp{get;}
        public string TimestampString { get { return Timestamp.ToString("dd.MM.yyyy HH:mm:ss"); } }
        protected abstract User User { get; }
        public string UserDisplayString { get { return User.DisplayString; } }

        public abstract int UserId();

        protected abstract string GenerateMessage();

        protected string _message;
        public string Message { 
            get {
                if (string.IsNullOrEmpty(_message))
                    _message = GenerateMessage();
                return _message;
            } 
        }

        internal static string TaxonomyStringFormatted(TaxonomyIdManager taxonomyIdManager, long refId) {
            IElement element = taxonomyIdManager.GetElement((int)refId);
            if (element == null) return ResourcesLogging.UnknownTaxonomyEntry;
            return "\"" + element.MandatoryLabel + "\" (" + element.Id + ")";
        }

        public static string GetAttribute(string xml, string attrName, string element = "root") {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(new StringReader(xml));
            return xmlDoc.DocumentElement.Attributes[attrName] == null ? string.Empty : xmlDoc.DocumentElement.Attributes[attrName].Value;
        }
        public abstract void SaveToDb(IDatabase conn);
    }
}
