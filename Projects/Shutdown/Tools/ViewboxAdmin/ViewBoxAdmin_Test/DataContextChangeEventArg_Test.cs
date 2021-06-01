using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdmin.CustomEventArgs;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class DataContextChangeEventArg_Test {

        private DataContextChangeEventArg<string> SUT;
        private string s;

        [SetUp]
        public void SetUp() {
            s = "something";
            SUT = new DataContextChangeEventArg<string>(s);
        }

        [Test]
        public void Constructor_Injection_Test() {
            //Act
            Assert.AreEqual(s,SUT.ViewModel);
        }
    }
}
