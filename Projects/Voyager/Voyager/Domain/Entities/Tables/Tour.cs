using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using DbAccess;

namespace Domain.Entities.Tables {
   public class Tour {

       private const string TABLE_NAME = "TOUR";
       private const string ID = "ID";
       private const string TOTALDISTANCE = "TOTALDISTANCE";
       private const string TOTALDURATION = "TOTALDURATION";
       private const string TOTALCOST = "TOTALCOST";

       /*************************************************************************************/
       #region properties

       public int TourID { get; set; }          //PK
       public double TotalDistance { get; set; }
       public DateTime TotalDuration { get; set; }
       public double TotalCost { get; set; }

       #endregion properties 

       /******************************************************************************************/
       #region methodes

       /// <summary>
       /// Creates a new table for the actual selected entitie implementation.
       /// </summary>
       /// <param name="db">The db.</param>
       public static void CreateTable(DbAccess.IDatabase db) {
           db.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) +" ("+
                                db.Enquote(ID) + " INTEGER  NOT NULL AUTO_INCREMENT ," +
                                db.Enquote(TOTALDISTANCE) +" DOUBLE NOT NULL ," +
                                db.Enquote(TOTALDURATION) + " DATETIME NOT NULL ," +
                                db.Enquote(TOTALCOST) +" DOUBLE NOT NULL ," +
                                "PRIMARY KEY (" + db.Enquote(ID) + ") ) ENGINE = MyIsam");
       }

       /// <summary>
       /// Loads the specified reader.
       /// </summary>
       /// <param name="reader">The reader.</param>
       /// <returns></returns>
       private static Tour Load(DbDataReader reader) {
           Tour tour = new Tour();
           tour.TourID = (int)reader[0];
           tour.TotalDistance = (double)reader[1];
           tour.TotalDuration = (DateTime)reader[2];
           tour.TotalCost = (double)reader[3];
           return tour;
       }

       /// <summary>
       /// Gets the list.
       /// </summary>
       /// <param name="db">The db.</param>
       /// <returns></returns>
       public static List<Tour> GetList(DbAccess.IDatabase db) {
           try {
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
               List<Tour> tourList = new List<Tour>();
               if (reader.HasRows) {
                   while (reader.Read()) {
                       tourList.Add(Load(reader));
                   }
                   reader.Close();
                   return tourList;
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
       public Tour Load(DbAccess.IDatabase db, int id) {
           Tour tour = new Tour();
           try {
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + "=" + id);
               if (reader.HasRows) {
                   while (reader.Read()) {
                       tour = Load(reader);
                   }
                   reader.Close();
                   return tour;
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
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.TourID);
               bool hasRows = reader.HasRows;
               reader.Close();

               if (hasRows) {
                   db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(TOTALDISTANCE) + " = " + this.TotalDistance + ", " +
                                                                                     db.Enquote(TOTALDURATION) + " = " + this.TotalDuration + ", " +
                                                                                     db.Enquote(TOTALCOST) + " = " + this.TotalCost);
               } else {
                   db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(TOTALDISTANCE) + ", " +
                                                                                       db.Enquote(TOTALDURATION) + ", " +
                                                                                       db.Enquote(TOTALCOST) + ") " +

                                      "VALUES (" + this.TotalDistance + ", " +
                                                    this.TotalDuration + ", " +
                                                    this.TotalCost + " )");
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
               db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.TourID);
           } catch (Exception ex) {
               throw ex;
           }
       }


   #endregion methodes
   }
}
