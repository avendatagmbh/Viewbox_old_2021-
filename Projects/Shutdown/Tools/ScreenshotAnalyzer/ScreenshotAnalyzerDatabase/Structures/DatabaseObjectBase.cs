// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using DbAccess;

namespace ScreenshotAnalyzerDatabase.Structures {
    public class DatabaseObjectBase<T> {
        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public T Id { get; set; }
        #endregion
    }
}
