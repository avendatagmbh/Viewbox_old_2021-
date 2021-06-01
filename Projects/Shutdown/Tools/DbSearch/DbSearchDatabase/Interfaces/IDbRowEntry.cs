using System.Xml;

namespace DbSearchDatabase.Interfaces {
    public interface IDbRowEntry {
        object Value { get; set; }
        string EditedValue { get; set; }
        void ToXml(XmlWriter writer);
        void FromXml(XmlReader reader, IDbColumn column, IDbRow dbRow);
    }
}