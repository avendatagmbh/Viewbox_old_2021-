using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
   public class TrainStation {

       private const string TABLE_NAME = "TRAINSTATION";
       private const string ID = "ID";
       private const string NAME = "NAME";
       private const string INFO = "INFO";

       /***********************************************************************/
       #region properties

       public int TrainStationID { get; set; }      //PK+FK(address
       public string Name { get; set; }         //varchar (100)
       public string Info { get; set; }         // varchar(255)

       #endregion properties
       /**********************************************************************/
       #region methodes

       /// <summary>
       /// Creates the table.
       /// </summary>
       /// <param name="db">The db.</param>
       public static void CreateTable(IDatabase db) {
           try {
               db.ExecuteNonQuery("CREATE  TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) + " (" +
                                    db.Enquote(ID) + " INTEGER  NOT NULL ," +
                                    db.Enquote(NAME) + " VARCHAR(100) NOT NULL ," +
                                    db.Enquote(INFO) + " VARCHAR(255) NULL ," +
                                    "PRIMARY KEY (" + db.Enquote(ID) + ") ) ENGINE = MyIsam");
           } catch (Exception ex) {
               throw ex;
           }
       }

       /// <summary>
       /// Loads the specified reader.
       /// </summary>
       /// <param name="reader">The reader.</param>
       /// <returns></returns>
       private static TrainStation Load(DbDataReader reader) {
           TrainStation trainst = new TrainStation();
           trainst.TrainStationID = (int)reader[0];
           trainst.Name = reader[1].ToString();
           trainst.Info = reader[2].ToString();
           return trainst;
       }


       /// <summary>
       /// Gets the list.
       /// </summary>
       /// <param name="db">The db.</param>
       /// <returns></returns>
       public static List<TrainStation> GetList(DbAccess.IDatabase db) {
           List<TrainStation> trainstList = new List<TrainStation>();
           try {
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
               if (reader.HasRows) {
                   while (reader.Read()) {
                       trainstList.Add(Load(reader));
                   }
                   reader.Close();
                   return trainstList;
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
       public TrainStation Load(DbAccess.IDatabase db, int id) {
           TrainStation trainst = new TrainStation();
           try {
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
               if (reader.HasRows) {
                   while (reader.Read()) {
                       trainst = Load(reader);
                   }
                   reader.Close();
                   return trainst;
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
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.TrainStationID);
               bool hasRows = reader.HasRows;
               reader.Close();

               if (hasRows) {
                   db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(NAME) + " = " + db.GetSqlString(this.Name) + ", " +
                                                                                     db.Enquote(INFO) + " = " + db.GetSqlString(this.Info));
               } else {
                   db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(NAME) + ", " +
                                                                                       db.Enquote(INFO) + ") " +

                                      "VALUES (" + db.GetSqlString(this.Name) + ", " +
                                                   db.GetSqlString(this.Info) + " )");
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
               db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.TrainStationID);
           } catch (Exception ex) {
               throw ex;
           }
       }


       #endregion methodes
   }
}
