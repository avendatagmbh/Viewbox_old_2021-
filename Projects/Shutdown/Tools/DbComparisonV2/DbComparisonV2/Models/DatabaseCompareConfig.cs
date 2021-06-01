using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess.Structures;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace DbComparisonV2.Models
{
    [DataContract, Serializable()]
    public class DatabaseCompareConfig :ComparConfigBase<DatabaseConfig,DatabaseConfig>
    {
        #region Constructors
        public DatabaseCompareConfig() : base() { }
        #endregion

        #region Properties
        [DataMember, XmlElement("DatenbankKonfiguration1")]
        public override DatabaseConfig Object1
        {
            get
            {
                return base.Object1;
            }
            set
            {
                base.Object1 = value;
            }
        }

        [DataMember, XmlElement("DatenbankKonfiguration2")]
        public override DatabaseConfig Object2
        {
            get
            {
                return base.Object2;
            }
            set
            {
                base.Object2 = value;
            }
        }

        #endregion Properties

        #region functions
        protected override string  GetConfigName()
        {
            if (_object1 != null && _object1 != null)
                return String.Format("{0}.{1}.{2}_{3}.{4}", _object1.Hostname, _object1.Username, _object1.DbName, _object2.Hostname, _object2.DbName);
            return string.Empty;
        }
        public override string ToString()
        {
            return GetConfigName();
        }
        #endregion
    }
    
}
