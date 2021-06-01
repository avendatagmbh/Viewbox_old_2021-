using SystemDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class TableListTest : BaseModelTest<TableList>
    {
        public override void CreateModel()
        {
            ViewboxSession.EnableRightsMode(Context.TestUser.Id, CredentialType.User);
            Model = new TableList();
            Model.Tables = Model.GetListFromSessionTables();
        }
    }
}