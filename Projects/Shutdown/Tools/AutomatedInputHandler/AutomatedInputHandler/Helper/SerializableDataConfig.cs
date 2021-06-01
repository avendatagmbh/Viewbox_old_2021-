using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace AutomatedInputHandler.Helper
{
    [XmlRootAttribute("DataConfig", IsNullable = false)]
    public class DataConfig
    {
        [XmlAttribute(AttributeName = "noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")] 
        public string noNamespaceSchemaLocation = ".\\Configs\\DataSchema.xsd";

        [XmlElement("AllCompaniesCount", Order = 1)]
        public int AllCompaniesCount { get; set; }

        [XmlElementAttribute("Company", Order = 2, IsNullable = false)]
        public List<Company> Companies;        
    }

    public class Company
    {
        [XmlElement("Identifier", Order = 1)]
        public string Identifier { get; set; }

        [XmlElement("Index", Order = 2)]
        public int Index { get; set; }

        [XmlElement("AllYearsCount", Order = 3)]
        public int AllYearsCount { get; set; }

        [XmlElementAttribute("Year", Order = 4, IsNullable = false)]
        public List<Year> Years;
    }

    public class Year
    {
        [XmlElement("Year", Order = 1)]
        public string year { get; set; }

        [XmlElement("Index", Order = 2)]
        public int Index { get; set; }

        [XmlElement("AllPeriodsCount", Order = 3)]
        public int AllPeriodsCount { get; set; }

        [XmlElementAttribute("Period", Order = 4, IsNullable = false)]
        public List<Period> Periods;
    }

    public class Period
    {
        [XmlElement("Period", Order = 1)]
        public string period { get; set; }

        [XmlElement("Index", Order = 2)]
        public int Index { get; set; }
    }





    /*class MyNode
    {
        List<EventItem> WhatToDo;

        List<MyNode> Children;
    }*/
}
