using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace EasyDocExtraction.Helper
{
    public class ConfigurationHelper
    {
        public static IEnumerable<string> GetEasyFoldersPath()
        {
            return ((EasyExportedFoldersConfigurationSection)ConfigurationManager.GetSection("easySection"))
                .Items.Cast<EasyExportedFolder>().Select(f => f.Value);
        }
        public static int GetDataChunckSize()
        {
            int size = 100; // default
            int.TryParse(ConfigurationManager.AppSettings["dataChunckSizeToSave"], out size);
            return size;
        }
    }

    public class EasyExportedFoldersConfigurationSection : ConfigurationSection 
    {
        [ConfigurationProperty("easyFolders", IsRequired = false, IsKey = false, IsDefaultCollection = true)]
        public EasyExportedFoldersCollection Items 
        { 
            get 
            {
                return ((EasyExportedFoldersCollection)(base["easyFolders"])); 
            } 
            set 
            {
                base["easyFolders"] = value; 
            } 
        } 
    }
    [ConfigurationCollection(typeof(EasyExportedFolder), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate)]
    public class EasyExportedFoldersCollection : ConfigurationElementCollection 
    {
        internal const string ItemPropertyName = "easyFolder";

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }
        protected override string ElementName
        {
            get
            {
                return ItemPropertyName;
            }
        }
        protected override bool IsElementName(string elementName) { return (elementName == ItemPropertyName); }
        protected override object GetElementKey(ConfigurationElement element) { return ((EasyExportedFolder)element).Value; }
        protected override ConfigurationElement CreateNewElement() { return new EasyExportedFolder(); }     
        public override bool IsReadOnly() { return false; }

        public EasyExportedFolder this[int index] 
        {
            get { return (EasyExportedFolder)BaseGet(index); }
        }
    }

    public class EasyExportedFolder : ConfigurationElement
    {

        [ConfigurationProperty("value")]   
        public string Value 
        {
            get
            {
                return (string)base["value"];
            }
            set
            {
                base["value"] = value;
            }
        }
    } 
}
