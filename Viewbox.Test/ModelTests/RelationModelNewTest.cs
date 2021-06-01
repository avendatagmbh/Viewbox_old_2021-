using System.Collections.Generic;
using SystemDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class RelationModelNewTest : BaseModelTest<RelationModelNew>
    {
        public override void CreateModel()
        {
            Model = new RelationModelNew(Context.TestColumn, 0,
                                         new List<IRelation>(Context.TestTable.Relations[Context.TestColumn]));
        }
    }
}