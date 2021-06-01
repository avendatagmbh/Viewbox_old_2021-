using System.Collections.Generic;
using SystemDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class RelationModelTest : BaseModelTest<RelationModel>
    {
        public override void CreateModel()
        {
            Model = new RelationModel("", new List<string>(), Context.TestTable,
                                      new List<IRelation>(Context.TestTable.Relations[Context.TestColumn]));
        }
    }
}