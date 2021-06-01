using System.Collections;
using System.Linq;
using System.Reflection;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using DbAccess.Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DbAccess.Test
{
    
    
    /// <summary>
    ///Dies ist eine Testklasse für "DbMappingBaseTest" und soll
    ///alle DbMappingBaseTest Komponententests enthalten.
    ///</summary>
    [TestClass()]
    public class DbMappingBaseTest {


        private TestContext testContextInstance;

        /// <summary>
        ///Ruft den Testkontext auf, der Informationen
        ///über und Funktionalität für den aktuellen Testlauf bietet, oder legt diesen fest.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Zusätzliche Testattribute
        // 
        //Sie können beim Verfassen Ihrer Tests die folgenden zusätzlichen Attribute verwenden:
        //
        //Mit ClassInitialize führen Sie Code aus, bevor Sie den ersten Test in der Klasse ausführen.
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Mit ClassCleanup führen Sie Code aus, nachdem alle Tests in einer Klasse ausgeführt wurden.
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Mit TestInitialize können Sie vor jedem einzelnen Test Code ausführen.
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Mit TestCleanup können Sie nach jedem einzelnen Test Code ausführen.
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        private DbConfig DbConfig = new DbConfig("MySQL"){ Hostname = "localhost", Username = "root", Password = "avendata", DbName = "DbAccess_unit_tests"};
        /// <summary>
        ///Ein Test für "FastLoad"
        ///</summary>
        //public List<T> FastLoadTestHelper<T>(IDatabase conn, string filter = null) where T:new(){
        //    List<T> expected = filter == null ? conn.DbMapping.Load<T>() : conn.DbMapping.Load<T>(filter);
        //    List<T> actual = filter == null ? conn.DbMapping.FastLoad<T>() : conn.DbMapping.FastLoad<T>(filter);
        //    ListEqualityTest<T>(expected, actual);
        //    return actual;
        //}

        public void ListEqualityTest<T>(IEnumerable<T> expected, IEnumerable<T> actual  ) {
            Assert.AreEqual(expected.Count(), actual.Count());
            IEnumerator<T> actualEnumerator = actual.GetEnumerator();

            foreach(var expectedChild in expected){
                actualEnumerator.MoveNext();
                TestEquality(expectedChild, actualEnumerator.Current);
            }
        }

        public void TestEquality<T>(T expected, T actual) {
            if (expected == null || actual == null) {
                Assert.AreEqual(expected, actual);
                return;
            }

            MethodInfo checkListEqualityMethod = typeof(DbMappingBaseTest).GetMethod("CheckListEquality");
            MethodInfo testEqualityMethod = typeof(DbMappingBaseTest).GetMethod("TestEquality");

            foreach (var property in typeof(T).GetProperties()) {
                if (property.GetCustomAttributes(typeof(DbColumnAttribute), true).Length > 0) {
                    if (property.PropertyType.IsArray) {
                        Type childType = null;
                        foreach (object element in (Array)property.GetValue(expected, null)) {
                            childType = element.GetType();
                            break;
                        }
                        if (childType == null)
                            foreach (object element in (Array)property.GetValue(actual, null)) {
                                childType = element.GetType();
                                break;
                            }
                        if (childType != null) {
                            MethodInfo genericMethod = checkListEqualityMethod.MakeGenericMethod(new Type[] { childType });
                            genericMethod.Invoke(this,
                                                 new object[2] {
                                                                       property.GetValue(expected, null),
                                                                       property.GetValue(actual, null)
                                                                   });
                        }
                    } else if (property.PropertyType.IsValueType || property.PropertyType.ToString() == "System.String") {
                        Assert.AreEqual(property.GetValue(expected, null), property.GetValue(actual, null));
                    }
                    else {
                        MethodInfo genericMethod = testEqualityMethod.MakeGenericMethod(new Type[] { property.PropertyType });
                        genericMethod.Invoke(this,
                                             new object[2] {
                                                                       property.GetValue(expected, null),
                                                                       property.GetValue(actual, null)
                                                                   });
                    }
                }
            }
        }

        public void CheckListEquality<T>(IEnumerable<T> expected, IEnumerable<T> actual) {
            IEnumerator<T> actualEnumerator = actual.GetEnumerator();
            foreach (var expectedChild in expected) {
                actualEnumerator.MoveNext();
                Assert.AreEqual(expectedChild, actualEnumerator.Current);
            }
        }

        [TestMethod()]
        public void FastLoadTest() {
            using (IDatabase conn = InitUnitTestDb()) {
                //List<TestParent> actual = FastLoadTestHelper<TestParent>(conn);
                List<TestParent> actual = conn.DbMapping.Load<TestParent>();
                ListEqualityTest(_testParents, actual);
            }
        }

        //Has been thouroughly tested
        //DbConfig viewboxDatabase = new DbConfig("MySQL") { Hostname = "dbstrabag", Username = "root", Password = "avendata", DbName = "viewbox"};
        //[TestMethod()]
        //public void FastLoadTest_orderAreas() {
        //    using (IDatabase conn = ConnectionManager.CreateConnection(viewboxDatabase)) {
        //        conn.Open();
        //        FastLoadTestHelper<User>(conn);
        //        FastLoadTestHelper<Language>(conn);
        //        FastLoadTestHelper<Role>(conn);
        //        FastLoadTestHelper<OptimizationGroup>(conn);
        //        FastLoadTestHelper<OptimizationGroupText>(conn);
        //        FastLoadTestHelper<Optimization>(conn);
        //        FastLoadTestHelper<OptimizationText>(conn);
        //        FastLoadTestHelper<OptimizationRoleMapping>(conn);
        //        FastLoadTestHelper<OptimizationUserMapping>(conn);
        //        FastLoadTestHelper<Category>(conn);
        //        FastLoadTestHelper<CategoryText>(conn);
        //        FastLoadTestHelper<CategoryRoleMapping>(conn);
        //        FastLoadTestHelper<CategoryUserMapping>(conn);
        //        FastLoadTestHelper<Issue>(conn, "type = " + (int)TableType.Issue);
        //        FastLoadTestHelper<View>(conn, "type = " + (int)TableType.View);
        //        FastLoadTestHelper<Table>(conn, "type = " + (int)TableType.Table);
        //    }
        //}


        List<TestParent> _testParents = new List<TestParent>() {
                                                                   new TestParent(){EnumField = DbColumnTypes.DbDate, ByteField = new byte[4]{128, 15, 27, 49}, LongField = 2132131231322323213, 
                                                                   IntegerField = 155, StringField = "ABCXYZ", 
                                                                   },
                                                                   new TestParent(){EnumField = DbColumnTypes.DbLongText, ByteField = new byte[4]{129, 11, 27, 49}, LongField = 21321322323213, 
                                                                   IntegerField = Int32.MaxValue, StringField = "2131ABCXYZ", 
                                                                   }
                                                               };

        private IDatabase InitUnitTestDb() {
            if (_testParents[0].Children.Count == 0) {
                _testParents[0].Children.Add(new TestChild() {Parent = _testParents[0], StringValue = "123abcd"});
                _testParents[0].Children.Add(new TestChild() {Parent = _testParents[0], StringValue = "abcd"});
            }
            _testParents[1].Child = new TestChild() {Parent = _testParents[1], StringValue = "test"};

            DbConfig configNoDatabase = (DbConfig) DbConfig.Clone();
            configNoDatabase.DbName = string.Empty;
            using(IDatabase conn = ConnectionManager.CreateConnection(configNoDatabase)) {
                conn.Open();
                if (!conn.DatabaseExists(DbConfig.DbName)) {
                    conn.CreateDatabase(DbConfig.DbName);
                }
            }
            IDatabase resultConn = ConnectionManager.CreateConnection(DbConfig);
            resultConn.Open();

            if (!resultConn.TableExists(resultConn.DbMapping.GetTableName<TestParent>()) || !resultConn.TableExists(resultConn.DbMapping.GetTableName<TestChild>())) {
                resultConn.DropTableIfExists(resultConn.DbMapping.GetTableName<TestParent>());
                resultConn.DropTableIfExists(resultConn.DbMapping.GetTableName<TestChild>());

                resultConn.DbMapping.CreateTableIfNotExists<TestParent>();
                resultConn.DbMapping.CreateTableIfNotExists<TestChild>();

                resultConn.DbMapping.Save(typeof(TestParent), _testParents);
                foreach (var parent in _testParents) {
                    resultConn.DbMapping.Save(typeof (TestChild), parent.Children);
                }
            }else {
                int id = 1;
                int childId = 1;
                foreach (var parent in _testParents) {
                    parent.Id = id++;
                    if (parent.Child != null)
                        parent.Child.Id = childId++;
                }
                foreach (var parent in _testParents) {
                    foreach (var child in parent.Children)
                        child.Id = childId++;
                }
                _testParents[1].Child.Parent = null;
            }

            return resultConn;
        }
    }

    [DbTable("table_parent")]
    class TestParent {
        public TestParent() {
            Children = new List<TestChild>();
        }

        [DbColumn("id"), DbPrimaryKey]
        public int Id { get; set; }

        [DbCollection("Parent")]
        public List<TestChild> Children { get; set; }

        [DbColumn("child")]
        public TestChild Child { get; set; }

        [DbColumn("integer_field")]
        public int IntegerField { get; set; }

        [DbColumn("long_field")]
        public long LongField { get; set; }

        [DbColumn("string_field")]
        public string StringField { get; set; }

        [DbColumn("enum_field")]
        public DbColumnTypes EnumField { get; set; }

        [DbColumn("byte_field")]
        public byte[] ByteField { get; set; }
    }

    [DbTable("table_child")]
    class TestChild {
        [DbColumn("id"), DbPrimaryKey]
        public int Id { get; set; }

        [DbColumn("parent",IsInverseMapping = true)]
        public TestParent Parent { get; set; }

        [DbColumn("string_value")]
        public string StringValue { get; set; }
    }
}
