using System;
using System.Reflection;
using SystemDb;
using SystemDb.Helper;
using SystemDb.Internal;
using DbAccess;
using NUnit.Framework;
using Rhino.Mocks;

namespace SystemDb_Test
{
    /// <summary>
    ///   Basically I hate testmethods which asserts several times in the same method, but at this point it seems the fastest way
    /// </summary>
    [TestFixture]
    internal class CloneBusinessObjects_Test
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            langMock = MockRepository.GenerateMock<ILanguage>();
            loctextcollMock = MockRepository.GenerateMock<ILocalizedTextCollection>();
            SUT = new CloneBusinessObjects();
        }

        #endregion

        private CloneBusinessObjects SUT;
        private ILanguage langMock;
        private ILocalizedTextCollection loctextcollMock;

        private void CheckColumnName(object classToCheck, string propertyname, string dataBaseColumnName)
        {
            Type mapType = classToCheck.GetType();
            string name = string.Empty;
            PropertyInfo pi;
            try
            {
                pi = mapType.GetProperty(propertyname);
                var column = Attribute.GetCustomAttribute(pi,
                                                          typeof (DbColumnAttribute));
                name = ((DbColumnAttribute) column).Name;
            }
            catch
            {
                name = "invalid propertyname: " + propertyname;
            }
            Assert.AreEqual(dataBaseColumnName, name);
        }

        private void CheckTableName(object classToCheck, string datatablename)
        {
            Type mapType = classToCheck.GetType();
            var table = Attribute.GetCustomAttribute(mapType, typeof (DbTableAttribute));
            string actualtablename = ((DbTableAttribute) table).Name;
            Assert.AreEqual(datatablename, actualtablename);
        }

        //[Test]
        //public void TableObjectClone_Test() {
        //    //Arrange
        //    var tableMock = MockRepository.GenerateMock<ITableObject>();
        //    var categoryMock = MockRepository.GenerateMock<ICategory>();
        //    var schemeMock = MockRepository.GenerateMock<IScheme>();
        //    //11 column, +1 ID
        //    int category = 2;
        //    string database = "SAP_Whatever";
        //    string name = "btcsev";
        //    TableType type = TableType.View;
        //    int row_count = 12345;
        //    bool visible = true;
        //    int default_scheme = 1;
        //    int transaction_nr = 491;
        //    bool user_defined = true;
        //    int ordinal = 490;
        //    bool archived = true;
        //    tableMock.Stub(x => x.Database).Return(database);
        //    tableMock.Stub(x => x.TableName).Return(name);
        //    tableMock.Stub(x => x.Type).Return(type);
        //    tableMock.Stub(x => x.RowCount).Return(row_count);
        //    tableMock.Stub(x => x.IsVisible).Return(visible);
        //    tableMock.Stub(x => x.DefaultScheme).Return(schemeMock);
        //    schemeMock.Stub(x => x.Id).Return(default_scheme);
        //    tableMock.Stub(x => x.TransactionNumber).Return(transaction_nr);
        //    tableMock.Stub(x => x.UserDefined).Return(user_defined);
        //    tableMock.Stub(x => x.Ordinal).Return(ordinal);
        //    tableMock.Stub(x => x.IsArchived).Return(archived);
        //    categoryMock.Stub(x => x.Id).Return(category);
        //    int offsetForOrdinal = 34;
        //    //Act
        //    TableObject actual = SUT.TableObjectClone(tableMock, categoryMock,offsetForOrdinal);
        //    //Assert
        //    CheckTableName(actual,"tables");
        //    Assert.AreEqual(category, actual.CategoryId);
        //    CheckColumnName(actual,"CategoryId","category");
        //    Assert.AreEqual(database,actual.Database);
        //    CheckColumnName(actual,"Database","database");
        //    Assert.AreEqual(name,actual.TableName);
        //    CheckColumnName(actual,"TableName","name");
        //    Assert.AreEqual(type,actual.Type);
        //    //due to property hiding... it is excluded from test... i dont have time to work on this issue
        //    //CheckColumnName(actual,"Type","type");
        //    Assert.AreEqual(row_count,actual.RowCount);
        //    CheckColumnName(actual,"RowCount","row_count");
        //    Assert.AreEqual(visible,actual.IsVisible);
        //    CheckColumnName(actual,"IsVisible","visible");
        //    Assert.AreEqual(actual.DefaultSchemeId,default_scheme);
        //    CheckColumnName(actual,"DefaultSchemeId","default_scheme");
        //    Assert.AreEqual(actual.TransactionNumber,transaction_nr);
        //    CheckColumnName(actual,"TransactionNumber","transaction_nr");
        //    Assert.AreEqual(actual.UserDefined,user_defined);
        //    CheckColumnName(actual,"UserDefined","user_defined");
        //    Assert.AreEqual(actual.Ordinal,ordinal+offsetForOrdinal);
        //    CheckColumnName(actual,"Ordinal","ordinal");
        //    Assert.AreEqual(actual.IsArchived,archived);
        //    CheckColumnName(actual,"IsArchived","archived");
        //}

        private void CreateLocalizedText(string text, string country_code)
        {
            langMock.Stub(x => x.CountryCode).Return(country_code);
            loctextcollMock.Stub(x => x[langMock]).Return(text);
        }

        private ILanguage CreateLanguageMock(string countryCode)
        {
            ILanguage language = MockRepository.GenerateMock<ILanguage>();
            language.Stub(x => x.CountryCode).Return(countryCode);
            return language;
        }

        private ILocalizedTextCollection CreateLocalizedTextCollection(string text, ILanguage language)
        {
            ILocalizedTextCollection localizedtextcollection = MockRepository.GenerateMock<ILocalizedTextCollection>();
            localizedtextcollection.Stub(x => x[language]).Return(text);
            return localizedtextcollection;
        }

        [TestCase(typeof (OptimizationGroupText))]
        [TestCase(typeof (OptimizationText))]
        [TestCase(typeof (TableObjectText))]
        [TestCase(typeof (ColumnText))]
        [TestCase(typeof (ParameterText))]
        [TestCase(typeof (ParameterValueSetText))]
        [TestCase(typeof (CategoryText))]
        public void LocalizedTestClone_Return_With_Correct_Type_Test(Type typetocheck)
        {
            var result = CloneLocalizedText(typetocheck, 44, "whatever", "whatever");
            Assert.IsInstanceOf(typetocheck, result);
        }

        [TestCase(typeof (Column))]
        [TestCase(typeof (object))]
        public void LocalizedTestClone_Throws_Exceptions_If_Not_(Type typetocheck)
        {
            Assert.Catch<Exception>(() => CloneLocalizedText(typetocheck, 44, "whatever", "whatever"));
        }

        private LocalizedText CloneLocalizedText(Type type, int refid, string text, string countryCode)
        {
            ILanguage language = CreateLanguageMock(countryCode);
            ILocalizedTextCollection textcollection = CreateLocalizedTextCollection(text, language);
            var result = SUT.LocalizedTextClone(type, language, refid, textcollection);
            return result;
        }

        [Test]
        public void CategoryClone()
        {
            Assert.Inconclusive("Nothing to test here");
        }

        [Test]
        public void CategoryRoleMapping()
        {
            //Arrange
            ICategory categoryMock = MockRepository.GenerateMock<ICategory>();
            IRole roleMock = MockRepository.GenerateMock<IRole>();
            RightType right = RightType.Write;
            int category_id = 345;
            int role_id = 789;
            categoryMock.Stub(x => x.Id).Return(category_id);
            roleMock.Stub(x => x.Id).Return(role_id);
            //Act
            CategoryRoleMapping actual = SUT.CategoryRoleMappingClone(categoryMock, roleMock, right);
            //Assert
            CheckTableName(actual, "category_roles");
            Assert.AreEqual(category_id, actual.CategoryId);
            CheckColumnName(actual, "CategoryId", "category_id");
            Assert.AreEqual(role_id, actual.RoleId);
            CheckColumnName(actual, "RoleId", "role_id");
            Assert.AreEqual(right, actual.Right);
            CheckColumnName(actual, "Right", "right");
        }

        [Test]
        public void CategoryUser_Test()
        {
            //Arrange
            ICategory catMock = MockRepository.GenerateMock<ICategory>();
            IUser userMock = MockRepository.GenerateMock<IUser>();
            int user_id = 8;
            int category_id = 1;
            RightType right = RightType.Read;
            catMock.Stub(x => x.Id).Return(category_id);
            userMock.Stub(x => x.Id).Return(user_id);
            //Act
            CategoryUserMapping actual = SUT.CategoryUser(catMock, userMock, right);
            //Assert
            CheckTableName(actual, "category_users");
            Assert.AreEqual(user_id, actual.UserId);
            CheckColumnName(actual, "UserId", "user_id");
            Assert.AreEqual(category_id, actual.CategoryId);
            CheckColumnName(actual, "CategoryId", "category_id");
            Assert.AreEqual(right, actual.Right);
            CheckColumnName(actual, "Right", "right");
        }

        [Test]
        public void ColumnClone_Test()
        {
            //Arrange
            IColumn columnMock = MockRepository.GenerateMock<IColumn>();
            ITableObject tableobjectMock = MockRepository.GenerateMock<ITableObject>();
            bool is_visible = true;
            bool is_tempedVisible = true;
            int table_id = 12;
            SqlType data_type = SqlType.DateTime;
            string original_name = "some name";
            int optimization_type = 12;
            bool is_empty = true;
            int max_length = 40;
            string const_value = "12;";
            string name = "MANDT";
            bool user_defined = true;
            int ordinal = 78;
            tableobjectMock.Stub(x => x.Id).Return(table_id);
            columnMock.Stub(x => x.IsVisible).Return(is_visible);
            columnMock.Stub(x => x.IsTempedHidden).Return(is_tempedVisible);
            columnMock.Stub(x => x.DataType).Return(data_type);
            columnMock.Stub(x => x.OriginalName).Return(original_name);
            columnMock.Stub(x => x.IsEmpty).Return(is_empty);
            columnMock.Stub(x => x.MaxLength).Return(max_length);
            columnMock.Stub(x => x.ConstValue).Return(const_value);
            columnMock.Stub(x => x.Name).Return(name);
            columnMock.Stub(x => x.UserDefined).Return(user_defined);
            columnMock.Stub(x => x.Ordinal).Return(ordinal);
            Assert.Inconclusive("explicit cast is required");
            //Column actual = SUT.ColumnClone(columnMock, tableobjectMock);
            //CheckTableName(actual,"columns");
            //Assert.AreEqual(is_visible,actual.IsVisible);
            //CheckColumnName(actual,"IsVisible","is_visible");
            //Assert.AreEqual(table_id,actual.TableId);
            //CheckColumnName(actual,"TableId","table_id");
            //Assert.AreEqual(original_name,actual.OriginalName);
            //CheckColumnName(actual,"OriginalName","original_name");
            ////optimization_type is not tested
            //Assert.AreEqual(data_type,actual.DataType);
            //CheckColumnName(actual,"DataType","data_type");
            //Assert.AreEqual(original_name,actual.OriginalName);
            //CheckColumnName(actual,"OriginalName","original_name");
            //Assert.AreEqual(optimization_type,actual.OptimizationType);
            //Assert.AreEqual(is_empty,actual.IsEmpty);
            //Assert.AreEqual(max_length,actual.MaxLength);
        }

        [Test]
        public void ColumnRoleMapping_Test()
        {
            //Arrange
            IColumn columnMock = MockRepository.GenerateMock<IColumn>();
            IRole roleMock = MockRepository.GenerateMock<IRole>();
            int user_id = 123;
            int table_id = 345;
            RightType right = RightType.Write;
            columnMock.Stub(x => x.Id).Return(table_id);
            roleMock.Stub(x => x.Id).Return(user_id);
            //Act
            ColumnRoleMapping actual = SUT.ColumnRoleMapping(right, columnMock, roleMock);
            //Assert
            CheckTableName(actual, "column_roles");
            Assert.AreEqual(user_id, actual.RoleId);
            CheckColumnName(actual, "RoleId", "role_id");
            Assert.AreEqual(table_id, actual.ColumnId);
            CheckColumnName(actual, "ColumnId", "column_id");
            Assert.AreEqual(right, actual.Right);
            CheckColumnName(actual, "Right", "right");
        }

        [Test]
        public void ColumnUserMapping_Test()
        {
            //Arrange
            IColumn columnMock = MockRepository.GenerateMock<IColumn>();
            IUser userMock = MockRepository.GenerateMock<IUser>();
            int user_id = 123;
            int table_id = 345;
            RightType right = RightType.Write;
            columnMock.Stub(x => x.Id).Return(table_id);
            userMock.Stub(x => x.Id).Return(user_id);
            //Act
            ColumnUserMapping actual = SUT.ColumnUserMapping(right, columnMock, userMock);
            //Assert
            CheckTableName(actual, "column_users");
            Assert.AreEqual(user_id, actual.UserId);
            CheckColumnName(actual, "UserId", "user_id");
            Assert.AreEqual(table_id, actual.ColumnId);
            CheckColumnName(actual, "ColumnId", "column_id");
            Assert.AreEqual(right, actual.Right);
            CheckColumnName(actual, "Right", "right");
        }

        [Test]
        public void IssueExtensionsClone_Text()
        {
            //Arrange
            IIssue issueMock = MockRepository.GenerateMock<IIssue>();
            ITableObject tableobjectMock = MockRepository.GenerateMock<ITableObject>();
            int ref_id = 12574;
            string command = "1sap_whatever";
            int obj_id = 0;
            bool split_value = true;
            bool index_value = true;
            bool sort_value = true;
            int flag = 44;
            bool _checked = true;
            issueMock.Stub(x => x.Id).Return(ref_id);
            issueMock.Stub(x => x.Command).Return(command);
            tableobjectMock.Stub(x => x.Id).Return(obj_id);
            issueMock.Stub(x => x.FilterTableObject).Return(tableobjectMock);
            issueMock.Stub(x => x.UseSplitValue).Return(split_value);
            issueMock.Stub(x => x.UseSortValue).Return(sort_value);
            issueMock.Stub(x => x.UseIndexValue).Return(index_value);
            issueMock.Stub(x => x.Flag).Return(flag);
            //issueMock.Stub(x=>x.Flag).Return()
            //Act

            IssueExtension actual = SUT.IssueExtensionClone(issueMock, ref_id);
            //Assert
            //Assert.Inconclusive("the checked flag should be ");
            CheckTableName(actual, "issue_extensions");
            Assert.AreEqual(command, actual.Command);
            CheckColumnName(actual, "Command", "command");
            Assert.AreEqual(ref_id, actual.RefId);
            CheckColumnName(actual, "RefId", "ref_id");
            Assert.AreEqual(obj_id, actual.TableObjectId);
            CheckColumnName(actual, "TableObjectId", "obj_id");
            Assert.AreEqual(split_value, actual.UseSplitValue);
            CheckColumnName(actual, "UseSplitValue", "split_value");
            Assert.AreEqual(index_value, actual.UseIndexValue);
            CheckColumnName(actual, "UseIndexValue", "index_value");
            Assert.AreEqual(sort_value, actual.UseSortValue);
            CheckColumnName(actual, "UseSortValue", "sort_value");
            Assert.AreEqual(flag, actual.Flag);
            CheckColumnName(actual, "Flag", "flag");
            Assert.Inconclusive("The checked flag is still unclear... clarify that");
            //Assert.AreEqual(_checked,actual.Checked);
        }

        [Test]
        public void LocalizedTextClone_Return_With_Correct_Ref_Id()
        {
            int refId = 12;
            var result = CloneLocalizedText(typeof (OptimizationGroupText), refId, "whatever", "country");
            Assert.AreEqual(refId, result.RefId);
        }

        [Test]
        public void LocalizedTextClone_Return_With_Correct_Text()
        {
            string text = "this is a localized text";
            var result = CloneLocalizedText(typeof (OptimizationGroupText), 12, text, "whatever");
            Assert.AreEqual(text, result.Text);
        }

        [Test]
        public void LocalizedTextClone_Return_With_CountryCode()
        {
            string countryCode = "countryCode";
            var result = CloneLocalizedText(typeof (OptimizationGroupText), 12, "text", countryCode);
            Assert.AreEqual(countryCode, result.CountryCode);
        }

        [Test]
        public void LocalizedTextClone_With_Null_String_Return_EmptyString()
        {
            string text = null;
            var result = CloneLocalizedText(typeof (OptimizationGroupText), 12, text, "whatever");
            Assert.AreEqual(String.Empty, result.Text);
        }

        [Test]
        public void OptimizationGroupClone_Test()
        {
            //Arrange
            IOptimizationGroup optgroupMock = MockRepository.GenerateMock<IOptimizationGroup>();
            OptimizationType type = OptimizationType.NotSet;
            optgroupMock.Stub(x => x.Type).Return(type);
            //Act
            OptimizationGroup actual = SUT.OptimizationGroupClone(optgroupMock);
            //Assert
            CheckTableName(actual, "optimization_groups");
            Assert.AreEqual(type, actual.Type);
            CheckColumnName(actual, "Type", "type");
        }

        [Test]
        public void OptimizationRoleClone_Test()
        {
            //Arrange
            IOptimization optMock = MockRepository.GenerateMock<IOptimization>();
            IRole roleMock = MockRepository.GenerateMock<IRole>();
            bool visibility = true;
            int optimization_id = 77;
            int role_id = 56;
            optMock.Stub(x => x.Id).Return(optimization_id);
            roleMock.Stub(x => x.Id).Return(role_id);
            //Act
            OptimizationRoleMapping actual = SUT.OptimizationRoleClone(optMock, roleMock, visibility);
            //Assert
            CheckTableName(actual, "optimization_roles");
            Assert.AreEqual(optimization_id, actual.OptimizationId);
            CheckColumnName(actual, "OptimizationId", "optimization_id");
            Assert.AreEqual(role_id, actual.RoleId);
            CheckColumnName(actual, "RoleId", "role_id");
            Assert.AreEqual(visibility, actual.Visible);
            CheckColumnName(actual, "Visible", "visible");
        }

        [Test]
        public void OptimizationUserMapping_Test()
        {
            //Arrange
            IOptimization optMock = MockRepository.GenerateMock<IOptimization>();
            IUser userMock = MockRepository.GenerateMock<IUser>();
            bool visibility = true;
            int optimization_id = 13;
            int user_id = 22;
            //Act
            optMock.Stub(x => x.Id).Return(optimization_id);
            userMock.Stub(x => x.Id).Return(user_id);
            OptimizationUserMapping actual = SUT.OptimizationUserMappingClone(optMock, userMock, visibility);
            //Assert
            CheckTableName(actual, "optimization_users");
            Assert.AreEqual(optimization_id, actual.OptimizationId);
            CheckColumnName(actual, "OptimizationId", "optimization_id");
            Assert.AreEqual(user_id, actual.UserId);
            CheckColumnName(actual, "UserId", "user_id");
            Assert.AreEqual(visibility, actual.Visible);
            CheckColumnName(actual, "Visible", "visible");
        }

        [Test]
        public void OrderAreaClone_Test()
        {
            //Arrange
            IOrderArea oaMock = MockRepository.GenerateMock<IOrderArea>();
            ITableObject toMock = MockRepository.GenerateMock<ITableObject>();
            int table_id = 12508;
            string index_value = "020";
            string split_value = "0100";
            string sort_value = "2000";
            long start = 12;
            long end = 144;
            toMock.Stub(x => x.Id).Return(table_id);
            oaMock.Stub(x => x.IndexValue).Return(index_value);
            oaMock.Stub(x => x.SplitValue).Return(split_value);
            oaMock.Stub(x => x.SortValue).Return(sort_value);
            oaMock.Stub(x => x.Start).Return(start);
            oaMock.Stub(x => x.End).Return(end);
            //Act
            OrderArea actual = SUT.OrderAreaClone(oaMock, toMock);
            //Assert
            CheckTableName(actual, "order_areas");
            Assert.AreEqual(table_id, actual.TableId);
            CheckColumnName(actual, "TableId", "table_id");
            Assert.AreEqual(index_value, actual.IndexValue);
            CheckColumnName(actual, "IndexValue", "index_value");
            Assert.AreEqual(split_value, actual.SplitValue);
            CheckColumnName(actual, "SplitValue", "split_value");
            Assert.AreEqual(sort_value, actual.SortValue);
            CheckColumnName(actual, "SortValue", "sort_value");
            Assert.AreEqual(start, actual.Start);
            CheckColumnName(actual, "Start", "start");
            Assert.AreEqual(end, actual.End);
            CheckColumnName(actual, "End", "end");
        }

        [Test]
        public void ParameterClone_Test()
        {
            //Arrange
            IParameter paramMOck = MockRepository.GenerateMock<IParameter>();
            ITableObject tableoMock = MockRepository.GenerateMock<ITableObject>();
            int issue_id = 2343;
            SqlType data_type = SqlType.Integer;
            string _default = "def";
            string name = "in_type";
            bool user_defined = true;
            int ordinal = 12;
            tableoMock.Stub(x => x.Id).Return(issue_id);
            paramMOck.Stub(x => x.DataType).Return(data_type);
            paramMOck.Stub(x => x.Default).Return(_default);
            paramMOck.Stub(x => x.Name).Return(name);
            paramMOck.Stub(x => x.UserDefined).Return(user_defined);
            paramMOck.Stub(x => x.Ordinal).Return(ordinal);
            //Act
            Parameter actual = SUT.ParameterClone(paramMOck, tableoMock);
            //Assert
            CheckTableName(actual, "parameter");
            Assert.AreEqual(issue_id, actual.IssueId);
            CheckColumnName(actual, "IssueId", "issue_id");
            Assert.AreEqual(data_type, actual.DataType);
            CheckColumnName(actual, "DataType", "data_type");
            Assert.AreEqual(_default, actual.Default);
            CheckColumnName(actual, "Default", "default");
            Assert.AreEqual(name, actual.Name);
            CheckColumnName(actual, "Name", "name");
            Assert.AreEqual(user_defined, actual.UserDefined);
            CheckColumnName(actual, "UserDefined", "user_defined");
            Assert.AreEqual(ordinal, actual.Ordinal);
            CheckColumnName(actual, "Ordinal", "ordinal");
        }

        [Test]
        public void ParameterValueTest()
        {
            //Arrange
            IParameterValue paramvalueMock = MockRepository.GenerateMock<IParameterValue>();
            int collection_id = 23;
            string value = "BABE";
            paramvalueMock.Stub(x => x.Value).Return(value);
            //Act
            ParameterValue actual = SUT.ParameterValue(paramvalueMock, collection_id, 0);
            //Assert
            CheckTableName(actual, "collections");
            Assert.AreEqual(collection_id, actual.CollectionId);
            CheckColumnName(actual, "CollectionId", "collection_id");
            Assert.AreEqual(value, actual.Value);
            CheckColumnName(actual, "Value", "value");
        }

        [Test]
        public void RelationClone_Test()
        {
            Assert.Inconclusive("casting operation...");
        }

        [Test]
        public void RoleClone_Test()
        {
            //Arrange
            IRole roleMock = MockRepository.GenerateMock<IRole>();
            string name = "Test";
            SpecialRights flags = SpecialRights.Super;
            roleMock.Stub(x => x.Name).Return(name);
            roleMock.Stub(x => x.Flags).Return(flags);
            //Act
            Role actual = SUT.RoleClone(roleMock);
            CheckTableName(actual, "roles");
            Assert.AreEqual(name, actual.Name);
            CheckColumnName(actual, "Name", "name");
            Assert.AreEqual(flags, actual.Flags);
            CheckColumnName(actual, "Flags", "flags");
        }

        [Test]
        public void TableRoleMapping_Test()
        {
            //Arrange
            ITableObject tableobMock = MockRepository.GenerateMock<ITableObject>();
            IRole roleMock = MockRepository.GenerateMock<IRole>();
            int user_id = 123;
            int table_id = 345;
            RightType right = RightType.Write;
            tableobMock.Stub(x => x.Id).Return(table_id);
            roleMock.Stub(x => x.Id).Return(user_id);
            //Act
            TableRoleMapping actual = SUT.TableRoleMappingClone(right, tableobMock, roleMock);
            //Assert
            CheckTableName(actual, "table_roles");
            Assert.AreEqual(user_id, actual.RoleId);
            CheckColumnName(actual, "RoleId", "role_id");
            Assert.AreEqual(table_id, actual.TableId);
            CheckColumnName(actual, "TableId", "table_id");
            Assert.AreEqual(right, actual.Right);
            CheckColumnName(actual, "Right", "right");
        }

        [Test]
        public void TableUserMapping_Test()
        {
            //Arrange
            ITableObject tableobMock = MockRepository.GenerateMock<ITableObject>();
            IUser userMock = MockRepository.GenerateMock<IUser>();
            int user_id = 123;
            int table_id = 345;
            RightType right = RightType.Write;
            tableobMock.Stub(x => x.Id).Return(table_id);
            userMock.Stub(x => x.Id).Return(user_id);
            //Act
            TableUserMapping actual = SUT.TableUserMappingClone(right, tableobMock, userMock);
            //Assert
            CheckTableName(actual, "table_users");
            Assert.AreEqual(user_id, actual.UserId);
            CheckColumnName(actual, "UserId", "user_id");
            Assert.AreEqual(table_id, actual.TableId);
            CheckColumnName(actual, "TableId", "table_id");
            Assert.AreEqual(right, actual.Right);
            CheckColumnName(actual, "Right", "right");
        }

        [Test]
        public void UserCloneTest()
        {
            Assert.Inconclusive("Casting required, not tested");
        }

        [Test]
        public void UserRoleClone_Test()
        {
            //Arrange
            IUser userMock = MockRepository.GenerateMock<IUser>();
            IRole roleMock = MockRepository.GenerateMock<IRole>();
            int ordinal = 12;
            int user_id = 45;
            int role_id = 67;
            userMock.Stub(x => x.Id).Return(user_id);
            roleMock.Stub(x => x.Id).Return(role_id);
            //Act
            UserRoleMapping actual = SUT.UserRoleMappingClone(userMock, roleMock, ordinal);
            //Assert
            CheckTableName(actual, "user_roles");
            Assert.AreEqual(user_id, actual.UserId);
            CheckColumnName(actual, "UserId", "user_id");
            Assert.AreEqual(role_id, actual.RoleId);
            CheckColumnName(actual, "RoleId", "role_id");
            Assert.AreEqual(ordinal, actual.Ordinal);
            CheckColumnName(actual, "Ordinal", "ordinal");
        }
    }
}