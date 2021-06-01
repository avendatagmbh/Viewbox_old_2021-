using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Utils;
using eBalanceKitBusiness.Structures.DbMapping;
using DbAccess;

namespace eBalanceKitBusiness.HyperCubes.DbMapping
{
    [DbTable("hypercube_import_templates", ForceInnoDb = true)]
    public class DbEntityHyperCubeImport : NotifyPropertyChangedBase
    {
        
        /// <summary>
        /// Unique ID for this template.
        /// </summary>
        [DbColumn("template_id", AllowDbNull = false, AutoIncrement = true)]
        [DbPrimaryKey]
        public int TemplateId { get; set; }
        
        [DbColumn("taxonomy_id", AllowDbNull = true)]
        [DbIndex("idx_hypercube_import_templates_taxid_cubeid")]
        public int TaxonomytId { get; set; }
        
        /// <summary>
        /// The Id to determine the type of HyperCube (eg. ChangesEquityStatement or part of assets analysis).
        /// </summary>
        [DbColumn("cube_element_id", AllowDbNull = true)]
        [DbIndex("idx_hypercube_import_templates_taxid_cubeid")]
        public int CubeElementId { get; set; }

        /// <summary>
        /// The name this template.
        /// </summary>
        [DbColumn("template_name", AllowDbNull = true)]
        public string TemplateName { get; set; }
        
        /// <summary>
        /// The xml that contains the assignments CSV - HyperCube.
        /// </summary>
        [DbColumn("xml_assignment", AllowDbNull = true, Length = 10000)]
        public string XmlAssignment { get; set; }
        
        /// <summary>
        /// A comment for this template.
        /// </summary>
        [DbColumn("comment", AllowDbNull = true, Length = 4096)]
        public string Comment { get; set; }


        /// <summary>
        /// The user who created the template.
        /// </summary>
        [DbColumn("creation_user", AllowDbNull = false)]
        public User Creator { get; set; }


        /// <summary>
        /// Time when the template was created.
        /// </summary>
        [DbColumn("cration_date", AllowDbNull = false)]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The user who last modified the template.
        /// </summary>
        [DbColumn("modification_user", AllowDbNull = true)]
        public User LastModifier { get; set; }
        
        /// <summary>
        /// The user who last modified the template.
        /// </summary>
        [DbColumn("inverse_assignment", AllowDbNull = false)]
        public bool IsInverse { get; set; }

        /// <summary>
        /// Time when the template was last modified.
        /// </summary>
        [DbColumn("modification_date", AllowDbNull = true)]
        public DateTime LastModified { get; set; }


        #region Coding
        private string _coding;

        /// <summary>
        /// The encoding (EncodingName) that is used to import the CSV.
        /// </summary>
        [DbColumn("encoding", AllowDbNull = false)]
        public string Coding {
            get { return _coding; }
            set {
                if (_coding != value) {
                    _coding = value;
                    OnPropertyChanged("Coding");
                }
            }
        }
        #endregion Coding
        
        #region Seperator
        private string _seperator;

        /// <summary>
        /// The seperator to split the CSV file.
        /// </summary>
        [DbColumn("seperator", AllowDbNull = false)]
        public string Seperator {
            get { return _seperator; }
            set {
                if (_seperator != value) {
                    _seperator = value;
                    OnPropertyChanged("Seperator");
                }
            }
        }
        #endregion Seperator
        
        #region Delimiter
        private string _delimiter;

        /// <summary>
        /// The limiter char to escape strings in the CSV file.
        /// </summary>
        [DbColumn("delimiter", AllowDbNull = false)]
        public string Delimiter {
            get { return _delimiter; }
            set {
                if (_delimiter != value) {
                    _delimiter = value;
                    OnPropertyChanged("Delimiter");
                }
            }
        }
        #endregion Delimiter


        #region FileName
        private string _fileName;

        /// <summary>
        /// The CSV file (path + name) that was the source for the template.
        /// </summary>
        [DbColumn("template_csv", AllowDbNull = false, Length = 200)]
        public string FileName {
            get { return _fileName; }
            set {
                if (_fileName != value) {
                    _fileName = value;
                    OnPropertyChanged("FileName");
                }
            }
        }
        #endregion FileName

        #region DimensionOrder
        private string _dimensionOrder;
        /// <summary>
        /// The order of dimensions. Required for nD cubes.
        /// Stored are the DimensionIds joined by pipe "|"
        /// </summary>
        [DbColumn("dimension_order", AllowDbNull = true)]
        public string DimensionOrder {
            get { return _dimensionOrder; }
            set {
                if (_dimensionOrder != value) {
                    _dimensionOrder = value;
                    OnPropertyChanged("DimensionOrder");
                }
            }
        }
        #endregion DimensionOrder


        #region Encoding
        /// <summary>
        /// The encoding of the CSV data.
        /// </summary>
        public Encoding Encoding {
            get { return Encoding.GetEncoding(Coding); }
            set {
                if (_coding != value.BodyName) {
                    Coding = value.BodyName;
                    OnPropertyChanged("Encoding");
                }
            }
        }
        #endregion



        // ToDo Localisation !!!
        public string LastModifierDisplayString {
            get {
                if (LastModifier != null) {
                    return string.Format("Zuletzt bearbeitet von {0} am {1} {2}", LastModifier.DisplayString,
                                         LastModified.ToLongDateString(),
                                         LastModified.ToShortTimeString());
                }
                return null;
            }
        }

        public string CreatorDisplayString {
            get {
                return string.Format("Erstellt von {0} am {1} {2}", Creator.DisplayString, CreationDate.ToLongDateString(),
                                     CreationDate.ToShortTimeString());
            }
        }


    }
}
