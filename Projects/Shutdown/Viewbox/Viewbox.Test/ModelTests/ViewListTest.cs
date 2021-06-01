using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class ViewListTest : BaseModelTest<ViewList>
    {
        public override void CreateModel()
        {
            Model = new ViewList();
            Model.Views = Model.GetListFromSessionViews();
        }
    }
}