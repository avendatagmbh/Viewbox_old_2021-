using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SystemDb.Internal;
using ViewboxBusiness.Exceptions;
using ViewboxBusiness.ProfileDb.Tables;
using ViewboxBusiness.Resources;
using ViewboxBusiness.Structures.Config;
using ViewboxBusiness.ViewBuilder;

namespace ViewboxBusiness.Common
{
	public static class ViewscriptParser
	{
		private enum EState
		{
			NOTHING,
			VIEWNAME,
			OPTIMIZE,
			LANG,
			DICTIONARY,
			KEYS,
			VIEW,
			PROCEDURE,
			PROCEDUREPARAMS,
			PROCEDURETEMP,
			FinalizeView,
			TEMPSTATEMENTS,
			REPORTPARAMS,
			REPORTFILTER,
			REPORTFILTERSPECIAL,
			PROCEDUREPARAMREFERENCE,
			OBJECTTYPE
		}

		private const string BeginView = "-- ##BEGIN_VIEW##";

		private const string OPTIMIZE = "-- ##OPTIMIZE##";

		private const string LANG = "-- ##LANGUAGES##";

		private const string DICTIONARY = "-- ##DICTIONARY##";

		private const string KEYS = "-- ##KEYS##";

		private const string VIEW = "-- ##VIEW##";

		private const string PROCEDURE = "-- ##PROCEDURE##";

		private const string PROCEDUREParams = "-- ##PROCEDUREPARAMS##";

		private const string PROCEDUREParamReference = "-- ##PROCEDUREPARAMREFERENCE##";

		private const string PROCEDURETemp = "-- ##PROCEDURETEMP##";

		private const string TempStatements = "-- ##TEMPSTATEMENTS##";

		private const string ReportParams = "-- ##REPORTPARAMS##";

		private const string ReportFilter = "-- ##REPORTFILTER##";

		private const string ReportFilterSpecial = "-- ##REPORTFILTERSPECIAL##";

		private const string EndVIEW = "-- ##END_VIEW##";

		private const string OBJECTTYPE = "-- ##OBJECTTYPE##";

		public static List<Viewscript> Parse(FileInfo file, ProfileConfig profile)
		{
			return Parse(file, profile, forViewComparison: false);
		}

		public static List<Viewscript> Parse(FileInfo file, ProfileConfig profile, bool forViewComparison)
		{
			List<Viewscript> views = new List<Viewscript>();
			if (file.Length > 2097152)
			{
				return views;
			}
			EState state = EState.NOTHING;
			using (StreamReader reader = new StreamReader(file.FullName, Encoding.Default))
			{
				string script = string.Empty;
				int rowNumber = 0;
				while (!reader.EndOfStream)
				{
					rowNumber++;
					string line = reader.ReadLine();
					if (line != null && line.Trim().StartsWith("-- ##"))
					{
						state = GetNextState(rowNumber, line);
					}
					switch (state)
					{
					case EState.FinalizeView:
					{
						script = script + Environment.NewLine + line;
						ViewInfo viewInfo = Parse(script, profile, forViewComparison);
						if (viewInfo != null)
						{
							Viewscript viewscript2 = new Viewscript(viewInfo)
							{
								Script = script,
								FileInfo = file
							};
							viewscript2.SetIndizes(GetIndizesFromViewInfo(viewInfo.Indizes));
							views.Add(viewscript2);
						}
						script = string.Empty;
						state = EState.NOTHING;
						break;
					}
					default:
						script = script + Environment.NewLine + line;
						break;
					case EState.NOTHING:
						break;
					}
				}
			}
			if (state != 0)
			{
				throw new InvalidScriptException("Fehlendes -- ##END_VIEW## Tag.");
			}
			if (profile.ViewboxDb != null && profile.ViewboxDb.ConnectionManager != null && !profile.ViewboxDb.ConnectionManager.IsDisposed)
			{
				foreach (Viewscript viewscript in views.Where((Viewscript v) => v.ViewInfo.ObjectType == null))
				{
					viewscript.ViewInfo.ObjectType = new ObjectType(ObjectTypeEnum.Other.ToString());
					if (viewscript.Name.IndexOf("_", StringComparison.Ordinal) > 0)
					{
						string prefix = viewscript.Name.Substring(0, viewscript.Name.IndexOf("_", StringComparison.Ordinal));
						foreach (ObjectTypeEnum objType in Enum.GetValues(typeof(ObjectTypeEnum)))
						{
							if (objType.ToString().ToLower() == prefix.ToLower())
							{
								viewscript.ViewInfo.ObjectType = new ObjectType(objType.ToString());
								break;
							}
						}
					}
					ViewboxBusiness.ViewBuilder.ViewBuilder.CreateObjectType(new ViewCreateOptions(), viewscript, null, profile);
				}
				return views;
			}
			return views;
		}

		public static ViewInfo Parse(string script, ProfileConfig profile, bool forViewComparison = false)
		{
			ViewInfo viewInfo = new ViewInfo();
			EState state = EState.NOTHING;
			string[] array = script.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
			int rowNumber = 0;
			StringBuilder tmpStatement = new StringBuilder();
			bool foundFirstLine = false;
			string[] array2 = array;
			foreach (string line in array2)
			{
				rowNumber++;
				if (line.Trim().Length == 0)
				{
					continue;
				}
				if (!foundFirstLine)
				{
					foundFirstLine = true;
					if (!line.Trim().StartsWith("-- ##"))
					{
						return null;
					}
				}
				if (line.Trim().StartsWith("-- ##"))
				{
					switch (state)
					{
					case EState.KEYS:
					{
						string[] array3 = tmpStatement.ToString().Split(';');
						foreach (string sKey in array3)
						{
							if (sKey.Trim().Length > 0)
							{
								viewInfo.Indizes.Add(sKey.Trim());
								if (forViewComparison)
								{
									viewInfo.CompleteStatement = viewInfo.CompleteStatement + sKey + ";";
								}
							}
						}
						break;
					}
					case EState.VIEW:
						viewInfo.CompleteStatement += tmpStatement.ToString();
						foreach (string statement in ViewInfo.SplitStatements(tmpStatement.ToString()))
						{
							if (statement.Trim().Length > 0)
							{
								viewInfo.Statements.Add(statement.Trim());
							}
						}
						break;
					case EState.PROCEDURE:
						viewInfo.SetCompleteProcedure(tmpStatement.ToString());
						if (forViewComparison)
						{
							viewInfo.CompleteStatement = string.Concat(viewInfo.CompleteStatement, tmpStatement, ";");
						}
						break;
					case EState.PROCEDURETEMP:
						viewInfo.ProcedureCreateTempTables = tmpStatement.ToString();
						break;
					case EState.TEMPSTATEMENTS:
						viewInfo.TempStatementsComplete = tmpStatement.ToString();
						break;
					}
					tmpStatement.Clear();
					state = GetNextState(rowNumber, line.Trim());
					if (state == EState.FinalizeView)
					{
						string error = viewInfo.FinalizeViewInfo();
						if (!string.IsNullOrEmpty(error))
						{
							throw new InvalidScriptException(error);
						}
						if (!string.IsNullOrEmpty(viewInfo.ProcedureName) && string.IsNullOrEmpty(viewInfo.CompleteProcedure))
						{
							throw new InvalidScriptException("Es wurde ein Prozedurname angegeben, aber es steht keine Prozedur im -- ##PROCEDURE## Teil.");
						}
						if (string.IsNullOrEmpty(viewInfo.ProcedureName) && !string.IsNullOrEmpty(viewInfo.CompleteProcedure))
						{
							throw new InvalidScriptException("Es wurde kein Prozedurname angegeben, aber es steht eine Prozedur im -- ##PROCEDURE## Teil.");
						}
						if ((!string.IsNullOrEmpty(viewInfo.ProcedureName) || !string.IsNullOrEmpty(viewInfo.CompleteProcedure)) && !viewInfo.OptimizeCriterias.DoClientSplit && !viewInfo.OptimizeCriterias.DoCompCodeSplit && !viewInfo.OptimizeCriterias.DoFYearSplit && viewInfo.ParameterDictionary.Count == 0)
						{
							throw new InvalidScriptException("Es wurde ein Prozedurname oder eine Prozedur angegeben, aber es gibt keine Parameter für die Prozedur.");
						}
						return viewInfo;
					}
				}
				else
				{
					if (line.Trim().StartsWith("#") || line.Trim().StartsWith("--"))
					{
						continue;
					}
					try
					{
						switch (state)
						{
						case EState.VIEWNAME:
						{
							string[] sName = line.Trim().Split(';');
							if (sName.Length == 1)
							{
								viewInfo.ProcedureName = sName[0];
								break;
							}
							viewInfo.Name = sName[0];
							for (int i = 1; i < sName.Length; i++)
							{
								viewInfo.Descriptions.Add(sName[i]);
							}
							break;
						}
						case EState.OPTIMIZE:
						{
							string[] sParts = line.Trim().Split(';');
							if (sParts.Count() < 2 || sParts.Count() > 3)
							{
								throw new InvalidScriptException("Fehler in den Optimierungkriterien: " + line.Trim() + " (Korrektes Format: 'Kriterium;Spalte;Formel')");
							}
							switch (sParts[0].Trim().ToUpper())
							{
							case "MANDT":
								viewInfo.OptimizeCriterias.DoClientSplit = true;
								viewInfo.OptimizeCriterias.ClientField = sParts[1].Trim();
								break;
							case "BUKRS":
								viewInfo.OptimizeCriterias.DoCompCodeSplit = true;
								viewInfo.OptimizeCriterias.CompCodeField = sParts[1].Trim();
								break;
							case "GJAHR":
								viewInfo.OptimizeCriterias.DoFYearSplit = true;
								viewInfo.OptimizeCriterias.GJahrField = sParts[1].Trim();
								if (sParts.Count() == 3 && sParts[2].ToLower() == "true")
								{
									viewInfo.OptimizeCriterias.YearRequired = true;
								}
								break;
							default:
								throw new InvalidScriptException("Ungültiges Optimierungskriterium: " + sParts[0].Trim() + " (Erlaubte Werte: 'MANDT', 'BUKRS', 'GJAHR')");
							}
							break;
						}
						case EState.LANG:
						{
							string[] array3 = line.Trim().Split(';');
							foreach (string lang in array3)
							{
								if (!string.IsNullOrEmpty(lang) && !viewInfo.Languages.Contains(lang))
								{
									viewInfo.Languages.Add(lang);
								}
							}
							break;
						}
						case EState.DICTIONARY:
						{
							string[] sDictEntry = line.Trim().Split(';');
							EnsureDefaultLanguage(viewInfo);
							if (sDictEntry.Count() > viewInfo.Languages.Count + 1)
							{
								throw new InvalidScriptException(string.Format(Resource.InvalidDictionary, line.Trim()));
							}
							Dictionary<string, string> langToDescr = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
							viewInfo.ColumnDictionary[sDictEntry[0]] = langToDescr;
							if (sDictEntry.Count() != 1)
							{
								for (int j = 1; j < sDictEntry.Length; j++)
								{
									langToDescr[viewInfo.Languages[j - 1]] = sDictEntry[j];
								}
							}
							break;
						}
						case EState.KEYS:
						case EState.VIEW:
						case EState.PROCEDURE:
						case EState.PROCEDURETEMP:
						case EState.TEMPSTATEMENTS:
							tmpStatement.Append(line).Append(" ").Append(Environment.NewLine);
							break;
						case EState.OBJECTTYPE:
						{
							string[] objecttype = line.Trim().Split(';');
							if (viewInfo.ObjectType != null)
							{
								throw new InvalidScriptException(Resource.OneObjectType);
							}
							if (viewInfo.Languages == null || viewInfo.Languages.Count == 0)
							{
								throw new InvalidScriptException(Resource.NoLanguageDefined);
							}
							if (objecttype.Length == 0 || objecttype.Length > viewInfo.Languages.Count + 1)
							{
								throw new InvalidScriptException(string.Format(Resource.InvalidObjectType, line.Trim()));
							}
							viewInfo.ObjectType = new ObjectType(objecttype[0]);
							Dictionary<string, string> langToDescrObjectType = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
							viewInfo.ObjectType.LangToDescription = langToDescrObjectType;
							if (objecttype.Count() != 1)
							{
								for (int k = 1; k < objecttype.Length; k++)
								{
									langToDescrObjectType[viewInfo.Languages[k - 1]] = objecttype[k];
								}
							}
							break;
						}
						case EState.PROCEDUREPARAMS:
						{
							string[] dictEntry = line.Trim().Split(';');
							EnsureDefaultLanguage(viewInfo);
							if (dictEntry.Length > viewInfo.Languages.Count + 2)
							{
								throw new InvalidScriptException(string.Format(Resource.InvalidParameter, line.Trim()));
							}
							Dictionary<string, string> langToDescr2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
							ProcedureParameter param;
							if (viewInfo.ParameterDictionary.ContainsKey(dictEntry[0]))
							{
								param = viewInfo.ParameterDictionary[dictEntry[0]];
							}
							else
							{
								param = new ProcedureParameter(dictEntry[0]);
								viewInfo.ParameterDictionary[dictEntry[0]] = param;
							}
							param.LangToDescription = langToDescr2;
							try
							{
								string typeString = dictEntry[1];
								if (typeString.StartsWith("_"))
								{
									param.TypeModifier = 1;
									typeString = typeString.Remove(0, 1);
								}
								param.TypeFromString(typeString);
							}
							catch (Exception ex2)
							{
								throw new InvalidScriptException(ex2.Message);
							}
							if (dictEntry.Count() != 2)
							{
								for (int l = 2; l < dictEntry.Length; l++)
								{
									langToDescr2[viewInfo.Languages[l - 2]] = dictEntry[l];
								}
							}
							break;
						}
						case EState.PROCEDUREPARAMREFERENCE:
						{
							string[] dictEntry2 = line.Trim().Split(';');
							if (dictEntry2.Length > 5)
							{
								throw new InvalidScriptException(string.Format(Resource.InvalidParameterReference, line.Trim()));
							}
							ProcedureParameter param2;
							if (viewInfo.ParameterDictionary.ContainsKey(dictEntry2[0]))
							{
								param2 = viewInfo.ParameterDictionary[dictEntry2[0]];
							}
							else
							{
								param2 = new ProcedureParameter(dictEntry2[0]);
								viewInfo.ParameterDictionary[dictEntry2[0]] = param2;
							}
							param2.DatabaseName = (string.IsNullOrWhiteSpace(dictEntry2[1]) ? profile.ProjectDb.DataDbName : dictEntry2[1]);
							param2.TableName = dictEntry2[2];
							param2.ColumnName = dictEntry2[3];
							if (dictEntry2.Length == 5)
							{
								param2.DefaultValue = dictEntry2[4];
							}
							break;
						}
						case EState.REPORTPARAMS:
						{
							string[] dictEntry3 = line.Trim().Split(';');
							EnsureDefaultLanguage(viewInfo);
							if (dictEntry3.Length < 5)
							{
								throw new InvalidScriptException(string.Format(Resource.LessReportParamMetadataThanExpected, line));
							}
							if (dictEntry3.Length > viewInfo.Languages.Count + 5)
							{
								throw new InvalidScriptException(string.Format(Resource.MoreReportParamDescriptionThanLanguageSpecified, line));
							}
							string typeString2 = dictEntry3[3];
							int typeModifier = 0;
							if (typeString2.StartsWith("_"))
							{
								typeModifier = 1;
								typeString2 = typeString2.Remove(0, 1);
							}
							ReportParameter parameter = new ReportParameter(profile.ProjectDb.DataDbName, dictEntry3[0], dictEntry3[1], dictEntry3[2], TypeConverter.TypeFromString(typeString2), dictEntry3[4], typeModifier);
							if (dictEntry3.Count() > 5)
							{
								for (int m = 5; m < dictEntry3.Length; m++)
								{
									parameter.ParameterDescription[viewInfo.Languages[m - 5]] = dictEntry3[m];
								}
							}
							if (viewInfo.ReportParameterDictionary.ContainsKey(parameter.ParameterName))
							{
								throw new InvalidScriptException(string.Format(Resource.ParameterExists, parameter.ParameterName));
							}
							viewInfo.ReportParameterDictionary.Add(parameter.ParameterName, parameter);
							break;
						}
						case EState.REPORTFILTER:
							viewInfo.ReportFilterLines.Add(line.Trim());
							break;
						case EState.REPORTFILTERSPECIAL:
							viewInfo.ReportFilterSpecialLines.Add(line.Trim());
							break;
						case EState.NOTHING:
						case EState.FinalizeView:
							break;
						}
					}
					catch (Exception ex)
					{
						throw new ArgumentException($"Error in viewscript [{viewInfo.Name}] ({ex.Message})", ex);
					}
				}
			}
			if (state != 0)
			{
				throw new InvalidScriptException("Fehlendes -- ##END_VIEW## Tag.");
			}
			return viewInfo;
		}

		private static EState GetNextState(int rowNumber, string line)
		{
			return line.Trim().ToUpper() switch
			{
				"-- ##BEGIN_VIEW##" => EState.VIEWNAME, 
				"-- ##END_VIEW##" => EState.FinalizeView, 
				"-- ##OPTIMIZE##" => EState.OPTIMIZE, 
				"-- ##LANGUAGES##" => EState.LANG, 
				"-- ##DICTIONARY##" => EState.DICTIONARY, 
				"-- ##KEYS##" => EState.KEYS, 
				"-- ##VIEW##" => EState.VIEW, 
				"-- ##PROCEDURE##" => EState.PROCEDURE, 
				"-- ##PROCEDUREPARAMS##" => EState.PROCEDUREPARAMS, 
				"-- ##PROCEDUREPARAMREFERENCE##" => EState.PROCEDUREPARAMREFERENCE, 
				"-- ##PROCEDURETEMP##" => EState.PROCEDURETEMP, 
				"-- ##TEMPSTATEMENTS##" => EState.TEMPSTATEMENTS, 
				"-- ##REPORTPARAMS##" => EState.REPORTPARAMS, 
				"-- ##REPORTFILTER##" => EState.REPORTFILTER, 
				"-- ##REPORTFILTERSPECIAL##" => EState.REPORTFILTERSPECIAL, 
				"-- ##OBJECTTYPE##" => EState.OBJECTTYPE, 
				_ => throw new InvalidScriptException("Unexpected identifier in line " + rowNumber + ": " + line), 
			};
		}

		private static List<Index> GetIndizesFromViewInfo(IEnumerable<string> indizes)
		{
			List<Index> output = new List<Index>();
			foreach (string i in indizes)
			{
				if (!Regex.IsMatch(i, " *--"))
				{
					Index index = new Index(i);
					if (!output.Contains(index))
					{
						output.Add(index);
					}
				}
			}
			return output;
		}

		private static void EnsureDefaultLanguage(ViewInfo viewInfo)
		{
			if (viewInfo.Languages.Count == 0)
			{
				viewInfo.Languages.Add(Language.DefaultLanguage.CountryCode);
			}
		}
	}
}
