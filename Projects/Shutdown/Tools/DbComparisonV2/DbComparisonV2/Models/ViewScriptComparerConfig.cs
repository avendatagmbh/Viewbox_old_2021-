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
    public class ViewScriptComparerConfig : ComparConfigBase<DatabaseConfig, ViewScriptConfig>
    {
        #region Constructors
        public ViewScriptComparerConfig() : base() { }
        #endregion


        #region Properties
        [DataMember, XmlElement]
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

        [DataMember, XmlElement]
        public override ViewScriptConfig Object2
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
            return string.Empty;
        }
        public override string ToString()
        {
            return GetConfigName();
        }
        #endregion
    }
    
}
