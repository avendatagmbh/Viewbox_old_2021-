using System;
using System.Collections.Generic;
using SystemDb;
using SystemDb.Helper;
using SystemDb.Internal;
using NUnit.Framework;
using Rhino.Mocks;

namespace SystemDb_Test
{
    [TestFixture]
    internal class MergeMetaDatabases_Test : MergeMetaDatabases_TestBase
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            // create testdoubles
            _mapMock = MockRepository.GenerateMock<IMapBusinessObjectList>();
            _cloneStub = MockRepository.GenerateMock<ICloneBusinessObjects>();
            // create SUT
            SUT = new MergeMetaDatabases(_cloneStub, _mapMock);
        }

        #endregion

        private MergeMetaDatabases SUT;
        private IMapBusinessObjectList _mapMock;
        private ICloneBusinessObjects _cloneStub;

        private User GenerateUserMock(int id, string Name = "some dummy name")
        {
            return new User {Id = id, UserName = Name};
        }

        private IUserIdCollection GenerateUserIdCollection(params IUser[] args)
        {
            IUserIdCollection useridcollectionMock = MockRepository.GenerateMock<IUserIdCollection>();
            List<IUser> users = new List<IUser>(args);
            useridcollectionMock.Stub(x => x.GetEnumerator()).WhenCalled(
                call => call.ReturnValue = users.GetEnumerator()).Return(null).Repeat.Any();
            return useridcollectionMock;
        }

        private ICategory GenerateCategoryMock(int id)
        {
            ICategory catMOck = MockRepository.GenerateMock<ICategory>();
            catMOck.Stub(x => x.Id).Return(id);
            return catMOck;
        }

        private ICategoryCollection GenerateCategoryCollection(params ICategory[] args)
        {
            ICategoryCollection useridcollectionMock = MockRepository.GenerateMock<ICategoryCollection>();
            List<ICategory> users = new List<ICategory>(args);
            useridcollectionMock.Stub(x => x.GetEnumerator()).WhenCalled(
                call => call.ReturnValue = users.GetEnumerator()).Return(null).Repeat.Any();
            return useridcollectionMock;
        }

        private ILanguage GenerateLanguageMock()
        {
            ILanguage lang = MockRepository.GenerateMock<ILanguage>();
            return lang;
        }

        private ILanguageCollection GenerateLanguageCollections(params ILanguage[] lang)
        {
            ILanguageCollection useridcollectionMock = MockRepository.GenerateMock<ILanguageCollection>();
            List<ILanguage> users = new List<ILanguage>(lang);
            useridcollectionMock.Stub(x => x.GetEnumerator()).WhenCalled(
                call => call.ReturnValue = users.GetEnumerator()).Return(null).Repeat.Any();
            return useridcollectionMock;
        }

        //[Test]
        //public void CategoryTextMapping_Test() {
        //    int oldid = 1;
        //    int newid = 4;
        //    ICategory cat = GenerateCategoryMock(oldid);
        //    ICategory catnew = GenerateCategoryMock(newid);
        //    ICategoryCollection catColl = GenerateCategoryCollection(cat);
        //    SUT.categorycategoryMapping.Add(cat, catnew);
        //    int MAGICnumberofCategories = 1;
        //    ILanguageCollection languages = GenerateLanguageCollections(GenerateLanguageMock(), GenerateLanguageMock());
        //    int MAGICnumberofLangugaes = 2;
        //    SUT.CategoryTexts(catColl, languages);
        //    Assert.AreEqual(MAGICnumberofCategories*MAGICnumberofLangugaes, SUT._categorytexts.Count,
        //                    "language TIMES categories item should be updated in DB");
        //    _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<CategoryText>>.Is.Anything));
        //    _cloneStub.AssertWasCalled(
        //        x => x.CategoryTextClone(Arg<ILanguage>.Is.Anything, Arg<ICategory>.Matches(o => o.Id == newid)));
        //        // the clone method was called with the new category id
        //}
        private ITableObject GenerateTableObject(int id, string name = "dummytablename", ICategory category = null,
                                                 TableType type = TableType.Table)
        {
            ITableObject tableMock = MockRepository.GenerateMock<ITableObject>();
            tableMock.Stub(x => x.Id).Return(id);
            tableMock.Stub(x => x.TableName).Return(name);
            tableMock.Stub(x => x.Category).Return(category);
            tableMock.Stub(x => x.Type).Return(type);
            return tableMock;
        }

        private ITableObjectCollection GenerateTableObjectCollections(params ITableObject[] tableobject)
        {
            ITableObjectCollection tableobjectCollectionMock = MockRepository.GenerateMock<ITableObjectCollection>();
            List<ITableObject> users = new List<ITableObject>(tableobject);
            tableobjectCollectionMock.Stub(x => x.GetEnumerator()).WhenCalled(
                call => call.ReturnValue = users.GetEnumerator()).Return(null).Repeat.Any();
            return tableobjectCollectionMock;
        }

        //[Test]
        //public void Table_Test() {
        //    int toid1 = 1;
        //    int toid2 = 2;
        //    string toname1 = "name1";
        //    string toname2 = "name2";
        //    int tocatid1 = 1;
        //    int tocatid2 = 2;
        //    int fromid1 = 111;
        //    int fromid2 = 222;
        //    string fromname1 = "____name1";
        //    string fromname2 = toname2;
        //    int fromcatid1 = 1;
        //    int fromcatid2 = 2;
        //    ICategory tocat1 = GenerateCategoryMock(tocatid1);
        //    ICategory tocat2 = GenerateCategoryMock(tocatid2);
        //    ICategory existcat1 = GenerateCategoryMock(fromcatid1);
        //    ICategory existcat2 = GenerateCategoryMock(fromcatid2);
        //    ITableObject toto1 = GenerateTableObject(toid1, toname1, tocat1);
        //    ITableObject toto2 = GenerateTableObject(toid2, toname2, tocat2);
        //    ITableObject existto1 = GenerateTableObject(fromid1, fromname1, existcat1);
        //    ITableObject existto2 = GenerateTableObject(fromid2, fromname2, existcat2);
        //    ITableObjectCollection toCollection = GenerateTableObjectCollections(toto1, toto2);
        //    int MAGICnumberofItems = 2;
        //    ITableObjectCollection alreadyExistingTables = GenerateTableObjectCollections(existto1, existto2);
        //    int MAGICnumberofItemsfrom = 2;
        //    SUT.categorycategoryMapping.Add(tocat1, existcat1);
        //    SUT.categorycategoryMapping.Add(tocat2, existcat2);
        //    Func<ITableObject, ICategory,int, TableObject> returner =
        //        (o, c,i) => (new Table() {TableName = o.TableName, CategoryId = c.Id});
        //    _cloneStub.Stub(x => x.TableObjectClone(Arg<ITableObject>.Is.Anything, Arg<ICategory>.Is.Anything, Arg<int>.Is.Anything)).Do(
        //        returner);

        //    SUT.Table(toCollection, alreadyExistingTables);
        //    Assert.AreEqual(MAGICnumberofItems, SUT.TableObjects.Count, "not all items went to the list to map ");
        //    _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<TableObject>>.Is.Anything));
        //    Assert.AreEqual(toname1, SUT.TableObjects[0].TableName);
        //    Assert.AreEqual(toname2, SUT.TableObjects[1].TableName);
        //    Assert.AreEqual(MAGICnumberofItems, SUT.tabletableMapping.Count, "The dictionary contains the items");
        //    Assert.AreEqual(fromid2, SUT.tabletableMapping[toto2].Id, "Unique conflict occured, it obtained the old id");
        //    Assert.AreNotEqual(fromid1, SUT.tabletableMapping[toto1].Id);
        //}

        private IColumn GenerateColumn(int columnid, ITableObject to)
        {
            IColumn column = MockRepository.GenerateMock<IColumn>();
            column.Stub(x => x.Table).Return(to);
            column.Stub(x => x.Id).Return(columnid);
            return column;
        }

        private IFullColumnCollection GenerateColumnCollections(params IColumn[] tableobject)
        {
            IFullColumnCollection columncollectionMock = MockRepository.GenerateMock<IFullColumnCollection>();
            List<IColumn> column = new List<IColumn>(tableobject);
            columncollectionMock.Stub(x => x.GetEnumerator()).WhenCalled(
                call => call.ReturnValue = column.GetEnumerator()).Return(null).Repeat.Any();
            return columncollectionMock;
        }

        //[Test]
        //public void TableText_test() {
        //    ITableObject to1 = GenerateTableObject(12, "name", null);
        //    ITableObject to2 = GenerateTableObject(23, "table", null);
        //    int magicnumberoftables = 2;
        //    int newid = 43;
        //    int newid2 = 68;
        //    ITableObject newto1 = GenerateTableObject(newid, "dsfaf", null);
        //    ITableObject newto2 = GenerateTableObject(newid2, "fdsffd", null);
        //    ITableObjectCollection tocoll = GenerateTableObjectCollections(to1, to2);
        //    ILanguageCollection langcoll = GenerateLanguageCollections(GenerateLanguageMock(), GenerateLanguageMock());
        //    int magicnumberoflangugaes = 2;
        //    SUT.tabletableMapping.Add(to1, newto1);
        //    SUT.tabletableMapping.Add(to2, newto2);
        //    Func<ILanguage, ITableObject, TableObjectText> returner = (l, o) => (new TableObjectText() {RefId = o.Id});
        //    _cloneStub.Stub(
        //        x =>
        //        x.TableTextClone(Arg<ILanguage>.Is.Anything,
        //                         Arg<ITableObject>.Matches(a => a.Id == newid || a.Id == newid2))).Do(returner);
        //    SUT.TableText(tocoll, langcoll);
        //    Assert.AreEqual(magicnumberoflangugaes*magicnumberoftables, SUT._tableobjectText.Count);
        //    _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<TableObjectText>>.Is.Anything));
        //    Assert.AreEqual(newid, SUT._tableobjectText[0].RefId);
        //}

        //[Test]
        //public void ColumnText_test() {
        //    int columnid = 12;
        //    int newcolumnid = 34;
        //    IColumn to1 = GenerateColumn(columnid, GenerateTableObject(1, "fhjfjh", null));
        //    IColumn newto1 = GenerateColumn(newcolumnid, GenerateTableObject(12, "dsfaf", null));
        //    int magicnumberofcolumns = 1;
        //    IFullColumnCollection tocoll = GenerateColumnCollections(to1);
        //    ILanguageCollection langcoll = GenerateLanguageCollections(GenerateLanguageMock(), GenerateLanguageMock());
        //    int magicnumberoflangugaes = 2;
        //    SUT.columncolumnMapping.Add(to1, newto1);

        //    Func<ILanguage, IColumn, ColumnText> returner = (l, o) => (new ColumnText() {RefId = o.Id});
        //    _cloneStub.Stub(
        //        x => x.ColumnTextsClone(Arg<ILanguage>.Is.Anything, Arg<IColumn>.Matches(a => a.Id == newcolumnid))).Do(
        //            returner);
        //    SUT.ColumnText(tocoll, langcoll);
        //    Assert.AreEqual(magicnumberoflangugaes*magicnumberofcolumns, SUT._columntexts.Count);
        //    _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<ColumnText>>.Is.Anything));
        //    Assert.AreEqual(newcolumnid, SUT._columntexts[0].RefId);
        //}
        private IOrderArea GenerateOrderArea()
        {
            IOrderArea orderarea = MockRepository.GenerateMock<IOrderArea>();
            return orderarea;
        }

        private IOrderAreaCollection GenerateOrderAreaCollection(params IOrderArea[] orderarea)
        {
            IOrderAreaCollection orderareacollection = MockRepository.GenerateMock<IOrderAreaCollection>();
            List<IOrderArea> orderarealist = new List<IOrderArea>(orderarea);
            orderareacollection.Stub(x => x.GetEnumerator()).Return(orderarealist.GetEnumerator());
            return orderareacollection;
        }

        private IRole GenerateRole(int id)
        {
            IRole role = MockRepository.GenerateMock<IRole>();
            role.Stub(x => x.Id).Return(id);
            return role;
        }

        private IRoleCollection GenerateRoleCollection(params IRole[] role)
        {
            List<IRole> roles = new List<IRole>(role);
            IRoleCollection rolecollection = MockRepository.GenerateMock<IRoleCollection>();
            rolecollection.Stub(x => x.GetEnumerator()).Return(roles.GetEnumerator());
            return rolecollection;
        }

        private IOptimizationGroup GenerateOptimizationGroup(int id)
        {
            IOptimizationGroup optgroup = MockRepository.GenerateMock<IOptimizationGroup>();
            optgroup.Stub(x => x.Id).Return(id);
            return optgroup;
        }

        private IOptimizationGroupCollection GenerateOptimizationGroupCollection(params IOptimizationGroup[] opt)
        {
            IOptimizationGroupCollection optgroupcollection =
                MockRepository.GenerateMock<IOptimizationGroupCollection>();
            List<IOptimizationGroup> optlist = new List<IOptimizationGroup>(opt);
            optgroupcollection.Stub(x => x.GetEnumerator()).Return(optlist.GetEnumerator());
            return optgroupcollection;
        }

        //[Test]
        //public void OptimizationGroupText_Test() {
        //    ILanguageCollection languages = GenerateLanguageCollections(GenerateLanguageMock(),GenerateLanguageMock(),GenerateLanguageMock());
        //    int optgid = 23;
        //    int newoptgid = 345;
        //    IOptimizationGroup oldopt = GenerateOptimizationGroup(optgid);
        //    IOptimizationGroup newopt = GenerateOptimizationGroup(newoptgid);
        //    IOptimizationGroupCollection optgroupcoll = GenerateOptimizationGroupCollection(oldopt);
        //    SUT.optgroupoptgroupMapping.Add(oldopt,newopt);
        //    SUT.OptimizationGroupText(optgroupcoll,languages);
        //    Assert.AreEqual(3,SUT._optimizationGroupTexts.Count);
        //    _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<OptimizationGroupText>>.Is.Anything));
        //}
        private IIssue GenerateIssue(int id)
        {
            IIssue issueMock = MockRepository.GenerateMock<IIssue>();
            issueMock.Stub(x => x.Id).Return(id);
            return issueMock;
        }

        private IIssueCollection GenerateIssueCollection(params IIssue[] issue)
        {
            List<IIssue> issuelist = new List<IIssue>(issue);
            IIssueCollection issuecoll = MockRepository.GenerateMock<IIssueCollection>();
            issuecoll.Stub(x => x.GetEnumerator()).WhenCalled(
                call => call.ReturnValue = issuelist.GetEnumerator()).Return(null).Repeat.Any();
            return issuecoll;
        }

        //[Test]
        //public void IssueExtension_Test() {
        //    int id1 = 12;
        //    int id2 = 34;
        //    int newid1 = 777;
        //    int newid2 = 888;
        //    IIssue issue1 = GenerateIssue(id1);
        //    IIssue issue2 = GenerateIssue(id2);
        //    IIssueCollection issuecollection = GenerateIssueCollection(issue1, issue2);
        //    SUT.IssueExtensions(issuecollection);
        //    _mapMock.AssertWasCalled(x=>x.MapCollection(Arg<List<IssueExtension>>.Is.Anything));
        //    Assert.AreEqual(2,SUT._issueextension.Count);
        //}

        private IParameter GenerateParameter(int id)
        {
            IParameter parameter = MockRepository.GenerateMock<IParameter>();
            parameter.Stub(x => x.Id).Return(id);
            return parameter;
        }

        private IParameterCollection GenerateParameterCollection(params IParameter[] param)
        {
            List<IParameter> parameters = new List<IParameter>(param);
            IParameterCollection paramcollMOck = MockRepository.GenerateMock<IParameterCollection>();
            paramcollMOck.Stub(x => x.GetEnumerator()).WhenCalled(
                call => call.ReturnValue = parameters.GetEnumerator()).Return(null).Repeat.Any();
            return paramcollMOck;
        }

        //[Test]
        //public void ParameterTexts_Test() {
        //    ILanguageCollection languages = GenerateLanguageCollections(GenerateLanguageMock(), GenerateLanguageMock(), GenerateLanguageMock());
        //    int parameterid = 23;
        //    int newparameterid = 345;
        //    IParameter oldopt = GenerateParameter(parameterid);
        //    IParameter newopt = GenerateParameter(newparameterid);
        //    IParameterCollection parametercoll = GenerateParameterCollection(oldopt);
        //    SUT.parameterparameterMapping.Add(oldopt, newopt);
        //    SUT.ParameterTexts(parametercoll, languages);
        //    Assert.AreEqual(3, SUT._parametertexts.Count);
        //    _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<ParameterText>>.Is.Anything));
        //}
        private IOptimization GenerateOptimization(int id, IOptimization opt = null, string value = "default value")
        {
            IOptimization optMock = MockRepository.GenerateMock<IOptimization>();
            optMock.Stub(x => x.Id).Return(id);
            optMock.Stub(x => x.Value).Return(value);
            optMock.Stub(x => x.Parent).Return(opt);
            return optMock;
        }

        private IOptimizationCollection GenerateOptimizationCollection(params IOptimization[] opt)
        {
            List<IOptimization> optlist = new List<IOptimization>(opt);
            IOptimizationCollection optimizationCollection = MockRepository.GenerateMock<IOptimizationCollection>();
            optimizationCollection.Stub(x => x.GetEnumerator()).WhenCalled(
                call => call.ReturnValue = optlist.GetEnumerator()).Return(null).Repeat.Any();
            return optimizationCollection;
        }

        //[Test]
        //public void OptimizationTexts_Test() {
        //    ILanguageCollection languages = GenerateLanguageCollections(GenerateLanguageMock(), GenerateLanguageMock(), GenerateLanguageMock());
        //    int parameterid = 23;
        //    int newparameterid = 345;
        //    IOptimization dummyParent = MockRepository.GenerateMock<IOptimization>();
        //    IOptimization oldopt = GenerateOptimization(parameterid, dummyParent);
        //    IOptimization newopt = GenerateOptimization(newparameterid);
        //    IOptimizationCollection optimizationCollection = GenerateOptimizationCollection(oldopt);
        //    SUT.optoptMapping.Add(oldopt, newopt);
        //    SUT.OptimizationText(optimizationCollection, languages);
        //    Assert.AreEqual(3, SUT._optimizationTexts.Count);
        //    _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<OptimizationText>>.Is.Anything));
        //}

        private RoleColumnRights RoleColumnRights(int columnid, int mappedcolumnid, int mappedroleid, int roleid,
                                                  int excolumnid,
                                                  int exroleid, out RoleColumnRights rcrExisting)
        {
            IRole role = GenerateRole(roleid);
            IRole exrole = GenerateRole(exroleid);
            IRole mappedrole = GenerateRole(mappedroleid);

            IColumn column = GenerateColumn(columnid, null);
            IColumn excolumn = GenerateColumn(excolumnid, null);
            IColumn mappedcolumn = GenerateColumn(mappedcolumnid, null);
            RoleColumnRights rcrtoMerge = new RoleColumnRights();
            rcrtoMerge.Add(role, column, 12, RightType.Read);
            rcrExisting = new RoleColumnRights();
            rcrExisting.Add(exrole, excolumn, 34, RightType.Write);
            SUT.roleroleMapping.Add(role, mappedrole);
            SUT.columncolumnMapping.Add(column, mappedcolumn);
            Func<RightType, IColumn, IRole, ColumnRoleMapping> returner =
                (rt, c, r) => (new ColumnRoleMapping {ColumnId = c.Id, RoleId = r.Id});
            _cloneStub.Stub(
                x => x.ColumnRoleMapping(Arg<RightType>.Is.Anything, Arg<IColumn>.Is.Anything, Arg<IRole>.Is.Anything)).
                Do(
                    returner);
            return rcrtoMerge;
        }

        private UserColumnRights UserColumnRights(int columnid, int mappedcolumnid, int mappeduserid, int userid,
                                                  int excolumnid,
                                                  int exroleid, out UserColumnRights rcrExisting)
        {
            IUser user = GenerateUserMock(userid);
            IUser exuser = GenerateUserMock(exroleid);
            IUser mappeduser = GenerateUserMock(mappeduserid);

            IColumn column = GenerateColumn(columnid, null);
            IColumn excolumn = GenerateColumn(excolumnid, null);
            IColumn mappedcolumn = GenerateColumn(mappedcolumnid, null);
            UserColumnRights rcrtoMerge = new UserColumnRights();
            rcrtoMerge.Add(user, column, 12, RightType.Read);
            rcrExisting = new UserColumnRights();
            rcrExisting.Add(exuser, excolumn, 34, RightType.Write);
            SUT.useruserMapping.Add(user, mappeduser);
            SUT.columncolumnMapping.Add(column, mappedcolumn);
            Func<RightType, IColumn, IUser, ColumnUserMapping> returner =
                (rt, c, r) => (new ColumnUserMapping {ColumnId = c.Id, UserId = r.Id});
            _cloneStub.Stub(
                x => x.ColumnUserMapping(Arg<RightType>.Is.Anything, Arg<IColumn>.Is.Anything, Arg<IUser>.Is.Anything)).
                Do(
                    returner);
            return rcrtoMerge;
        }

        private UserCategoryRights UserCategoryRights(int columnid, int mappedcolumnid, int mappeduserid, int userid,
                                                      int excolumnid,
                                                      int exroleid, out UserCategoryRights rcrExisting)
        {
            IUser user = GenerateUserMock(userid);
            IUser exuser = GenerateUserMock(exroleid);
            IUser mappeduser = GenerateUserMock(mappeduserid);

            ICategory category = GenerateCategoryMock(columnid);
            ICategory excategory = GenerateCategoryMock(excolumnid);
            ICategory mappedcategory = GenerateCategoryMock(mappedcolumnid);
            UserCategoryRights rcrtoMerge = new UserCategoryRights();
            rcrtoMerge.Add(user, category, 12, RightType.Read);
            rcrExisting = new UserCategoryRights();
            rcrExisting.Add(exuser, excategory, 34, RightType.Write);
            SUT.useruserMapping.Add(user, mappeduser);
            SUT.categorycategoryMapping.Add(category, mappedcategory);
            Func<ICategory, IUser, RightType, CategoryUserMapping> returner =
                (c, u, r) => (new CategoryUserMapping {CategoryId = c.Id, UserId = u.Id});
            _cloneStub.Stub(
                x => x.CategoryUser(Arg<ICategory>.Is.Anything, Arg<IUser>.Is.Anything, Arg<RightType>.Is.Anything)).Do(
                    returner);
            return rcrtoMerge;
        }

        private RoleCategoryRights RoleCategoryRights(int columnid, int mappedcolumnid, int mappeduserid, int userid,
                                                      int excolumnid,
                                                      int exroleid, out RoleCategoryRights rcrExisting)
        {
            IRole user = GenerateRole(userid);
            IRole exuser = GenerateRole(exroleid);
            IRole mappeduser = GenerateRole(mappeduserid);

            ICategory category = GenerateCategoryMock(columnid);
            ICategory excategory = GenerateCategoryMock(excolumnid);
            ICategory mappedcategory = GenerateCategoryMock(mappedcolumnid);
            RoleCategoryRights rcrtoMerge = new RoleCategoryRights();
            rcrtoMerge.Add(user, category, 12, RightType.Read);
            rcrExisting = new RoleCategoryRights();
            rcrExisting.Add(exuser, excategory, 34, RightType.Write);
            SUT.roleroleMapping.Add(user, mappeduser);
            SUT.categorycategoryMapping.Add(category, mappedcategory);
            Func<ICategory, IRole, RightType, CategoryRoleMapping> returner =
                (c, u, r) => (new CategoryRoleMapping {CategoryId = c.Id, RoleId = u.Id});
            _cloneStub.Stub(
                x =>
                x.CategoryRoleMappingClone(Arg<ICategory>.Is.Anything, Arg<IRole>.Is.Anything,
                                           Arg<RightType>.Is.Anything)).Do(
                                               returner);
            return rcrtoMerge;
        }

        private RoleTableObjectRights RoleTableRights(int columnid, int mappedcolumnid, int mappeduserid, int userid,
                                                      int excolumnid,
                                                      int exroleid, out RoleTableObjectRights rcrExisting)
        {
            IRole user = GenerateRole(userid);
            IRole exuser = GenerateRole(exroleid);
            IRole mappeduser = GenerateRole(mappeduserid);

            ITableObject category = GenerateTableObject(columnid, null, null);
            ITableObject excategory = GenerateTableObject(excolumnid, null, null);
            ITableObject mappedcategory = GenerateTableObject(mappedcolumnid, null, null);
            RoleTableObjectRights rcrtoMerge = new RoleTableObjectRights();
            rcrtoMerge.Add(user, category, 12, RightType.Read);
            rcrExisting = new RoleTableObjectRights();
            rcrExisting.Add(exuser, excategory, 34, RightType.Write);
            SUT.roleroleMapping.Add(user, mappeduser);
            SUT.tabletableMapping.Add(category, mappedcategory);
            Func<RightType, ITableObject, IRole, TableRoleMapping> returner =
                (c, u, r) => (new TableRoleMapping {TableId = u.Id, RoleId = r.Id});
            _cloneStub.Stub(
                x =>
                x.TableRoleMappingClone(Arg<RightType>.Is.Anything, Arg<ITableObject>.Is.Anything,
                                        Arg<IRole>.Is.Anything)).Do(
                                            returner);
            return rcrtoMerge;
        }

        private UserTableObjectRights UserTableRights(int columnid, int mappedcolumnid, int mappeduserid, int userid,
                                                      int excolumnid,
                                                      int exroleid, out UserTableObjectRights rcrExisting)
        {
            IUser user = GenerateUserMock(userid);
            IUser exuser = GenerateUserMock(exroleid);
            IUser mappeduser = GenerateUserMock(mappeduserid);

            ITableObject category = GenerateTableObject(columnid, null, null);
            ITableObject excategory = GenerateTableObject(excolumnid, null, null);
            ITableObject mappedcategory = GenerateTableObject(mappedcolumnid, null, null);
            UserTableObjectRights rcrtoMerge = new UserTableObjectRights();
            rcrtoMerge.Add(user, category, 12, RightType.Read);
            rcrExisting = new UserTableObjectRights();
            rcrExisting.Add(exuser, excategory, 34, RightType.Write);
            SUT.useruserMapping.Add(user, mappeduser);
            SUT.tabletableMapping.Add(category, mappedcategory);
            Func<RightType, ITableObject, IUser, TableUserMapping> returner =
                (c, u, r) => (new TableUserMapping {TableId = u.Id, UserId = r.Id});
            _cloneStub.Stub(
                x =>
                x.TableUserMappingClone(Arg<RightType>.Is.Anything, Arg<ITableObject>.Is.Anything,
                                        Arg<IUser>.Is.Anything)).Do(
                                            returner);
            return rcrtoMerge;
        }

        private UserOptimizationRights UserOptimizationRights(int columnid, int mappedcolumnid, int mappeduserid,
                                                              int userid, int excolumnid,
                                                              int exroleid, out UserOptimizationRights rcrExisting)
        {
            IUser user = GenerateUserMock(userid);
            IUser exuser = GenerateUserMock(exroleid);
            IUser mappeduser = GenerateUserMock(mappeduserid);

            IOptimization category = GenerateOptimization(columnid, null, null);
            IOptimization excategory = GenerateOptimization(excolumnid, null, null);
            IOptimization mappedcategory = GenerateOptimization(mappedcolumnid, null, null);
            UserOptimizationRights rcrtoMerge = new UserOptimizationRights();
            rcrtoMerge.Add(user, category, 12, RightType.Read);
            rcrExisting = new UserOptimizationRights();
            rcrExisting.Add(exuser, excategory, 34, RightType.Write);
            SUT.useruserMapping.Add(user, mappeduser);
            SUT.optoptMapping.Add(category, mappedcategory);
            Func<IOptimization, IUser, bool, OptimizationUserMapping> returner =
                (c, u, r) => (new OptimizationUserMapping {OptimizationId = c.Id, UserId = u.Id});
            _cloneStub.Stub(
                x =>
                x.OptimizationUserMappingClone(Arg<IOptimization>.Is.Anything, Arg<IUser>.Is.Anything,
                                               Arg<bool>.Is.Anything)).Do(
                                                   returner);
            return rcrtoMerge;
        }

        private RoleOptimizationRights RoleOptimizationRights(int columnid, int mappedcolumnid, int mappeduserid,
                                                              int userid, int excolumnid,
                                                              int exroleid, out RoleOptimizationRights rcrExisting)
        {
            IRole user = GenerateRole(userid);
            IRole exuser = GenerateRole(exroleid);
            IRole mappeduser = GenerateRole(mappeduserid);

            IOptimization category = GenerateOptimization(columnid, null, null);
            IOptimization excategory = GenerateOptimization(excolumnid, null, null);
            IOptimization mappedcategory = GenerateOptimization(mappedcolumnid, null, null);
            RoleOptimizationRights rcrtoMerge = new RoleOptimizationRights();
            rcrtoMerge.Add(user, category, 12, RightType.Read);
            rcrExisting = new RoleOptimizationRights();
            rcrExisting.Add(exuser, excategory, 34, RightType.Write);
            SUT.roleroleMapping.Add(user, mappeduser);
            SUT.optoptMapping.Add(category, mappedcategory);
            Func<IOptimization, IRole, bool, OptimizationRoleMapping> returner =
                (c, u, r) => (new OptimizationRoleMapping {OptimizationId = c.Id, RoleId = u.Id});
            _cloneStub.Stub(
                x =>
                x.OptimizationRoleClone(Arg<IOptimization>.Is.Anything, Arg<IRole>.Is.Anything, Arg<bool>.Is.Anything)).
                Do(
                    returner);
            return rcrtoMerge;
        }

        [TestCase(1, 4, 5, 134, Result = 135)]
        [TestCase(2333, 1, 456, 3, Result = 2334)]
        public int GetRelationOffset2_Test(params int[] id)
        {
            List<Relation> list = new List<Relation>();
            foreach (int i in id)
            {
                list.Add(new Relation {RelationId = i});
            }
            int expexted = SUT.GetRelationOffset(list);
            return expexted;
        }

        [TestCase(3, 23, 66)]
        public void TableText_The_Correct_Number_Of_Items_Are_Put_In_The_Collection(int numberofLangugaes,
                                                                                    int tableIdBefore, int tableIdAfter)
        {
            //Arrange
            ITableObjectCollection tableObjectCollection;
            var langugaes = SetUp_TableObjectText(numberofLangugaes, tableIdBefore, tableIdAfter,
                                                  out tableObjectCollection);
            //Act
            SUT.TableText(tableObjectCollection, langugaes);
            //Assert
            Assert.AreEqual(numberofLangugaes, SUT.LocalizedText.Count);
        }

        [TestCase(4, 56, 99)]
        public void TableText_The_Cloner_Was_Called_With_Correct_Type(int numberofLangugaes, int tableIdBefore,
                                                                      int tableIdAfter)
        {
            //Arrange
            ITableObjectCollection tableObjectCollection;
            var langugaes = SetUp_TableObjectText(numberofLangugaes, tableIdBefore, tableIdAfter,
                                                  out tableObjectCollection);
            //Act
            SUT.TableText(tableObjectCollection, langugaes);
            //Assert
            localizedtextCloneAssertWasCalled(typeof (TableObjectText), tableIdAfter, numberofLangugaes);
        }

        [TestCase(3, 23, 66)]
        public void TableText_The_DataBase_Mapping_Was_Called(int numberofLangugaes, int tableIdBefore, int tableIdAfter)
        {
            //Arrange
            ITableObjectCollection tableObjectCollection;
            var langugaes = SetUp_TableObjectText(numberofLangugaes, tableIdBefore, tableIdAfter,
                                                  out tableObjectCollection);
            //Act
            SUT.TableText(tableObjectCollection, langugaes);
            //Assert
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<LocalizedText>>.Is.Anything));
        }

        [TestCase(3, 56, 890)]
        public void ColumnText_The_Correct_Number_Of_Texts_Are_Added_To_The_Collection(int numberoflangugaes,
                                                                                       int columnidBefore,
                                                                                       int columnidAfter)
        {
            //Arrange
            ILanguageCollection lc;
            var fcc = SetUpColumnTexts(numberoflangugaes, columnidBefore, columnidAfter, out lc);
            //Act
            SUT.ColumnText(fcc, lc);
            //Assert
            Assert.AreEqual(numberoflangugaes, SUT.LocalizedText.Count);
        }

        [TestCase(3, 56, 890)]
        public void ColumnText_The_Cloner_Was_called_Correctly(int numberoflangugaes, int columnidBefore,
                                                               int columnidAfter)
        {
            //Arrange
            ILanguageCollection lc;
            var fcc = SetUpColumnTexts(numberoflangugaes, columnidBefore, columnidAfter, out lc);
            //Act
            SUT.ColumnText(fcc, lc);
            //Assert
            localizedtextCloneAssertWasCalled(typeof (ColumnText), columnidAfter, numberoflangugaes);
        }

        [TestCase(3, 56, 890)]
        public void ColumnText_CallDb_Mapping(int numberoflangugaes, int columnidBefore, int columnidAfter)
        {
            //Arrange
            ILanguageCollection lc;
            var fcc = SetUpColumnTexts(numberoflangugaes, columnidBefore, columnidAfter, out lc);
            //Act
            SUT.ColumnText(fcc, lc);
            //Assert
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<LocalizedText>>.Is.Anything));
        }

        [TestCase(12, 23, 8999)]
        public void CategoryText_Add_Correct_Items_To_List(int numberoflang, int categoryidBefore, int categoryidAfter)
        {
            //Arrange
            ICategoryCollection catcollection;
            var langugaes = SetUpCategoryText(numberoflang, categoryidBefore, categoryidAfter, out catcollection);
            //Act
            SUT.CategoryTexts(catcollection, langugaes);
            //Assert
            Assert.AreEqual(numberoflang, SUT.LocalizedText.Count);
        }

        [TestCase(34, 65, 88)]
        public void CategoryText_Call_Cloner_Correctly(int numberoflang, int categoryidBefore, int categoryidAfter)
        {
            //Arrange
            ICategoryCollection catcollection;
            var langugaes = SetUpCategoryText(numberoflang, categoryidBefore, categoryidAfter, out catcollection);
            //Act
            SUT.CategoryTexts(catcollection, langugaes);
            //Assert
            localizedtextCloneAssertWasCalled(typeof (CategoryText), categoryidAfter, numberoflang);
        }

        [TestCase(3, 8, 8)]
        public void CategoryText_Call_DbMapper(int numberoflang, int categoryidBefore, int categoryidAfter)
        {
            //Arrange
            ICategoryCollection catcollection;
            var langugaes = SetUpCategoryText(numberoflang, categoryidBefore, categoryidAfter, out catcollection);
            //Act
            SUT.CategoryTexts(catcollection, langugaes);
            //Assert
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<LocalizedText>>.Is.Anything));
        }

        [TestCase(23, 45, 56)]
        public void OptimizationText_Add_Items_To_Collection(int numOfLanguages, int idBefore, int idAfter)
        {
            SetUpOptimizationTexts(numOfLanguages, idBefore, idAfter);
            Assert.AreEqual(numOfLanguages, SUT.LocalizedText.Count);
        }

        [TestCase(20, 452, 5666)]
        public void OptimizationText_Call_Cloner(int numOfLanguages, int idBefore, int idAfter)
        {
            SetUpOptimizationTexts(numOfLanguages, idBefore, idAfter);
            localizedtextCloneAssertWasCalled(typeof (OptimizationText), idAfter, numOfLanguages);
        }

        [TestCase(23, 111111, 121212)]
        public void OptimizationText_Call_Db_Method(int numOfLanguages, int idBefore, int idAfter)
        {
            SetUpOptimizationTexts(numOfLanguages, idBefore, idAfter);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<LocalizedText>>.Is.Anything));
        }

        [TestCase(1, 4, 76)]
        public void ParameterText_Add_Items_To_Collection(int numberOfLangugaes, int paramidBefore, int paramidAfter)
        {
            //Arrange
            ILanguageCollection lcollection;
            var pcollection = SetUPParameterCollectionText(numberOfLangugaes, paramidBefore, paramidAfter,
                                                           out lcollection);
            //Act
            SUT.ParameterTexts(pcollection, lcollection);
            //Assert
            Assert.AreEqual(numberOfLangugaes, SUT.LocalizedText.Count);
        }

        [TestCase(3, 45, 76)]
        public void ParameterText_Call_Cloner(int numberOfLangugaes, int paramidBefore, int paramidAfter)
        {
            //Arrange
            ILanguageCollection lcollection;
            var pcollection = SetUPParameterCollectionText(numberOfLangugaes, paramidBefore, paramidAfter,
                                                           out lcollection);
            //Act
            SUT.ParameterTexts(pcollection, lcollection);
            //Assert
            localizedtextCloneAssertWasCalled(typeof (ParameterText), paramidAfter, numberOfLangugaes);
        }

        [TestCase(11, 48, 76)]
        public void ParameterText_Add_Call_Db_Mapper(int numberOfLangugaes, int paramidBefore, int paramidAfter)
        {
            //Arrange
            ILanguageCollection lcollection;
            var pcollection = SetUPParameterCollectionText(numberOfLangugaes, paramidBefore, paramidAfter,
                                                           out lcollection);
            //Act
            SUT.ParameterTexts(pcollection, lcollection);
            //Assert
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<LocalizedText>>.Is.Anything));
        }

        [TestCase(2, 4, 34)]
        public void ParameterValue_AKA_Collections_Add_Items_To_List(int numberofLanguages, int paramValueIdBefore,
                                                                     int paramValueIdAfter)
        {
            //Arrange
            ILanguageCollection lc;
            var pvlist = SetUpParameterValues(numberofLanguages, paramValueIdBefore, paramValueIdAfter, out lc);
            //Act
            SUT.ParameterValueText(pvlist, lc);
            //Assert
            Assert.AreEqual(numberofLanguages, SUT.LocalizedText.Count);
        }

        [TestCase(29, 45, 34)]
        public void ParameterValue_AKA_Collections_Call_Cloner(int numberofLanguages, int paramValueIdBefore,
                                                               int paramValueIdAfter)
        {
            //Arrange
            ILanguageCollection lc;
            var pvlist = SetUpParameterValues(numberofLanguages, paramValueIdBefore, paramValueIdAfter, out lc);
            //Act
            SUT.ParameterValueText(pvlist, lc);
            //Assert
            localizedtextCloneAssertWasCalled(typeof (ParameterValueSetText), paramValueIdAfter, numberofLanguages);
        }

        [TestCase(23, 41, 34)]
        public void ParameterValue_AKA_Collections_Call_Db_Mapper(int numberofLanguages, int paramValueIdBefore,
                                                                  int paramValueIdAfter)
        {
            ILanguageCollection lc;
            var pvlist = SetUpParameterValues(numberofLanguages, paramValueIdBefore, paramValueIdAfter, out lc);
            SUT.ParameterValueText(pvlist, lc);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<LocalizedText>>.Is.Anything));
        }

        [TestCase(12, 13, 14)]
        public void OptimizationGroupText_Add_Items_To_Collection(int numOfLanguages, int idbefore, int idafter)
        {
            ILanguageCollection lc;
            var optgcoll = SetUpOptimizationGroupText(numOfLanguages, idbefore, idafter, out lc);
            SUT.OptimizationGroupText(optgcoll, lc);
            Assert.AreEqual(numOfLanguages, SUT.LocalizedText.Count);
        }

        [TestCase(12, 3313, 5514)]
        public void OptimizationGroupText_CallCloner(int numOfLanguages, int idbefore, int idafter)
        {
            ILanguageCollection lc;
            var optgcoll = SetUpOptimizationGroupText(numOfLanguages, idbefore, idafter, out lc);
            SUT.OptimizationGroupText(optgcoll, lc);
            localizedtextCloneAssertWasCalled(typeof (OptimizationGroupText), idafter, numOfLanguages);
        }

        [TestCase(111, 2222, 222214)]
        public void OptimizationGroupText_CallMApper(int numOfLanguages, int idbefore, int idafter)
        {
            ILanguageCollection lc;
            var optgcoll = SetUpOptimizationGroupText(numOfLanguages, idbefore, idafter, out lc);
            SUT.OptimizationGroupText(optgcoll, lc);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<LocalizedText>>.Is.Anything));
        }

        private IOptimizationGroupCollection SetUpOptimizationGroupText(int numOfLanguages, int idbefore, int idafter,
                                                                        out ILanguageCollection lc)
        {
            IOptimizationGroup optg1 = GenerateOptimizationGroup(idbefore);
            IOptimizationGroup optg2 = GenerateOptimizationGroup(idafter);
            IOptimizationGroupCollection optgcoll = GenerateOptimizationGroupCollection(optg1);
            lc = GenerateSomeLanguages(numOfLanguages);
            SUT.optgroupoptgroupMapping.Add(optg1, optg2);
            return optgcoll;
        }

        private List<ParameterValue> SetUpParameterValues(int numberofLanguages, int paramValueIdBefore,
                                                          int paramValueIdAfter,
                                                          out ILanguageCollection lc)
        {
            ParameterValue pv1 = new ParameterValue {Id = paramValueIdBefore};
            ParameterValue pv2 = new ParameterValue {Id = paramValueIdAfter};
            List<ParameterValue> pvlist = new List<ParameterValue> {pv1};
            lc = GenerateSomeLanguages(numberofLanguages);
            SUT.parametervalueparametervalueMapping.Add(pv1, pv2);
            return pvlist;
        }

        private IParameterCollection SetUPParameterCollectionText(int numberOfLangugaes, int paramidBefore,
                                                                  int paramidAfter,
                                                                  out ILanguageCollection lcollection)
        {
            IParameter p1 = GenerateParameter(paramidBefore);
            IParameter p2 = GenerateParameter(paramidAfter);
            IParameterCollection pcollection = GenerateParameterCollection(p1);
            SUT.parameterparameterMapping.Add(p1, p2);
            lcollection = GenerateSomeLanguages(numberOfLangugaes);
            return pcollection;
        }

        private void SetUpOptimizationTexts(int numOfLanguages, int idBefore, int idAfter)
        {
            IOptimization o1 = GenerateOptimization(idBefore, GenerateOptimization(23));
            IOptimization o2 = GenerateOptimization(idAfter, GenerateOptimization(45));
            IOptimizationCollection optcoll = GenerateOptimizationCollection(o1);
            SUT.optoptMapping.Add(o1, o2);
            ILanguageCollection lcoll = GenerateSomeLanguages(numOfLanguages);
            SUT.OptimizationText(optcoll, lcoll);
        }

        private ILanguageCollection SetUpCategoryText(int numberoflang, int categoryidBefore, int categoryidAfter,
                                                      out ICategoryCollection catcollection)
        {
            ILanguageCollection langugaes = GenerateSomeLanguages(numberoflang);
            ICategory cat1 = GenerateCategoryMock(categoryidBefore);
            ICategory cat2 = GenerateCategoryMock(categoryidAfter);
            catcollection = GenerateCategoryCollection(cat1);
            SUT.categorycategoryMapping.Add(cat1, cat2);
            return langugaes;
        }

        private IFullColumnCollection SetUpColumnTexts(int numberoflangugaes, int columnidBefore, int columnidAfter,
                                                       out ILanguageCollection lc)
        {
            IColumn c1 = GenerateColumn(columnidBefore, null);
            IColumn c2 = GenerateColumn(columnidAfter, null);
            IFullColumnCollection fcc = GenerateColumnCollections(c1);
            lc = GenerateSomeLanguages(numberoflangugaes);
            SUT.columncolumnMapping.Add(c1, c2);
            return fcc;
        }

        private ILanguageCollection SetUp_TableObjectText(int numberofLangugaes, int tableIdBefore, int tableIdAfter,
                                                          out ITableObjectCollection tableObjectCollection)
        {
            ILanguageCollection langugaes = GenerateSomeLanguages(numberofLangugaes);
            ITableObject to = GenerateTableObject(tableIdBefore);
            ITableObject to2 = GenerateTableObject(tableIdAfter);
            tableObjectCollection = GenerateTableObjectCollections(to);
            SUT.tabletableMapping.Add(to, to2);
            return langugaes;
        }

        private void localizedtextCloneAssertWasCalled(Type type, int refid, int repeat)
        {
            _cloneStub.AssertWasCalled(
                x =>
                x.LocalizedTextClone(Arg<Type>.Matches(a => a.Name == type.Name), Arg<ILanguage>.Is.Anything,
                                     Arg<int>.Is.Equal(refid), Arg<ILocalizedTextCollection>.Is.Anything),
                t => t.Repeat.Times(repeat));
        }

        private ILanguageCollection GenerateSomeLanguages(int numOfLanguages)
        {
            ILanguageCollection langugaes;
            ILanguage[] lang = new ILanguage[numOfLanguages];
            for (int i = 0; i < numOfLanguages; i++)
            {
                lang[i] = GenerateLanguageMock();
            }
            return langugaes = GenerateLanguageCollections(lang);
        }

        [Test]
        public void CategoryMapping_Test()
        {
            var fromCategory = GenerateCategoryCollection(GenerateCategoryMock(1), GenerateCategoryMock(2),
                                                          GenerateCategoryMock(3));
            int MAGICnumberofItems = 3;
            SUT.CategoryMapping(fromCategory);
            Assert.AreEqual(MAGICnumberofItems, SUT.categorycategoryMapping.Count);
            Assert.AreEqual(MAGICnumberofItems, SUT._categories.Count);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<Category>>.Is.Anything));
        }

        [Test]
        public void CategoryRole_Test()
        {
            int categoryid = 12;
            int userid = 38;
            int excategoryid = 88;
            int exuserid = 66;
            int mappedcategoryid = 888;
            int mappeduserid = 999;

            RoleCategoryRights rcrExisting;
            var rcrtoMerge = RoleCategoryRights(categoryid, mappedcategoryid, mappeduserid, userid, excategoryid,
                                                exuserid, out rcrExisting);
            SUT.CategoryRole(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<CategoryRoleMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._categoryrolemappings.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreNotEqual(excategoryid, SUT._categoryrolemappings[0].CategoryId);
            Assert.AreNotEqual(exuserid, SUT._categoryrolemappings[0].RoleId);
        }

        [Test]
        public void CategoryRole_Unique_Key_Test()
        {
            int columnid = 12;
            int userid = 34;
            int excolumnid = 88;
            int exuserid = 66;
            int mappedcolumnid = excolumnid;
            int mappeduserid = exuserid;
            RoleCategoryRights rcrExisting;
            var rcrtoMerge = RoleCategoryRights(columnid, mappedcolumnid, mappeduserid, userid, excolumnid, exuserid,
                                                out rcrExisting);
            SUT.CategoryRole(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<CategoryRoleMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._categoryrolemappings.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreEqual(excolumnid, SUT._categoryrolemappings[0].CategoryId);
            Assert.AreEqual(exuserid, SUT._categoryrolemappings[0].RoleId);
        }

        [Test]
        public void CategoryUser_Test()
        {
            int categoryid = 12;
            int userid = 38;
            int excategoryid = 88;
            int exuserid = 66;
            int mappedcategoryid = 888;
            int mappeduserid = 999;

            UserCategoryRights rcrExisting;
            var rcrtoMerge = UserCategoryRights(categoryid, mappedcategoryid, mappeduserid, userid, excategoryid,
                                                exuserid, out rcrExisting);
            SUT.CategoryUser(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<CategoryUserMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._categoryusermappings.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreNotEqual(excategoryid, SUT._categoryusermappings[0].CategoryId);
            Assert.AreNotEqual(exuserid, SUT._categoryusermappings[0].UserId);
        }

        [Test]
        public void CategoryUser_Unique_Key_Test()
        {
            int columnid = 12;
            int userid = 34;
            int excolumnid = 88;
            int exuserid = 66;
            int mappedcolumnid = excolumnid;
            int mappeduserid = exuserid;
            UserCategoryRights rcrExisting;
            var rcrtoMerge = UserCategoryRights(columnid, mappedcolumnid, mappeduserid, userid, excolumnid, exuserid,
                                                out rcrExisting);
            SUT.CategoryUser(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<CategoryUserMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._categoryusermappings.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreEqual(excolumnid, SUT._categoryusermappings[0].CategoryId);
            Assert.AreEqual(exuserid, SUT._categoryusermappings[0].UserId);
        }

        [Test]
        public void CheckUniqueKeyForOptimizations_Test()
        {
            int parentid = 23;
            int id = 77;
            string value = "kutya";
            int groupid = 99;
            IOptimization parentopt = GenerateOptimization(parentid, null);
            IOptimization opt = GenerateOptimization(id, parentopt, value);
            IOptimizationGroup optgroup = MockRepository.GenerateMock<IOptimizationGroup>();
            optgroup.Stub(x => x.Id).Return(groupid);
            opt.Stub(x => x.Group).Return(optgroup);
            Optimization o = new Optimization();
            o.ParentId = parentid;
            o.Value = value;
            SUT.CheckUniqueKeyForOptimization(o, GenerateOptimizationCollection(opt));
            Assert.AreEqual(groupid, o.OptimizationGroupId);
            Assert.AreEqual(id, o.Id);
        }

        [Test]
        public void ColumnRole_CheckUniqueKey_Test()
        {
            int columnid = 12;
            int roleid = 34;
            int mappedcolumnid = 888;
            int mappedroleid = 999;
            int excolumnid = mappedcolumnid;
            int exroleid = mappedroleid;

            RoleColumnRights rcrExisting;
            var rcrtoMerge = RoleColumnRights(columnid, mappedcolumnid, mappedroleid, roleid, excolumnid, exroleid,
                                              out rcrExisting);
            SUT.ColumnRole(rcrtoMerge, rcrExisting);
            Assert.AreEqual(excolumnid, SUT._columnroleMapping[0].ColumnId);
            Assert.AreEqual(exroleid, SUT._columnroleMapping[0].RoleId);
        }

        [Test]
        public void ColumnRole_Test()
        {
            int columnid = 12;
            int roleid = 34;
            int excolumnid = 88;
            int exroleid = 66;
            int mappedcolumnid = 888;
            int mappedroleid = 999;

            RoleColumnRights rcrExisting;
            var rcrtoMerge = RoleColumnRights(columnid, mappedcolumnid, mappedroleid, roleid, excolumnid, exroleid,
                                              out rcrExisting);
            SUT.ColumnRole(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<ColumnRoleMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._columnroleMapping.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreNotEqual(excolumnid, SUT._columnroleMapping[0].ColumnId);
            Assert.AreNotEqual(exroleid, SUT._columnroleMapping[0].RoleId);
        }

        [Test]
        public void ColumnUser_Test()
        {
            int columnid = 12;
            int userid = 34;
            int excolumnid = 88;
            int exuserid = 66;
            int mappedcolumnid = 888;
            int mappeduserid = 999;

            UserColumnRights rcrExisting;
            var rcrtoMerge = UserColumnRights(columnid, mappedcolumnid, mappeduserid, userid, excolumnid, exuserid,
                                              out rcrExisting);
            SUT.ColumnUser(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<ColumnUserMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._ColumnUserMappings.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreNotEqual(excolumnid, SUT._ColumnUserMappings[0].ColumnId);
            Assert.AreNotEqual(exuserid, SUT._ColumnUserMappings[0].UserId);
        }

        [Test]
        public void ColumnUser_Unique_Key_Test()
        {
            int columnid = 12;
            int userid = 34;
            int excolumnid = 88;
            int exuserid = 66;
            int mappedcolumnid = excolumnid;
            int mappeduserid = exuserid;

            UserColumnRights rcrExisting;
            var rcrtoMerge = UserColumnRights(columnid, mappedcolumnid, mappeduserid, userid, excolumnid, exuserid,
                                              out rcrExisting);
            SUT.ColumnUser(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<ColumnUserMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._ColumnUserMappings.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreEqual(excolumnid, SUT._ColumnUserMappings[0].ColumnId);
            Assert.AreEqual(exuserid, SUT._ColumnUserMappings[0].UserId);
        }

        [Test]
        public void Column_Test()
        {
            int tableid1 = 77;
            int tableid2 = 88;
            ITableObject to1 = GenerateTableObject(tableid1, "table1", null);
            ITableObject to2 = GenerateTableObject(tableid2, "table2", null);
            IColumn column = GenerateColumn(3333, to1);
            IColumn column2 = GenerateColumn(4444, to2);
            int magicnumberofitems = 2;
            IFullColumnCollection columns = GenerateColumnCollections(column, column2);
            int newtableid1 = 99;
            int newtableid2 = 111;
            SUT.tabletableMapping.Add(to1, GenerateTableObject(newtableid1, "whocares", null));
            SUT.tabletableMapping.Add(to2, GenerateTableObject(newtableid2, "etwas", null));
            Func<IColumn, ITableObject, Column> returner = (o, t) => (new Column {TableId = t.Id});
            _cloneStub.Stub(x => x.ColumnClone(Arg<IColumn>.Is.Anything, Arg<TableObject>.Is.Anything)).Do(returner);
            SUT.Columns(columns);
            Assert.AreEqual(magicnumberofitems, SUT._column.Count);
            _mapMock.Stub(x => x.MapCollection(Arg<List<Column>>.Is.Anything));
            Assert.AreEqual(magicnumberofitems, SUT._column.Count);
            Assert.AreEqual(magicnumberofitems, SUT.columncolumnMapping.Count);
            Assert.AreEqual(newtableid1, ((Column) SUT.columncolumnMapping[column]).TableId);
        }

        [Test]
        public void GetStartingOrdinal_Test()
        {
            int ordinal1 = 0;
            int ordinal2 = 7;
            int maxordinal = 6789;
            ITableObject to1 = MockRepository.GenerateMock<ITableObject>();
            ITableObject to2 = MockRepository.GenerateMock<ITableObject>();
            ITableObject to3 = MockRepository.GenerateMock<ITableObject>();
            to1.Stub(x => x.Ordinal).Return(ordinal1);
            to2.Stub(x => x.Ordinal).Return(ordinal2);
            to3.Stub(x => x.Ordinal).Return(maxordinal);
            ITableObjectCollection tocollection = GenerateTableObjectCollections(to3, to1, to3);
            int expected = maxordinal + 1;
            int actual = SUT.GetStartingOrdinal(tocollection);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Issuextensions()
        {
            Assert.Inconclusive("Not tested, cannot test hardcore casting");
        }

        [Test]
        public void MapUser_User_Added_To_The_List_To_Map()
        {
            //Arrange
            int fromId1 = 12;
            int fromId2 = 23;
            int MAGIC = 2; // items in incoming list
            string fromName1 = "userfrom1";
            string fromName2 = "name from 1";
            int toid1 = 43;
            int toId2 = 78;
            int toId3 = 99;
            string toName1 = "USername1to";
            string toNamecollision = fromName2;
            string toName = "kutya";
            var fromusers = GenerateUserIdCollection(GenerateUserMock(fromId1, fromName1),
                                                     GenerateUserMock(fromId2, fromName2));
            var tousers = GenerateUserIdCollection(GenerateUserMock(toid1, toName1),
                                                   GenerateUserMock(toId2, toNamecollision),
                                                   GenerateUserMock(toId3, toName));
            Func<IUser, User> returner = (o) => (new User {UserName = o.UserName});
            _cloneStub.Stub(x => x.UserClone(Arg<User>.Is.Anything)).Do(returner);
            //Act
            SUT.Users(fromusers, tousers);
            //Assert, sorry for multiple assertion :D
            Assert.AreEqual(MAGIC, SUT._users.Count, "The list should contain 2 elements to write DB");
            Assert.AreEqual(fromName1, SUT._users[0].UserName, "The name stored correctly for 1st item");
            Assert.AreEqual(fromName2, SUT._users[1].UserName, "The name stored correctly for 2 nd item too");
            Assert.AreNotEqual(fromId1, SUT._users[0].Id, "The old id is not present any more");
            Assert.AreEqual(toId2, SUT._users[1].Id,
                            "the item already exist with the given unique key, so the old ID will be used");
            Assert.AreEqual(MAGIC, SUT.useruserMapping.Count);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<User>>.Is.Anything));
        }

        [Test]
        public void OptimizationGroup_Test()
        {
            IOptimizationGroup optgroup1 = GenerateOptimizationGroup(23);
            IOptimizationGroup optgroup2 = GenerateOptimizationGroup(78);
            IOptimizationGroupCollection optgroupcollection = GenerateOptimizationGroupCollection(optgroup1, optgroup2);
            Func<IOptimizationGroup, OptimizationGroup> returner = (l) => (new OptimizationGroup());
            _cloneStub.Stub(x => x.OptimizationGroupClone(Arg<IOptimizationGroup>.Is.Anything)).Do(returner);
            SUT.OptimizationGroup(optgroupcollection);
            Assert.AreEqual(2, SUT.optgroupoptgroupMapping.Count);
            Assert.AreEqual(2, SUT._optgroupmappings.Count);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<OptimizationGroup>>.Is.Anything));
        }

        [Test]
        public void OptimizationRole_Test()
        {
            int categoryid = 12;
            int userid = 38;
            int excategoryid = 88;
            int exuserid = 66;
            int mappedcategoryid = 888;
            int mappeduserid = 999;

            RoleOptimizationRights rcrExisting;
            var rcrtoMerge = RoleOptimizationRights(categoryid, mappedcategoryid, mappeduserid, userid, excategoryid,
                                                    exuserid, out rcrExisting);
            SUT.OptimizationRole(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<OptimizationRoleMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._optrolesrights.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreNotEqual(excategoryid, SUT._optrolesrights[0].OptimizationId);
            Assert.AreNotEqual(exuserid, SUT._optrolesrights[0].RoleId);
        }

        [Test]
        public void OptimizationRole_Unique_Key_Test()
        {
            int columnid = 12;
            int userid = 34;
            int excolumnid = 88;
            int exuserid = 66;
            int mappedcolumnid = excolumnid;
            int mappeduserid = exuserid;
            RoleOptimizationRights rcrExisting;
            var rcrtoMerge = RoleOptimizationRights(columnid, mappedcolumnid, mappeduserid, userid, excolumnid, exuserid,
                                                    out rcrExisting);
            SUT.OptimizationRole(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<OptimizationRoleMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._optrolesrights.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreEqual(excolumnid, SUT._optrolesrights[0].OptimizationId);
            Assert.AreEqual(exuserid, SUT._optrolesrights[0].RoleId);
        }

        [Test]
        public void OptimizationText_Not_Mapped_If_It_Is_A_Root_Element_In_Tree()
        {
            //Arrange
            // the magic numbers are just placeholders... simply dummy ID-s...
            //a rootoptimization
            IOptimization parentopt1 = null;
            IOptimization opt1 = GenerateOptimization(12, parentopt1);
            //A non-root optimization :D
            IOptimization opt2 = GenerateOptimization(34, GenerateOptimization(34, null));
            //Add optimization to an optimizationcollection
            IOptimizationCollection optcoll = GenerateOptimizationCollection(opt1, opt2);
            ILanguageCollection langcoll = GenerateSomeLanguages(2);
            //the mapped pairs
            IOptimization opt2Mapped = GenerateOptimization(56, null);
            IOptimization opt1mapped = GenerateOptimization(13, parentopt1);
            SUT.optoptMapping.Add(opt1, opt1mapped);
            SUT.optoptMapping.Add(opt2, opt2Mapped);
            //Act
            SUT.OptimizationText(optcoll, langcoll);
            //Assert
            Assert.AreEqual(2, SUT.LocalizedText.Count);
        }

        [Test]
        public void OptimizationUser_Test()
        {
            int categoryid = 12;
            int userid = 38;
            int excategoryid = 88;
            int exuserid = 66;
            int mappedcategoryid = 888;
            int mappeduserid = 999;

            UserOptimizationRights rcrExisting;
            var rcrtoMerge = UserOptimizationRights(categoryid, mappedcategoryid, mappeduserid, userid, excategoryid,
                                                    exuserid, out rcrExisting);
            SUT.OptimizationUser(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<OptimizationUserMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._optuserrights.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreNotEqual(excategoryid, SUT._optuserrights[0].OptimizationId);
            Assert.AreNotEqual(exuserid, SUT._optuserrights[0].UserId);
        }

        [Test]
        public void OptimizationUser_Unique_Key_Test()
        {
            int columnid = 12;
            int userid = 34;
            int excolumnid = 88;
            int exuserid = 66;
            int mappedcolumnid = excolumnid;
            int mappeduserid = exuserid;
            UserOptimizationRights rcrExisting;
            var rcrtoMerge = UserOptimizationRights(columnid, mappedcolumnid, mappeduserid, userid, excolumnid, exuserid,
                                                    out rcrExisting);
            SUT.OptimizationUser(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<OptimizationUserMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._optuserrights.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreEqual(excolumnid, SUT._optuserrights[0].OptimizationId);
            Assert.AreEqual(exuserid, SUT._optuserrights[0].UserId);
        }

        [Test]
        public void OrderAreaTest()
        {
            int newtableId = 23;
            int oldtableId = 12;
            ITableObject tableMock = GenerateTableObject(oldtableId, "tabelname", null);
            ITableObject newtableMock = GenerateTableObject(newtableId, "fjsldkfjhas", null);
            IOrderArea orderareaMock = GenerateOrderArea();
            tableMock.Stub(x => x.OrderAreas).Return(GenerateOrderAreaCollection(orderareaMock));
            ITableObjectCollection tablecollMock = GenerateTableObjectCollections(tableMock);
            ITableObjectCollection newTablecollMock = GenerateTableObjectCollections(newtableMock);
            SUT.tabletableMapping.Add(tableMock, newtableMock);
            Func<IOrderArea, ITableObject, OrderArea> returner = (l, o) => (new OrderArea {TableId = o.Id});
            _cloneStub.Stub(x => x.OrderAreaClone(Arg<IOrderArea>.Is.Anything, Arg<ITableObject>.Is.Anything)).Do(
                returner);
            SUT.OrderArea(tablecollMock, newTablecollMock);
            Assert.AreEqual(1, SUT.OrderAreas.Count);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<OrderArea>>.Is.Anything));
            Assert.AreEqual(newtableId, SUT.OrderAreas[0].TableId);
        }

        [Test]
        public void Role_Test()
        {
            IRole role1 = GenerateRole(4);
            IRole role2 = GenerateRole(5);
            IRoleCollection rolecollecion = GenerateRoleCollection(role1, role2);
            SUT.Roles(rolecollecion);
            Assert.AreEqual(2, SUT.roleroleMapping.Count);
            Assert.AreEqual(2, SUT._roles.Count);
            _mapMock.Stub(x => x.MapCollection(Arg<List<Role>>.Is.Anything));
        }

        [Test]
        public void TableRole_Test()
        {
            int categoryid = 12;
            int userid = 38;
            int excategoryid = 88;
            int exuserid = 66;
            int mappedcategoryid = 888;
            int mappeduserid = 999;

            RoleTableObjectRights rcrExisting;
            var rcrtoMerge = RoleTableRights(categoryid, mappedcategoryid, mappeduserid, userid, excategoryid, exuserid,
                                             out rcrExisting);
            SUT.TableRole(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<TableRoleMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._tablerolerights.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreNotEqual(excategoryid, SUT._tablerolerights[0].TableId);
            Assert.AreNotEqual(exuserid, SUT._tablerolerights[0].RoleId);
        }

        [Test]
        public void TableRole_Unique_Key_Test()
        {
            int columnid = 12;
            int userid = 34;
            int excolumnid = 88;
            int exuserid = 66;
            int mappedcolumnid = excolumnid;
            int mappeduserid = exuserid;
            RoleTableObjectRights rcrExisting;
            var rcrtoMerge = RoleTableRights(columnid, mappedcolumnid, mappeduserid, userid, excolumnid, exuserid,
                                             out rcrExisting);
            SUT.TableRole(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<TableRoleMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._tablerolerights.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreEqual(excolumnid, SUT._tablerolerights[0].TableId);
            Assert.AreEqual(exuserid, SUT._tablerolerights[0].RoleId);
        }

        [Test]
        public void TableUser_Test()
        {
            int categoryid = 12;
            int userid = 38;
            int excategoryid = 88;
            int exuserid = 66;
            int mappedcategoryid = 888;
            int mappeduserid = 999;

            UserTableObjectRights rcrExisting;
            var rcrtoMerge = UserTableRights(categoryid, mappedcategoryid, mappeduserid, userid, excategoryid, exuserid,
                                             out rcrExisting);
            SUT.TableUser(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<TableUserMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._tableuserrights.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreNotEqual(excategoryid, SUT._tableuserrights[0].TableId);
            Assert.AreNotEqual(exuserid, SUT._tableuserrights[0].UserId);
        }

        [Test]
        public void TableUser_Unique_Key_Test()
        {
            int columnid = 12;
            int userid = 34;
            int excolumnid = 88;
            int exuserid = 66;
            int mappedcolumnid = excolumnid;
            int mappeduserid = exuserid;
            UserTableObjectRights rcrExisting;
            var rcrtoMerge = UserTableRights(columnid, mappedcolumnid, mappeduserid, userid, excolumnid, exuserid,
                                             out rcrExisting);
            SUT.TableUser(rcrtoMerge, rcrExisting);
            _mapMock.AssertWasCalled(x => x.MapCollection(Arg<List<TableUserMapping>>.Is.Anything));
            Assert.AreEqual(1, SUT._tableuserrights.Count);
            // no unique key collision... items will got their Id-s later.
            Assert.AreEqual(excolumnid, SUT._tableuserrights[0].TableId);
            Assert.AreEqual(exuserid, SUT._tableuserrights[0].UserId);
        }
    }
}