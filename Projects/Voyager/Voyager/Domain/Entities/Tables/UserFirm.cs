using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
    public class UserFirm {

        private const string TABLE_NAME = "USERFIRM";
        private const string ID = "ID";
        private const string NAME = "NAME";
        private const string ADDRESS = "ADDRESS";
        private const string USERNAME = "USERNAME";
        private const string PASSWORD = "PASSWORD";
        private const string PHONE = "PHONE";
        private const string DESCRIPTION = "DESCRIPTION";

        /***************************************************************************/
        #region properties

        public int UserFirmID { get; set; }    //PK
        public string Name { get; set; }        //varchar(100)
        public int Address { get; set; }       //FK(address-class)
        public string UserName { get; set; }    //varchar(20)
        public string Password { get; set; }    //varchar(256)
        public string Phone { get; set; }       // varchar(20)
        public string Description { get; set; } //varchar(255)

        #endregion properties
        /**************************************************************************/
        #region constructor

        public UserFirm() {
            this.UserFirmID = 0;
            this.Name = string.Empty;
            this.Address = 0;
            this.UserName = string.Empty;
            this.Password = string.Empty;
            this.Phone = string.Empty;
            this.Description = string.Empty;
        }

        #endregion constructor
        /***************************************************************************/
        #region methodes

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="db">The db.</param>
        public static void CreateTable(IDatabase db) {
            try {
                db.ExecuteNonQuery("CREATE  TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) + " (" +
                                    db.Enquote(ID) + " INTEGER  NOT NULL AUTO_INCREMENT ," +
                                    db.Enquote(NAME) + " VARCHAR(100) NOT NULL ," +
                                    db.Enquote(ADDRESS) + " INTEGER  NOT NULL ," +
                                    db.Enquote(USERNAME) + " VARCHAR(20) NOT NULL ," +
                                    db.Enquote(PASSWORD) + "  VARCHAR(256) NOT NULL ," +
                                    db.Enquote(PHONE) + "  VARCHAR(20) NOT NULL ," +
                                    db.Enquote(DESCRIPTION) + " VARCHAR(255) NULL ," +
                                    " PRIMARY KEY (" + db.Enquote(ID) + ") ) ENGINE = MyIsam");
            } catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// Loads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private static UserFirm Load(DbDataReader reader) {
            UserFirm uf = new UserFirm();
            uf.UserFirmID = (int)reader[0];
            uf.Name = reader[1].ToString();
            uf.Address = (int)reader[2];
            uf.UserName = reader[3].ToString();
            uf.Password = reader[4].ToString();
            uf.Phone = reader[5].ToString();
            return uf;
        }


        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns></returns>
        public static List<UserFirm> GetList(DbAccess.IDatabase db) {
            List<UserFirm> uflList = new List<UserFirm>();
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
                if (reader.HasRows) {
                    while (reader.Read()) {
                        uflList.Add(Load(reader));
                    }
                    reader.Close();
                    return uflList;
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
        public static UserFirm Load(DbAccess.IDatabase db, int id) {
            UserFirm uf = new UserFirm();
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
                if (reader.HasRows) {
                    while (reader.Read()) {
                        uf = Load(reader);
                    }
                    reader.Close();
                    return uf;
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
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.UserFirmID);
                bool hasRows = reader.HasRows;
                reader.Close();

                if (hasRows) {
                    db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(NAME) + " = " + db.GetSqlString(this.Name) + ", " +
                                                                                      db.Enquote(ADDRESS) + " = " + this.Address + ", " +
                                                                                      db.Enquote(USERNAME) + " = " + db.GetSqlString(this.UserName) + ", " +
                                                                                      db.Enquote(PASSWORD) + " = " + db.GetSqlString(this.Password) + ", " +
                                                                                      db.Enquote(PHONE) + " = " + db.GetSqlString(this.Phone) + ", " +
                                                                                      db.Enquote(DESCRIPTION) + " = " + db.GetSqlString(this.Description));
                } else {
                    db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(NAME) + ", " +
                                                                                        db.Enquote(ADDRESS) + ", " +
                                                                                        db.Enquote(USERNAME) + ", " +
                                                                                        db.Enquote(PASSWORD) + ", " +
                                                                                        db.Enquote(PHONE) + ", " +
                                                                                        db.Enquote(DESCRIPTION) + ") " +

                                       "VALUES (" + db.GetSqlString(this.Name) + ", " +
                                                     this.Address + ", " +
                                                     db.GetSqlString(this.UserName) + ", " +
                                                     db.GetSqlString(this.Password) + ", " +
                                                     db.GetSqlString(this.Phone) + ", " +
                                                    db.GetSqlString(this.Description) + " )");
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
                db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.UserFirmID);
            } catch (Exception ex) {
                throw ex;
            }
        }

        public static void TestInsert(IDatabase db, UserFirm testFirm) {
            try {
                testFirm.Save(db);
            } catch (Exception ex) {
                throw ex;
            }
        }


        #endregion methodes
    }
}
