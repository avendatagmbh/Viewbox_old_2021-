using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
    public class Airport {

        private const string TABLE_NAME = "AIRPORT";
        private const string ID = "ID";
        private const string NAME = "NAME";
        private const string IATACODE = "IATACODE";
        private const string INFO = "INFO";


        /**************************************************************************/
        #region properties

        public int AirportID { get; set; }   // PK+FK(address-class)
        public string Name { get; set; }        //varchar(100)
        public string IATACode { get; set; }    //varchar(5)
        public string Info { get; set; }        // varchar(255)

        #endregion properties
        /**************************************************************************/
        #region methodes

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="db">The db.</param>
        public static void CreateTable(IDatabase db) {
            db.ExecuteNonQuery("CREATE  TABLE IF NOT EXISTS " +db.Enquote(TABLE_NAME) + " (" +
                                db.Enquote(ID) +" INTEGER  NOT NULL ," +
                                db.Enquote(NAME) + " VARCHAR(100) NOT NULL ," +
                                db.Enquote(IATACODE) + " VARCHAR(5) NOT NULL ," +
                                db.Enquote(INFO) + " VARCHAR(255) NULL ," +
                                "PRIMARY KEY (" + db.Enquote(ID) +  ") ) ENGINE = MyIsam");
        }

        /// <summary>
        /// Loads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private static Airport Load(DbDataReader reader) {
            Airport airport = new Airport();
            airport.AirportID = (int)reader[0];
            airport.Name = reader[1].ToString();
            airport.IATACode = reader[2].ToString();
            airport.Info = reader[3].ToString();
            return airport;
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns></returns>
        public static List<Airport> GetList(IDatabase db) {
            List<Airport> list = new List<Airport>();
            DbDataReader reader =  db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
            if (reader.HasRows) {
                while (reader.Read()) {
                    list.Add(Load(reader));
                }
                reader.Close();
                return list;
            } else {
                reader.Close();
                return null;
            }
        }



        /// <summary>
        /// Loads the specified db.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Airport Load(IDatabase db, int id) {
            Airport airport = new Airport();
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
                if (reader.HasRows) {
                    while (reader.Read()) {
                        airport = Load(reader);
                    }
                    reader.Close();
                    return airport;
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
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.AirportID);
                bool hasRows = reader.HasRows;
                reader.Close();

                if (hasRows) {
                    db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(NAME) + " = " + db.GetSqlString(this.Name) + ", " +
                                                                                      db.Enquote(IATACODE) + " = " + db.GetSqlString(this.IATACode) + ", " +
                                                                                      db.Enquote(INFO) + " = " + db.GetSqlString(this.Info));
                } else {
                    db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(NAME) + ", " +
                                                                                        db.Enquote(IATACODE) + ", " +
                                                                                        db.Enquote(INFO) + ") " +

                                       "VALUES (" + db.GetSqlString(this.Name) + ", " +
                                                     db.GetSqlString(this.IATACode) + ", " +
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
                db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.AirportID);
            }catch (Exception ex){
                throw ex;
            }
        }


        #endregion methodes
    }
}
