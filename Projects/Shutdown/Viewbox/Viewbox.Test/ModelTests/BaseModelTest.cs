using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class BaseModelTest : BaseModelTest<BaseModel>
    {
        public override void CreateModel()
        {
            Model = new BaseModel();
        }
    }
}