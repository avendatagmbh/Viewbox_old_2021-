using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
   public class TourStep{

       private const string TABLE_NAME = "TOURSTEP";
       private const string ID = "ID";
       private const string START = "START";
       private const string END = "END";
       private const string STEPDISTANCE = "STEPDISTANCE";
       private const string STEPDURATION = "STEPDURATION";
       private const string STEPCOST = "STEPCOST";
       private const string DRIVINGCOMPANY = "DRIVINGCOMPANY";
       private const string DRIVINGTYPE = "DRIVINGTYPE";
       private const string REPORT = "REPORT";

       /************************************************************************/
       #region properties

       public int TourStepID { get; set; }         // PK
       public int Start { get; set; }              // FK(address-class) + NN
       public int End { get; set; }                // FK(address-class)  + NN
       public double StepDistance { get; set; }     // NN
       public DateTime StepDuration { get; set; }   // NN
       public double StepCost { get; set; }         // NN
       public int DrivingCompany { get; set; }     // FK(drivingcompany-class) + NULL
       public DrivingType DrivingType { get; set; }        // FK(drivingtype-enum ) +NN
       public string Report { get; set; }           // varchar(255)

       #endregion properties

       /*************************************************************************/

       #region methodes


       /// <summary>
       /// Creates a new table for the actual selected entitie implementation.
       /// </summary>
       /// <param name="db">The db.</param>
       public static void CreateTable(IDatabase db) {

           db.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME)+" ("+
                                db.Enquote(ID) +" INTEGER  NOT NULL AUTO_INCREMENT ," +
                                db.Enquote(START) +" INTEGER  NOT NULL ," +
                                db.Enquote(END) +" INTEGER  NOT NULL ," +
                                db.Enquote(DRIVINGTYPE) + " INTEGER  NOT NULL ," +
                                db.Enquote(DRIVINGCOMPANY) + " INTEGER NULL ," +
                                db.Enquote(STEPDISTANCE) +" DOUBLE NOT NULL ," +
                                db.Enquote(STEPDURATION) +" DATETIME NOT NULL ,"+
                                db.Enquote(STEPCOST) +" DOUBLE NOT NULL ," +
                                db.Enquote(REPORT) +" VARCHAR(255) NULL ," +
                                "PRIMARY KEY (" + db.Enquote(ID) + ") ) ENGINE = MyIsam");
       }


       /// <summary>
       /// Loads the specified reader.
       /// </summary>
       /// <param name="reader">The reader.</param>
       /// <returns></returns>
       private static TourStep Load(DbDataReader reader) {
           TourStep tourstep = new TourStep();
           tourstep.TourStepID = (int)reader[0];
           tourstep.Start = (int)reader[1];
           tourstep.End = (int)reader[2];
           tourstep.DrivingType = (DrivingType)reader.GetInt32(3);
           tourstep.DrivingCompany = (int)reader[4];
           tourstep.StepDistance = (double)reader[5];
           tourstep.StepDuration = (DateTime)reader[6];
           tourstep.StepCost = (double)reader[7];
           tourstep.Report = reader[8].ToString();
           return tourstep;
       }

       /// <summary>
       /// Gets the list.
       /// </summary>
       /// <param name="db">The db.</param>
       /// <returns></returns>
       public static List<TourStep> GetList(IDatabase db) {
           List<TourStep> tourStepList = new List<TourStep>();
           try {
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
               if (reader.HasRows) {
                   while (reader.Read()) {
                       tourStepList.Add(Load(reader));
                   }
                   reader.Close();
                   return tourStepList;
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
       public TourStep Load(IDatabase db, int id ) {
           TourStep tourStep = new TourStep();
           try {
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
               if (reader.HasRows) {
                   while (reader.Read()) {
                       tourStep = Load(reader);
                   }
                   reader.Close();
                   return tourStep;
               } else {
                   // Insert
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
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.TourStepID);
               bool hasRows = reader.HasRows;
               reader.Close();

               if (hasRows) {
                   db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(START) + " = " + this.Start + ", " +
                                                                                     db.Enquote(END) + " = " + this.End + ", " +
                                                                                     db.Enquote(DRIVINGTYPE) + " = " + (int)this.DrivingType + ", " +
                                                                                     db.Enquote(DRIVINGCOMPANY) + " = " + this.DrivingCompany + ", " +
                                                                                     db.Enquote(STEPDISTANCE) + " = " + this.StepDistance + ", " +
                                                                                     db.Enquote(STEPDURATION) + " = " + this.StepDuration + ", " +
                                                                                     db.Enquote(STEPCOST) + " = " + this.StepCost + ", " +
                                                                                     db.Enquote(REPORT) + " = " + db.GetSqlString(this.Report));
               } else {
                   db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(START) + ", " +
                                                                                       db.Enquote(END) + ", " +
                                                                                       db.Enquote(DRIVINGTYPE) + ", " +
                                                                                       db.Enquote(DRIVINGCOMPANY) + ", " +
                                                                                       db.Enquote(STEPDISTANCE) + ", " +
                                                                                       db.Enquote(STEPDURATION) + ", " +
                                                                                       db.Enquote(STEPCOST) + ", " +
                                                                                       db.Enquote(REPORT) + ") " +

                                      "VALUES (" + this.Start + ", " +
                                                   this.End + ", " +
                                                   (int)this.DrivingType + ", " +
                                                    this.DrivingCompany + ", " +
                                                    this.StepDistance + ", " +
                                                    this.StepDuration + ", " +
                                                    this.StepCost + ", " +
                                                   db.GetSqlString(this.Report) + " )");
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
               db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.TourStepID);
           } catch (Exception ex) {
               throw ex;
           }
       }
       #endregion methodes
   }
}
