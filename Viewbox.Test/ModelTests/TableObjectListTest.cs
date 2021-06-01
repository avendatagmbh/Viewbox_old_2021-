using SystemDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class TableObjectListTest : BaseModelTest<TableObjectList>
    {
        public override void CreateModel()
        {
            ViewboxSession.EnableRightsMode(Context.TestUser.Id, CredentialType.User);
            base.CreateModel();
        }
    }
}