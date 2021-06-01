// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 11:40:55
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using DbAccess.Structures;

namespace ViewValidatorLogic.Structures.Logic {
    public class ColumnInfoHelper : DbColumnInfo{
        //The index of the column when using a DbDataReader
        public int Index { get; set; }

        public static ColumnInfoHelper FromDbColumnInfo(DbColumnInfo info, int index) {
            return new ColumnInfoHelper {
                AllowDBNull = info.AllowDBNull,
                AutoIncrement = info.AutoIncrement,
                DefaultValue = info.DefaultValue,
                Index = index,
                IsIdentity = info.IsIdentity,
                IsPrimaryKey = info.IsPrimaryKey,
                IsUnsigned = info.IsUnsigned,
                MaxLength = info.MaxLength,
                Name = info.Name,
                NumericScale = info.NumericScale,
                OriginalType = info.OriginalType,
                Type = info.Type
            };
        }
    }
}
