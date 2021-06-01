// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using ScreenshotAnalyzerDatabase.Interfaces;
using ScreenshotAnalyzerDatabase.Structures;

namespace ScreenshotAnalyzerDatabase.Config {

    [DbTable("result_columns", ForceInnoDb = true)]
    internal class DbResultColumn : DatabaseObjectBase<int>, IDbResultColumn {
        public DbResultColumn() {
            
        }

        public DbResultColumn(DbResult dbResult, DbScreenshotGroup dbScreenshotGroup, DbOcrRectangle dbRect, string header) {
            Result = dbResult;
            ScreenshotGroup = dbScreenshotGroup;
            RectangleId = dbRect.Id;
            Header = header;
            Save();
        }


        #region Properties
        [DbColumn("scr_group_id", IsInverseMapping = true)]
        internal DbScreenshotGroup ScreenshotGroup { get; set; }

        [DbColumn("result_id", IsInverseMapping = true)]
        internal DbResult Result { get; set; }

        //The id of one of the rectangles the column belongs to
        [DbColumn("rect_id", IsInverseMapping = true)]
        public int RectangleId { get; set; }
        //internal DbOcrRectangle Rectangle { get; set; }

        #region Header
        private string _header;


        [DbColumn("header", AllowDbNull = false)]
        public string Header {
            get { return _header; }
            set {
                if (_header != value) {
                    _header = value;
                    Save();
                }
            }
        }
        #endregion Header

        #endregion Properties

        #region Methods
        private void Save() {
            if (ScreenshotGroup == null) return;
            using (IDatabase conn = ScreenshotGroup.Table.Profile.GetOpenConnection()) {
                conn.DbMapping.Save(this);
            }
        }
        #endregion Methods
    }
}
