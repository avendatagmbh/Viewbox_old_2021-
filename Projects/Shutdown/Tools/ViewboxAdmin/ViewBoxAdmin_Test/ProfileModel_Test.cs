using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using Rhino.Mocks;
using NUnit.Framework;
using Utils;
using ViewboxAdmin.Models.ProfileRelated;
using ViewboxAdmin_ViewModel.Structures.Config;
using Part = SystemDb.SystemDb.Part;

namespace ViewBoxAdmin_Test
{
    [TestFixture]
    class ProfileModel_Test {
        private IProfileModel _profileModel;
        private IProfile _profileMock;
        private ISystemDb _systemDbMock;
        private IProgressCalculator _progressforctor;
        private IProgressCalculator _progressforProperty;
        private IProfilePartLoadingEnumHelper _partloaderMock;
        [SetUp]
        public void SetUp() {
            _progressforctor = MockRepository.GenerateMock<IProgressCalculator>();
            _progressforProperty = MockRepository.GenerateMock<IProgressCalculator>();
            _profileMock = MockRepository.GenerateMock<IProfile>();
            _systemDbMock = MockRepository.GenerateMock<ISystemDb>();
            _partloaderMock = MockRepository.GenerateMock<IProfilePartLoadingEnumHelper>();
            _profileMock.Stub(x => x.SystemDb).Return(_systemDbMock);
            _profileModel = new ProfileModel(_profileMock, _progressforctor, _partloaderMock);
        }

       [Test]
        public void Constructor_Profile_Injection_Test() {
           Assert.AreEqual(_profileMock,_profileModel.Profile);
       }

        [Test]
        public void Constructor_Progress_Injection_LoadingProgress() {
            Assert.AreEqual(_profileModel.LoadingProgress,_progressforctor);
        }

        [Test]
        public void LoadingProgress_Property_Get_Set_Test() {
            
            _profileModel.LoadingProgress = _progressforProperty;
           Assert.AreEqual(_progressforProperty,_profileModel.LoadingProgress);
        }
        [Test]
        public void LoadingProgress_Property_PropertyChanged() {
            bool isfired = false;
            _profileModel.PropertyChanged += (o, e) => { if (e.PropertyName == "LoadingProgress") isfired = true; };
            _profileModel.LoadingProgress = _progressforProperty;
            Assert.IsTrue(isfired);
        }

        [Test]
        public void LoadingProgress_Property_PropertyChanged_With_Same_Value_Not_Fired() {
            bool isfired = false;
            _profileModel.LoadingProgress = _progressforProperty;
            _profileModel.PropertyChanged += (o, e) => { if (e.PropertyName == "LoadingProgress") isfired = true; };
            _profileModel.LoadingProgress = _progressforProperty;
            Assert.IsFalse(isfired);
        }
        [Test]
        public void LoadData_If_IsLoading_Was_Not_Called() {
            ProfileIsLoadedIsLoading(true, false);
            _profileModel.LoadData();
            _profileMock.AssertWasNotCalled(x => x.Load());
        }

        private void ProfileIsLoadedIsLoading(bool isloading, bool isloaded) {
            _profileMock.Stub(x => x.IsLoading).Return(isloading);
            _profileMock.Stub(x => x.Loaded).Return(isloaded);
        }

        [Test]
        public void LoadData_If_IsLoaded_Was_Not_Called() {
            ProfileIsLoadedIsLoading(false, true);
            _profileModel.LoadData();
            _profileMock.AssertWasNotCalled(x => x.Load());
        }

        [Test]
        public void LoadData_Call_Load() {
            ProfileIsLoadedIsLoading(false, false);
            _profileModel.LoadData();
            _profileMock.AssertWasCalled(x => x.Load());
        }
        [Test]
        public void LoadData_FinishLoading_Event_Is_Reported() {
            bool isfinishedloading = false;
            ProfileIsLoadedIsLoading(false,false);
            _profileModel.FinishedProfileLoadingEvent +=
                (o, e) => { if (e.GetException() == null) isfinishedloading = true; };
            _profileModel.LoadData();
            _systemDbMock.Raise(x => x.LoadingFinished += null);
            Assert.IsTrue(isfinishedloading);
        }

        [Test]
        public void LoadData_Exceptions_Are_Reported() {
            Exception exception = new Exception("This is an exception 2 throw");
            Exception expected = null;
            ProfileIsLoadedIsLoading(false, false);
            _profileModel.FinishedProfileLoadingEvent +=
                (o, e) => { expected = e.GetException(); };
            _profileMock.Stub(x => x.Load()).Throw(exception);
            _profileModel.LoadData();
            Assert.AreSame(expected,exception);
            
        }
        [Test]
        public void LoadData_Report_Part_Loaded_Completed_To_ProgressBar_Make_StepDone() {
            SystemDb.SystemDb.Part part = SystemDb.SystemDb.Part.Columns;
            ReportLOadedPart(part);
            _progressforctor.AssertWasCalled(x => x.StepDone());
        }

        private void ReportLOadedPart(Part part) {
            ProfileIsLoadedIsLoading(false, false);
            _profileModel.LoadData();
            _systemDbMock.Raise(x => x.PartLoadingCompleted += null, null, part);
        }

        [Test]
        public void Constructor_PartLoading_Enum_Helper_Injection_Test() {
            Assert.AreEqual(_partloaderMock,_profileModel.ProfilePartLoadingEnumHelper);
        }

        [Test]
        public void LoadData_Report_Partly_Completed_NOT_last_Description_Should_Be_Set() {
            Part testpart = Part.Languages;
            _partloaderMock.Stub(x => x.IsNotLast(testpart)).Return(true);
            ReportLOadedPart(testpart);
            _progressforctor.AssertWasCalled(x=>x.Description=Arg<string>.Is.Anything);
        }

        [Test]
        public void LoadData_Report_Partly_Completed_Not_Last_Get_Next_Is_Called() {
            Part testpart = Part.Languages;
            _partloaderMock.Stub(x => x.IsNotLast(testpart)).Return(true);
            ReportLOadedPart(testpart);
            _partloaderMock.AssertWasCalled(x => x.GetNextPartEnum(testpart));

        }
        [Test]
        public void Constructor_Initalizes_Dictionary() {
            Assert.NotNull(_profileModel.LoadingCompletedPartDictionary);
        }

        [Test]
        public void LoadingCompletedPartDictionary_Contains_Every_Enum_Item() {
            foreach (Part part in Enum.GetValues(typeof(Part))) {
                Assert.DoesNotThrow(() => { string s = _profileModel.LoadingCompletedPartDictionary[Part.About]; },part.ToString()+" is missing from the dictionary");
            }
        }
    }
}
