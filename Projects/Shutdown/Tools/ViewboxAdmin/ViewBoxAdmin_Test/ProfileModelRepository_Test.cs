using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Mocks;
using NUnit.Framework;
using ViewboxAdmin.Manager;
using ViewboxAdmin.Models.ProfileRelated;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class ProfileModelRepository_Test {

        private IProfileModelRepository _profileModel;
        private IProfile _profileMock;

        [SetUp]
        public void SetUp() {
            _profileModel = new ProfileModelRepository();
            _profileMock = MockRepository.GenerateMock<IProfile>();
        }
        [Test]
        public void  GetModel_Called_With_Null_Will_Return_Null() {
            IProfileModel profmodel = _profileModel.GetModel(null);
            Assert.IsNull(profmodel);
        }

        [Test]
        public void GetModel_Called_With_Person_Will_Return_The_Correct_Model() {
            IProfileModel profmodel = _profileModel.GetModel(_profileMock);
            Assert.AreEqual(_profileMock, profmodel.Profile);
        }
    }
}
