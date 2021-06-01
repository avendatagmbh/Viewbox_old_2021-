using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdmin_ViewModel;

namespace Viewbox_Admin_ViewModel_Test
{
    [TestFixture]
    public class NotifyBase_Test {

        private NotifyBase _notifyBase ;

        [SetUp]
        public void SetUp() {
            _notifyBase = new NotifyBase();
        }
        [Test]
        public void TestInfrastructure() {
            Assert.IsTrue(true);
        }

        ////[Test]
        //public void AfterCalling_OnPropertyChange_The_Event_Is_Fired() {
        //    bool isFired=false;
        //    _notifyBase.PropertyChanged+=(o,e)=>isFired=true;
        //    _notifyBase.OnPropertyChanged("Whatever it is");

        //    Assert.IsTrue(isFired);
        //}
    }
}
