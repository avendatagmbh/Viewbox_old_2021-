using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
    public class AddressType {

        private const string TABLE_NAME = "ADDRESSTYPE";
        private const string ID = "ID";
        private const string TYPE = "TYPE";

        /***************************************************************************************/
        #region properties

        public uint TypeID { get; set; }    //PK
        public string Type { get; set; }   // varchar(30)


        #endregion properties
        /**************************************************************************************/

        #region methodes

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="db">The db.</param>
        public static void CreateTable(IDatabase db) {

            db.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) +" (" +
                                db.Enquote(ID) +" INTEGER  NOT NULL AUTO_INCREMENT ," +
                                db.Enquote(TYPE) +" VARCHAR(30) NOT NULL ,"+
                                "PRIMARY KEY (" + db.Enquote(ID) +") ) ENGINE = MyIsam;");
        }

        /// <summary>
        /// Loads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private AddressType Load(DbDataReader reader) {
            AddressType addtype = new AddressType();
            addtype.TypeID = (uint)reader[0];
            addtype.Type = reader[1].ToString();
            return addtype;
        }


        /// <summary>
        /// Loads the specified db.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public AddressType Load(IDatabase db, int id) {
            DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
            if (reader.HasRows) {
                AddressType addressType = Load(reader);
                reader.Close();
                return addressType;

            } else {
                // Insert
                reader.Close();
                return null;
            }

        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns></returns>
        public static List<AddressType> GetList(IDatabase db) {
            List<AddressType> addressTypeList = new List<AddressType>();
            DbDataReader reader =  db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
            if (reader.HasRows) {
                while (reader.Read()) {
                    AddressType type = new AddressType();
                    addressTypeList.Add(type.Load(reader));
                }
                reader.Close();
                return addressTypeList;
            } else {
                reader.Close();
                return null;
            }   
        }

        /// <summary>
        /// Saves the specified db.
        /// </summary>
        /// <param name="db">The db.</param>
        public void Save(IDatabase db) {
            DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + TypeID);
            if (reader.HasRows) {
                // Update
            } else {
                //Insert
            }
        }

        #endregion methodes
    }
}
