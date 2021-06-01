using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
    public class Category {

        private const string TABLE_NAME = "CATEGORY";
        private const string ID = "ID";
        private const string CATEGORYTYPE = "CATEGORYTYPE";

        /**************************************************************************/
        #region properties

        public uint CategoryID { get; set; }    // PK
        public string Type { get; set; }        // VARCHAR(45)

        #endregion properties

        /***************************************************************************/

        #region methodes

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="db">The db.</param>
        public static void CreateTable(IDatabase db) {
            db.ExecuteNonQuery("CREATE  TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) + " (" +
                                db.Enquote(ID) + " INTEGER  NOT NULL AUTO_INCREMENT ," +
                                db.Enquote(CATEGORYTYPE) + " VARCHAR(45) NOT NULL ," +
                                "PRIMARY KEY (" + db.Enquote(ID) +") ) ENGINE = MyIsam;");
        }

        /// <summary>
        /// Loads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private Category Load(DbDataReader reader) {
            Category ctg = new Category();
            ctg.CategoryID = (uint)reader[0];
            ctg.Type = reader[1].ToString();
            return ctg;
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns></returns>
        public static List<Category> GetList(IDatabase db) {
            List<Category> ctgList = new List<Category>();
            DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
            if (reader.HasRows) {
                while (reader.Read()) {
                    Category ctg = new Category();
                    ctgList.Add(ctg.Load(reader));
                }
                reader.Close();
                return ctgList;
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
        public Category Load(IDatabase db, int id) {
            Category ctg = new Category();
            DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + "=" + id);
            if (reader.HasRows) {
                ctg.Load(reader);
                reader.Close();
                return ctg;
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
            DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.CategoryID);
            bool hasRow = reader.HasRows;
            reader.Close();
            if (hasRow) {
                db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(CATEGORYTYPE) + " = " + db.GetSqlString(this.Type));
            } else {
                db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + " (" + db.Enquote(CATEGORYTYPE) + ") " + 
                                   "VALUE (" + db.GetSqlString(this.Type) + ") "); 
            }

        }


        /// <summary>
        /// Deletes the specified db.
        /// </summary>
        /// <param name="db">The db.</param>
        public void Delete(IDatabase db){
            db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.CategoryID);

        }
        #endregion methodes
    }
}
