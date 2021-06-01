using SystemDb;
using SystemDb.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class ExportModelTest : BaseModelTest<ExportModel>
    {
        public override void CreateModel()
        {
            Model = new ExportModel(TableType.Issue, new CategoryCollection(), new CategoryCollection());
        }
    }
}