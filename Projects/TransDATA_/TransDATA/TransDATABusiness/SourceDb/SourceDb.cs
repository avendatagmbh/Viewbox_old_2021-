using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using TransDATABusiness.ConfigDb.Tables;
using TransDATABusiness.Enums;
using System.Data;
using System.Windows;
using DbAccess.Structures;
using DbAccess;

namespace TransDATABusiness.SourceDb {
    
    /// <summary>
    /// This class provides a read only access to the source database.
    /// </summary>
    internal static class SourceDb {

        /// <summary>
        /// Gets the structure.
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        public static void GetStructure(Profile profile) {

            try {
                DbConfig cfg = profile.ConfigDatabase;
                if (cfg == null) return;
                
                using (IDatabase db = ConnectionManager.CreateConnection(cfg)) {

                    db.Open();

                    profile.Tables.Clear();
                    int count = 0;

                    List<string> tblList = db.GetTableList(profile.ConfigDatabase.DbName);

                    foreach (string tbl in tblList) {
                        count++;
                        Table tb = new Table();

                        tb.TableInfo.Schema = profile.ConfigDatabase.DbName;
                        tb.TableInfo.Name = tbl;
                        tb.TableInfo.Description = "";
                        tb.TableInfo.RunningNumber = count;

                        string sqlColumnsInfo = "SELECT COUNT(*) FROM `" + tb.TableInfo.Name + "`";
                        tb.TableInfo.Count = Convert.ToInt64(db.ExecuteScalar(sqlColumnsInfo));
                        tb.TableInfo.StrCount = tb.TableInfo.Count.ToString("#,#####");
                        tb.TableInfo.CountString = "( " + tb.TableInfo.StrCount + " )";

                        
                        List<DbColumnInfo> clmList = db.GetColumnInfos(profile.ConfigDatabase.DbName, tbl);
                        tb.TableInfo.ClmCount = 0;

                             foreach (DbColumnInfo clrow in clmList) {
                                 Column clm = new Column();
                                 clm.ColumnInfo.Name = clrow.Name;
                                 //clm.ColumnInfo.TypeName = clrow.Type;
                                 clm.ColumnInfo.Length = clrow.MaxLength;
                                 clm.ColumnInfo.NumericPrecision = clrow.NumericScale;
                                 clm.ColumnInfo.Ordinal = tb.TableInfo.ClmCount+1; 

                                 tb.Columns.Add(clm);
                                tb.TableInfo.ClmCount += 1;
                            }

                        tb.TableInfo.StrClmCount = tb.TableInfo.ClmCount.ToString("#,#####");
                        profile.Tables.Add(tb);

                    }

                    db.Close();

                }
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message); ;
            }
           
        }
    }
}
