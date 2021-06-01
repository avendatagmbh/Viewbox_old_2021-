using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class DatabaseUpgradeFinishedModelTest : BaseModelTest<DatabaseUpgradeFinishedModel>
    {
        public override void CreateModel()
        {
            Model = new DatabaseUpgradeFinishedModel("");
            Model = new DatabaseUpgradeFinishedModel("error");
        }
    }
}