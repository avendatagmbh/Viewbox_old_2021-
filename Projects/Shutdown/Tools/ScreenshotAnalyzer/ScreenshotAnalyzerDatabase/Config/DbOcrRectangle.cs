// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-13
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using ScreenshotAnalyzerDatabase.Interfaces;
using ScreenshotAnalyzerDatabase.Structures;
using System.Windows;

namespace ScreenshotAnalyzerDatabase.Config {
    public enum RectType {
        Text,
        Anchor
    };

    [DbTable("rectangles", ForceInnoDb = true)]
    internal class DbOcrRectangle : DatabaseObjectBase<int>, IDbOcrRectangle {
        public DbOcrRectangle() {
            _upperLeft = new Point(0,0);
            _lowerRight = new Point(0, 0);
        }

        public DbOcrRectangle(DbScreenshot dbScreenshot) {
            DbScreenshot = dbScreenshot;
        }

        public DbOcrRectangle(DbScreenshot dbScreenshot, Point upperLeft, Point lowerRight, RectType type) {
            UpperLeft = upperLeft;
            LowerRight = lowerRight;
            Type = type;
            DbScreenshot = dbScreenshot;
            Save();
        }

        #region Properties
        [DbColumn("scr_id", IsInverseMapping = true)]
        internal DbScreenshot DbScreenshot { get; set; }

        #region X1
        [DbColumn("x1")]
        public double X1 {
            get { return _x1; }
            set {
                _x1 = value;
                _upperLeft.Offset(-_upperLeft.X+value, 0);
            }
        }
        private double _x1;
        #endregion X1

        #region Y1
        [DbColumn("y1")]
        public double Y1 {
            get { return _y1; }
            set {
                _y1 = value;
                _upperLeft.Offset(0, -_upperLeft.Y + value);
            }
        }
        private double _y1;
        #endregion Y1

        #region X2
        [DbColumn("x2")] 
        public double X2 {
            get { return _x2; }
            set {
                _x2 = value;
                _lowerRight.Offset(-_lowerRight.X + value, 0);
            }
        }
        private double _x2;
        #endregion X2

        #region Y2
        [DbColumn("y2")] 
        public double Y2 {
            get { return _y2; }
            set {
                _y2 = value;
                _lowerRight.Offset(0,-_lowerRight.Y + value);
            }
        }
        private double _y2;
        #endregion Y2

        #region UpperLeft
        private Point _upperLeft;
        public Point UpperLeft {
            get { return _upperLeft; }
            set {
                if (_upperLeft != value) {
                    _upperLeft = value;
                    X1 = _upperLeft.X;
                    Y1 = _upperLeft.Y;
                    Save();
                }
            }
        }


        #endregion UpperLeft

        #region LowerRight
        private Point _lowerRight;
        public Point LowerRight {
            get { return _lowerRight; }
            set {
                if (_lowerRight != value) {
                    _lowerRight = value;
                    X2 = _lowerRight.X;
                    Y2 = _lowerRight.Y;
                    Save();
                }
            }
        }
        #endregion LowerRight

        #region Additional Info
        private string _additionalInfo;


        [DbColumn("add_info")]
        public string AdditionalInfo {
            get { return _additionalInfo; }
            set {
                if (_additionalInfo != value) {
                    _additionalInfo = value;
                    Save();
                }
            }
        }
        #endregion Additional Info

        [DbColumn("type")]
        public RectType Type { get; set; }

        public bool DisableSaving { get; set; }


        #endregion Properties

        public void Save() {
            if (DbScreenshot == null || DisableSaving) return;
            using (IDatabase conn = DbScreenshot.ScreenshotGroup.Table.Profile.GetOpenConnection()) {
                conn.DbMapping.Save(this);
            }
        }

        public void Delete() {
            if (DbScreenshot == null) return;
            using (IDatabase conn = DbScreenshot.ScreenshotGroup.Table.Profile.GetOpenConnection()) {
                conn.ExecuteNonQuery("DELETE FROM " + conn.DbMapping.GetTableName<DbResultEntry>() + " WHERE rect_id=" +
                                     Id);
                conn.ExecuteNonQuery("DELETE FROM " + conn.DbMapping.GetTableName<DbResultColumn>() + " WHERE rect_id=" +
                                     Id);
                conn.DbMapping.Delete(this);
            }
        }
    }
}
