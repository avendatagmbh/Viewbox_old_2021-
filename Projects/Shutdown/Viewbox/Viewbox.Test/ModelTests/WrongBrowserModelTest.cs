using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class WrongBrowserModelTest : BaseModelTest<WrongBrowserModel>
    {
        public override void CreateModel()
        {
            Model = new WrongBrowserModel();
        }
    }
}