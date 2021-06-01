// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using AvdCommon.DataGridHelper.Interfaces;
using ScreenshotAnalyzerDatabase.Factories;
using ScreenshotAnalyzerDatabase.Interfaces;
using Utils;

namespace ScreenshotAnalyzerBusiness.Structures.Results {
    public class ResultColumn : NotifyPropertyChangedBase,IDataColumn{
        public ResultColumn(string name, IDbResult dbResult, IDbScreenshotGroup dbScreenshotGroup, IDbOcrRectangle dbOcrRect) {
            DbResultColumn = DatabaseObjectFactory.CreateResultColumn(dbResult, dbScreenshotGroup, dbOcrRect, name);
        }

        public ResultColumn(IDbResultColumn dbResultColumn) {
            DbResultColumn = dbResultColumn;
        }

        #region Name
        public string Name {
            get {
                return DbResultColumn.Header; 
            }
            set {
                if (Name != value) {
                    DbResultColumn.Header = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public Color Color { get; set; }
        #endregion Name

        private IDbResultColumn DbResultColumn { get; set; }
    }
}
