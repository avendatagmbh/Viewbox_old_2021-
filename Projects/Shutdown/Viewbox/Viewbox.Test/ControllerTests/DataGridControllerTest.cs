using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SystemDb;
using SystemDb.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Viewbox.Controllers;
using ViewboxDb;

namespace Viewbox.Test.ControllerTests
{
    [TestClass]
    public class DataGridControllerTest : BaseControllerTest<DataGridController>
    {
        [TestMethod]
        public void TestOptimizationDeleteTest()
        {
            //Controller.TestOptimizationDelete();
        }

        [TestMethod]
        public void TableIndexesTest()
        {
            var tableObject = Context.TestTable;
            if (tableObject != null)
            {
                ActionResult result = Controller.TableIndexes(false, tableObject.Id);
                Assert.IsNotNull(result);
                result = Controller.TableIndexes(true, tableObject.Id);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void IndexTest()
        {
            var tableObject = Context.TestTable;
            if (tableObject != null)
            {
                ActionResult result = Controller.Index(tableObject.Id, -1, 0, 0, false, true);
                Assert.IsNotNull(result);
                result = Controller.Index(tableObject.Id, -1, 0, 0, true, false);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void UpdateIsArchiveCheckedTest()
        {
            var tableObject = Context.TestTable;
            if (tableObject != null)
            {
                Controller.UpdateIsArchiveChecked(tableObject.Id, !tableObject.IsArchived);
                Controller.UpdateIsArchiveChecked(tableObject.Id, !tableObject.IsArchived);
            }
        }

        [TestMethod]
        public void UpdateColumnTest()
        {
            var column = Context.TestColumn;
            if (column != null)
            {
                Controller.UpdateColumn(column.Id, !column.IsVisible);
                Controller.UpdateColumn(column.Id, column.IsVisible);
            }
        }

        [TestMethod]
        public void UpdateColumnsTest()
        {
            var tableObject = Context.TestTable;
            if (tableObject != null)
            {
                Controller.UpdateColumns(tableObject.Id, false);
                Controller.UpdateColumns(tableObject.Id, true);
            }
        }

        [TestMethod]
        public void ColumnListTest()
        {
            var tableObject = Context.TestTable;
            if (tableObject != null)
            {
                ActionResult result = Controller.ColumnList(tableObject.Id);
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        public void TableObjectListTest()
        {
            ActionResult result = Controller.TableObjectList(TableType.View);
            Assert.IsNotNull(result);
            result = Controller.TableObjectList(TableType.Issue);
            Assert.IsNotNull(result);
            result = Controller.TableObjectList(TableType.Table);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AJobThatsRunningLongAndDoesNothingTest()
        {
            ActionResult result = Controller.AJobThatsRunningLongAndDoesNothing();
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ValidateIssueParameterInputTest()
        {
            // In IssueTests
        }

        [TestMethod]
        public void ExecuteIssueTest()
        {
            // In IssueTests
        }

        [TestMethod]
        public void SortTest()
        {
            ActionResult result = Controller.Sort(Context.TestColumn.Id, SortDirection.Descending);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Group2Test()
        {
            ActionResult result = Controller.Group2(Context.TestTable.Id, new List<int> {Context.TestColumn.Id},
                                                    new AggregationCollection
                                                        {
                                                            new Aggregation
                                                                {
                                                                    agg = AggregationFunction.Avg,
                                                                    cid = Context.TestColumn.Id
                                                                }
                                                        }, "");
            Assert.IsNotNull(result);
            result = Controller.Group2(Context.TestTable.Id, new List<int> {Context.TestColumn.Id},
                                       new AggregationCollection
                                           {
                                               new Aggregation {agg = AggregationFunction.Avg, cid = Context.TestColumn.Id}
                                           }, Context.TestColumn.Name + " IS NULL");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GroupTest()
        {
            ActionResult result = Controller.Group(Context.TestTable.Id, new List<int> {Context.TestColumn.Id},
                                                   new List<AggregationFunction> {AggregationFunction.Avg}, "");
            Assert.IsNotNull(result);
            result = Controller.Group(Context.TestTable.Id, new List<int> {Context.TestColumn.Id},
                                      new List<AggregationFunction> {AggregationFunction.Avg},
                                      Context.TestColumn.Name + " IS NULL");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RelationsTest()
        {
            ActionResult result = Controller.Relations(Context.TestTable.Id, Context.TestTable2.Id,
                                                       new List<JoinColumns>
                                                           {
                                                               new JoinColumns
                                                                   {
                                                                       Column1 = Context.TestColumn.Id,
                                                                       Column2 = Context.TestColumn2.Id,
                                                                       Direction = SortDirection.Ascending
                                                                   }
                                                           });
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DeleteRelationsTest()
        {
            //Controller.DeleteRelations();
        }

        [TestMethod]
        public void JoinTest()
        {
            ActionResult result = Controller.Join(Context.TestTable.Id, Context.TestTable2.Id,
                                                  new JoinColumnsCollection
                                                      {
                                                          new JoinColumns
                                                              {
                                                                  Column1 = Context.TestColumn.Id,
                                                                  Column2 = Context.TestColumn2.Id,
                                                                  Direction = SortDirection.Ascending
                                                              }
                                                      }, "", "",
                                                  JoinType.Inner);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SortAndFilterTest()
        {
            ActionResult result = Controller.SortAndFilter(Context.TestTable.Id,
                                                           new SortCollection
                                                               {
                                                                   new Sort
                                                                       {
                                                                           cid = Context.TestColumn.Id,
                                                                           dir = SortDirection.Ascending
                                                                       }
                                                               }, "",
                                                           new DescriptionCollection());
            Assert.IsNotNull(result);
            result = Controller.SortAndFilter(Context.TestTable.Id, null, "test", new DescriptionCollection());
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void RefreshFilterViewTest()
        {
            ActionResult result = Controller.RefreshFilterView(Context.TestTable.Id,
                                                               Context.TestColumn.Name + " IS NULL");
            Assert.IsNotNull(result);
            result = Controller.RefreshFilterView(Context.TestTable.Id, "");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void FilterTest()
        {
            ActionResult result = Controller.Filter(Context.TestTable.Id, Context.TestColumn.Id,
                                                    Context.TestColumn.Name + " IS NULL", new SortCollection());
            Assert.IsNotNull(result);
            result = Controller.Filter(Context.TestTable.Id, Context.TestColumn.Id, "", new SortCollection());
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SearchColumnsTest()
        {
            ActionResult result = Controller.SearchColumns(Context.TestTable.Id, "test", "_ViewOptionsPartial");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UpdateColumnOrderTest()
        {
            StringBuilder order = new StringBuilder();
            foreach (Column column in ViewboxSession.Columns.Where(w => w.Table.Id == Context.TestTable.Id))
            {
                if (order.Length != 0)
                    order.Append(",");
                order.Append(column.Id);
            }
            ActionResult result = Controller.UpdateColumnOrder(Context.TestTable.Id, Context.TestColumn.Id,
                                                               order.ToString());
            Assert.IsNotNull(result);
            result = Controller.UpdateColumnOrder(Context.TestTable.Id, Context.TestColumn.Id, order.ToString());
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TransactionNumberChangeTest()
        {
            ActionResult result = Controller.TransactionNumberChange(Context.TestTable.Id, 1);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ShowIssueDescriptionDialogTest()
        {
            ActionResult result = Controller.ShowIssueDescriptionDialog(Context.TestTable.Id,
                                                                        Context.TestColumn.Name + " IS NULL");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ResetColumnOrderTest()
        {
            ActionResult result = Controller.ResetColumnOrder(Context.TestTable.Id);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ShowRelationPartialTest()
        {
            ActionResult result = Controller.ShowRelationPartial(Context.TestColumn.Id, "", "");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ShowRelationPartialNewTest()
        {
            ActionResult result = Controller.ShowRelationPartialNew(Context.TestColumn.Id, 1);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SaveColumnSizesTest()
        {
            //Controller.SaveColumnSizes();
        }

        [TestMethod]
        public void GenerateNeededTableTest()
        {
            //Controller.GenerateNeededTable();
        }

        [TestMethod]
        public void GenerateConfirmationDialogTest()
        {
            ActionResult result = Controller.GenerateConfirmationDialog("test", "test");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetDataLoadingTest()
        {
            ActionResult result = Controller.GetDataLoading();
            Assert.IsNotNull(result);
        }

// GenerateCode
    }
}