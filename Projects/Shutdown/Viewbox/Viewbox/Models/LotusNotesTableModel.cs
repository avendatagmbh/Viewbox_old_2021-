using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using SystemDb;
using DbAccess;
using Viewbox.LotusNotes;

namespace Viewbox.Models
{
	public class LotusNotesTableModel : ViewboxModel
	{
		private static LotusNotesTableModel table;

		private readonly LotusNode _treeRoot;

		private List<LotusNode> _nodes = new List<LotusNode>();

		public int NodeCount => _nodes.Count;

		public LotusNode TreeRoot => _treeRoot;

		public List<Tuple<IParameter, string>> Parameters { get; private set; }

		public int Page { get; set; }

		public int Size { get; set; }

		public long Count { get; set; }

		public string Filter { get; set; }

		public static LotusNotesTableModel Table
		{
			get
			{
				if (table == null)
				{
					table = new LotusNotesTableModel();
				}
				return table;
			}
		}

		public override string LabelCaption
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		private LotusNotesTableModel()
		{
			_treeRoot = new LotusNode();
			_nodes.Add(_treeRoot);
			Parameters = new List<Tuple<IParameter, string>>();
		}

		private void ReadData(int page = 1, int size = 25, string filter = "")
		{
			string sqlQuery = string.Format("SELECT {0} FROM `{1}`.`{2}` ", "id_sortfield", "lotus", "eingangsdokument");
			if (filter != string.Empty)
			{
				sqlQuery += string.Format("WHERE {0} LIKE '%{1}%' ", "id_sortfield", filter);
			}
			sqlQuery += string.Format(" GROUP BY {0} ", "id_sortfield");
			sqlQuery += $" LIMIT {(page - 1) * size}, {size}; ";
			using (DatabaseBase databaseBase = ViewboxApplication.Database.ConnectionManager.GetConnection())
			{
				try
				{
					using IDataReader reader = databaseBase.ExecuteReader(sqlQuery.ToString());
					while (reader.Read())
					{
						LotusNode node = new LotusNode();
						node.SorterFeld = reader.GetString(0);
						_nodes.Add(node);
					}
				}
				catch (Exception)
				{
					throw;
				}
			}
			sqlQuery = string.Format("SELECT COUNT(DISTINCT {0}) FROM `{1}`.`{2}` ", "id_sortfield", "lotus", "eingangsdokument");
			if (filter != string.Empty)
			{
				sqlQuery += string.Format("WHERE {0} LIKE '%{1}%' ", "id_sortfield", filter);
			}
			using DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection();
			try
			{
				Count = long.Parse(conn.ExecuteScalar(sqlQuery).ToString());
			}
			catch (Exception)
			{
				throw;
			}
		}

		public void BuildTreeView(int page = 1, int size = 25, string filter = "")
		{
			_nodes.Clear();
			ReadData(page, size, filter);
			foreach (LotusNode rootNode in _nodes)
			{
				rootNode.Children = GetSorterNodes(rootNode.SorterFeld);
				foreach (LotusNode sorterNode in rootNode.Children)
				{
					sorterNode.Children = GetBuNodes(sorterNode.SorterFeld, sorterNode.BU);
					foreach (LotusNode buNodes in sorterNode.Children)
					{
						buNodes.Children = GetDokartNodes(buNodes.SorterFeld, buNodes.BU, buNodes.Dokart);
						foreach (LotusNode dokartNode in buNodes.Children)
						{
							dokartNode.Children = GetKategorieNodes(dokartNode.SorterFeld, dokartNode.BU, dokartNode.Dokart, dokartNode.Kategorie);
						}
					}
				}
				Thread.Sleep(25);
			}
		}

		public List<LotusNode> GetRootNodes()
		{
			return _nodes;
		}

		private List<LotusNode> GetDokartNodes(string sorterfeld, string BU, string dokart)
		{
			string query = $"SELECT id_kategorie FROM `lotus`.`eingangsdokument` where `id_sortfield` = '{sorterfeld}' and id_bu = '{BU}' AND id_dokart = '{dokart}' group by id_kategorie;";
			List<LotusNode> dokartNodes = new List<LotusNode>();
			using (DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection())
			{
				try
				{
					using IDataReader reader = conn.ExecuteReader(query);
					while (reader.Read())
					{
						LotusNode node = new LotusNode();
						node.SorterFeld = sorterfeld;
						node.BU = BU;
						node.Dokart = dokart;
						node.Kategorie = reader.GetString(0);
						dokartNodes.Add(node);
					}
				}
				catch (Exception)
				{
					throw;
				}
			}
			return dokartNodes;
		}

		private List<LotusNode> GetKategorieNodes(string sorterfeld, string BU, string dokart, string kategorie)
		{
			string query = $"SELECT id_dokdatum, ID_AUFTRAGSNUMMER_ISV, ID_AUFTRAGSNUMMER_SAP, id_produkt, ID_BETREFF, ID_NOTESUNID FROM `lotus`.`eingangsdokument` where `id_sortfield` = '{sorterfeld}' and id_bu = '{BU}' AND id_dokart = '{dokart}' AND id_kategorie = '{kategorie}';";
			List<LotusNode> kategorieNodes = new List<LotusNode>();
			using (DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection())
			{
				try
				{
					using IDataReader reader = conn.ExecuteReader(query);
					while (reader.Read())
					{
						LotusNode node = new LotusNode();
						node.UNID = reader.GetString(5);
						node.SorterFeld = sorterfeld;
						node.BU = BU;
						node.Dokart = dokart;
						node.Kategorie = kategorie;
						node.DokDatum = reader.GetString(0);
						node.KwTermin = reader.GetString(1);
						node.AuftragsNrSap = reader.GetString(2);
						node.Produkt = reader.GetString(3);
						node.Betreff = reader.GetString(4);
						node.UNID = reader.GetString(5);
						kategorieNodes.Add(node);
					}
				}
				catch (Exception)
				{
					throw;
				}
			}
			return kategorieNodes;
		}

		private List<LotusNode> GetBuNodes(string sorterfeld, string BU)
		{
			List<LotusNode> buNodes = new List<LotusNode>();
			string query = $"SELECT id_dokart FROM `lotus`.`eingangsdokument` where `id_sortfield` = '{sorterfeld}' and id_bu = '{BU}' group by id_dokart;";
			LotusNodeCollection nodeCollection = new LotusNodeCollection();
			using (DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection())
			{
				try
				{
					using IDataReader reader = conn.ExecuteReader(query);
					while (reader.Read())
					{
						LotusNode node = new LotusNode();
						node.SorterFeld = sorterfeld;
						node.BU = BU;
						node.Dokart = reader.GetString(0);
						buNodes.Add(node);
					}
				}
				catch (Exception)
				{
					throw;
				}
			}
			return buNodes;
		}

		private List<LotusNode> GetSorterNodes(string sorterfeld)
		{
			List<LotusNode> sorterFelds = new List<LotusNode>();
			string query = $"SELECT `id_bu` FROM `lotus`.`eingangsdokument` where `id_sortfield` = '{sorterfeld}' group by `id_bu`;";
			using (DatabaseBase conn = ViewboxApplication.Database.ConnectionManager.GetConnection())
			{
				try
				{
					using IDataReader reader = conn.ExecuteReader(query);
					while (reader.Read())
					{
						LotusNode node = new LotusNode();
						node.SorterFeld = sorterfeld;
						node.BU = reader.GetString(0);
						sorterFelds.Add(node);
					}
				}
				catch (Exception)
				{
					throw;
				}
			}
			return sorterFelds;
		}
	}
}
