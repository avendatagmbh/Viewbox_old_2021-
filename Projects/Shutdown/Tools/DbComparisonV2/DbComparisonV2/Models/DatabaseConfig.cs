using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess.Structures;
using System.Xml.Serialization;
using DbAccess;
using System.Runtime.Serialization;

namespace DbComparisonV2.Models
{
    /// <summary>
    /// TODO: namimg are missleading between DbConfig & ConfigBase whom have different purposes 
    /// DatabaseConfig should inherit from both class <paramref name="DbConfig"/> and 
    /// <paramref name="ConfigBase"/> in order to provide functionalities of the original DbComparison
    /// and support for the new version DbConfig in DbAccess,
    /// workaround: ConfigBase is as a private field with same interfaces
    /// </summary>
    [DataContract]
    public class DatabaseConfig : DbConfig, ICompareConfig
    {
        ConfigBase _configBase;
        public DatabaseConfig()
            : base()
        {
            _configBase = new ConfigBase();
        }
        public DatabaseConfig(string dbType, IDbTemplate dbTemplate = null )
            : base(dbType,dbTemplate)
        {
            _configBase = new ConfigBase();
        }
        
        [XmlIgnore]
        public uint Locks
        {
            get { return _configBase.Locks; }
        }
        [XmlIgnore]
        public bool Locked
        {
            get { return _configBase.Locked; }
        }
        [XmlIgnore]
        public virtual bool Changed
        {
            get { return _configBase.Changed; }
            set { _configBase.Changed = value; }
        }
        public void Lock()
        {
            _configBase.Lock();
        }
        public void Unlock()
        {
            _configBase.Unlock();
        }
    }
}
