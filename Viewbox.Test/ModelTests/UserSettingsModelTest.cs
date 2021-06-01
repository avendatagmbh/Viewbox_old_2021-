using SystemDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class UserSettingsModelTest : BaseModelTest<UserSettingsModel>
    {
        public override void CreateModel()
        {
            ViewboxSession.EnableRightsMode(Context.TestUser.Id, CredentialType.User);
            base.CreateModel();
        }
    }
}