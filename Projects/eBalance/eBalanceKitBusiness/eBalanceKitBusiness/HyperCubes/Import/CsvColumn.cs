using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using AvdCommon.DataGridHelper.Interfaces;
using Utils;

namespace eBalanceKitBusiness.HyperCubes.Import
{
    public class CsvColumn : NotifyPropertyChangedBase, IDataColumn
    {
        public CsvColumn(string name) { Name = name; }
        public CsvColumn(bool flag, string name) {
            AssignmentFlag = flag; Name = name; }

        #region AssignmentFlag
        private bool _assignmentFlag;

        public bool AssignmentFlag {
            get { return _assignmentFlag; }
            set {
                if (_assignmentFlag != value) {
                    _assignmentFlag = value;
                    OnPropertyChanged("AssignmentFlag");
                    if (value) {
                        this.Color = assigned;
                    } else {
                        //this.Color = Color.White;
                    }
                }
            }
        }
        #endregion AssignmentFlag

        #region Implementation of IDataColumn
        //public string Name { get; set; }

        #region Name
        private string _name;

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion NAME

        private readonly Color assigned = Color.Brown;

        #region Color
        private Color _color;

        public Color Color {
            get { return _color; }
            set {
                if (_color != value) {
                    _color = value;
                    OnPropertyChanged("Color");
                }
            }
        }
        #endregion Color

        #endregion
    }
}
