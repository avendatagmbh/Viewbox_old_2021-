using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Domain.Entities.Tables;

namespace Domain.TextDaten {
  public class Test {

      public Employee testEmp { get; set; }
      public UserFirm testFirm { get; set; }
      public Client testClient { get; set; }
      public Date testDate { get; set; }
      public List<Address> testAddressList { get; set; }

      public Test() {
          testEmp = new Employee(); 
          testAddressList = new List<Address>(){
              new Address { Street= "Potsdamer Straße 102", Postcode= "10785", Land = "Berlin", Latitude = 52310, Longitude=13249, TypeOfAddress = TypeOfAddress.Mitarbeiter},
              new Address { Street = "Salzufer 8", Postcode = "10587", Land = "Berlin", Latitude = 52310, Longitude = 13249, TypeOfAddress= TypeOfAddress.Firma},
              new Address { Street= "Biebricher Allee 2", Postcode = "65187", Land = "Wiesbaden", Latitude = 52310, Longitude =13249 , TypeOfAddress = TypeOfAddress.Kunde}
          };
          testFirm = new UserFirm();
          testClient = new Client();
          testDate = new Date();
          TestInsert();
      }


      private void TestInsert() {
        testEmp = new Employee {
              Address = 1,
              Name = "Testmann",
              FirstName = "Test",
              Category = CategoryEmployee.Vertrieb,
              E_Mail = "te",
              Password = Manager.Manager.ComputePasswordHash("tt", "te"),
              UserFirm = 1
          };
       
        testFirm = new UserFirm { Name = "Avendata", Address = 2, UserName = "testFirm", Password = Manager.Manager.ComputePasswordHash("testfirm", testFirm.UserName), Phone = "030/3045875" };
        testClient = new Client { Name = "TestKunde", UserFirm = 1, Address = 3, Phone = "364/12345676", specification = "Investment" };
      }
    }
}
