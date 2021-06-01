using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Viewbox.Test
{
    [TestClass]
    public class BaseTest
    {
        protected TestContext Context;

        [TestInitialize]
        public virtual void Init()
        {
            Context = TestContext.GetInstance();
            Context.InitSession();
        }
    }
}