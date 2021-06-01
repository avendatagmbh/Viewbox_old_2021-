using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
    public class Hotel {

        private const string TABLE_NAME = "HOTEL";
        private const string ID = "ID";
        private const string NAME = "NAME";
        private const string STARNUMBER = "STARNUMBER";
        private const string RATING = "RATING";
        private const string PHONE = "PHONE";
        private const string INFO = "INFO";

        /*********************************************************************/
        #region properties

        public int HotelID { get; set; }   //PK+FK(address)
        public string Name { get; set; }    // varchar(100)
        public int StarNummer { get; set; }
        public int Rating { get; set; }
        public string Phone { get; set; }   //varchar(20)
        public string Info { get; set; }    // varchar(255)

        #endregion properties

        /********************************************************************/
        #region methodes

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="db">The db.</param>
        public static void CreateTable(IDatabase db) {
            db.ExecuteNonQuery("CREATE  TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) +" (" +
                                db.Enquote(ID) +" INTEGER  NOT NULL ," +
                                db.Enquote(NAME) +" VARCHAR(100) NOT NULL ," +
                                db.Enquote(STARNUMBER) + " INTEGER  NOT NULL ," +
                                db.Enquote(RATING) + " INTEGER NOT NULL ," +
                                db.Enquote(PHONE) + " VARCHAR(20) NOT NULL ," +
                                db.Enquote(INFO) + " VARCHAR(255) NULL ," +
                                "PRIMARY KEY (" + db.Enquote(ID) +") ) ENGINE = MyIsam");
        }

        /// <summary>
        /// Loads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private static Hotel Load(DbDataReader reader) {
            Hotel hotel = new Hotel();
            hotel.HotelID = (int)reader[0];
            hotel.Name = reader[1].ToString();
            hotel.StarNummer = (int)reader[2];
            hotel.Rating = (int)reader[3];
            hotel.Phone = reader[4].ToString();
            hotel.Info = reader[5].ToString();
            return hotel;
        }


        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns></returns>
        public static List<Hotel> GetList(DbAccess.IDatabase db) {
            List<Hotel> hotelList = new List<Hotel>();
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
                if (reader.HasRows) {
                    while (reader.Read()) {
                        hotelList.Add(Load(reader));
                    }
                    reader.Close();
                    return hotelList;
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
        public Hotel Load(DbAccess.IDatabase db, int id) {
            Hotel hotel = new Hotel();
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
                if (reader.HasRows) {
                    while (reader.Read()) {
                        hotel = Load(reader);
                    }
                    reader.Close();
                    return hotel;
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
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.HotelID);
                bool hasRows = reader.HasRows;
                reader.Close();

                if (hasRows) {
                    db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(NAME) + " = " + db.GetSqlString(this.Name) + ", " +
                                                                                      db.Enquote(STARNUMBER) + " = " + this.StarNummer + ", " +
                                                                                      db.Enquote(RATING) + " = " + this.Rating + ", " +
                                                                                      db.Enquote(PHONE) + " = " + db.GetSqlString(this.Phone) + ", " +
                                                                                      db.Enquote(INFO) + " = " + db.GetSqlString(this.Info));
                } else {
                    db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(NAME) + ", " +
                                                                                        db.Enquote(STARNUMBER) + ", " +
                                                                                        db.Enquote(RATING) + ", " +
                                                                                        db.Enquote(PHONE) + ", " +
                                                                                        db.Enquote(INFO) + ") " +

                                       "VALUES (" + db.GetSqlString(this.Name) + ", " +
                                                     this.StarNummer + ", " +
                                                     this.Rating + ", " +
                                                     db.GetSqlString(this.Phone) + ", " +
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
                db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.HotelID);
            } catch (Exception ex) {
                throw ex;
            }
        }

        #endregion methodes
    }
}
