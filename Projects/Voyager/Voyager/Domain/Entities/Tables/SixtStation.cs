using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
   public class SixtStation {

       private const string TABLE_NAME = "SIXTSTATION";
       private const string ID = "ID";
       private const string STATIONNUMBER = "STATIONNUMBER";
       private const string GDSCODE = "GDSCODE";

       /**********************************************************************/
       #region properties

       public int SixtID { get; set; }         //PK+FK(address-class)
       public int StationNumber { get; set; }
       public string GDS_Code { get; set; }     //varchar(10)

       #endregion properties
       /**********************************************************************/
       #region methodes

       /// <summary>
       /// Creates the table.
       /// </summary>
       /// <param name="db">The db.</param>
       public static void CreateTable(IDatabase db) {
           db.ExecuteNonQuery("CREATE  TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) +" (" +
                                db.Enquote(ID) + " INTEGER  NOT NULL ," +
                                db.Enquote(STATIONNUMBER) +" INTEGER NOT NULL ," +
                                db.Enquote(GDSCODE) +" VARCHAR(10) NULL ," +
                                "PRIMARY KEY (" + db.Enquote(ID) + ") ) ENGINE = MyIsam");
       }

       /// <summary>
       /// Loads the specified reader.
       /// </summary>
       /// <param name="reader">The reader.</param>
       /// <returns></returns>
       private static SixtStation Load(DbDataReader reader) {
           SixtStation sixt = new SixtStation();
           sixt.SixtID = (int)reader[0];
           sixt.StationNumber = (int)reader[1];
           sixt.GDS_Code = reader[2].ToString();
           return sixt;
       }


       /// <summary>
       /// Gets the list.
       /// </summary>
       /// <param name="db">The db.</param>
       /// <returns></returns>
       public static List<SixtStation> GetList(DbAccess.IDatabase db) {
           List<SixtStation> sixtList = new List<SixtStation>();
           try {
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
               if (reader.HasRows) {
                   while (reader.Read()) {
                       sixtList.Add(Load(reader));
                   }
                   reader.Close();
                   return sixtList;
               } else {
                   reader.Close();
                   return null;
               }
           } catch (Exception ex) {
               throw ex;
           }
       }


       /// <summary>
       /// Loads the specified db.
       /// </summary>
       /// <param name="db">The db.</param>
       /// <param name="id">The id.</param>
       /// <returns></returns>
       public SixtStation Load(DbAccess.IDatabase db, int id) {
           try {
               SixtStation sixt = new SixtStation();
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
               if (reader.HasRows) {
                   while (reader.Read()) {
                       sixt = Load(reader);
                   }
                   reader.Close();
                   return sixt;
               } else {
                   reader.Close();
                   return null;
               }
           } catch (Exception ex) {
               throw ex;
           }
       }

       /// <summary>
       /// Saves the specified db.
       /// </summary>
       /// <param name="db">The db.</param>
       public void Save(IDatabase db) {
           try {
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.SixtID);
               bool hasRows = reader.HasRows;
               reader.Close();

               if (hasRows) {
                   db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(STATIONNUMBER) + " = " + this.StationNumber + ", " +
                                                                                     db.Enquote(GDSCODE) + " = " + db.GetSqlString(this.GDS_Code));
               } else {
                   db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(STATIONNUMBER) + ", " +
                                                                                       db.Enquote(GDSCODE) + ") " +

                                      "VALUES (" + this.StationNumber + ", " +
                                                   db.GetSqlString(this.GDS_Code) + " )");
               }
           } catch (Exception ex) {
               throw ex;
           }
       }

       /// <summary>
       /// Deletes the specified db.
       /// </summary>
       /// <param name="db">The db.</param>
       public void Delete(IDatabase db) {
           try {
               db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.SixtID);
           } catch (Exception ex) {
               throw ex;
           }
       }

       #endregion methodes
   }
}
