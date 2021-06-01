using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvdCommon.DataGridHelper.Interfaces;
using Utils;

namespace eBalanceKitBusiness.HyperCubes.Import
{
    public class DataValue : NotifyPropertyChangedBase,IDataRowEntry
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content">The visible value.</param>
        /// <param name="assignmentFlag">Is the row assigned, not assigned or modified at the moment?</param>
        public DataValue(string content, bool? assignmentFlag)
        {
            _content = content;
            _assignmentFlag = assignmentFlag;
        }

        /// <param name="content">The visible value.</param>
        /// <param name="assignmentFlag">Is the row assigned, not assigned or modified at the moment?</param>
        public DataValue(object content, bool? assignmentFlag)
        {
            _content = content;
            _assignmentFlag = assignmentFlag;
        }
        object _content;
        bool? _assignmentFlag;

        #region Implementation of IDataRowEntry
        public string DisplayString { get { return _content == null ? string.Empty : _content.ToString(); } }
        public bool? Flag { get { return _assignmentFlag; } set { _assignmentFlag = value; OnPropertyChanged("Flag");}}
        #endregion

        public override string ToString() { return DisplayString; }
    }
}
