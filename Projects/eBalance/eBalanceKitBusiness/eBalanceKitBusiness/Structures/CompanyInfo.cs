using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Structures
{
    public class CompanyInfo : IEquatable<CompanyInfo>
    {
        private int _id;
        private string _name; 

        public int Id
        {
            get { return this._id; }
        }
        public string Name
        {
            get { return this._name; }
        }

        public CompanyInfo(int id, string name)
        {
            this._id = id;
            this._name = name;
        }

        public CompanyInfo(Company company)
        {
            this._id = company.Id;
            this._name = company.Name;
        }

        public override int GetHashCode()
        {
            return this.Id;
        }

        public bool Equals(CompanyInfo other)
        {
            if (this.Id == other.Id & this.Name == other.Name)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
    
    internal class CompanyInfoSerializer : IXmlSerializable
    {
        private const string rootElement = "Companies";
        private const string idElement = "Id";
        private List<CompanyInfo> _companies;

        public List<CompanyInfo> Companies
        {
            get { return this._companies; }
        }

        internal CompanyInfoSerializer()
        {
        }

        internal CompanyInfoSerializer(List<CompanyInfo> companies)
        {
            this._companies = companies;
        }

        public global::System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(global::System.Xml.XmlReader reader)
        {
            _companies = new List<CompanyInfo>();
            if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == rootElement)
            {
                if (!reader.IsEmptyElement)
                {
                    reader.ReadStartElement();
                    while(reader.IsStartElement())
                    {
                        reader.ReadStartElement();
                        int id = Convert.ToInt32(reader.ReadString());
                        Company company = CompanyManager.Instance.Companies.FirstOrDefault(c => c.Id == id);
                        if (company != null)
                            _companies.Add(new CompanyInfo(company));
                        reader.ReadEndElement();
                    }
                    reader.ReadEndElement();
                }
            }
        }

        public void WriteXml(global::System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement(rootElement);
            foreach (CompanyInfo company in _companies)
            {
                writer.WriteStartElement(idElement);
                writer.WriteValue(company.Id);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}
