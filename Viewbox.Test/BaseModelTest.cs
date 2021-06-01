using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test
{
    public class BaseModelTest<T> : BaseTest where T : BaseModel
    {
        protected T Model;

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            CreateModel();
        }

        public virtual void CreateModel()
        {
            if (Model != null) return;
            Model = Activator.CreateInstance<T>();
            Assert.IsNotNull(Model);
        }

        [TestMethod]
        public void TestProperties()
        {
            foreach (var prop in typeof (T).GetProperties())
            {
                try
                {
                    if (!prop.GetIndexParameters().Any())
                        prop.GetValue(Model, null);
                }
                catch (NotImplementedException)
                {
                }
            }
        }
    }
}