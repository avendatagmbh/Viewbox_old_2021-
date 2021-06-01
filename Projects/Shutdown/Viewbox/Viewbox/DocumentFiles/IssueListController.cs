using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SystemDb;
using Viewbox.Properties;
using Viewbox.Models;
using ViewboxDb;

namespace Viewbox.Controllers {

    [Authorize, ViewboxFilter]
    public class IssueListController : TableObjectControllerBase {

        public ActionResult Index(int id = -1, string search = null, int sortColumn = 0, ViewboxDb.SortDirection direction = ViewboxDb.SortDirection.Ascending, 
                                    bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showHidden = false) {
            var model = new Models.IssueList(id, search, showHidden);
            model.CurrentPage = page;
            model.PerPage = size;

            var list = model.GetIssueList(showHidden, sortColumn, direction);

            model.SortColumn = sortColumn;
            model.Direction = direction;
            model.ShowHidden = showHidden;

            model.Issues = list.GetRange(model.From, Math.Min(list.Count - model.From, model.PerPage));
            model.Count = list.Count;

            if (catRefresh) return PartialView("_TableObjectsPartial", model);

            return ResultView(model, json);
        }

        public PartialViewResult CategoryTabs(int id = -1, string search = null, int sortColumn = 0,
                ViewboxDb.SortDirection direction = ViewboxDb.SortDirection.Ascending, int page = 0, int size = 25, bool showHidden = false) {
            var model = new Models.IssueList(id, search, showHidden);
            model.CurrentPage = page;
            model.PerPage = size;

            var list = model.GetIssueList(showHidden, sortColumn, direction);

            model.SortColumn = sortColumn;
            model.Direction = direction;
            model.Count = list.Count;
            model.ShowHidden = showHidden;

            return PartialView("_CategoryTabsPartial", model);
        }

        public ActionResult Sort(int id = -1, string search = null, int sortColumn = 0,
            ViewboxDb.SortDirection direction = ViewboxDb.SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showHidden = false) {

            return Index(id, search, sortColumn, direction, json, page, size, catRefresh, showHidden);
        }

        public ActionResult DeleteIssue(int id, string search = null, int sortColumn = 0,
            ViewboxDb.SortDirection direction = ViewboxDb.SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showHidden = false) {

            ViewboxSession.DeleteIssue(id);

            if (ViewboxSession.Issues.Count == 0)
                return null;

            return Index(-1, search, sortColumn, direction, json, 0, size, catRefresh, showHidden);
        }

        public ActionResult HideTableObject(int id = -1, string search = null, int sortColumn = 0,
            ViewboxDb.SortDirection direction = ViewboxDb.SortDirection.Ascending, bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showHidden = false) {

                return Index(id, search, sortColumn, direction, json, page, size, catRefresh, showHidden);
        }

        public void ChangeTableOrder(int id, int ordinal) {
            var tobj = id < 0 ? ViewboxSession.TempTableObjects[id].Table : ViewboxSession.TableObjects[id];
            tobj.Ordinal = ordinal;
            var tableOrder = String.Join(",", from i in ViewboxSession.Issues orderby i.Ordinal select i.Id);
            ViewboxApplication.Database.SystemDb.UpdateTableObjectOrder(ViewboxSession.User, TableType.Issue, tableOrder);
        }

        public PartialViewResult GetConfirmationDialog() {
            return PartialView("_DialogPartial", new DialogModel {
                DialogType = DialogModel.Type.Info,
                Title = Resources.ExecuteIssueTitle,
                Content = String.Format(Resources.Proceed, Resources.ExecuteIssueTitle),
                Buttons = new List<DialogModel.Button> {
                    new DialogModel.Button { Caption = Resources.Yes, Data = true.ToString() },
                    new DialogModel.Button { Caption = Resources.No, Data = false.ToString() }
                }
            });
        }

        public PartialViewResult DeleteConfirmationDialog() {
            return PartialView("_DialogPartial", new DialogModel {
                DialogType = DialogModel.Type.Info,
                Title = Resources.DeleteIssueTitle,
                Content = String.Format(Resources.Proceed, Resources.DeleteIssueTitle),
                Buttons = new List<DialogModel.Button> {
                    new DialogModel.Button { Caption = Resources.Yes, Data = true.ToString() },
                    new DialogModel.Button { Caption = Resources.No, Data = false.ToString() }
                }
            });
        }

        public PartialViewResult EditIssueDialog(int id) {
            var issue = ViewboxSession.Issues[id];
            var inputs = new List<Tuple<string, string, string, string, int>> { 
                new Tuple<string, string, string, string, int>(Resources.Issue, "text", "tableName", issue.GetDescription(ViewboxSession.Language), 256) 
            };
            var hidden = new List<Tuple<string, string, string>> { new Tuple<string, string, string>("countryCode", ViewboxSession.Language.CountryCode, "countryCode") };

            int j = 0;
            foreach (var l in ViewboxApplication.Languages) {
                hidden.Add(new Tuple<string, string, string>("descriptions[" + j + "].countryCode", l.CountryCode, ""));
                hidden.Add(new Tuple<string, string, string>("descriptions[" + j + "].descriptions", issue.GetDescription(l), l.CountryCode + "_tableName"));
                j++;
            }

            int i = 1;
            foreach (var p in issue.Parameters) {
                inputs.Add(new Tuple<string, string, string, string, int>(Resources.Parameter + " " + i, "text", p.Name, p.GetDescription(), 256));

                j = 0;
                foreach (var l in ViewboxApplication.Languages) {
                    hidden.Add(new Tuple<string, string, string>("descriptions[" + j + "].descriptions", p.GetDescription(l), l.CountryCode + "_" + p.Name));
                    j++;
                }

                i++;
            }

            var dialog = new DialogModel {
                Title = Resources.UserDefinedIssueDescriptionsCaption,
                Content = ViewboxApplication.Languages.Count() > 1 ? Resources.UserDefinedIssueDescriptionsTextLanguages : Resources.UserDefinedIssueDescriptionsText,
                DialogType = DialogModel.Type.Info,
                Inputs = inputs,
                HiddenFields = hidden,
                Buttons = new List<Models.DialogModel.Button> {
                        new Models.DialogModel.Button { Caption = Resources.Cancel, Data = false.ToString() },
                        new Models.DialogModel.Button { Caption = Resources.Submit, Data = true.ToString() }
                    }
            };

            if (ViewboxApplication.Languages.Count() > 1)
                dialog.Select = new Tuple<string, string, List<string>, List<string>, int>(
                    Resources.LanguageSelection,
                    "language",
                    new List<string>(from l in ViewboxApplication.Languages select l.LanguageName),
                    new List<string>(from l in ViewboxApplication.Languages select l.CountryCode),
                    new List<ILanguage>(ViewboxApplication.Languages).IndexOf(ViewboxSession.Language));

            return PartialView("_ExtendedDialogPartial", dialog);
        }

        public ActionResult EditIssue(int id, DescriptionCollection descriptions, string search = null, int sortColumn = 0, ViewboxDb.SortDirection direction = ViewboxDb.SortDirection.Ascending, 
                                        bool json = false, int page = 0, int size = 25, bool catRefresh = false, bool showHidden = false) {
            ViewboxApplication.Database.EditIssue(ViewboxSession.Issues[id], ViewboxApplication.Languages, descriptions);

            return Index(-1, search, sortColumn, direction, json, 0, size, catRefresh, showHidden);
        }
    }
}
