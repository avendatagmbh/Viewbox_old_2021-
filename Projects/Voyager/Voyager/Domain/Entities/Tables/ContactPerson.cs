using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
    public enum FormOfAddress : int {
        Herr =1,
        Frau,
        Dr,
    }


    public class ContactPerson {

        private const string TABLE_NAME = "CONTACTPERSON";
        private const string ID = "ID";
        private const string NAME = "NAME";
        private const string FORMOFADDRESS = "FORMOFADDRESS";
        private const string CLIENT = "CLIENT";
        private const string DIRECTIONDIAL = "DIRECTIONDIAL";
        private const string SPECIFICATION = " SPECIFICATION";

        /**************************************************************************/
        #region properties

        public int ContactPersonID { get; set; }   // PK
        public string Name { get; set; }            //varchar(45)
        public FormOfAddress FormOfAddress { get; set; }   //varchar(10)
        public int Client { get; set; }            // FK (client-class)
        public string DirectDial { get; set; }      // varchar (10)
        public string Specification { get; set; }   // varchar(255)

        #endregion properties

        /**************************************************************************/
        #region methodes

        public static void CreateTable(IDatabase db) {
            db.ExecuteNonQuery("CREATE  TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) + " (" +
                                db.Enquote(ID) + " INTEGER  NOT NULL AUTO_INCREMENT ," +
                                db.Enquote(NAME) + " VARCHAR(45) NOT NULL ," +
                                db.Enquote(FORMOFADDRESS) + " VARCHAR(10) NOT NULL ," +
                                db.Enquote(CLIENT) + " INTEGER  NOT NULL ," +
                                db.Enquote(DIRECTIONDIAL) + " VARCHAR(10) NOT NULL ," +
                                db.Enquote(SPECIFICATION) + " VARCHAR(255) NULL , PRIMARY KEY (" +
                                db.Enquote(ID) + ") ) ENGINE = MyIsam" );
        }

        /// <summary>
        /// Loads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private static ContactPerson Load(DbDataReader reader) {
            ContactPerson ctp = new ContactPerson();
            ctp.ContactPersonID = (int)reader[0];
            ctp.Name = reader[1].ToString();
            ctp.FormOfAddress = (FormOfAddress)reader.GetInt32(2);
            ctp.Client =(int)reader[3];
            ctp.DirectDial = reader[4].ToString();
            ctp.Specification = reader[5].ToString();
            return ctp;
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns></returns>
        public static List<ContactPerson> GetList(IDatabase db) {
            List<ContactPerson> ctpList = new List<ContactPerson>();
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
                if (reader.HasRows) {
                    while (reader.Read()) {
                        ctpList.Add(Load(reader));
                    }
                    reader.Close();
                    return ctpList;
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
        public static ContactPerson Load(IDatabase db, int id) {
            ContactPerson ctp = new ContactPerson();
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + "=" + id);
                if (reader.HasRows) {
                    while (reader.Read()) {
                        ctp = Load(reader);
                    }
                    reader.Close();
                    return ctp;
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
            DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.ContactPersonID);
            bool hasRows = reader.HasRows;
            reader.Close();

            if (hasRows) {
                db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(NAME) + " = " + db.GetSqlString(this.Name) + ", " +
                                                                                  db.Enquote(FORMOFADDRESS) + " = " + db.GetSqlString(this.FormOfAddress.ToString()) + ", " +
                                                                                  db.Enquote(CLIENT) + " = " + this.Client + ", " +
                                                                                  db.Enquote(DIRECTIONDIAL) + " = " + db.GetSqlString(this.DirectDial) + ", " +
                                                                                  db.Enquote(SPECIFICATION) + " = " + db.GetSqlString(this.Specification));
                                                                                
            } else {
                db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(NAME) + ", " +
                                                                                    db.Enquote(FORMOFADDRESS) + ", " +
                                                                                    db.Enquote(CLIENT) + ", " +
                                                                                    db.Enquote(DIRECTIONDIAL) + ", " +
                                                                                    db.Enquote(SPECIFICATION) + ") " +

                                   "VALUES (" + db.GetSqlString(this.Name) + ", " +
                                                 db.GetSqlString(this.FormOfAddress.ToString()) + ", " +
                                                 this.Client + ", " +
                                                 db.GetSqlString(this.DirectDial) + ", " +
                                                db.GetSqlString(this.Specification) + " )");
            }
        }

        /// <summary>
        /// Deletes the specified db.
        /// </summary>
        /// <param name="db">The db.</param>
        public void Delete(IDatabase db) {
            db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.ContactPersonID);
        }

        #endregion methodes
    }
}
