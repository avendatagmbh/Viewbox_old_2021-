using System;
using System.Collections.Generic;
using System.Data;
using DbAccess;

namespace Viewbox.Models
{
	public class LotusEmailDetails : ViewboxModel
	{
		public Dictionary<string, string> Attributes;

		public List<LotusAttachment> Attachments { get; set; }

		public int Page { get; set; }

		public int Size { get; set; }

		public long Count { get; set; }

		public string Filter { get; set; }

		public override string LabelCaption
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public LotusEmailDetails(string unid)
		{
			Attributes = new Dictionary<string, string>();
			string sqlQuery = string.Format("SELECT * FROM `{0}`.`{1}` WHERE `{2}`='{3}'", "lotus", "eingangsdokument", "id_notesunid", unid);
			using (DatabaseBase databaseBase = ViewboxApplication.Database.ConnectionManager.GetConnection())
			{
				try
				{
					using IDataReader dataReader = databaseBase.ExecuteReader(sqlQuery.ToString());
					dataReader.Read();
					Attributes.Add("unid", dataReader.GetString(0));
					Attributes.Add("SortField", dataReader.GetString(1));
					Attributes.Add("Name", dataReader.GetString(2));
					Attributes.Add("BU", dataReader.GetString(3));
					Attributes.Add("Sachbearbeiter", dataReader.GetString(4));
					Attributes.Add("DokArt", dataReader.GetString(5));
					Attributes.Add("Kategorie", dataReader.GetString(6));
					Attributes.Add("RisNr", dataReader.GetString(7));
					Attributes.Add("Betreff", dataReader.GetString(8));
					Attributes.Add("DokDatum", dataReader.GetString(9));
					Attributes.Add("Produkt", dataReader.GetString(10));
					Attributes.Add("Auftragsnummer-ISV", dataReader.GetString(11));
					Attributes.Add("Auftragsnummer-SAP", dataReader.GetString(12));
				}
				catch (Exception)
				{
					throw;
				}
			}
			Attachments = new List<LotusAttachment>();
			sqlQuery = string.Format("SELECT filename,path FROM `{0}`.`{1}` WHERE `{2}`='{3}'", "rdb_final", "belegarchiv", "notesunid", unid);
			using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
			try
			{
				using IDataReader reader = conn.ExecuteReader(sqlQuery.ToString());
				while (reader.Read())
				{
					Attachments.Add(new LotusAttachment(reader.GetString(0), reader.GetString(1)));
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
