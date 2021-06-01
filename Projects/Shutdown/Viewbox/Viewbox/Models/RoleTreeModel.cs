using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class RoleTreeModel : SettingsModel
	{
		public class Node
		{
			public int Id;

			public string Descr;

			public List<Node> Children;

			public Node Parent;

			public Node(int id, string descr, Node parent)
			{
				Id = id;
				Descr = descr;
				if (parent != null)
				{
					Parent = parent;
					if (Parent.Children == null)
					{
						Parent.Children = new List<Node>();
					}
					Parent.Children.Add(this);
				}
			}
		}

		private int _id = 0;

		public Node OptimizationTree { get; set; }

		public Node RowTree { get; set; }

		private int ID
		{
			get
			{
				_id++;
				return _id;
			}
			set
			{
				_id = value;
			}
		}

		public override string Partial => "_RoleTreePartial";

		public RoleTreeModel()
			: base(SettingsType.RoleTree)
		{
			int ID = 0;
			RowTree = new Node(ID, "", null);
			List<int> roleIDs = new List<int>();
			if (ViewboxSession.RightsModeCredential.Type == CredentialType.Role)
			{
				roleIDs.Add(ViewboxSession.RightsModeCredential.Id);
			}
			else
			{
				foreach (IRole role3 in ViewboxApplication.Database.SystemDb.Users[ViewboxSession.RightsModeCredential.Id].Roles)
				{
					roleIDs.Add(role3.Id);
				}
			}
			if (ViewboxSession.IsTableTypeAllowedInRightsMode(TableType.Issue))
			{
				Node issueBase = new Node(ID, Resources.Issue, RowTree);
				IIssueCollection issues2 = ViewboxApplication.Database.SystemDb.Issues;
				foreach (int roleID3 in roleIDs)
				{
					IRole role5 = ViewboxApplication.Roles[roleID3];
					Node roleNode4 = new Node(roleID3, (role5 == null) ? roleID3.ToString() : role5.Name, issueBase);
					List<IIssue> validIssues3 = issues2.Where((IIssue x) => x.RoleBasedFilters != null).ToList();
					validIssues3.AddRange(issues2.Where((IIssue x) => x.FilterTableObject != null && x.FilterTableObject.RoleBasedFilters != null));
					List<Tuple<string, IIssue>> filters3 = new List<Tuple<string, IIssue>>();
					foreach (IIssue item4 in validIssues3)
					{
						if (item4 != null && item4.RoleBasedFilters != null && item4.RoleBasedFilters.Keys != null && item4.RoleBasedFilters.Keys.Contains(roleID3) && item4.RoleBasedFilters[roleID3].FirstOrDefault() != null && !filters3.Any((Tuple<string, IIssue> x) => x.Item1 == item4.RoleBasedFilters[roleID3].FirstOrDefault().Item1.ToString()))
						{
							filters3.Add(Tuple.Create(ViewboxApplication.Database.SystemDb.Columns[item4.FilterTableObject.RoleBasedFilters[roleID3].FirstOrDefault().Item1].GetDescription() + ": " + item4.RoleBasedFilters[roleID3].FirstOrDefault().Item2.FirstOrDefault() + "...", item4));
						}
						else if (item4 != null && item4.FilterTableObject != null && item4.FilterTableObject.RoleBasedFilters != null && item4.FilterTableObject.RoleBasedFilters.Keys != null && item4.FilterTableObject.RoleBasedFilters.Keys.Contains(roleID3) && item4.FilterTableObject.RoleBasedFilters[roleID3].FirstOrDefault() != null && !filters3.Any((Tuple<string, IIssue> x) => x.Item1 == item4.FilterTableObject.RoleBasedFilters[roleID3].FirstOrDefault().Item1.ToString()))
						{
							filters3.Add(Tuple.Create(ViewboxApplication.Database.SystemDb.Columns[item4.FilterTableObject.RoleBasedFilters[roleID3].FirstOrDefault().Item1].GetDescription() + ": " + item4.FilterTableObject.RoleBasedFilters[roleID3].FirstOrDefault().Item2.FirstOrDefault() + "...", item4));
						}
					}
					foreach (IGrouping<string, Tuple<string, IIssue>> filter3 in from x in filters3
						group x by x.Item1)
					{
						Node filterNode3 = new Node(ID, filter3.Key, roleNode4);
						foreach (Tuple<string, IIssue> item3 in filter3)
						{
							if (item3.Item2.IsAllowedInRightsMode() && (filterNode3.Children == null || filterNode3.Children.Count == 0 || !filterNode3.Children.Any((Node x) => x != null && x.Descr != null && x.Descr.StartsWith(item3.Item2.TransactionNumber))))
							{
								Node issueNode3 = new Node(ID, item3.Item2.TransactionNumber + ": " + item3.Item2.GetDescription(), filterNode3);
							}
						}
					}
				}
			}
			if (ViewboxSession.IsTableTypeAllowedInRightsMode(TableType.View))
			{
				Node viewBase = new Node(ID, Resources.Views, RowTree);
				IViewCollection issues3 = ViewboxApplication.Database.SystemDb.Views;
				foreach (int roleID2 in roleIDs)
				{
					IRole role4 = ViewboxApplication.Roles[roleID2];
					Node roleNode3 = new Node(roleID2, (role4 == null) ? roleID2.ToString() : role4.Name, viewBase);
					List<IView> validIssues2 = issues3.Where((IView x) => x.RoleBasedFilters != null).ToList();
					List<Tuple<string, IView>> filters2 = new List<Tuple<string, IView>>();
					foreach (IView item2 in validIssues2)
					{
						if (item2 != null && item2.RoleBasedFilters != null && item2.RoleBasedFilters.Keys != null && item2.RoleBasedFilters.Keys.Contains(roleID2) && item2.RoleBasedFilters[roleID2].FirstOrDefault() != null && !filters2.Any((Tuple<string, IView> x) => x.Item1 == item2.RoleBasedFilters[roleID2].FirstOrDefault().Item1.ToString()))
						{
							filters2.Add(Tuple.Create(ViewboxApplication.Database.SystemDb.Columns[item2.RoleBasedFilters[roleID2].FirstOrDefault().Item1].GetDescription() + ": " + item2.RoleBasedFilters[roleID2].FirstOrDefault().Item2.FirstOrDefault() + "...", item2));
						}
					}
					foreach (IGrouping<string, Tuple<string, IView>> filter2 in from x in filters2
						group x by x.Item1)
					{
						Node filterNode2 = new Node(ID, filter2.Key, roleNode3);
						foreach (Tuple<string, IView> item7 in filter2)
						{
							if (item7.Item2.IsAllowedInRightsMode())
							{
								Node issueNode2 = new Node(ID, item7.Item2.TransactionNumber + ": " + item7.Item2.GetDescription(), filterNode2);
							}
						}
					}
				}
			}
			if (ViewboxSession.IsTableTypeAllowedInRightsMode(TableType.Table))
			{
				Node tableBase = new Node(ID, Resources.Tables, RowTree);
				ITableCollection issues = ViewboxApplication.Database.SystemDb.Tables;
				foreach (int roleID in roleIDs)
				{
					IRole role2 = ViewboxApplication.Roles[roleID];
					Node roleNode2 = new Node(roleID, (role2 == null) ? roleID.ToString() : role2.Name, tableBase);
					List<ITable> validIssues = issues.Where((ITable x) => x.RoleBasedFilters != null).ToList();
					List<Tuple<string, ITable>> filters = new List<Tuple<string, ITable>>();
					foreach (ITable item in validIssues)
					{
						if (item != null && item.RoleBasedFilters != null && item.RoleBasedFilters.Keys != null && item.RoleBasedFilters.Keys.Contains(roleID) && item.RoleBasedFilters[roleID].FirstOrDefault() != null && !filters.Any((Tuple<string, ITable> x) => x.Item1 == item.RoleBasedFilters[roleID].FirstOrDefault().Item1.ToString()))
						{
							filters.Add(Tuple.Create(ViewboxApplication.Database.SystemDb.Columns[item.RoleBasedFilters[roleID].FirstOrDefault().Item1].GetDescription() + ": " + item.RoleBasedFilters[roleID].FirstOrDefault().Item2.FirstOrDefault() + "...", item));
						}
					}
					foreach (IGrouping<string, Tuple<string, ITable>> filter in from x in filters
						group x by x.Item1)
					{
						Node filterNode = new Node(ID, filter.Key, roleNode2);
						foreach (Tuple<string, ITable> item6 in filter)
						{
							Node issueNode = new Node(ID, item6.Item2.Name, filterNode);
						}
					}
				}
			}
			ID = 0;
			OptimizationTree = new Node(ID, "", null);
			foreach (int roleID4 in roleIDs)
			{
				List<IOptimization> allowedOpts = new List<IOptimization>();
				foreach (Tuple<IRole, IOptimization, RightType> item5 in ViewboxApplication.Database.SystemDb.RoleOptimizationRights)
				{
					if (item5.Item1 != null && item5.Item1.Id == roleID4 && item5.Item3 == RightType.Read)
					{
						allowedOpts.Add(item5.Item2);
					}
				}
				IRole role = ViewboxApplication.Roles[roleID4];
				Node roleNode = new Node(roleID4, (role == null) ? roleID4.ToString() : role.Name, OptimizationTree);
				IOptimization[] systems = ViewboxSession.AllowedSystem;
				IOptimization[] array = systems;
				foreach (IOptimization syss in array)
				{
					if (!IsAllowed(syss, allowedOpts))
					{
						continue;
					}
					Node sysNode = new Node(ID, syss.GetDescription(), roleNode);
					IOptimization[] mandants = ViewboxSession.AllowedMandant;
					foreach (IOptimization mandant in mandants.Where((IOptimization x) => x.Parent.Id == syss.Id))
					{
						if (!IsAllowed(mandant, allowedOpts))
						{
							continue;
						}
						Node mandantNode = new Node(ID, mandant.Value + ": " + mandant.GetDescription(), sysNode);
						IOptimization[] buchungs = ViewboxSession.AllowedBukrs;
						foreach (IOptimization bukrs in buchungs.Where((IOptimization x) => x.Parent.Id == mandant.Id))
						{
							if (!IsAllowed(bukrs, allowedOpts))
							{
								continue;
							}
							Node bukrsNode = new Node(ID, bukrs.Value + ": " + bukrs.GetDescription(), mandantNode);
							IOptimization[] gJahrs = ViewboxSession.AllowedGjahr;
							string gJahrString = "";
							int tmp = 0;
							int lastYear = 0;
							foreach (IOptimization gJahr in from x in gJahrs
								where x.Parent.Id == bukrs.Id
								select x into y
								orderby y.Value
								select y)
							{
								if (IsAllowed(gJahr, allowedOpts) && gJahr != null && gJahr.Value != null && gJahr.Value != "" && int.TryParse(gJahr.Value.ToString(), out tmp))
								{
									if (lastYear == 0)
									{
										lastYear = tmp;
										gJahrString += lastYear;
										continue;
									}
									if (lastYear != 0 && tmp == lastYear + 1)
									{
										lastYear = tmp;
										continue;
									}
									gJahrString = gJahrString + "=>" + lastYear + ", " + tmp;
									lastYear = tmp;
								}
							}
							if (!gJahrString.EndsWith(tmp.ToString()))
							{
								gJahrString = ((lastYear != tmp) ? (gJahrString + ", " + tmp) : (gJahrString + "=>" + tmp));
							}
							if (!string.IsNullOrEmpty(gJahrString))
							{
								Node gJahrNode = new Node(ID, gJahrString, bukrsNode);
							}
						}
					}
				}
			}
		}

		private bool IsAllowed(IOptimization opt, List<IOptimization> allowedOpts)
		{
			foreach (IOptimization allowedOpt in allowedOpts)
			{
				if (allowedOpt == null)
				{
					continue;
				}
				if (opt.Group.Type == allowedOpt.Group.Type)
				{
					if (opt.Id == allowedOpt.Id)
					{
						return true;
					}
				}
				else if (CheckParent(opt, allowedOpt.Parent))
				{
					return true;
				}
			}
			return false;
		}

		private bool CheckParent(IOptimization opt, IOptimization parent)
		{
			if (parent == null)
			{
				return false;
			}
			if (opt.Group.Type == parent.Group.Type)
			{
				return opt.Id == parent.Id;
			}
			return CheckParent(opt, parent.Parent);
		}
	}
}
