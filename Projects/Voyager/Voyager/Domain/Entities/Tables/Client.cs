using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
    public class Client {

        private const string TABLE_NAME = "Client";
        private const string ID = "ID";
        private const string NAME = "NAME";
        private const string ADDRESS = "ADDRESS";
        private const string PHONE = "PHONE";
        private const string USERFIRM ="USERFIRM";
        private const string SPECIFICATION = "SPECIFICATION";
        private const string INDEXCLIENT = "client_FK_user_firm";

        /**************************************************************************************/
        #region properties

        public int ClientID { get; set; }      // PK
        public string Name { get; set; }        // varchar(80)
        public int Address { get; set; }       // FK(address-class)
        public string Phone { get; set; }       //varchar(20)
        public int UserFirm { get;set;}        // FK (UserFirm-class)
        public string specification { get; set; }   //varchar (255)


        #endregion properties

        /***************************************************************************************/
        #region constructor 

        public Client() {
            this.ClientID = 0;
            this.Name = string.Empty;
            this.Address = 0;
            this.Phone = string.Empty;
            this.UserFirm = 0;
            this.specification = string.Empty;

        }

        #endregion constructor
        /*************************************************************************************/
        #region methodes

        /// <summary>
        /// Creates a new table for the actual selected entitie implementation.
        /// </summary>
        /// <param name="db">The db.</param>
        public static void CreateTable(IDatabase db) {
            try {
                db.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) + " (" +
                                    db.Enquote(ID) + " INTEGER NOT NULL AUTO_INCREMENT ," +
                                    db.Enquote(NAME) + " VARCHAR(80) NOT NULL ," +
                                    db.Enquote(ADDRESS) + " INTEGER NOT NULL ," +
                                    db.Enquote(PHONE) + " VARCHAR(20) NOT NULL ," +
                                    db.Enquote(USERFIRM) + " INTEGER NOT NULL ," +
                                    db.Enquote(SPECIFICATION) + " VARCHAR(255) NULL ," +
                                    "PRIMARY KEY (" + db.Enquote(ID) + "),"+
                                    "INDEX " + db.Enquote(INDEXCLIENT)+ " ("  + db.Enquote(USERFIRM)+" ASC" + ")) ENGINE = MyIsam");
            } catch (Exception ex) {
                throw ex;
            }
        }


        /// <summary>
        /// Loads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private static  Client Load(DbDataReader reader) {
            Client client = new Client();
            client.ClientID = (int)reader[0];
            client.Name = reader[1].ToString();
            client.Address = (int)reader[2];
            client.Phone = reader[3].ToString();
            client.UserFirm = (int)reader[4];
            client.specification = reader[5].ToString();
            return client;
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns></returns>
        public static Dictionary<uint, Client> GetList(IDatabase db, int firmid) {
            Dictionary<uint, Client> clientList = new Dictionary<uint, Client>();
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(USERFIRM) + " = " + firmid);
                while (reader.Read()) {
                    Client cl = Load(reader);
                    clientList.Add((uint)cl.ClientID, cl);
                }
                reader.Close();
                return clientList;
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
        public Client Load(IDatabase db, int id) {
            Client client = new Client();
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
                if (reader.HasRows) {
                    while (reader.Read()) {
                        client = Load(reader);
                    }
                    reader.Close();
                    return client;
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
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.ClientID);
                bool hasRows = reader.HasRows;
                reader.Close();

                if (hasRows) {
                    db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(NAME) + " = " + db.GetSqlString(this.Name) + ", " +
                                                                                      db.Enquote(ADDRESS) + " = " + this.Address + ", " +
                                                                                      db.Enquote(PHONE) + " = " + db.GetSqlString(this.Phone) + ", " +
                                                                                      db.Enquote(USERFIRM) + " = " + this.UserFirm  + ", " +
                                                                                      db.Enquote(SPECIFICATION) + " = " + db.GetSqlString(this.specification));
                } else {
                    db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(NAME) + ", " +
                                                                                        db.Enquote(ADDRESS) + ", " +
                                                                                        db.Enquote(PHONE) + ", " +
                                                                                        db.Enquote(USERFIRM) + ", " +
                                                                                        db.Enquote(SPECIFICATION) + ") " +
                                       "VALUES (" + db.GetSqlString(this.Name) + ", " +
                                                    this.Address + ", " +
                                                    db.GetSqlString(this.Phone) + ", " +
                                                    this.UserFirm + ", " +
                                                    db.GetSqlString(this.specification) + " )");
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
                db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.ClientID);
            } catch (Exception ex) {
                throw ex;
            }
        }


        public static void TestInsert(IDatabase db, Client testClient) {
            try {
                testClient.Save(db);

            } catch (Exception ex) {
                throw ex;
            }

        }
        #endregion methodes
    }
}
