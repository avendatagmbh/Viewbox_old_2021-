using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;
using System.Security.Cryptography;

namespace Domain.Entities.Tables {
    public enum CategoryEmployee : int {
        // // Diese Enumeration entspricht in der Datenbank einer Tabelle
        Geschäftsführung = 1,
        Agent_Vertrieb,
        Vertrieb,
        Technik,
        Technik_Admin,
        Buchhaltung,
        Verwaltung,
        Grafiker,
        Default
    }

   public class Employee {

       private const string TABLE_NAME = "EMPLOYEE";
       private const string ID = "ID";
       private const string NAME = "NAME";
       private const string FIRSTNAME = "FIRSTNAME";
       private const string ADDRESS = "ADDRESS";
       private const string CATEGORY = "CATEGORY";
       private const string EMAIL = "EMAIL";
       private const string PASSWORD = "PASSWORD";
       private const string USERFIRM = "USERFIRM";
       private const string SPECIFICATION = "SPECIFICATION";
       private const string EMPLOYEE_EMAIL_UNIQUE = "EMPLOYEE_EMAIL_UNIQUE";

       /********************************************************************/
       #region properties

       public int EmployeeID { get; set; } //PK
       public string Name { get; set; }     // varchar(45)
       public string FirstName { get; set; } //varchar(45)
       public int Address { get; set; }    //FK(address-class)
       public CategoryEmployee Category { get; set; }   //FK(category-class)
       public string E_Mail { get; set; }   //varchar(256)
       public string Password { get; set; } //varchar(512)
       public int UserFirm { get; set;}    // FK(userfirm)
       public string Specification { get; set; }    // varchar(255)

       #endregion properties

       /**********************************************************************/
       #region constructor 

       public Employee() {
           this.EmployeeID = 0;
           this.Name = string.Empty;
           this.FirstName = string.Empty;
           this.Address = 0;
           this.Category = CategoryEmployee.Default;
           this.E_Mail = string.Empty;
           this.Password = string.Empty;
           this.UserFirm = 0;
           this.Specification = string.Empty;

       }

       #endregion constructor
       /***************************************************************************/
       #region methodes

       /// <summary>
       /// Creates the table.
       /// </summary>
       /// <param name="db">The db.</param>
       public static void CreateTable(IDatabase db) {
           db.ExecuteNonQuery("CREATE  TABLE IF NOT EXISTS " + db.Enquote(TABLE_NAME) +" ("+
                                db.Enquote(ID) + " INTEGER  NOT NULL AUTO_INCREMENT ," +
                                db.Enquote(NAME) + "  VARCHAR(45) NOT NULL ," +
                                db.Enquote(FIRSTNAME) + "  VARCHAR(45) NOT NULL ," +
                                db.Enquote(ADDRESS) + "  INTEGER  NOT NULL ," +
                                db.Enquote(CATEGORY) + " INTEGER  NOT NULL ," +
                                db.Enquote(EMAIL) + " VARCHAR(256) NOT NULL ," +
                                db.Enquote(PASSWORD) + " VARCHAR(512) NOT NULL ," +
                                db.Enquote(USERFIRM) + " INTEGER  NOT NULL ," +
                                db.Enquote(SPECIFICATION) + "  VARCHAR(255) NULL ," +
                                " PRIMARY KEY (" + db.Enquote(ID) + ") ,"+
                                "UNIQUE INDEX " + db.Enquote(EMPLOYEE_EMAIL_UNIQUE) + " (" + db.Enquote(EMAIL) + "ASC)) ENGINE = MyIsam");
       }

       /// <summary>
       /// Loads the specified reader.
       /// </summary>
       /// <param name="reader">The reader.</param>
       /// <returns></returns>
       private static Employee Load(DbDataReader reader) {
           Employee Employee = new Employee();
           Employee.EmployeeID = (int)reader[0];
           Employee.Name = reader[1].ToString();
           Employee.FirstName = reader[2].ToString();
           Employee.Address = (int)reader[3];
           Employee.Category = (CategoryEmployee)reader.GetInt32(4);
           Employee.E_Mail = reader[5].ToString();
           Employee.Password = reader[6].ToString();
           Employee.UserFirm = (int)reader[7];
           Employee.Specification = reader[8].ToString();
           return Employee;
       }


       /// <summary>
       /// Gets the list.
       /// </summary>
       /// <param name="db">The db.</param>
       /// <returns></returns>
       public static List<Employee> GetList(DbAccess.IDatabase db) {
           List<Employee> employeeList = new List<Employee>();
           DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME));
           if (reader.HasRows) {
               while (reader.Read()) {
                   employeeList.Add(Employee.Load(reader));
               }
               reader.Close();
               return employeeList;
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
       public static Employee Load(DbAccess.IDatabase db, int id) {
           try {
               List<Employee> empIDList = new List<Employee>();
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + id);
               if (reader.HasRows) {
                   while (reader.Read()) {
                       empIDList.Add(Employee.Load(reader));
                   }
                   reader.Close();
                   return empIDList.ElementAt(0);
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
       /// <param name="username">The username.</param>
       /// <returns></returns>
       public static Employee Load(IDatabase db, string username) {
           try {
               List<Employee> empUserNameList = new List<Employee>();
               DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + "WHERE" + db.Enquote(EMAIL) + " = " + db.GetSqlString(username));
               if (reader.HasRows) {
                   while (reader.Read()) {
                       empUserNameList.Add(Employee.Load(reader));
                   }
                   reader.Close();
                   return empUserNameList.ElementAt(0);
               } else {
                   reader.Close();
                   return null;
               }
           } catch (Exception ex) {
               throw ex;
           }
       }

       /// <summary>
       /// Employees the exists.
       /// </summary>
       /// <param name="db">The db.</param>
       /// <param name="username">The username.</param>
       /// <returns></returns>
       internal static bool EmployeeExists(IDatabase db, string username) {
           return (Convert.ToInt32(db.ExecuteScalar(
               "SELECT COUNT(*) FROM " + db.Enquote(TABLE_NAME) + " " +
               "WHERE " + db.Enquote(EMAIL) + "=" + db.GetSqlString(username))) > 0);
       }

       /// <summary>
       /// Checks the password.
       /// </summary>
       /// <param name="password">The password.</param>
       /// <returns></returns>
       public bool CheckPassword(string password) {
           string passwordHash = ComputePasswordHash(password, this.E_Mail);
           return this.Password.Equals(passwordHash);
       }


       /// <summary>
       /// Computes this instance.
       /// </summary>
       /// <param name="password">The password.</param>
       /// <param name="salt">The salt.</param>
       /// <returns></returns>
       private string ComputePasswordHash(string password, string salt) {
           return ByteArrayToString(
                   new SHA256CryptoServiceProvider().ComputeHash(
                       ASCIIEncoding.ASCII.GetBytes(password + salt)));
       }

       /// <summary>
       /// Bytes the array to string.
       /// </summary>
       /// <param name="arrInput">The arr input.</param>
       /// <returns></returns>
       private static string ByteArrayToString(byte[] arrInput) {
           StringBuilder sOutput = new StringBuilder(arrInput.Length);
           for (int i = 0; i < arrInput.Length - 1; i++) {
               sOutput.Append(arrInput[i].ToString("X2"));
           }
           return sOutput.ToString();
       }

       /// <summary>
       /// Saves the specified db.
       /// </summary>
       /// <param name="db">The db.</param>
       public void Save(IDatabase db) {
           DbDataReader reader = db.ExecuteReader("SELECT * FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.EmployeeID);
           bool hasRows = reader.HasRows;
           reader.Close();

           if (hasRows) {
               db.ExecuteNonQuery("UPDATE " + db.Enquote(TABLE_NAME) + " SET " + db.Enquote(NAME) + " = " + db.GetSqlString(this.Name) + ", " +
                                                                                 db.Enquote(FIRSTNAME) + " = " + db.GetSqlString(this.FirstName) + ", " +
                                                                                 db.Enquote(ADDRESS) + " = " + this.Address + ", " +
                                                                                 db.Enquote(CATEGORY) + " = " + (int)this.Category + ", " +
                                                                                 db.Enquote(EMAIL) + " = " + db.GetSqlString(this.E_Mail) + ", " +
                                                                                 db.Enquote(PASSWORD) + " = " + db.GetSqlString(this.Password) + ", " +
                                                                                 db.Enquote(USERFIRM) + " = " + this.UserFirm + ", " +
                                                                                 db.Enquote(SPECIFICATION) + " = " + db.GetSqlString(this.Specification));
           } else {
               db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(NAME) + ", " +
                                                                                   db.Enquote(FIRSTNAME) + ", " +
                                                                                   db.Enquote(ADDRESS) + ", " +
                                                                                   db.Enquote(CATEGORY) + ", " +
                                                                                   db.Enquote(EMAIL) + ", " +
                                                                                   db.Enquote(PASSWORD) + ", " +
                                                                                   db.Enquote(USERFIRM) + ", " +
                                                                                   db.Enquote(SPECIFICATION) + ") " +

                                  "VALUES (" + db.GetSqlString(this.Name) + ", " +
                                               db.GetSqlString(this.FirstName) + ", " +
                                               this.Address + ", " +
                                                (int)this.Category + ", " +
                                                db.GetSqlString(this.E_Mail) + ", " +
                                                db.GetSqlString(this.Password) + ", " +
                                                this.UserFirm + ", " +
                                               db.GetSqlString(this.Specification) + " )");
           }
       }

       /// <summary>
       /// Deletes the specified db.
       /// </summary>
       /// <param name="db">The db.</param>
       public void Delete(IDatabase db) {
           db.ExecuteNonQuery("DELETE FROM " + db.Enquote(TABLE_NAME) + " WHERE " + db.Enquote(ID) + " = " + this.EmployeeID);
       }


       public static void TestInsert(IDatabase db, Employee testEmp) {
           
           try {
               
               if (EmployeeExists(db,testEmp.E_Mail)) return;
               db.ExecuteNonQuery("INSERT INTO " + db.Enquote(TABLE_NAME) + "( " + db.Enquote(NAME) + ", " +
                                                                                  db.Enquote(FIRSTNAME) + ", " +
                                                                                  db.Enquote(ADDRESS) + ", " +
                                                                                  db.Enquote(CATEGORY) + ", " +
                                                                                  db.Enquote(EMAIL) + ", " +
                                                                                  db.Enquote(PASSWORD) + ", " +
                                                                                  db.Enquote(USERFIRM) + ", " +
                                                                                  db.Enquote(SPECIFICATION) + ") " +

                                 "VALUES (" + db.GetSqlString(testEmp.Name) + ", " +
                                              db.GetSqlString(testEmp.FirstName) + ", " +
                                              testEmp.Address + ", " +
                                               (int)testEmp.Category + ", " +
                                               db.GetSqlString(testEmp.E_Mail) + ", " +
                                               db.GetSqlString(testEmp.Password) + ", " +
                                               testEmp.UserFirm + ", " +
                                              db.GetSqlString(testEmp.Specification) + " )");
           } catch (Exception ex) {
               throw ex;
           } 
       }

       #endregion methodes
   }
}
