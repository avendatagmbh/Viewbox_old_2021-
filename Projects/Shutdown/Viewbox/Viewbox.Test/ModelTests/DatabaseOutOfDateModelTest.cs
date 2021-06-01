using SystemDb.Upgrader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class DatabaseOutOfDateModelTest : BaseModelTest<DatabaseOutOfDateModel>
    {
        public override void CreateModel()
        {
            Model = new DatabaseOutOfDateModel(new DatabaseOutOfDateInformation(""));
        }
    }
}