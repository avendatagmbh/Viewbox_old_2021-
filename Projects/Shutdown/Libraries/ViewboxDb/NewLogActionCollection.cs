using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SystemDb;

namespace ViewboxDb
{
	public class NewLogActionCollection : List<NewLogAction>
	{
		public NewLogActionCollection()
		{
		}

		public NewLogActionCollection(List<NewLogAction> items)
		{
			foreach (NewLogAction newLogAction in items)
			{
				if (newLogAction.QueryString != null)
				{
					newLogAction.ActionParameters = FromXML(newLogAction.QueryString);
				}
				if (newLogAction.Parentid > 0)
				{
					NewLogAction parrentLogAction = this.FirstOrDefault((NewLogAction a) => a.Id == newLogAction.Parentid);
					if (newLogAction.ActionParameters.ContainsKey("size") && parrentLogAction.ActionParameters.ContainsKey("size") && (int)parrentLogAction.ActionParameters["size"] != (int)newLogAction.ActionParameters["size"])
					{
						newLogAction.SizeChanged = true;
					}
					if (newLogAction.ActionController != NewLogActionControllers.DocumentsIndex && !newLogAction.ActionParameters.ContainsKey("page") && newLogAction.ActionParameters.ContainsKey("start") && parrentLogAction.ActionParameters.ContainsKey("start") && (int)parrentLogAction.ActionParameters["start"] != (int)newLogAction.ActionParameters["start"])
					{
						newLogAction.ActionParameters.Add("page", (int)newLogAction.ActionParameters["start"] / (int)newLogAction.ActionParameters["size"]);
					}
					parrentLogAction.SubActions.Add(newLogAction);
				}
				else
				{
					Add(newLogAction);
				}
			}
		}

		private Dictionary<string, object> FromXML(string queryString)
		{
			Dictionary<string, object> result = new Dictionary<string, object>();
			using XmlReader reader = XmlReader.Create(new StringReader(queryString));
			bool keyElement = false;
			bool valueElement = false;
			string data = string.Empty;
			string attribute = string.Empty;
			while (reader.Read())
			{
				switch (reader.NodeType)
				{
				case XmlNodeType.Element:
					if (reader.Name == "Key")
					{
						keyElement = true;
					}
					else
					{
						if (!(reader.Name == "Value"))
						{
							break;
						}
						attribute = reader.GetAttribute(0);
						if (attribute.Equals("ViewboxDb.SortCollection"))
						{
							SortCollection sortCollectionData = new SortCollection();
							int cid = 0;
							SortDirection dir = SortDirection.Ascending;
							bool inCid = false;
							bool inDir = false;
							while (reader.Read() && (reader.NodeType != XmlNodeType.EndElement || reader.Name != "Value"))
							{
								if (reader.NodeType == XmlNodeType.Element)
								{
									if (reader.Name == "cid")
									{
										inCid = true;
									}
									if (reader.Name == "dir")
									{
										inDir = true;
									}
								}
								if (reader.NodeType == XmlNodeType.EndElement)
								{
									if (reader.Name == "Sort")
									{
										sortCollectionData.Add(new Sort(cid, dir));
									}
									if (reader.Name == "cid")
									{
										inCid = false;
									}
									if (reader.Name == "dir")
									{
										inDir = false;
									}
								}
								if (reader.NodeType != XmlNodeType.Text)
								{
									continue;
								}
								if (inCid)
								{
									cid = int.Parse(reader.Value);
								}
								if (inDir)
								{
									if (reader.Value == "Descending")
									{
										dir = SortDirection.Descending;
									}
									if (reader.Value == "Ascending")
									{
										dir = SortDirection.Ascending;
									}
								}
							}
							result.Add(data, sortCollectionData);
							valueElement = false;
						}
						else if (attribute.Equals("System.Collections.Generic.List`1[ViewboxDb.JoinColumns]"))
						{
							bool inCol2 = false;
							bool inCol4 = false;
							int col2 = 0;
							int col4 = 0;
							List<JoinColumns> joinColumnsList2 = new List<JoinColumns>();
							while (reader.Read() && (reader.NodeType != XmlNodeType.EndElement || reader.Name != "Value"))
							{
								if (reader.NodeType == XmlNodeType.Element)
								{
									if (reader.Name == "col1")
									{
										inCol2 = true;
									}
									if (reader.Name == "col2")
									{
										inCol4 = true;
									}
								}
								if (reader.NodeType == XmlNodeType.EndElement)
								{
									if (reader.Name == "Relation")
									{
										joinColumnsList2.Add(new JoinColumns
										{
											Column1 = col2,
											Column2 = col4,
											Direction = SortDirection.Ascending
										});
									}
									if (reader.Name == "col1")
									{
										inCol2 = false;
									}
									if (reader.Name == "col2")
									{
										inCol4 = false;
									}
								}
								if (reader.NodeType == XmlNodeType.Text)
								{
									if (inCol2)
									{
										col2 = int.Parse(reader.Value);
									}
									if (inCol4)
									{
										col4 = int.Parse(reader.Value);
									}
								}
							}
							result.Add(data, joinColumnsList2);
							valueElement = false;
						}
						else if (attribute.Equals("ViewboxDb.JoinColumnsCollection"))
						{
							bool inCol1 = false;
							bool inCol3 = false;
							int col1 = 0;
							int col3 = 0;
							List<JoinColumns> joinColumnsList = new List<JoinColumns>();
							while (reader.Read() && (reader.NodeType != XmlNodeType.EndElement || reader.Name != "Value"))
							{
								if (reader.NodeType == XmlNodeType.Element)
								{
									if (reader.Name == "col1")
									{
										inCol1 = true;
									}
									if (reader.Name == "col2")
									{
										inCol3 = true;
									}
								}
								if (reader.NodeType == XmlNodeType.EndElement)
								{
									if (reader.Name == "Join")
									{
										joinColumnsList.Add(new JoinColumns
										{
											Column1 = col1,
											Column2 = col3,
											Direction = SortDirection.Ascending
										});
									}
									if (reader.Name == "col1")
									{
										inCol1 = false;
									}
									if (reader.Name == "col2")
									{
										inCol3 = false;
									}
								}
								if (reader.NodeType == XmlNodeType.Text)
								{
									if (inCol1)
									{
										col1 = int.Parse(reader.Value);
									}
									if (inCol3)
									{
										col3 = int.Parse(reader.Value);
									}
								}
							}
							result.Add(data, joinColumnsList);
							valueElement = false;
						}
						else if (reader.IsEmptyElement)
						{
							result.Add(data, "");
						}
						else
						{
							valueElement = true;
						}
					}
					break;
				case XmlNodeType.EndElement:
					if (reader.Name == "Key")
					{
						keyElement = false;
					}
					else if (reader.Name == "Value")
					{
						valueElement = false;
					}
					break;
				case XmlNodeType.Text:
					if (keyElement)
					{
						data = reader.Value;
					}
					if (valueElement)
					{
						result.Add(data, TypeCastData(attribute, reader.Value));
					}
					break;
				}
			}
			return result;
		}

		private object TypeCastData(string typeString, string data)
		{
			if (typeString.ToLower() == "system.int32")
			{
				return int.Parse(data);
			}
			if (typeString.ToLower() == "system.int64")
			{
				return long.Parse(data);
			}
			if (typeString.ToLower() == "system.string")
			{
				return data;
			}
			if (typeString.ToLower() == "null")
			{
				return null;
			}
			if (typeString.ToLower() == "system.boolean")
			{
				return bool.Parse(data);
			}
			if (typeString.ToLower() == "viewboxdb.sortdirection")
			{
				if (data == "Ascending")
				{
					return SortDirection.Ascending;
				}
				if (data == "Descending")
				{
					return SortDirection.Descending;
				}
			}
			if (typeString.ToLower() == "systemdb.tabletype")
			{
				switch (data)
				{
				case "View":
					return TableType.View;
				case "Table":
					return TableType.Table;
				case "Issue":
					return TableType.Issue;
				}
			}
			if (typeString.ToLower() == "viewboxdb.exporttype")
			{
				switch (data)
				{
				case "Excel":
					return ExportType.Excel;
				case "GDPdU":
					return ExportType.GDPdU;
				case "PDF":
					return ExportType.PDF;
				case "HTML":
					return ExportType.HTML;
				}
			}
			if (typeString.ToLower() == "system.collections.generic.list`1[system.string]")
			{
				return data;
			}
			if (typeString.ToLower() == "system.collections.generic.list`1[system.int32]")
			{
				return data;
			}
			if (typeString.ToLower() == "system.collections.generic.list`1[viewboxdb.aggregationfunction]")
			{
				return data;
			}
			if (typeString.ToLower() == "viewboxdb.descriptioncollection")
			{
				return data;
			}
			if (typeString.ToLower() == "viewboxdb.joincolumnscollection")
			{
				return data;
			}
			if (typeString.ToLower() == "system.collections.generic.list`1[viewboxdb.joincolumns]")
			{
				return data;
			}
			if (typeString.ToLower() == "system.string[]")
			{
				return data;
			}
			if (typeString.ToLower() == "system.collections.generic.list`1[viewbox.models.propertysettingsmodel+idvaluepair]")
			{
				return data;
			}
			if (typeString.ToLower() == "viewboxdb.jointype")
			{
				switch (data)
				{
				case "Inner":
					return JoinType.Inner;
				case "Left":
					return JoinType.Left;
				case "OnlyLeft":
					return JoinType.OnlyLeft;
				case "OnlyRight":
					return JoinType.OnlyRight;
				case "Right":
					return JoinType.Right;
				}
			}
			if (typeString == "Viewbox.Models.LogOnModel")
			{
				return data;
			}
			throw new InvalidCastException("Unknown type");
		}
	}
}
