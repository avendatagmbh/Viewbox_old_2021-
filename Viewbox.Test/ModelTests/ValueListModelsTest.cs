using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class ValueListModelsTest : BaseModelTest<ValueListModels>
    {
        public override void CreateModel()
        {
            Model = new ValueListModels(Context.TestParameter.Id, 0, Context.TestOptimization);
        }
    }
}