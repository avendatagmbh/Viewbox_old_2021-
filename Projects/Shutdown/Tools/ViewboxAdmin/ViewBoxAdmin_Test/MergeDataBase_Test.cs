using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using SystemDb.Internal;
using NUnit.Framework;
using Rhino.Mocks;
using ViewboxAdmin.ViewModels;
using ViewboxAdmin.ViewModels.MergeDataBase;

namespace ViewBoxAdmin_Test
{
    /// <summary>
    /// the correct method call order is not checked :(
    /// all method call are verified
    /// These tests are definitelly smelling  
    /// </summary>
    [TestFixture]
    class MergeDataBase_Test {
        private IMergeMetaDatabaseFactory mergefactoryMock;
        private IMergeDataBase SUT;
        private IMergeMetaDatabases mergerMock;
        private ILoaderHack loaderMock;
        private ISystemDb systemDbFrom;
        private ISystemDb systemDbTo;
        private IIssueCollection issueMock;

        [SetUp]
        public void SetUp() {
            //set up test doubles 
            mergefactoryMock = MockRepository.GenerateMock<IMergeMetaDatabaseFactory>();
            mergerMock = MockRepository.GenerateMock<IMergeMetaDatabases>();
            systemDbFrom = MockRepository.GenerateMock<ISystemDb>();
            issueMock = MockRepository.GenerateMock<IIssueCollection>();
            systemDbTo = MockRepository.GenerateMock<ISystemDb>();
            loaderMock = MockRepository.GenerateMock<ILoaderHack>();
            loaderMock.Stub(x => x.LoadParameterValue()).Return(new List<ParameterValue>());
            issueMock.Stub(x => x.GetEnumerator()).Return(new List<IIssue>(){MockRepository.GenerateMock<IIssue>()}.GetEnumerator());
            systemDbFrom.Stub(x => x.Issues).Return(issueMock);
           // method stubs for loader
            loaderMock.Stub(x => x.LoadParameterCollectionFromTable()).Return(new List<ParameterCollectionMapping>());
            loaderMock.Stub(x => x.LoadRelationFromTable()).Return(new List<Relation>());
            //method stubs for factory
            mergefactoryMock.Stub(x => x.Create(Arg<ISystemDb>.Is.Anything)).Return(mergerMock);
            mergefactoryMock.Stub(x => x.CreateLoader(Arg<ISystemDb>.Is.Anything)).Return(loaderMock);
            // create the SUT
            SUT = new ViewboxAdmin.ViewModels.MergeDataBase.MergeDataBase(mergefactoryMock);
        }

        [Test]
        public void HighestLevel_MergeMethods_Are_Called() {
            //Act
            SUT.MergeDataBases(systemDbTo, systemDbFrom);
            //Assert 6
            mergerMock.AssertWasCalled(x=>x.Languages(Arg<ILanguageCollection>.Is.Anything,Arg<ILanguageCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.CategoryMapping(Arg<ICategoryCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.Users(Arg<IUserIdCollection>.Is.Anything,Arg<IUserIdCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.Roles(Arg<IRoleCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.OptimizationGroup(Arg<IOptimizationGroupCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.Scheme(Arg<ISchemeCollection>.Is.Anything));
        }

        [Test]
        public void TablesAndColumns_Methods_Are_Called_Test() {
            //Act
            SUT.MergeDataBases(systemDbTo,systemDbFrom);
            //Assert 2
            mergerMock.AssertWasCalled(x=>x.Table(Arg<ITableObjectCollection>.Is.Anything,Arg<ITableObjectCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.Columns(Arg<IFullColumnCollection>.Is.Anything));
        }

        [Test]
        public void Order_Issue_Parameter_Optimization_Methids_Are_Called_Test() {
            //Act
            SUT.MergeDataBases(systemDbTo,systemDbFrom);
            //Assert 4
            mergerMock.AssertWasCalled(x => x.OrderArea(Arg<ITableObjectCollection>.Is.Anything, Arg<ITableObjectCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.IssueExtensions(Arg<IIssueCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x => x.Parameters(Arg<IIssueCollection>.Is.Anything, Arg<IIssueCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.OptimizationAggregate(Arg<IOptimizationCollection>.Is.Anything,Arg<IOptimizationCollection>.Is.Anything));
        }
        [Test]
        public void UserRoleRightsVisibility_Methods_Are_Called() {
            //Act
            SUT.MergeDataBases(systemDbTo, systemDbFrom);
            //Assert 9
            mergerMock.AssertWasCalled(x=>x.ColumnRole(Arg<IRoleColumnRights>.Is.Anything,Arg<IRoleColumnRights>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.ColumnUser(Arg<IUserColumnRights>.Is.Anything,Arg<IUserColumnRights>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.TableRole(Arg<IRoleTableObjectRights>.Is.Anything,Arg<IRoleTableObjectRights>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.TableUser(Arg<IUserTableObjectRights>.Is.Anything,Arg<IUserTableObjectRights>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.CategoryRole(Arg<IRoleCategoryRights>.Is.Anything, Arg<IRoleCategoryRights>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.CategoryUser(Arg<IUserCategoryRights>.Is.Anything, Arg<IUserCategoryRights>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.OptimizationRole(Arg<IRoleOptimizationRights>.Is.Anything,Arg<IRoleOptimizationRights>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.OptimizationUser(Arg<IUserOptimizationRights>.Is.Anything,Arg<IUserOptimizationRights>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.UserRole(Arg<IUserRoleCollection>.Is.Anything,Arg<IUserRoleCollection>.Is.Anything));
        }
        [Test]
        public void SomeDirtyMerge_Methdods_Are_Called() {
            //Act
            SUT.MergeDataBases(systemDbTo,systemDbFrom);
            //Assert 4
            mergerMock.AssertWasCalled(x => x.Relation(Arg<List<Relation>>.Is.Anything, Arg<List<Relation>>.Is.Anything));
            mergerMock.AssertWasCalled(x=> x.ParameterValues(Arg<List<ParameterValue>>.Is.Anything,Arg<List<ParameterValue>>.Is.Anything));
            mergerMock.AssertWasCalled(x=> x.ParameterValueText(Arg<List<ParameterValue>>.Is.Anything,Arg<ILanguageCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=> x.ParameterCollectionMapping(Arg<List<ParameterCollectionMapping>>.Is.Anything));
        }
        [Test]
        public void TextMerge() {
            //Act
            SUT.MergeDataBases(systemDbTo,systemDbFrom);
            //Assert 6
            mergerMock.AssertWasCalled(x=>x.OptimizationText(Arg<IOptimizationCollection>.Is.Anything,Arg<ILanguageCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.ColumnText(Arg<IFullColumnCollection>.Is.Anything,Arg<ILanguageCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.TableText(Arg<ITableObjectCollection>.Is.Anything,Arg<ILanguageCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.OptimizationGroupText(Arg<IOptimizationGroupCollection>.Is.Anything,Arg<ILanguageCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.CategoryTexts(Arg<ICategoryCollection>.Is.Anything,Arg<ILanguageCollection>.Is.Anything));
            mergerMock.AssertWasCalled(x=>x.ParameterTexts(Arg<IParameterCollection>.Is.Anything,Arg<ILanguageCollection>.Is.Anything));
        }
    }
}
