using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities.Tables {
    public class GetTour {


        private const string TABLE_NAME = "GETTOUR";
        private const string TOUR = "TOUR";
        private const string TOURSTEP = "TOURSTEP";
        private const string DESCRIPTION = "DESCRIPTION";

        /****************************************************************************/
        #region properties

        public int TourID { get; set; }    // PK+FK(tour-class)
        public int TourStepID { get; set; }    //PK +Fk(TourStep-class)
        public string description { get; set; } // varchar(255)

        #endregion properties
        /****************************************************************************/
        #region methodes

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="db">The db.</param>
        public static void CreateTable(IDatabase db) {
            db.ExecuteNonQuery("CREATE  TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) + " (" +
                                db.Enquote(TOUR) + " INTEGER  NOT NULL ," +
                                db.Enquote(TOURSTEP) +" INTEGER  NOT NULL ," +
                                db.Enquote(DESCRIPTION) + " VARCHAR(255) NULL ," +
                                "PRIMARY KEY (" + db.Enquote(TOUR) + ", " + db.Enquote(TOURSTEP) + ") ) ENGINE = MyIsam");
        }

        /// <summary>
        /// Loads the specified reader.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        private static GetTour Load(DbDataReader reader) {
            GetTour getTour = new GetTour();
            getTour.TourID = (int)reader[0];
            getTour.TourStepID = (int)reader[1];
            getTour.description = reader[2].ToString();
            return getTour;
        }


        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns></returns>
        public static List<GetTour> GetList(DbAccess.IDatabase db) {
            List<GetTour> getTourList = new List<GetTour>();
            try {
                DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
                if (reader.HasRows) {
                    while (reader.Read()) {
                        getTourList.Add(Load(reader));
                    }
                    reader.Close();
                    return getTourList;
                } else {
                    reader.Close();
                    return null;
                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        #endregion methodes
    }
}
