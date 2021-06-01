using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;

namespace Domain.Entities {
   public class DType{

       private const string TABLE_NAME = "DRIVINGTYPE";
       private const string ID = "ID";
       private const string TYPENAME = "TYPENAME";

       /***************************************************************/
       #region properties

       public uint DrivingTypeID { get; set; }   //PK
       public string TypeName { get; set; }     // varchar(20)

       #endregion properties

       /********************************************************************/
       #region methodes

       /// <summary>
       /// Creates a new table for the actual selected entitie implementation.
       /// </summary>
       /// <param name="db">The db.</param>
       public static void CreateTable(IDatabase db) {

           db.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) + " ("+
                                db.Enquote(ID) + " INTEGER  NOT NULL AUTO_INCREMENT ," +
                                db.Enquote(TYPENAME) + " VARCHAR(20) NOT NULL ," +
                                "PRIMARY KEY (" + db.Enquote(ID) + ") ) ENGINE = MyIsam;" );          
       }

       /// <summary>
       /// Loads the specified reader.
       /// </summary>
       /// <param name="reader">The reader.</param>
       /// <returns></returns>
       private DType Load(DbDataReader reader) {
           DType dt = new DType();
           dt.DrivingTypeID = (uint)reader[0];
           dt.TypeName = reader[1].ToString();
           return dt;
       }

       /// <summary>
       /// Gets the list.
       /// </summary>
       /// <param name="db">The db.</param>
       /// <returns></returns>
       public List<DType> GetList(IDatabase db) {
           List<DType> drivingTypeList = new List<DType>();
           DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
           if (reader.HasRows) {
               while (reader.Read()) {
                   DType type = new DType();
                   drivingTypeList.Add(type.Load(reader));
               }
               reader.Close();
               return drivingTypeList;
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
       public DType Load(IDatabase db, int id) {
           DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
           if (reader.HasRows) {
               DType type = Load(reader);
               reader.Close();
               return type;
           } else {
               // Insert
               reader.Close();
               return null;
           }
       }

       #endregion methodes
   }
}
