using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess.Structures;
using DbAccess;
using Domain.Entities.Tables;
using System.Security.Cryptography;
using Domain.TextDaten;


namespace Domain.Manager {

   public static class Manager  {

       /***************************************************************/
        #region methodes


       public static void CreateDatabase(DbConfig conf){
           conf.DbName = string.Empty;
            using (IDatabase db = ConnectionManager.CreateConnection(conf)) {
                db.Open();
                
                db.CreateDatabaseIfNotExists(db.Enquote(Config.DbName));
            }
            conf.DbName = Config.DbName;
        }
        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="conf">The conf.</param>
       public static void CreateTable(DbConfig conf){

            using (IDatabase db = ConnectionManager.CreateConnection(conf)) {
                db.Open();
                Address.CreateTable(db);
                AddressType.CreateTable(db);
                Client.CreateTable(db);
                ContactPerson.CreateTable(db);
                Date.CreateTable(db);
                DrivingCompany.CreateTable(db);
                Employee.CreateTable(db);
                EmployeeDatelist.CreateTable(db);
                GetTour.CreateTable(db);
                Hotel.CreateTable(db);
                Tour.CreateTable(db);
                TourStep.CreateTable(db);
                UserFirm.CreateTable(db);
                Airport.CreateTable(db);
                SixtStation.CreateTable(db);
                TrainStation.CreateTable(db);
                Datelist.CreateTable(db);
            }
        }

       /// <summary>
       /// Gets the employee by mail.
       /// </summary>
       /// <param name="email">The email.</param>
       /// <param name="password">The password.</param>
       /// <returns></returns>
       public static Employee GetEmployeeByMail(string email){
           using (IDatabase db = ConnectionManager.CreateConnection(Config.Conf)) {
               db.Open();
               return Employee.Load(db, email);
           };    
       }


       /// <summary>
       /// Computes this instance.
       /// </summary>
       /// <param name="password">The password.</param>
       /// <param name="salt">The salt.</param>
       /// <returns></returns>
       public static string ComputePasswordHash(string password, string salt) {
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

//**********************Save*************************************************************************/

       public static void Save(DbConfig conf, Date date) {
            using (IDatabase db = ConnectionManager.CreateConnection(conf)) {
                db.Open();
                date.Save(db);
            }
        }

//***********************Load************************************************************************/


       public static Address LoadAddress(DbConfig conf, int id){
            using (IDatabase db = ConnectionManager.CreateConnection(conf)) {
                db.Open();
                return Address.Load(db, id);
            }
        }

       public static UserFirm LoadUserFirm(DbConfig conf, int id) {
           using (IDatabase db = ConnectionManager.CreateConnection(conf)) {
               db.Open();
               return UserFirm.Load(db, id);
           }
       }

//***********************GetList*********************************************************************/

       public static List<Address> GetListAddresses(DbConfig conf) {
           using (IDatabase db = ConnectionManager.CreateConnection(Config.Conf)) {
               db.Open();
               return Address.GetList(db);
           };  
        }

       public static Dictionary<uint, Client> LoadClientList(DbConfig conf, int firmid) {
           using (IDatabase db = ConnectionManager.CreateConnection(Config.Conf)) {
               db.Open();
               return Client.GetList(db, firmid);
           };  
       }



      
//**************************Test***********************************************************************/      
       public static void TestInsert(Test test) {
           try{
               using (IDatabase db = ConnectionManager.CreateConnection(Config.Conf)) {
                   db.Open();
                   Address.TestInsert(db, test.testAddressList);
                   UserFirm.TestInsert(db, test.testFirm);
                   Employee.TestInsert(db, test.testEmp);
                   Client.TestInsert(db, test.testClient);
               }
           }catch(Exception ex){
               throw ex;
           }
       }



        #endregion methodes

    }
}
