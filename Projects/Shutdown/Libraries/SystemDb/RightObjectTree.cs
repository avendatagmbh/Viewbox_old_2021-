using System;
using System.Linq;

namespace SystemDb
{
	public class RightObjectTree
	{
		private RightObjectNodeCollection _nodes = new RightObjectNodeCollection();

		public RightObjectNodeCollection Nodes => _nodes;

		public void ClearRightTree()
		{
			_nodes = new RightObjectNodeCollection();
		}

		public void FillRightTree(ICredential credential, SystemDb systemDb)
		{
			FillRightTreeWithOptimizations(credential, systemDb.Optimizations[0], systemDb);
			if (credential.Type == CredentialType.User)
			{
				foreach (ICategory c2 in systemDb.Categories)
				{
					RightType cat_right2 = systemDb.UserCategoryRights[credential as IUser, c2];
					RightObjectNode catRightNode2 = new RightObjectNode
					{
						Id = c2.Id,
						Type = UpdateRightType.Category
					};
					catRightNode2.RightHierarchy.Add(new Tuple<ICredential, RightType>(credential as IUser, cat_right2));
					foreach (IRole role3 in (credential as IUser).Roles)
					{
						catRightNode2.RightHierarchy.Add(new Tuple<ICredential, RightType>(role3, systemDb.RoleCategoryRights[role3, c2]));
						if (RightType.Inherit >= cat_right2)
						{
							cat_right2 = systemDb.RoleCategoryRights[role3, c2];
						}
					}
					catRightNode2.RightHierarchy.Reverse();
					catRightNode2.Right = cat_right2;
					Nodes.Add(catRightNode2);
					foreach (ITableObject t2 in c2.TableObjects)
					{
						RightType obj_right2 = systemDb.UserTableObjectRights[credential as IUser, t2];
						RightObjectNode objRightNode2 = new RightObjectNode
						{
							Id = t2.Id,
							Type = UpdateRightType.TableObject,
							Parent = catRightNode2
						};
						objRightNode2.RightHierarchy.Add(new Tuple<ICredential, RightType>(credential as IUser, obj_right2));
						foreach (IRole role2 in (credential as IUser).Roles)
						{
							objRightNode2.RightHierarchy.Add(new Tuple<ICredential, RightType>(role2, systemDb.RoleTableObjectRights[role2, t2]));
							if (RightType.Inherit >= obj_right2)
							{
								obj_right2 = systemDb.RoleTableObjectRights[role2, t2];
							}
						}
						objRightNode2.RightHierarchy.Reverse();
						objRightNode2.Right = obj_right2;
						catRightNode2.Children.Add(objRightNode2);
						Nodes.Add(objRightNode2);
						foreach (IColumn col2 in t2.Columns)
						{
							RightType col_right2 = systemDb.UserColumnRights[credential as IUser, col2];
							RightObjectNode colRightNode2 = new RightObjectNode
							{
								Id = col2.Id,
								Type = UpdateRightType.Column,
								Parent = objRightNode2
							};
							colRightNode2.RightHierarchy.Add(new Tuple<ICredential, RightType>(credential as IUser, col_right2));
							foreach (IRole role in (credential as IUser).Roles)
							{
								colRightNode2.RightHierarchy.Add(new Tuple<ICredential, RightType>(role, systemDb.RoleColumnRights[role, col2]));
								if (RightType.Inherit >= col_right2)
								{
									col_right2 = systemDb.RoleColumnRights[role, col2];
								}
							}
							colRightNode2.RightHierarchy.Reverse();
							colRightNode2.Right = col_right2;
							objRightNode2.Children.Add(colRightNode2);
							Nodes.Add(colRightNode2);
						}
					}
				}
				return;
			}
			foreach (ICategory c in systemDb.Categories)
			{
				RightType cat_right = systemDb.RoleCategoryRights[credential as IRole, c];
				RightObjectNode catRightNode = new RightObjectNode
				{
					Id = c.Id,
					Type = UpdateRightType.Category,
					Right = cat_right
				};
				catRightNode.RightHierarchy.Add(new Tuple<ICredential, RightType>(credential as IRole, cat_right));
				Nodes.Add(catRightNode);
				foreach (ITableObject t in c.TableObjects)
				{
					RightType obj_right = systemDb.RoleTableObjectRights[credential as IRole, t];
					RightObjectNode objRightNode = new RightObjectNode
					{
						Id = t.Id,
						Type = UpdateRightType.TableObject,
						Right = obj_right,
						Parent = catRightNode
					};
					objRightNode.RightHierarchy.Add(new Tuple<ICredential, RightType>(credential as IRole, obj_right));
					catRightNode.Children.Add(objRightNode);
					Nodes.Add(objRightNode);
					foreach (IColumn col in t.Columns)
					{
						RightType col_right = systemDb.RoleColumnRights[credential as IRole, col];
						RightObjectNode colRightNode = new RightObjectNode
						{
							Id = col.Id,
							Type = UpdateRightType.Column,
							Right = col_right,
							Parent = objRightNode
						};
						colRightNode.RightHierarchy.Add(new Tuple<ICredential, RightType>(credential as IRole, col_right));
						objRightNode.Children.Add(colRightNode);
						Nodes.Add(colRightNode);
					}
				}
			}
		}

		private void FillRightTreeWithOptimizations(ICredential credential, IOptimization optimization, SystemDb systemDb)
		{
			if (credential.Type == CredentialType.User)
			{
				RightObjectNode optRightParent2 = ((optimization.Id == 0) ? null : Nodes[optimization.Id, UpdateRightType.Optimization]);
				foreach (IOptimization optChild2 in optimization.Children)
				{
					RightType right2 = systemDb.UserOptimizationRights[credential as IUser, optChild2];
					RightObjectNode optRightNode2 = new RightObjectNode
					{
						Id = optChild2.Id,
						Type = UpdateRightType.Optimization,
						Parent = optRightParent2
					};
					optRightNode2.RightHierarchy.Add(new Tuple<ICredential, RightType>(credential, right2));
					foreach (IRole role in (credential as IUser).Roles)
					{
						optRightNode2.RightHierarchy.Add(new Tuple<ICredential, RightType>(role, systemDb.RoleOptimizationRights[role, optChild2]));
						if (RightType.Inherit >= right2)
						{
							right2 = systemDb.RoleOptimizationRights[role, optChild2];
						}
					}
					optRightNode2.RightHierarchy.Reverse();
					optRightNode2.Right = right2;
					optRightParent2?.Children.Add(optRightNode2);
					Nodes.Add(optRightNode2);
					FillRightTreeWithOptimizations(credential, optChild2, systemDb);
				}
				return;
			}
			RightObjectNode optRightParent = ((optimization.Id == 0) ? null : Nodes[optimization.Id, UpdateRightType.Optimization]);
			foreach (IOptimization optChild in optimization.Children)
			{
				RightType right = systemDb.RoleOptimizationRights[credential as IRole, optChild];
				RightObjectNode optRightNode = new RightObjectNode
				{
					Id = optChild.Id,
					Type = UpdateRightType.Optimization,
					Parent = optRightParent,
					Right = right
				};
				optRightNode.RightHierarchy.Add(new Tuple<ICredential, RightType>(credential, right));
				optRightParent?.Children.Add(optRightNode);
				Nodes.Add(optRightNode);
				FillRightTreeWithOptimizations(credential, optChild, systemDb);
			}
		}

		public static bool DeleteTableObjectIsAllowed(ITableObject tobj, SystemDb systemDb)
		{
			if (!systemDb.Users.Any((IUser u) => u.IsSuper) && !systemDb.Users.Any((IUser u) => systemDb.UserTableObjectRights[u, tobj] > RightType.None))
			{
				return !systemDb.Users.Any((IUser u) => systemDb.UserTableObjectRights[u, tobj] == RightType.Inherit && systemDb.UserCategoryRights[u, tobj.Category] > RightType.None);
			}
			return false;
		}
	}
}
