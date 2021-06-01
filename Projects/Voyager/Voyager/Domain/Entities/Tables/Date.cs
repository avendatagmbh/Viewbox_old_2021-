using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {

    public class Date{

        private const string TABLE_NAME = "DATE";
        private const string ID = "ID";
        private const string CLIENT = "CLIENT";
        private const string STARTADDRESS = "STARTADDRESS";
        private const string ENDADDRESS = "ENDADDRESS";
        private const string CONTACTPERSON = "CONTACTPERSON";
        private const string STARTTIME = "STARTTIME";
        private const string ENDTIME = "ENDTIME";
        private const string TOUR = "TOUR";
        private const string REPORT = "REPORT";
        private const string DATELIST = "DATELIST";
        private const string DATE_FK_DATELIST = "DATE_FK_DATELIST";

        /*********************************************************************************************/
        #region properties

        public int DateID { get; set; }        // PK
        public int Client { get; set; }        // FK(client-class)
        public DateTime StartTime { get; set; } 
        public DateTime EndTime { get; set; }
        public int StartAddress { get; set; }  //FK(address-class)
        public int EndAddress { get; set; }    //FK(address-class)
        public int ContactPerson { get; set; } //FK(contactperson-class)
        public int Tour { get; set; }          // FK(tour-class)
        public int Datelist { get; set; }
        public string Report { get; set; }      // varchar(255)
        
        #endregion properties

        /*****************************************************************************************/
        #region constructor

        public Date() {
            this.DateID = 0;
            this.Client = 0;
            this.StartTime = DateTime.Now;
            this.EndTime = DateTime.Now;
            this.StartAddress = 0;
            this.EndAddress = 0;
            this.ContactPerson = 0;
            this.Tour = 0;
            this.Report = string.Empty;
            this.Datelist = 0;
        }

        #endregion constructor
        /*****************************************************************************************/
        #region methodes
        /// <summary>
        /// Creates a new table for the actual selected entitie implementation.
        /// </summary>
        /// <param name="db">The db.</param>
        public static void CreateTable(IDatabase db) {
            db.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) +" (" +
                                db.Enquote(ID) + " INTEGER  NOT NULL AUTO_INCREMENT ," +
                                db.Enquote(CLIENT) +" INTEGER  NOT NULL ," +
                                db.Enquote(STARTADDRESS) + " INTEGER  NOT NULL ," +
                                db.Enquote(ENDADDRESS) + " INTEGER  NOT NULL ," +
                                db.Enquote(CONTACTPERSON) + " INTEGER  NOT NULL ," +
                                db.Enquote(STARTTIME) + " DATETIME NOT NULL ," +
                                db.Enquote(ENDTIME) + " DATETIME NOT NULL ," +
                                db.Enquote(TOUR) + " INTEGER  NOT NULL ," +
                                db.Enquote(DATELIST) + " INTEGER  NOT NULL ," +
                                db.Enquote(REPORT)+ " VARCHAR(255) NULL , PRIMARY KEY (" +
                                db.Enquote(ID) + ", "+
                                "INDEX "+ db.Enquote(DATE_FK_DATELIST) +" ("+ db.Enquote(DATELIST) + " ASC) ,"+
                                ") ) ENGINE = MyIsam" );
        }



        /// <summary>
        /// Loads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private static Date Load(DbDataReader reader) {
            Date date = new Date();
            date.DateID = (int)reader[0];
            date.Client = (int)reader[1];
            date.StartAddress = (int)reader[2];
            date.EndAddress = (int)reader[3];
            date.ContactPerson = (int)reader[4];
            date.StartTime = (DateTime)reader[5];
            date.EndTime = (DateTime)reader[6];
            date.Tour = (int)reader[7];
            date.Datelist = (int)reader[8];
            date.Report = reader[9].ToString();
            return date;
        }


        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns></returns>
        public static List<Date> GetList(DbAccess.IDatabase db) {
            List<Date> dateList = new List<Date>();
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
                if (reader.HasRows) {
                    while (reader.Read()) {
                        dateList.Add(Load(reader));
                    }
                    reader.Close();
                    return dateList;
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
        public static Date Load(DbAccess.IDatabase db, int id) {
            try {
                Date date = new Date();
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
                if (reader.HasRows) {
                    while (reader.Read()) {
                       date = Load(reader);
                    }
                    reader.Close();
                    return date;
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
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.DateID);
                bool hasRows = reader.HasRows;
                reader.Close();

                if (hasRows) {
                    db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(CLIENT) + " = " + this.Client + ", " +
                                                                                      db.Enquote(STARTADDRESS) + " = " + this.StartAddress + ", " +
                                                                                      db.Enquote(ENDADDRESS) + " = " + this.EndAddress + ", " +
                                                                                      db.Enquote(CONTACTPERSON) + " = " + this.ContactPerson + ", " +
                                                                                      db.Enquote(STARTTIME) + " = " + db.GetSqlString((this.StartTime).ToString("yyyy-MM-dd HH:mm:ss")) + ", " +
                                                                                      db.Enquote(ENDTIME) + " = " + db.GetSqlString((this.EndTime).ToString("yyyy-MM-dd HH:mm:ss")) + ", " +
                                                                                      db.Enquote(TOUR) + " = " + this.Tour + ", " +
                                                                                      db.Enquote(DATELIST) +" = " + this.Datelist +", "+
                                                                                      db.Enquote(REPORT) + " = " + db.GetSqlString(this.Report));
                } else {
                    db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(CLIENT) + ", " +
                                                                                        db.Enquote(STARTADDRESS) + ", " +
                                                                                        db.Enquote(ENDADDRESS) + ", " +
                                                                                        db.Enquote(CONTACTPERSON) + ", " +
                                                                                        db.Enquote(STARTTIME) + ", " +
                                                                                        db.Enquote(ENDTIME) + ", " +
                                                                                        db.Enquote(TOUR) + ", " +
                                                                                        db.Enquote(DATELIST) +", "+
                                                                                        db.Enquote(REPORT) + ") " +

                                       "VALUES (" + this.Client + ", " +
                                                     this.StartAddress + ", " +
                                                     this.EndAddress + ", " +
                                                     this.ContactPerson + ", " +
                                                     db.GetSqlString((this.StartTime).ToString("yyyy-MM-dd HH:mm:ss")) + ", " +
                                                     db.GetSqlString((this.EndTime).ToString("yyyy-MM-dd HH:mm:ss")) + ", " +
                                                     this.Tour + ", " +
                                                     this.Datelist + ", " +
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
                db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.DateID);
            } catch (Exception ex) {
                throw ex;
            }
        }

        public static void TestInsert(IDatabase db, Date testDate) {
            try {
                testDate.Save(db);
            } catch (Exception ex) {
                throw ex;
            }
        }

        #endregion methodes
        /***********************************************************************************************/
    }
}
