using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SystemDb;
using DbAccess;
using Viewbox.Models;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Controllers
{
	public class HeaderController : BaseController
	{
		public ActionResult ShowEditOptimizationTextDialog(int id)
		{
			IOptimization optimization = ViewboxApplication.Database.SystemDb.Optimizations[id];
			if (optimization == null)
			{
				return PartialView("_DialogPartial", new DialogModel
				{
					DialogType = DialogModel.Type.Warning,
					Content = Resources.IncorrectInput,
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = Resources.OK
						}
					}
				});
			}
			string text = optimization.Group.GetName(ViewboxSession.Language);
			List<Tuple<string, string, string, string, int, int>> inputs = new List<Tuple<string, string, string, string, int, int>>
			{
				new Tuple<string, string, string, string, int, int>(text, "text", "tableName", optimization.GetDescription(ViewboxSession.Language), 256, 0)
			};
			List<Tuple<string, string, string>> hidden = new List<Tuple<string, string, string>>
			{
				new Tuple<string, string, string>("countryCode", ViewboxSession.Language.CountryCode, "countryCode")
			};
			int i = 0;
			foreach (ILanguage j in ViewboxApplication.Languages)
			{
				hidden.Add(new Tuple<string, string, string>("descriptions[" + i + "].countryCode", j.CountryCode, ""));
				hidden.Add(new Tuple<string, string, string>("descriptions[" + i + "].descriptions", optimization.GetDescription(j), j.CountryCode + "_tableName"));
				i++;
			}
			hidden.Add(new Tuple<string, string, string>("optId", id.ToString(), ""));
			DialogModel dialog = new DialogModel
			{
				Title = Resources.UserDefinedIssueDescriptionsCaption,
				Content = ((ViewboxApplication.Languages.Count() > 1) ? Resources.UserDefinedIssueDescriptionsTextLanguages : Resources.UserDefinedIssueDescriptionsText),
				DialogType = DialogModel.Type.Info,
				Inputs = inputs,
				HiddenFields = hidden,
				Buttons = new List<DialogModel.Button>
				{
					new DialogModel.Button
					{
						Caption = Resources.Cancel,
						Data = false.ToString()
					},
					new DialogModel.Button
					{
						Caption = Resources.Submit,
						Data = true.ToString()
					}
				}
			};
			if (ViewboxApplication.Languages.Count() > 1)
			{
				dialog.Select = new List<Tuple<string, string, List<string>, List<string>, int>>
				{
					new Tuple<string, string, List<string>, List<string>, int>(Resources.LanguageSelection, "language", new List<string>(ViewboxApplication.Languages.Select((ILanguage l) => l.LanguageName)), new List<string>(ViewboxApplication.Languages.Select((ILanguage l) => l.CountryCode)), new List<ILanguage>(ViewboxApplication.Languages).IndexOf(ViewboxSession.Language))
				};
			}
			return PartialView("_ExtendedDialogPartial", dialog);
		}

		public ActionResult ChangedOptimizationTexts(int optId, DescriptionCollection descriptions)
		{
			IOptimization optimization = ViewboxApplication.Database.SystemDb.Optimizations[optId];
			if (optimization == null)
			{
				return null;
			}
			foreach (Description description in descriptions)
			{
				optimization.Descriptions[ViewboxApplication.Languages.First((ILanguage lang) => lang.CountryCode == description.CountryCode)] = description.Descriptions[0];
			}
			Dictionary<string, string> langToDescription = descriptions.Select((Description descr) => new KeyValuePair<string, string>(descr.CountryCode, descr.Descriptions[0])).ToDictionary((KeyValuePair<string, string> t) => t.Key, (KeyValuePair<string, string> t) => t.Value);
			global::ViewboxDb.ViewboxDb db = ViewboxApplication.Database;
			IOptimization opt = db.SystemDb.Optimizations.SingleOrDefault((IOptimization o) => o.Id == optId);
			string newName = langToDescription[ViewboxApplication.DefaultLanguage.CountryCode];
			if (opt != null)
			{
				ILanguage language = ViewboxApplication.BrowserLanguage;
				using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
				if (!conn.IsOpen)
				{
					conn.Open();
				}
				string query = string.Format("SELECT count(*) FROM {0} WHERE `ref_id` = {1} AND country_code = '{2}';", conn.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "optimization_texts"), optId, ViewboxApplication.DefaultLanguage.CountryCode);
				int count = int.Parse(conn.ExecuteScalar(query).ToString());
				query = string.Empty;
				if (count > 0)
				{
					query = string.Format("UPDATE {0} SET `text` = '{1}' WHERE `ref_id` = {2} AND country_code = '{3}'", conn.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "optimization_texts"), newName, optId, ViewboxApplication.DefaultLanguage.CountryCode);
				}
				else
				{
					query = string.Format("SELECT COUNT(*) FROM information_schema.COLUMNS WHERE `TABLE_SCHEMA` = '{0}' AND `table_name` = '{1}' and `column_name` = 'id' and `extra` LIKE '%auto_increment%';", ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "optimization_texts");
					if (int.Parse(conn.ExecuteScalar(query).ToString()) == 0)
					{
						query = string.Format("ALTER TABLE {0} CHANGE COLUMN `id` `id` INT(11) NOT NULL AUTO_INCREMENT; ", conn.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "optimization_texts"));
						try
						{
							conn.ExecuteNonQuery(query);
						}
						catch (Exception)
						{
							throw;
						}
					}
					query = string.Format("INSERT INTO {0} VALUES (null, {1}, '{2}', '{3}');", conn.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "optimization_texts"), optId, ViewboxApplication.DefaultLanguage.CountryCode, newName);
				}
				try
				{
					conn.ExecuteNonQuery(query);
				}
				catch (Exception)
				{
				}
			}
			return null;
		}

		[HttpGet]
		public ActionResult UpdateOptimizationText(int optId, string newName)
		{
			global::ViewboxDb.ViewboxDb db = ViewboxApplication.Database;
			IOptimization opt = db.SystemDb.Optimizations.SingleOrDefault((IOptimization o) => o.Id == optId);
			if (opt != null)
			{
				ILanguage language = ViewboxApplication.BrowserLanguage;
				using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
				if (!conn.IsOpen)
				{
					conn.Open();
				}
				string query = string.Format("SELECT count(*) FROM {0} WHERE `ref_id` = {1} AND country_code = '{2}';", conn.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "optimization_texts"), optId, ViewboxApplication.DefaultLanguage.CountryCode);
				int count = int.Parse(conn.ExecuteScalar(query).ToString());
				query = string.Empty;
				query = ((count <= 0) ? string.Format("INSERT INTO {0} VALUES (null, {1}, '{2}', '{3}');", conn.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "optimization_texts"), optId, ViewboxApplication.DefaultLanguage.CountryCode, newName) : string.Format("UPDATE {0} SET `text` = '{1}' WHERE `ref_id` = {2} AND country_code = '{3}'", conn.Enquote(ViewboxApplication.Database.SystemDb.ConnectionManager.DbConfig.DbName, "optimization_texts"), newName, optId, ViewboxApplication.DefaultLanguage.CountryCode));
				try
				{
					conn.ExecuteNonQuery(query);
					return Json(true, JsonRequestBehavior.AllowGet);
				}
				catch (Exception)
				{
				}
			}
			return Json(false, JsonRequestBehavior.AllowGet);
		}
	}
}
