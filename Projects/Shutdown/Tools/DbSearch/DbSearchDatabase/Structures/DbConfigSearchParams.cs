using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DbSearchDatabase.Structures {
    public class DbConfigSearchParams {
        public bool? CaseIgnore { get; set; }
        public bool? InStringSearch { get; set; }
        public bool? StringSearch { get; set; }
        public bool? SearchRoundedValues { get; set; }

        public void ToXml(XmlWriter writer) {
            writer.WriteStartElement("SP");
            if (CaseIgnore.HasValue) writer.WriteAttributeString("CI", CaseIgnore.ToString());
            if (InStringSearch.HasValue) writer.WriteAttributeString("ISS", InStringSearch.ToString());
            if (StringSearch.HasValue) writer.WriteAttributeString("SS", StringSearch.ToString());
            if (SearchRoundedValues.HasValue) writer.WriteAttributeString("SRV", SearchRoundedValues.ToString());
            writer.WriteEndElement();
        }

        public bool FromXml(XmlReader reader) {
            if (reader.Name == "SP") {
                CaseIgnore = InStringSearch = StringSearch = SearchRoundedValues = null;
                if (reader.GetAttribute("CI") != null) CaseIgnore = Convert.ToBoolean(reader.GetAttribute("CI"));
                if (reader.GetAttribute("ISS") != null) InStringSearch = Convert.ToBoolean(reader.GetAttribute("ISS"));
                if (reader.GetAttribute("SS") != null) StringSearch = Convert.ToBoolean(reader.GetAttribute("SS"));
                if (reader.GetAttribute("SRV") != null) SearchRoundedValues = Convert.ToBoolean(reader.GetAttribute("SRV"));
                return true;
            }
            return false;
        }
    }
}
