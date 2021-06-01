using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class ControlCenterModelTest : BaseModelTest<ControlCenterModel>
    {
        public override void CreateModel()
        {
            Model = new ControlCenterModel();
        }
    }
}