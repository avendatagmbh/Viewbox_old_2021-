using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Xml.Serialization;

namespace DbComparisonV2.Models
{
    /// <summary>
    /// Carry on here:
    /// set an abstract layer on top of DatabaseComparerConfig 
    /// to enable a a config databaseconfig/databaseconfig and another databaseconfig/viewscriptconfig
    /// some kind of viewScriptconfig shoud be created to hold information about script files 
    /// </summary>
    /// <typeparam name="TComparableItem1">DatabaseConfig or a viewScriptconfig</typeparam>
    /// <typeparam name="TComparableItem2"></typeparam>
    [DataContract]
    public abstract class ComparConfigBase<TComparableObject1, TComparableObject2> :ICompareConfigBase<ICompareConfig,ICompareConfig>, INotifyPropertyChanged
        where TComparableObject1 : ICompareConfig, new() 
        where TComparableObject2 : ICompareConfig, new()
    {
        #region Fields
        protected TComparableObject1 _object1;
        protected TComparableObject2 _object2;
        #endregion

        #region Constructors
        public ComparConfigBase() 
        { 
            _object1 = new TComparableObject1();
            _object2 = new TComparableObject2();
        }
        #endregion

        #region Properties
        [DataMember]
        public virtual TComparableObject2 Object2
        {
            get { return _object2; }
            set { 
                if (!Object.ReferenceEquals(_object2, value))
                {
                    _object2 = value;
                    OnPropertyChanged("Object2");
                }
            }
        }
        [DataMember]
        public virtual TComparableObject1 Object1
        {
            get { return _object1; }
            set { 
                if (!Object.ReferenceEquals(_object1, value))
                {
                    _object1 = value;
                    OnPropertyChanged("Object1");
                }
            }
        }
        /// <summary>
        /// Loads the config name to display in the drop down
        /// provides an empty setter for the Deserializer
        /// </summary>
        [DataMember]
        public virtual string ConfigName
        {
            get
            {
                return GetConfigName();
            }
            set { }
        }
        [XmlElement("AusgabeVerzeichnis")]
        public virtual string OutputDir { get; set; }
        #endregion

        #region functions
        protected abstract string GetConfigName();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


        ICompareConfig ICompareConfigBase<ICompareConfig, ICompareConfig>.Object1
        {
            get
            {
                return Object1;
            }
            set
            {
                Object1 = (TComparableObject1)value;
            }
        }

        ICompareConfig ICompareConfigBase<ICompareConfig, ICompareConfig>.Object2
        {
            get
            {
                return Object1;
            }
            set
            {
                Object2 = (TComparableObject2)value;
            }
        }
    }
}
