using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Domain.Entities.Tables {

    public enum TypeOfAddress : int{
        // Diese Enumeration entspricht in der Datenbank einer Tabelle
        Default,
        Hotel = 1,
        Bahnhof = 2,
        Flughafen = 3,
        SixtStation = 4,
        Mitarbeiter,
        Kunde,
        Firma,
        
    }

   public class Address {

       private const string TABLE_NAME = "ADDRESS";
       private const string ID = "ID";
       private const string STREET = "STREET";
       private const string POSTCODE = "POSTCODE";
       private const string LAND = "LAND";
       private const string LATITUDE = "LATITUDE";
       private const string LONGITUDE = "LONGITUDE";
       private const string ADDRESSTYPE = "ADDRESSTYPE";
       private const string DESCRIPTION = "DESCRIPTION";


       private string _addressString ;
       /*****************************************************************************/
       #region properties
       

       public int AddressID { get; set; }      //PK

       [Required]
       [DisplayName("Straße")]
       public string Street { get; set; }       //varchar(100)

       [Required]
       [DisplayName("PLZ")]
       public string Postcode { get; set; }     // varchar(20)

       [Required]
       [DisplayName("Land")]
       public string Land { get; set; }         // varchar(100)

       public double Latitude { get; set; }     // double
       public double Longitude { get; set; }    // double
       
       public TypeOfAddress TypeOfAddress { get;set;} // FK (AddressType)  
    
       [DisplayName("Beschreibung")]
       [DataType(DataType.MultilineText)]
       public string description { get; set; }  //varchar(255)

       [Required]
       public string AddressString {
           get {
               if (this.Street != "" || this.Postcode != "" || this.Land != "") {
                   return _addressString = this.Street + ", " + this.Postcode + ", " + this.Land;
               } else {
                   return _addressString = "";
               }
           }
           set { }
       }

       public bool IsChecked { get; set; }

       #endregion properties
       /*********************************************************************************/
       #region constructor

       public Address() {
           this.AddressID = 0;
           this.Street = "";
           this.Postcode = "";
           this.Land = "";
           this.Latitude = 0.0;
           this.Longitude = 0.0;
           this.TypeOfAddress = Tables.TypeOfAddress.Default;
           this.description = "";
       }

       #endregion constructor
       /**********************************************************************************/
       #region methodes
       /// <summary>
       /// Creates a new table for the actual selected entitie implementation.
       /// </summary>
       /// <param name="db">The db.</param>
       public static void CreateTable(IDatabase db) {
           db.ExecuteNonQuery("CREATE  TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) + " (" +
                             db.Enquote(ID) + " INTEGER NOT NULL AUTO_INCREMENT ," +
                             db.Enquote(STREET) + " VARCHAR(100) NOT NULL ," +
                             db.Enquote(POSTCODE) + " VARCHAR(20) NOT NULL ," +
                             db.Enquote(LAND) + " VARCHAR(100) NOT NULL ," +
                             db.Enquote(LATITUDE) +" DOUBLE NOT NULL ," +
                             db.Enquote(LONGITUDE) + " DOUBLE NOT NULL ," +
                             db.Enquote(ADDRESSTYPE) + " INTEGER  NOT NULL ," +
                             db.Enquote(DESCRIPTION) + " VARCHAR(255) NULL ," +
                             "PRIMARY KEY (" + db.Enquote(ID) +") ) ENGINE = MyIsam");
       }


       /// <summary>
       /// Loads the specified reader.
       /// </summary>
       /// <param name="reader">The reader.</param>
       /// <returns></returns>
       private static Address Load(DbDataReader reader) {
           Address address = new Address();
           address.AddressID = (int)reader[0];
           address.Street = reader[1].ToString();
           address.Postcode = reader[2].ToString();
           address.Land = reader[3].ToString();
           address.Latitude = (double)reader[4];
           address.Longitude = (double)reader[5];
           address.TypeOfAddress = (TypeOfAddress)reader.GetInt32(6);
           address.description = reader[7].ToString();
           return address;
       }


       /// <summary>
       /// Gets the list.
       /// </summary>
       /// <param name="db">The db.</param>
       /// <returns></returns>
       public static List<Address> GetList(IDatabase db) {
           List<Address> addressList = new List<Address>();
           try {
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
               while (reader.Read()) {
                   addressList.Add(Load(reader));
               }
               reader.Close();
               return addressList;

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
       public static Address Load(IDatabase db, int id) {
           Address address = new Address();
           try {
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
               if (reader.HasRows) {
                   while (reader.Read()) {
                       address = Load(reader);
                   }
                   reader.Close();
                   return address;

               } else {
                   // Insert
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
           DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.AddressID);
           bool hasRows = reader.HasRows;
           reader.Close();

           if (hasRows) {
               db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(STREET) + " = " + db.GetSqlString(this.Street) + ", " +
                                                                                 db.Enquote(POSTCODE) + " = " + db.GetSqlString(this.Postcode) + ", " +
                                                                                 db.Enquote(LAND) + " = " + db.GetSqlString(this.Land) + ", " +
                                                                                 db.Enquote(LATITUDE) + " = " + this.Latitude + ", " +
                                                                                 db.Enquote(LONGITUDE) + " = " + this.Longitude + ", " +
                                                                                 db.Enquote(ADDRESSTYPE) + " = " + (int)this.TypeOfAddress +", " +
                                                                                 db.Enquote(DESCRIPTION) + " = " + db.GetSqlString(this.description));
           } else {
               db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(STREET) + ", " +
                                                                                   db.Enquote(POSTCODE) + ", " +
                                                                                   db.Enquote(LAND) + ", " +
                                                                                   db.Enquote(LATITUDE) + ", " +
                                                                                   db.Enquote(LONGITUDE) + ", " +
                                                                                   db.Enquote(ADDRESSTYPE) + ", " +
                                                                                   db.Enquote(DESCRIPTION) + ") " +
                                                                                   
                                  "VALUES (" + db.GetSqlString(this.Street) + ", " +
                                                db.GetSqlString(this.Postcode) + ", " +
                                                db.GetSqlString(this.Land) + ", " +
                                                this.Latitude + ", " +
                                                this.Longitude + ", " +
                                                (int)this.TypeOfAddress + ", " +
                                               db.GetSqlString(this.description) + " )");
           }
       }

       /// <summary>
       /// Deletes the specified db.
       /// </summary>
       /// <param name="db">The db.</param>
       public void Delete(IDatabase db) {
           db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.AddressID);
       }



       public static void TestInsert(IDatabase db, List<Address> testAddressList) {
           foreach (Address add in testAddressList) {
               try {
                   add.Save(db);
               } catch (Exception ex) {
                   throw ex;
               } 
           }
       }
       #endregion methodes
       /************************************************************************************************/
   }
}
