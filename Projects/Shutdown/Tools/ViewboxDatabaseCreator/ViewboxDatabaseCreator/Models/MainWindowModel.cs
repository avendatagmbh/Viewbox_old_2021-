using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using DbAccess.Structures;

namespace ViewboxDatabaseCreator.Models {
    public class MainWindowModel {
        public MainWindowModel() {
            DatabaseModelSource = new DatabaseModel("Source");
            DatabaseModelDestination = new DatabaseModel("Destination");

            DatabaseModelDestination.DbConfig.DbName = "viewbox";
            DatabaseModelSource.DbConfig.DbName = "dbsearch_generated_data_test";

            //DatabaseModelSource.DbConfig.Hostname = "dbstrabag"; 
            //DatabaseModelDestination.DbConfig.Hostname = "dbstrabag";
            //DatabaseModelDestination.DbConfig.DbName = "new_test_view_database";
            //DatabaseModelSource.DbConfig.DbName = "viewbox";
        }

        #region Properties
        public DatabaseModel DatabaseModelSource { get; set; }
        public DatabaseModel DatabaseModelDestination { get; set; }
        #endregion Properties

        #region Methods
        private DbConfig ConfigWithoutDatabase(DbConfig dbConfig) {
            DbConfig configWithoutDb = (DbConfig)dbConfig.Clone();
            configWithoutDb.DbName = string.Empty;
            return configWithoutDb;
        }

        public bool ViewboxDbExists() {
            using (IDatabase viewboxConn = ConnectionManager.CreateConnection(ConfigWithoutDatabase(DatabaseModelDestination.DbConfig))) {
                viewboxConn.Open();
                if (viewboxConn.DatabaseExists(DatabaseModelDestination.DbConfig.DbName))
                    return true;
            }
            return false;
        }

        public void CreateViewboxDb() {
            using (IDatabase viewboxConn = ConnectionManager.CreateConnection(ConfigWithoutDatabase(DatabaseModelDestination.DbConfig))) {
                viewboxConn.Open();
                viewboxConn.DropDatabaseIfExists(DatabaseModelDestination.DbConfig.DbName);
                viewboxConn.CreateDatabase(DatabaseModelDestination.DbConfig.DbName);
            }

            using (IDatabase sourceConn = ConnectionManager.CreateConnection(DatabaseModelSource.DbConfig), viewboxConn = ConnectionManager.CreateConnection(DatabaseModelDestination.DbConfig)) {
                sourceConn.Open();
                viewboxConn.Open();
                
                viewboxConn.DbMapping.CreateTableIfNotExists<Language>();
                viewboxConn.DbMapping.CreateTableIfNotExists<User>();
                viewboxConn.DbMapping.CreateTableIfNotExists<Role>();
                viewboxConn.DbMapping.CreateTableIfNotExists<UserRoleMapping>();
                viewboxConn.DbMapping.CreateTableIfNotExists<OptimizationGroup>();
                viewboxConn.DbMapping.CreateTableIfNotExists<OptimizationGroupText>();
                viewboxConn.DbMapping.CreateTableIfNotExists<Optimization>();
                viewboxConn.DbMapping.CreateTableIfNotExists<OptimizationText>();
                viewboxConn.DbMapping.CreateTableIfNotExists<OptimizationRoleMapping>();
                viewboxConn.DbMapping.CreateTableIfNotExists<OptimizationUserMapping>();

                viewboxConn.DbMapping.CreateTableIfNotExists<Category>();
                viewboxConn.DbMapping.CreateTableIfNotExists<CategoryText>();
                viewboxConn.DbMapping.CreateTableIfNotExists<CategoryRoleMapping>();
                viewboxConn.DbMapping.CreateTableIfNotExists<CategoryUserMapping>();

                viewboxConn.DbMapping.CreateTableIfNotExists<Property>();
                viewboxConn.DbMapping.CreateTableIfNotExists<PropertyText>();
                viewboxConn.DbMapping.CreateTableIfNotExists<Scheme>();
                viewboxConn.DbMapping.CreateTableIfNotExists<SchemeText>();
                viewboxConn.DbMapping.CreateTableIfNotExists<TableObject>();
                viewboxConn.DbMapping.CreateTableIfNotExists<TableObjectText>();
                viewboxConn.DbMapping.CreateTableIfNotExists<TableRoleMapping>();
                viewboxConn.DbMapping.CreateTableIfNotExists<TableUserMapping>();
                viewboxConn.DbMapping.CreateTableIfNotExists<IssueExtension>();
                viewboxConn.DbMapping.CreateTableIfNotExists<TableObjectSchemeMapping>();
                viewboxConn.DbMapping.CreateTableIfNotExists<OrderArea>();

                viewboxConn.DbMapping.CreateTableIfNotExists<Column>();
                viewboxConn.DbMapping.CreateTableIfNotExists<ColumnText>();
                viewboxConn.DbMapping.CreateTableIfNotExists<ColumnRoleMapping>();
                viewboxConn.DbMapping.CreateTableIfNotExists<ColumnUserMapping>();

                viewboxConn.DbMapping.CreateTableIfNotExists<UserColumnSettings>();
                viewboxConn.DbMapping.CreateTableIfNotExists<UserTableObjectSettings>();
                viewboxConn.DbMapping.CreateTableIfNotExists<UserColumnOrderSettings>();
                viewboxConn.DbMapping.CreateTableIfNotExists<UserPropertySettings>();
                viewboxConn.DbMapping.CreateTableIfNotExists<UserControllerSettings>();
                viewboxConn.DbMapping.CreateTableIfNotExists<UserTableObjectOrderSettings>();
                viewboxConn.DbMapping.CreateTableIfNotExists<UserOptimizationSettings>();

                viewboxConn.DbMapping.CreateTableIfNotExists<Parameter>();
                viewboxConn.DbMapping.CreateTableIfNotExists<ParameterText>();
                viewboxConn.DbMapping.CreateTableIfNotExists<ParameterValueSetText>();
                viewboxConn.DbMapping.CreateTableIfNotExists<ParameterCollectionMapping>();

                viewboxConn.DbMapping.CreateTableIfNotExists<Relation>();
                viewboxConn.DbMapping.CreateTableIfNotExists<About>();
                viewboxConn.DbMapping.CreateTableIfNotExists<Note>();

                //Create optimization with database of source
                string sql = "INSERT INTO " + viewboxConn.Enquote(viewboxConn.DbMapping.GetTableName<Optimization>()) +
                             " (id, parent_id,value, optimization_group_id) VALUES (1,0," +
                             viewboxConn.GetSqlString(DatabaseModelSource.DbConfig.DbName) + ",1)";
                viewboxConn.ExecuteNonQuery(sql);
                OptimizationGroup optimizationGroup = new OptimizationGroup(){Type=OptimizationType.System};
                viewboxConn.DbMapping.Save(optimizationGroup);
                OptimizationGroupText optimizationGroupText = new OptimizationGroupText(){CountryCode = "de", RefId = 1, Text = "System"};
                viewboxConn.DbMapping.Save(optimizationGroupText);

                //Create admin user
                User admin = new User(){UserName = "avendata",Flags = SpecialRights.Super, Password = "avendata"};
                viewboxConn.DbMapping.Save(admin);

                //Add tables
                List<string> tables = sourceConn.GetTableList();
                int tableOrdinal = 0;
                foreach (var tableName in tables) {
                    Table tableObject = new Table() { Database = DatabaseModelSource.DbConfig.DbName ,TableName=tableName, Type=TableType.Table, RowCount = sourceConn.CountTable(tableName), Ordinal = tableOrdinal++, IsVisible = true};
                    viewboxConn.DbMapping.Save(tableObject);

                    //Add columns
                    int columnOrdinal = 0;
                    foreach (var columnInfo in sourceConn.GetColumnInfos(tableName)) {
                        
                        Column column = new Column() {
                                                         Name = columnInfo.Name,
                                                         IsVisible = true,
                                                         DataType = DbColumnTypeToSqlType(columnInfo.Type),
                                                         TableId = tableObject.Id,
                                                         Ordinal = columnOrdinal++
                                                     };
                        viewboxConn.DbMapping.Save(column);
                    }
                }
            }
        }

        private SqlType DbColumnTypeToSqlType(DbColumnTypes type) {
            switch(type) {
                case DbColumnTypes.DbNumeric:
                    return SqlType.Numeric;
                case DbColumnTypes.DbInt:
                case DbColumnTypes.DbBigInt:
                    return SqlType.Integer;
                case DbColumnTypes.DbBool:
                    return SqlType.Boolean;
                case DbColumnTypes.DbText:
                case DbColumnTypes.DbLongText:
                    return SqlType.String;
                case DbColumnTypes.DbDate:
                    return SqlType.Date;
                case DbColumnTypes.DbTime:
                    return SqlType.Time;
                case DbColumnTypes.DbDateTime:
                    return SqlType.DateTime;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }


        #region Test
        public void Test() {
            using (IDatabase targetConn = ConnectionManager.CreateConnection(ConfigWithoutDatabase(DatabaseModelDestination.DbConfig))) {
                targetConn.Open();
                targetConn.DropDatabaseIfExists(DatabaseModelDestination.DbConfig.DbName);
                targetConn.CreateDatabase(DatabaseModelDestination.DbConfig.DbName);
            }

            using (IDatabase sourceConn = ConnectionManager.CreateConnection(DatabaseModelSource.DbConfig), targetConn = ConnectionManager.CreateConnection(DatabaseModelDestination.DbConfig)) {
                sourceConn.Open();
                targetConn.Open();
                Stopwatch watch = new Stopwatch();
                watch.Start();
                List<Tuple<string,string>> databaseToTableName = new List<Tuple<string, string>>();
                using(DbDataReader reader = sourceConn.ExecuteReader("SELECT * FROM `tables` WHERE type <> 3")) {
                    while (reader.Read()) {
                        databaseToTableName.Add(new Tuple<string, string>(reader["database"].ToString(),reader["name"].ToString()));
                    }
                }
                StringBuilder sql = new StringBuilder();
                int i = 0;
                foreach(var pair in databaseToTableName){
                    string database = pair.Item1;
                    string tableName = pair.Item2;
                    sql.Append("CREATE VIEW " + targetConn.Enquote(database.Replace("fahrleitungsbau", "fbau") + "_" + tableName) + " AS SELECT * FROM " +
                                    targetConn.Enquote(database, tableName)+ ";" + Environment.NewLine);
                    if (i++ > 1000) {
                        try {
                            Stopwatch newWatch = new Stopwatch();
                            newWatch.Start();
                            targetConn.ExecuteNonQuery(sql.ToString());
                            newWatch.Stop();
                            Console.WriteLine("Sekunden vergangen: " + newWatch.ElapsedMilliseconds / 1000.0);
                        }catch(Exception ex) {
                            Console.WriteLine(ex.Message);
                        }
                        sql = new StringBuilder();
                        i = 0;
                    }
                }
                if(sql.Length > 0) {
                    try {
                        targetConn.ExecuteNonQuery(sql.ToString());
                    } catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                    }
                    
                }
                watch.Stop();
                Console.WriteLine("Sekunden vergangen: " + watch.ElapsedMilliseconds/1000.0);
            }
        }
        #endregion Test
        #endregion Methods
    }
}
