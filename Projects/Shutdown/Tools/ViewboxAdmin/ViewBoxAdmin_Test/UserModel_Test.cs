using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdmin.ViewModels.Users;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    internal class UserModel_Test {
        [SetUp]
        public void SetUp() { }

        private UserModel CreateSUT() { return new UserModel(); }

        [TestCase("_doo")]
        public void Validation_UserName_Not_Empty_Valid(string userName) {
            UserModel user = CreateSUT();
            user.UserName = userName;
            Assert.IsNull(user["UserName"]);
        }

        [TestCase("")]
        [TestCase(null)]
        public void UserName_Invalid_Test(string userName) {
            UserModel user = CreateSUT();
            user.UserName = userName;
            Assert.IsNotNull(user["UserName"]);
        }

        [TestCase("John Doo")]
        public void Name_Valid(string name) {
            UserModel user = CreateSUT();
            user.Name = name;
            Assert.IsNull(user["Name"]);
        }

        [TestCase("")]
        [TestCase(null)]
        public void Name_Invalid(string name) {
            UserModel user = CreateSUT();
            user.Name = name;
            Assert.IsNotNull("Name");
        }
   


}
}
