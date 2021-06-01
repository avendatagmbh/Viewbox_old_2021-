using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
    public enum DrivingType : int {
        // Diese Schnittstelle entspricht einer Tabelle in einer Datenbank
        Bahn=1,
        Flug,
        RentCar,
        eigenes_Auto,
        Zu_Fuß,
        Taxi,
    }

    public class DrivingCompany {

        private const string TABLE_NAME = "DRIVINGCOMPANY";
        private const string ID = "ID";
        private const string NAME = "NAME";
        private const string DRIVINGTYPE = "DRIVINGTYPE";
        private const string ADDRESS = "ADDRESS";
        private const string PHONE = "PHONE";
        private const string INTERNET = "INTERNET";
        private const string SPECIFICATION = "SPECIFICATION";

    /*********************************************************************/
        #region properties

        public int DrivingCompanyID { get; set; }  //PK
        public string Name { get; set; }            // varchar(100)
        public DrivingType DrivingType { get; set; }       // FK(drivingtype)
        public int Address { get; set; }           //FK(address-class)
        public string Phone { get; set; }           // varchar (20)
        public string Internet { get; set; }        // varchar(50)
        public string Specification { get; set; }   // varchar(255)

        #endregion properties
        /*********************************************************************/
        #region methodes

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="db">The db.</param>
        public static void CreateTable(IDatabase db) {
            db.ExecuteNonQuery("CREATE  TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) +" (" +
                                db.Enquote(ID) + " INTEGER  NOT NULL AUTO_INCREMENT ," +
                                db.Enquote(NAME) + " VARCHAR(100) NOT NULL ," +
                                db.Enquote(DRIVINGTYPE) + " INTEGER  NOT NULL ," +
                                db.Enquote(ADDRESS) + " INTEGER  NOT NULL ," +
                                db.Enquote(PHONE) + " VARCHAR(20) NOT NULL ," +
                                db.Enquote(INTERNET) + " VARCHAR(50) NULL ," +
                                db.Enquote(SPECIFICATION) + " VARCHAR(255) NULL ," +
                                "PRIMARY KEY (" +
                                db.Enquote(ID) +") ) ENGINE = MyIsam" );
        }

        /// <summary>
        /// Loads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private static DrivingCompany Load(DbDataReader reader) {
            DrivingCompany drivingCompany = new DrivingCompany();
            drivingCompany.DrivingCompanyID = (int)reader[0];
            drivingCompany.Name= reader[1].ToString();
            drivingCompany.DrivingType = (DrivingType)reader.GetInt32(2);
            drivingCompany.Address= (int)reader[3];
            drivingCompany.Phone = reader[4].ToString();
            drivingCompany.Internet = reader[5].ToString();
            drivingCompany.Specification =reader[6].ToString();
            return drivingCompany;
        }


        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns></returns>
        public static List<DrivingCompany> GetList(DbAccess.IDatabase db) {
            List<DrivingCompany> companyList = new List<DrivingCompany>();
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
                if (reader.HasRows) {
                    while (reader.Read()) {
                        companyList.Add(Load(reader));
                    }
                    reader.Close();
                    return companyList;
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
        public DrivingCompany Load(DbAccess.IDatabase db, int id) {
            try {
                DrivingCompany company = new DrivingCompany();
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
                if (reader.HasRows) {
                    while (reader.Read()) {
                        company = Load(reader);
                    }
                    reader.Close();
                    return company;
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
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.DrivingCompanyID);
                bool hasRows = reader.HasRows;
                reader.Close();

                if (hasRows) {
                    db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(NAME) + " = " + db.GetSqlString(this.Name) + ", " +
                                                                                      db.Enquote(DRIVINGTYPE) + " = " + (int)this.DrivingType + ", " +
                                                                                      db.Enquote(ADDRESS) + " = " + this.Address + ", " +
                                                                                      db.Enquote(PHONE) + " = " + db.GetSqlString(this.Phone) + ", " +
                                                                                      db.Enquote(INTERNET) + " = " + db.GetSqlString(this.Internet) + ", " +
                                                                                      db.Enquote(SPECIFICATION) + " = " + db.GetSqlString(this.Specification));
                } else {
                    db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(NAME) + ", " +
                                                                                        db.Enquote(DRIVINGTYPE) + ", " +
                                                                                        db.Enquote(ADDRESS) + ", " +
                                                                                        db.Enquote(PHONE) + ", " +
                                                                                        db.Enquote(INTERNET) + ", " +
                                                                                        db.Enquote(SPECIFICATION) + ") " +

                                       "VALUES (" + db.GetSqlString(this.Name) + ", " +
                                                     (int)this.DrivingType + ", " +
                                                      this.Address + ", " +
                                                     db.GetSqlString(this.Phone) + ", " +
                                                     db.GetSqlString(this.Internet) + ", " +
                                                   db.GetSqlString(this.Specification) + " )");
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
                db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.DrivingCompanyID);
            } catch (Exception ex) {
                throw ex;
            }
        }

        #endregion methodes
    }
}
