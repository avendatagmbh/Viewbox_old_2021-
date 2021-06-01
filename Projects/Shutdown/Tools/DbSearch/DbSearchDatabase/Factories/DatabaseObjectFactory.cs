using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DbAccess.Structures;
using DbSearchDatabase.Config;
using DbSearchDatabase.Interfaces;
using DbSearchDatabase.Results;
using DbSearchDatabase.TableRelated;

namespace DbSearchDatabase.Factories {
    public static class DatabaseObjectFactory {
        public static IDbQueries CreateDbTables(IDbProfile dbProfile) {
            return new DbQueries(dbProfile);
        }

        public static IDbQuery DbQueryFromXml(XmlReader reader, IDbProfile profile) {
            return DbQuery.FromXml(reader, profile);
        }

        public static IDbResultSet CreateResultSet(IDbQuery query) {
            return new DbResultSet((DbQuery) query);
        }

        public static IDbRow CreateRow(IDbQuery query) {
            return new DbRow((DbQuery) query, -1);
        }

        public static IDbColumn CreateColumn(IDbQuery query) {
            return new DbColumn(new DbColumnInfo(){Type=DbColumnTypes.DbText}, (DbQuery) query);
        }

        public static IDbQuery CreateDbQuery(IDbProfile profile, string name) {
            return DbQuery.CreateEmptyDbQuery(profile, name);
            
        }
    }
}
