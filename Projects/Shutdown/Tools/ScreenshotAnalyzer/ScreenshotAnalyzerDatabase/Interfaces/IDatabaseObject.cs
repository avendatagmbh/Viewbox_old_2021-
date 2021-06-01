// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using DbAccess;

namespace ScreenshotAnalyzerDatabase.Interfaces {
    public interface IDatabaseObject<T> {
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        T Id { get; }
    }
}
