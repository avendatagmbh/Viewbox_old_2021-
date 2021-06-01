using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
    public class EmployeeDatelist {

        private const string TABLE_NAME = "EMPLOYEEDATELIST";
        private const string EMPLOYEE = "EMPLOYEE";
        private const string DATELIST = "DATELIST";

        /***********************************************************************/
        #region properties

        public int Employee { get; set; }      //PK+FK(employee)
        public int Datelist { get; set; }          // PK+FK(date)

        #endregion properties

        /************************************************************************/
        #region methodes

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="db">The db.</param>
        public static void CreateTable(IDatabase db) {
            db.ExecuteNonQuery("CREATE  TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) +" ("+
                                db.Enquote(EMPLOYEE) + " INTEGER  NOT NULL ," +
                                db.Enquote(DATELIST) + " INTEGER  NOT NULL ," +
                                "PRIMARY KEY (" + db.Enquote(EMPLOYEE) +", " + db.Enquote(DATELIST) + ") ) ENGINE = MyIsam");
        }

        /// <summary>
        /// Loads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private static EmployeeDatelist Load(DbDataReader reader) {
            EmployeeDatelist EmployeeDatelist = new EmployeeDatelist();
            EmployeeDatelist.Employee = (int)reader[0];
            EmployeeDatelist.Datelist = (int)reader[1];
            return EmployeeDatelist;
        }


        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns></returns>
        public static List<EmployeeDatelist> GetList(DbAccess.IDatabase db) {
            List<EmployeeDatelist> employeeDateList = new List<EmployeeDatelist>();
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
                if (reader.HasRows) {
                    while (reader.Read()) {
                        employeeDateList.Add(Load(reader));
                    }
                    reader.Close();
                    return employeeDateList;
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
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " /*TODO*/);
                bool hasRows = reader.HasRows;
                reader.Close();

                if (hasRows) {
                    db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " /*TODO*/);
                } else {
                    db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) /*TODO*/);
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
                db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " /*TODO*/);
            } catch (Exception ex) {
                throw ex;
            }
        }

        #endregion methodes
    }
}
