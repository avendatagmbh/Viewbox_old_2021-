
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DbAccess;
using DbAccess.Structures;

namespace TestdataGenerator {
    enum FieldType {
        String,
        Int,
        Numeric,
        DateTime
    }

    struct ColumnHelper {
        public string Name;
        public FieldType FieldType;
    }

    public class Generator {
        public static void CreateDB(string outputFile) {
            Stream objStream = null;
            FileStream objFileStream = null;
            try {
                byte[] abytResource;
                System.Reflection.Assembly objAssembly =
                System.Reflection.Assembly.GetExecutingAssembly();

                objStream = objAssembly.GetManifestResourceStream("TestdataGenerator.EmptyDatabase.mdb");
                abytResource = new Byte[objStream.Length];
                objStream.Read(abytResource, 0, (int)objStream.Length);
                objFileStream = new FileStream(outputFile,
                FileMode.Create);
                objFileStream.Write(abytResource, 0, (int)objStream.Length);
                objFileStream.Close();

            } finally {
                if (objFileStream != null) {
                    objFileStream.Close();
                }
            }
        }

        //public static string DbName = "dbsearch_generated_data_test";
        public static string DbName = "dbsearch_generated_data_test_large";
        public static void Main() {
            /*UnitTestDatabaseGenerator unitTestGenerator = new UnitTestDatabaseGenerator();
            unitTestGenerator.Create();*/
            //DbConfig validationConfig = new DbConfig("Access") { Hostname = System.IO.Directory.GetCurrentDirectory() + "\\test_validation_database.mdb" };
            DbConfig validationConfig = new DbConfig("Access") { Hostname = System.IO.Directory.GetCurrentDirectory() + "\\test_validation_database_large.mdb" };
            DbConfig viewConfig = new DbConfig("MySQL") { Hostname = "localhost", Username = "root", Password = "avendata", DbName = DbName };

            Generator generator = new Generator();
            generator.GenerateData(validationConfig, viewConfig, 10000, 10);
        }

        
        public void GenerateData(DbConfig validationConfig, DbConfig viewConfig, int numRowsMax, int numTables) {
            System.IO.Directory.CreateDirectory(new FileInfo(validationConfig.Hostname).DirectoryName);
            CreateDB(validationConfig.Hostname);

            DbConfig viewConfigWithoutDatabaseName = (DbConfig) viewConfig.Clone();
            viewConfigWithoutDatabaseName.DbName = string.Empty;

            //Create MySQL Database
            using (IDatabase connView = ConnectionManager.CreateConnection(viewConfigWithoutDatabaseName)) {
                connView.Open();
                if (!connView.DatabaseExists(viewConfig.DbName)) connView.CreateDatabase(viewConfig.DbName);
            }

            using (IDatabase connValidation = ConnectionManager.CreateConnection(validationConfig), connView = ConnectionManager.CreateConnection(viewConfig)) {
                connValidation.Open();
                connView.Open();

                CreateTables(connValidation, connView, numRowsMax, numTables);
            }
        }


        #region Random Helper
        private string RandomString(Random random, int size, bool lowerCase) {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++) {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        DateTime _start = new DateTime(1995, 1, 1);
        DateTime RandomDay(Random gen) {
            int range = ((TimeSpan)(DateTime.Today - _start)).Days;
            
            return _start.AddDays(gen.Next(range)).AddSeconds(gen.Next(0,60*60*24));
        }
        #endregion

        #region CreateTables

        private void CreateTables(IDatabase connValidation, IDatabase connView, int numRowsMax, int numTables) {
            List<ColumnHelper> Columns = new List<ColumnHelper>() 
            {
                new ColumnHelper(){Name="_row_no_", FieldType = FieldType.Int},
                new ColumnHelper(){Name="string1", FieldType = FieldType.String},
                new ColumnHelper(){Name="int1", FieldType = FieldType.Int},
                new ColumnHelper(){Name="numeric1", FieldType = FieldType.Numeric},
                new ColumnHelper(){Name="datetime1", FieldType = FieldType.DateTime},
            };
            Random random = new Random(Seed: 1);

            //For a large database
            for (int i = 0; i < 200; ++i) {
                FieldType type = (FieldType)Enum.Parse(typeof(FieldType), random.Next(0, 3).ToString());
                Columns.Add(new ColumnHelper() { Name = RandomString(random, random.Next(10, 10), false), FieldType = type });
            }

            List<DbColumnValues> values = new List<DbColumnValues>(numRowsMax);

            for (int i = 0; i < numRowsMax; ++i) {
                DbColumnValues value = new DbColumnValues();
                foreach (var column in Columns) {
                    if (column.Name == "_row_no_") value[column.Name] = i + 1;
                    else {
                        switch (column.FieldType) {
                            case FieldType.String:
                                value[column.Name] = RandomString(random, random.Next(0, 15), false);
                                break;
                            case FieldType.Int:
                                value[column.Name] = random.Next(Int32.MinValue, Int32.MaxValue);
                                break;
                            case FieldType.Numeric:
                                value[column.Name] = (random.NextDouble() - 0.5)*1000;
                                break;
                            case FieldType.DateTime:
                                value[column.Name] = RandomDay(random);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                values.Add(value);
            }

            for (int tableId = numTables; tableId >= 1; --tableId) {
                int percent = (int) (tableId*100.0f/numTables);
                string tableName = "table" + percent;
                CreateTable(connValidation, tableName, Columns, true);
                CreateTable(connView, tableName, Columns, false);

                int rowNo = 1;
                foreach (var value in values) {
                    value["_row_no_"] = rowNo++;
                    //connValidation.InsertInto(tableName, value);
                }
                connView.InsertInto(tableName, values);

                int drop = (int)(numRowsMax/numTables);
                for (int i = 0; i < drop; ++i) {
                    values.RemoveAt(random.Next(0, values.Count-1));
                }
            }
        }
        #endregion

        #region TranslateFieldType

        string TranslateFieldType(FieldType type, bool validation) {
            if (validation) {
                switch(type) {
                    case FieldType.String:
                        return "VARCHAR (255)";
                    case FieldType.Int:
                        return "INT";
                    case FieldType.Numeric:
                        return "Double";
                    case FieldType.DateTime:
                        return "DATETIME";
                    default:
                        throw new ArgumentOutOfRangeException("type");
                }
            } else {
                switch (type) {
                    case FieldType.String:
                        return "VARCHAR (255)";
                    case FieldType.Int:
                        return "INT";
                    case FieldType.Numeric:
                        return "DECIMAL(20,4)";
                    case FieldType.DateTime:
                        return "DATETIME";
                    default:
                        throw new ArgumentOutOfRangeException("type");
                }
            }
        }

        #endregion TranslateFieldType

        #region CreateTable

        private void CreateTable(IDatabase conn, string tableName, List<ColumnHelper> columns, bool validation) {
            if(!validation) conn.DropTableIfExists(tableName);
            string sql = "CREATE TABLE " + tableName + "(";
            foreach (var column in columns) {
                sql += " " + conn.Enquote(column.Name) + " " + TranslateFieldType(column.FieldType, validation) + ",";
            }
            //Delete last ,
            sql = sql.Substring(0, sql.Length - 1);
            sql += ")";

            conn.ExecuteNonQuery(sql);
        }

        #endregion CreateTable
    }
}
