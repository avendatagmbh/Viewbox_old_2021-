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
    class DebugEventArgs_Test {

        private DebugEventArgs _debugEventArgs;

        [SetUp]
        public void SetUp() {
            _debugEventArgs = new DebugEventArgs();
        }

        [Test]
        public void DebugMessage_Property_Get_Set_Test() {
            string s = "This is a debug message";
            _debugEventArgs.DebugMessage = s;
            Assert.AreEqual(s,_debugEventArgs.DebugMessage);
        }

        [Test]
        public void Constructor_Message_Is_Injected_Test() { 
            string s = "This is a debug message";
            _debugEventArgs= new DebugEventArgs(s);
            Assert.AreEqual(s,_debugEventArgs.DebugMessage);
        }


    }
}
