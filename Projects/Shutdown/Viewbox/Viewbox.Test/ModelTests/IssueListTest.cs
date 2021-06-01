using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Models;

namespace Viewbox.Test.ModelTests
{
    [TestClass]
    public class IssueListTest : BaseModelTest<IssueList>
    {
        public override void CreateModel()
        {
            Model = new IssueList();
            IEnumerable<int> favIds = ViewboxApplication.Database.SystemDb.GetUserFavoriteIssues(ViewboxSession.User);
            int fullTableListCount;
            Model.Issues = Model.GetListFromSessionIssues(favIds, true, "", ViewboxSession.Language,
                                                          out fullTableListCount);
        }
    }
}