using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DbAccess.Structures;
using DbSearchDatabase.Results;
using DbSearchDatabase.TableRelated;
using DbSearchLogic.SearchCore.Structures.Db;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearchLogic.SearchCore.Structures.Result.DataMapper {
    class ColumnHitInfoMapper {
        public static DbColumnHitInfo Map(ColumnHitInfo hitInfo, ResultSet resultSet, ColumnResult columnResult) {
            DbColumnHitInfo dbColumnHitInfo = new DbColumnHitInfo();
            dbColumnHitInfo.DbQuery = (DbQuery) resultSet.SnapshotQuery.DbQuery;
            dbColumnHitInfo.DbResultSet = (DbResultSet) resultSet.DbResultSet;
            dbColumnHitInfo.SearchColumnId = resultSet.SnapshotQuery.IndexOfColumn(columnResult.Column);
            dbColumnHitInfo.TableName = hitInfo.TableInfo.Name;
            dbColumnHitInfo.HitColumnName = hitInfo.ColumnName;

            return dbColumnHitInfo;
        }

        public static ColumnHitInfo Map(DbColumnHitInfo dbColumnHit, TableInfoManager tableInfoManager) {
            ColumnHitInfo hitInfo = new ColumnHitInfo(tableInfoManager.GetTableInfo(dbColumnHit.TableName),dbColumnHit.HitColumnName,DbColumnTypes.DbUnknown,null);
            return hitInfo;
        }
    }
}
