using System.Web.Optimization;

namespace Viewbox
{
	public class BundleConfig
	{
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-2.1.1.min.js", "~/Scripts/jquery.form.min.js", "~/Scripts/jquery-ui.min.js", "~/Scripts/jquery-ui-i18n.js"));
			bundles.Add(new ScriptBundle("~/bundles/viewbox-js").Include("~/Scripts/slide.js", "~/Scripts/Viewbox.js", "~/Scripts/Search.js", "~/Scripts/ListControls.js", "~/Scripts/Tooltip.js", "~/Scripts/Other.js", "~/Scripts/numberSeparator.js", "~/Scripts/jquery.blockUI.js", "~/Scripts/jquery.cyclic-fade.js", "~/Scripts/history.js"));
			bundles.Add(new ScriptBundle("~/bundles/datagrid-js").Include("~/Scripts/DataGrid.js", "~/Scripts/jstree.min.js", "~/Scripts/jquery.treeview.js"));
			bundles.Add(new ScriptBundle("~/bundles/login-js").Include("~/Scripts/jquery.validate.min.js", "~/Scripts/jquery.validate.unobtrusive.min.js"));
			bundles.Add(new ScriptBundle("~/bundles/roles-js").Include("~/Scripts/jquery.treeTable.js"));
			bundles.Add(new ScriptBundle("~/bundles/documents-js").Include("~/Scripts/jquery.zoom.js"));
			bundles.Add(new ScriptBundle("~/bundles/filesystem-js").Include("~/Scripts/FileSystem.js"));
			bundles.Add(new ScriptBundle("~/bundles/tablecruds-js").Include("~/Scripts/jquery.unobtrusive-ajax.min.js", "~/Scripts/jquery.validate.unobtrusive.min.js"));
			bundles.Add(new ScriptBundle("~/bundles/lotus-js").Include("~/Scripts/LotusNotes.js"));
			bundles.Add(new ScriptBundle("~/bundles/converter-js").Include("~/Scripts/Converter.js"));
			bundles.Add(new ScriptBundle("~/bundles/settings-js").Include("~/Scripts/jquery.unobtrusive-ajax.min.js"));
			bundles.Add(new StyleBundle("~/Content/css").
				Include("~/Content/reset.css", "~/Content/jquery-ui/jquery-ui.min.css",
				"~/Content/Site.css", "~/Content/viewbox.css", "~/Content/slider.css"));
			bundles.Add(new StyleBundle("~/Content/datagrid-css").Include("~/Content/DataGrid.css", "~/Content/ViewOptions.css", "~/Content/jquery.treeview.css"));
			bundles.Add(new StyleBundle("~/Content/viewoptions-css").Include("~/Content/ViewOptions.css"));
			bundles.Add(new StyleBundle("~/Content/usermanagement-css").Include("~/Content/ViewOptions.css", "~/Content/DataGrid.css"));
			bundles.Add(new StyleBundle("~/Content/login-css").Include("~/Content/style.css"));
		}
	}
}
