using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
   public class Datelist {

       private const string TABLE_NAME = "DATELIST";
       private const string ID = "ID";
       private const string COST = "COST";

       /*************************************************************/
       #region properties   
       public int DatelistID { get; set; }
       public double Cost { get; set; }

        #endregion properties
       /*************************************************************/
       #region methode
       public static void CreateTable(IDatabase db) {
           db.ExecuteNonQuery("CREATE  TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) + " (" +
                               db.Enquote(ID) + " INTEGER  NOT NULL ," +
                               db.Enquote(COST) + " INTEGER  NOT NULL ," +
                               "PRIMARY KEY (" + db.Enquote(ID) + ", " + db.Enquote(COST) + ") ) ENGINE = MyIsam");
       }

       private static Datelist Load(DbDataReader reader) {
           Datelist datelist = new Datelist();
           datelist.DatelistID = (int)reader[0];
           datelist.Cost = (int)reader[1];
           return datelist;
       }

       public static List<Datelist> GetList(DbAccess.IDatabase db) {
           List<Datelist> datelistcatalog = new List<Datelist>();
           try {
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
               if (reader.HasRows) {
                   while (reader.Read()) {
                       datelistcatalog.Add(Load(reader));
                   }
                   reader.Close();
                   return datelistcatalog;
               } else {
                   reader.Close();
                   return null;
               }
           } catch (Exception ex) {
               throw ex;
           }
       }

       public static Datelist Load(DbAccess.IDatabase db, int id) {
           try {
               Datelist datelist = new Datelist();
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
               if (reader.HasRows) {
                   while (reader.Read()) {
                       datelist = Load(reader);
                   }
                   reader.Close();
                   return datelist;
               } else {
                   reader.Close();
                   return null;
               }
           } catch (Exception ex) {
               throw ex;
           }
       }


        public void Save(IDatabase db) {
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.DatelistID);
                bool hasRows = reader.HasRows;
                reader.Close();

                if (hasRows) {
                    db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(COST) + " = " + this.Cost);
                                                                                    
                } else {
                    db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(COST) + ") " +

                                       "VALUES (" + this.Cost +  " )");
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
                db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.DatelistID);
            } catch (Exception ex) {
                throw ex;
            }
        }


        #endregion methodes
    }
}
