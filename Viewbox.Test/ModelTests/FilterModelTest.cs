using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class FilterModelTest : BaseModelTest<FilterModel>
    {
        public override void CreateModel()
        {
            Model = FilterModelFactory.GetFilterFromOp(Operator.And);
            Model = FilterModelFactory.GetFilterModel(Model.Filter, null);
        }
    }
}