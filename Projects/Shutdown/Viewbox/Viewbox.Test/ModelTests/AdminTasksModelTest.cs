using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class AdminTasksModelTest : BaseModelTest<AdminTasksModel>
    {
        public override void CreateModel()
        {
            Model = new AdminTasksModel();
        }
    }
}