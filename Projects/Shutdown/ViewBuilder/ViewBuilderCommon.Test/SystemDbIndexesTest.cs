using System;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using SystemDb;
using DbAccess;
using DbAccess.DbSpecific.ADO;
using DbAccess.Structures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SystemDb.Internal;
using Database = DbAccess.DbSpecific.MySQL.Database;

namespace ViewBuilderCommon.Test
{
    [TestClass]
    public class SystemDbIndexesTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            SystemDb.SystemDb systemDb = new SystemDb.SystemDb();

            var tableObject = new Table() { TableName = "TestTable" };
            tableObject.Columns.Add(new Column(){ Name = "TestColumn1"});

            PrivateObject _objects = new PrivateObject(systemDb);
            (_objects.GetField("_objects") as ITableObjectCollection).Add(tableObject);
            var database= MockRepository.GenerateMock<Database>(new DbConfig("MySQL"));
            // todo: mock CreateDatabaseIfNotExists properly
//            database.AssertWasCalled(database1 => database1.CreateDatabaseIfNotExists(""));
//            systemDb.PopulateWithIndexes();

        }
    }
}
