using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SystemDb.Internal;
using AV.Log;
using ProjectDb.Tables;
using ViewBuilderBusiness.Exceptions;
using ViewBuilderBusiness.Resources;
using ViewBuilderBusiness.Structures.Config;
using ViewBuilderCommon;
using log4net;
using Index = ViewBuilderCommon.Index;

namespace ViewBuilderBusiness.Persist
{
    public enum ObjectTypeEnum
    {
        am,
        ap,
        ar,
        bu,
        co,
        ek,
        fi,
        mm,
        other,
        pm,
        sd,
        ue,
        dart,
    }

    /// <summary>
    /// </summary>
    public static class ViewscriptParser
    {
        private const string BEGIN_VIEW = "-- ##BEGIN_VIEW##";
        private const string OPTIMIZE = "-- ##OPTIMIZE##";
        private const string LANG = "-- ##LANGUAGES##";
        private const string DICTIONARY = "-- ##DICTIONARY##";
        private const string KEYS = "-- ##KEYS##";
        private const string VIEW = "-- ##VIEW##";
        private const string PROCEDURE = "-- ##PROCEDURE##";
        private const string PROCEDURE_PARAMS = "-- ##PROCEDURE_PARAMS##";
        private const string PROCEDURE_PARAM_REFERENCE = "-- ##PROCEDURE_PARAM_REFERENCE##";
        private const string PROCEDURE_TEMP = "-- ##PROCEDURE_TEMP##";
        private const string TEMP_STATEMENTS = "-- ##TEMP_STATEMENTS##";
        private const string REPORT_PARAMS = "-- ##REPORT_PARAMS##";
        private const string REPORT_FILTER = "-- ##REPORT_FILTER##";
        private const string REPORT_FILTER_SPECIAL = "-- ##REPORT_FILTER_SPECIAL##";
        private const string END_VIEW = "-- ##END_VIEW##";
        private const string OBJECTTYPE = "-- ##OBJECTTYPE##";
        private static ILog log = LogHelper.GetLogger();

        public static List<Viewscript> Parse(FileInfo file, ProfileConfig Profile)
        {
            return Parse(file, Profile, false);
        }

        /// <summary>
        ///   Reads the specified file.
        /// </summary>
        /// <param name="file"> The file. </param>
        /// <returns> </returns>
        public static List<Viewscript> Parse(FileInfo file, ProfileConfig Profile, bool forViewComparison)
        {
            List<Viewscript> views = new List<Viewscript>();
            // limit file size to avoid reading of large non-viewscript sql files, e.g. database dumps
            if (file.Length > 1024*2048) return views;
            eState state = eState.NOTHING;

            using (StreamReader reader = new StreamReader(file.FullName, Encoding.Default))
            {
                string script = string.Empty;
                int rowNumber = 0;

                while (!reader.EndOfStream)
                {
                    rowNumber++;
                    string line = reader.ReadLine();
                    if (line.Trim().StartsWith("-- ##"))
                    {
                        state = GetNextState(rowNumber, line);
                    }
                    switch (state)
                    {
                        case eState.NOTHING:
                            // case ignored
                            break;
                        case eState.FinalizeView:
                            script += Environment.NewLine + line;
                            ViewInfo viewInfo = Parse(script, Profile, forViewComparison);

                            if (viewInfo != null)
                            {
                                var viewscript = new Viewscript(viewInfo)
                                                     {
                                                         Script = script,
                                                         FileInfo = file,
                                                     };
                                viewscript.SetIndizes(GetIndizesFromViewInfo(viewInfo.Indizes));
                                views.Add(viewscript);
                            }
                            script = string.Empty;
                            state = eState.NOTHING;
                            break;
                        default:
                            script += Environment.NewLine + line;
                            break;
                    }
                }
            }
            if (state != eState.NOTHING)
            {
                throw new InvalidScriptException("Fehlendes " + END_VIEW + " Tag.");
            }

            #region [ Object type as other if not exists or fallback based on viewname ]

            // DEVNOTE: create as default object type
            // if Profile.ViewboxDb == null the it was called from DbComparison and no need to create object type
            if (Profile.ViewboxDb != null && Profile.ViewboxDb.ConnectionManager != null &&
                !Profile.ViewboxDb.ConnectionManager.IsDisposed)
            {
                foreach (Viewscript viewscript in views.Where(v => v.ViewInfo.ObjectType == null))
                {
                    viewscript.ViewInfo.ObjectType = new ObjectType(ObjectTypeEnum.other.ToString());
                    if (viewscript.Name.IndexOf("_") > 0)
                    {
                        string prefix = viewscript.Name.Substring(0, viewscript.Name.IndexOf("_"));
                        foreach (ObjectTypeEnum objType in Enum.GetValues(typeof (ObjectTypeEnum)))
                        {
                            if (objType.ToString().ToLower() == prefix.ToLower())
                            {
                                viewscript.ViewInfo.ObjectType = new ObjectType(objType.ToString());
                                break;
                            }
                        }
                    }
                    ViewBuilder.CreateObjectType(new ViewCreateOptions(), viewscript, null, Profile);
                }
            }

            #endregion [ Object type as other if not exists or fallback based on viewname ]

            return views;
        }

        /// <summary>
        ///   Parses the specified script.
        /// </summary>
        /// <param name="script"> The script. </param>
        /// <returns> </returns>
        public static ViewInfo Parse(string script, ProfileConfig Profile, bool forViewComparison = false)
        {
            ViewInfo viewInfo = new ViewInfo();
            eState state = eState.NOTHING;
            string[] lines = script.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            int rowNumber = 0;
            StringBuilder tmpStatement = new StringBuilder();
            bool foundFirstLine = false;

            foreach (string line in lines)
            {
                rowNumber++;
                if (line.Trim().Length == 0) continue;
                if (!foundFirstLine)
                {
                    foundFirstLine = true;
                    // file is no viewscript file
                    if (!line.Trim().StartsWith("-- ##")) return null;
                }
                if (line.Trim().StartsWith("-- ##"))
                {
                    if (state == eState.KEYS)
                    {
                        string[] keys = tmpStatement.ToString().Split(';');
                        foreach (string sKey in keys)
                            if (sKey.Trim().Length > 0)
                            {
                                viewInfo.Indizes.Add(sKey.Trim());
                                if (forViewComparison)
                                {
                                    viewInfo.CompleteStatement += sKey + ";";
                                }
                            }
                    }
                    else if (state == eState.VIEW)
                    {
                        viewInfo.CompleteStatement += tmpStatement.ToString();
                        //string[] statements = tmpStatement.ToString().Split(';');
                        //Regex splitRegex = new Regex(";",RegexOptions.IgnoreCase);
                        //string[] statements = splitRegex.Split(tmpStatement.ToString());
                        var statements = ViewInfo.SplitStatements(tmpStatement.ToString());
                        foreach (string statement in statements)
                            if (statement.Trim().Length > 0)
                                viewInfo.Statements.Add(statement.Trim());
                    }
                    else if (state == eState.PROCEDURE)
                    {
                        viewInfo.SetCompleteProcedure(tmpStatement.ToString());
                        if (forViewComparison)
                        {
                            viewInfo.CompleteStatement += tmpStatement + ";";
                        }
                    }
                    else if (state == eState.PROCEDURE_TEMP)
                        viewInfo.ProcedureCreateTempTables = tmpStatement.ToString();
                    else if (state == eState.TEMP_STATEMENTS)
                        viewInfo.TempStatementsComplete = tmpStatement.ToString();
                    tmpStatement.Clear();
                    state = GetNextState(rowNumber, line.Trim());
                    if (state == eState.FinalizeView)
                    {
                        string error = viewInfo.FinalizeViewInfo();
                        if (!string.IsNullOrEmpty(error)) throw new InvalidScriptException(error);
                        //Do some consistency checking
                        if (!string.IsNullOrEmpty(viewInfo.ProcedureName) &&
                            string.IsNullOrEmpty(viewInfo.CompleteProcedure))
                            throw new InvalidScriptException(
                                "Es wurde ein Prozedurname angegeben, aber es steht keine Prozedur im -- ##PROCEDURE## Teil.");
                        if (string.IsNullOrEmpty(viewInfo.ProcedureName) &&
                            (!string.IsNullOrEmpty(viewInfo.CompleteProcedure)))
                            throw new InvalidScriptException(
                                "Es wurde kein Prozedurname angegeben, aber es steht eine Prozedur im -- ##PROCEDURE## Teil.");
                        if ((!string.IsNullOrEmpty(viewInfo.ProcedureName) ||
                             !string.IsNullOrEmpty(viewInfo.CompleteProcedure)) &&
                            (!viewInfo.OptimizeCriterias.DoClientSplit && !viewInfo.OptimizeCriterias.DoCompCodeSplit &&
                             !viewInfo.OptimizeCriterias.DoFYearSplit && viewInfo.ParameterDictionary.Count == 0))
                            throw new InvalidScriptException(
                                "Es wurde ein Prozedurname oder eine Prozedur angegeben, aber es gibt keine Parameter für die Prozedur.");
                        return viewInfo;
                    }
                    continue;
                }
                // ignore comment lines
                if (line.Trim().StartsWith("#") || line.Trim().StartsWith("--")) continue;
                try
                {
                    switch (state)
                    {
                        case eState.NOTHING:
                            break; // ignore all text between begin_view and end_view blocks
                        case eState.VIEWNAME:
                            string[] sName = line.Trim().Split(';');
                            //if (sName.Count() > 3) {
                            //    throw new InvalidScriptException("Fehler im Viewnamen (korrektes Format: 'Name;Beschreibung_Sprache1; Beschreibung_Sprache2').");
                            //} else {
                            if (sName.Length == 1) viewInfo.ProcedureName = sName[0];
                            else
                            {
                                viewInfo.Name = sName[0];
                                for (int i = 1; i < sName.Length; ++i)
                                {
                                    viewInfo.Descriptions.Add(sName[i]);
                                }
                                //if (sName.Count() >= 3)
                                //    viewInfo.ProcedureName = sName[2];
                                //if (sName.Count() >= 2) {
                                //    viewInfo.Description = sName[1];
                                //}
                                //else {
                                //    viewInfo.Description = string.Empty;
                                //}
                            }
                            //state = eState.NOTHING;
                            //}
                            break;
                        case eState.OPTIMIZE:
                            string[] sParts = line.Trim().Split(';');
                            if ((sParts.Count() < 2) || (sParts.Count() > 3))
                            {
                                throw new InvalidScriptException("Fehler in den Optimierungkriterien: " + line.Trim() +
                                                                 " (Korrektes Format: 'Kriterium;Spalte;Formel')");
                            }
                            else
                            {
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
                                            viewInfo.OptimizeCriterias.YearRequired = true;
                                        break;
                                    default:
                                        throw new InvalidScriptException("Ungültiges Optimierungskriterium: " +
                                                                         sParts[0].Trim() +
                                                                         " (Erlaubte Werte: 'MANDT', 'BUKRS', 'GJAHR')");
                                }
                            }
                            break;
                        case eState.LANG:
                            string[] langEntries = line.Trim().Split(';');
                            foreach (string lang in langEntries)
                            {
                                if (string.IsNullOrEmpty(lang)) continue;
                                if (!viewInfo.Languages.Contains(lang))
                                    viewInfo.Languages.Add(lang);
                            }
                            break;
                        case eState.DICTIONARY:
                            {
                                string[] sDictEntry = line.Trim().Split(';');
                                EnsureDefaultLanguage(viewInfo);
                                if (sDictEntry.Count() > viewInfo.Languages.Count + 1)
                                {
                                    throw new InvalidScriptException(string.Format(Resource.InvalidDictionary,
                                                                                   line.Trim()));
                                }
                                Dictionary<string, string> langToDescr =
                                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                viewInfo.ColumnDictionary[sDictEntry[0]] = langToDescr;
                                if (sDictEntry.Count() != 1)
                                {
                                    //Add descriptions for all languages
                                    for (int i = 1; i < sDictEntry.Length; ++i)
                                        langToDescr[viewInfo.Languages[i - 1]] = sDictEntry[i];
                                }
                            }
                            break;
                        case eState.KEYS:
                        case eState.VIEW:
                        case eState.PROCEDURE:
                        case eState.PROCEDURE_TEMP:
                        case eState.TEMP_STATEMENTS:
                            tmpStatement.Append(line).Append(" ").Append(Environment.NewLine);
                            break;
                        case eState.OBJECTTYPE:
                            string[] objecttype = line.Trim().Split(';');
                            if (viewInfo.ObjectType != null)
                            {
                                throw new InvalidScriptException(Resource.OneObjectType);
                            }
                            // this region should come after the ##LANGUAGES## region
                            if (viewInfo.Languages == null || viewInfo.Languages.Count == 0)
                            {
                                throw new InvalidScriptException(Resource.NoLanguageDefined);
                            }
                            if (objecttype.Length == 0 || objecttype.Length > viewInfo.Languages.Count + 1)
                            {
                                throw new InvalidScriptException(string.Format(Resource.InvalidObjectType, line.Trim()));
                            }
                            viewInfo.ObjectType = new ObjectType(objecttype[0]);
                            Dictionary<string, string> langToDescrObjectType =
                                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            viewInfo.ObjectType.LangToDescription = langToDescrObjectType;
                            if (objecttype.Count() != 1)
                            {
                                //Add descriptions for all languages
                                for (int i = 1; i < objecttype.Length; ++i)
                                    langToDescrObjectType[viewInfo.Languages[i - 1]] = objecttype[i];
                            }
                            break;
                        case eState.PROCEDURE_PARAMS:
                            {
                                string[] dictEntry = line.Trim().Split(';');
                                EnsureDefaultLanguage(viewInfo);
                                if (dictEntry.Length > viewInfo.Languages.Count + 2)
                                {
                                    throw new InvalidScriptException(string.Format(Resource.InvalidParameter,
                                                                                   line.Trim()));
                                }
                                Dictionary<string, string> langToDescr =
                                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                //ViewBuilderCommon.ProcedureParameter param = new ViewBuilderCommon.ProcedureParameter(dictEntry[0]);
                                //viewInfo.ParameterDictionary[dictEntry[0]] = param;
                                ProcedureParameter param = null;
                                if (viewInfo.ParameterDictionary.ContainsKey(dictEntry[0]))
                                    param = viewInfo.ParameterDictionary[dictEntry[0]];
                                else
                                {
                                    param = new ProcedureParameter(dictEntry[0]);
                                    viewInfo.ParameterDictionary[dictEntry[0]] = param;
                                }
                                param.LangToDescription = langToDescr;
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
                                catch (Exception e)
                                {
                                    throw new InvalidScriptException(e.Message);
                                }
                                if (dictEntry.Count() != 2)
                                {
                                    //Add descriptions for all languages
                                    for (int i = 2; i < dictEntry.Length; ++i)
                                        langToDescr[viewInfo.Languages[i - 2]] = dictEntry[i];
                                }
                                break;
                            }
                        case eState.PROCEDURE_PARAM_REFERENCE:
                            {
                                string[] dictEntry = line.Trim().Split(';');
                                if (dictEntry.Length > 5)
                                {
                                    throw new InvalidScriptException(string.Format(Resource.InvalidParameterReference,
                                                                                   line.Trim()));
                                }
                                ProcedureParameter param = null;
                                if (viewInfo.ParameterDictionary.ContainsKey(dictEntry[0]))
                                    param = viewInfo.ParameterDictionary[dictEntry[0]];
                                else
                                {
                                    param = new ProcedureParameter(dictEntry[0]);
                                    viewInfo.ParameterDictionary[dictEntry[0]] = param;
                                }
                                param.DatabaseName = string.IsNullOrWhiteSpace(dictEntry[1])
                                                         ? Profile.ProjectDb.DataDbName
                                                         : dictEntry[1];
                                param.TableName = dictEntry[2];
                                param.ColumnName = dictEntry[3];
                                // if default value is present
                                if (dictEntry.Length == 5)
                                {
                                    param.DefaultValue = dictEntry[4];
                                }
                                break;
                            }
                        case eState.REPORT_PARAMS:
                            {
                                const int paramRelatedSctiptMetadataCount = 5;
                                string[] dictEntry = line.Trim().Split(';');
                                EnsureDefaultLanguage(viewInfo);
                                if (dictEntry.Length < paramRelatedSctiptMetadataCount)
                                    throw new InvalidScriptException(
                                        string.Format(Resource.LessReportParamMetadataThanExpected, line));
                                if (dictEntry.Length > viewInfo.Languages.Count + paramRelatedSctiptMetadataCount)
                                    throw new InvalidScriptException(
                                        string.Format(Resource.MoreReportParamDescriptionThanLanguageSpecified, line));
                                string typeString = dictEntry[3];
                                int typeModifier = 0;
                                if (typeString.StartsWith("_"))
                                {
                                    typeModifier = 1;
                                    typeString = typeString.Remove(0, 1);
                                }
                                ReportParameter parameter = new ReportParameter(Profile.ProjectDb.DataDbName,
                                                                                dictEntry[0], dictEntry[1], dictEntry[2],
                                                                                TypeConverter.TypeFromString(typeString),
                                                                                dictEntry[4], typeModifier);
                                if (dictEntry.Count() > paramRelatedSctiptMetadataCount)
                                {
                                    //Add descriptions for all languages
                                    for (int i = paramRelatedSctiptMetadataCount; i < dictEntry.Length; ++i)
                                        parameter.ParameterDescription[
                                            viewInfo.Languages[i - paramRelatedSctiptMetadataCount]] = dictEntry[i];
                                }
                                if (viewInfo.ReportParameterDictionary.ContainsKey(parameter.ParameterName))
                                    throw new InvalidScriptException(string.Format(Resource.ParameterExists,
                                                                                   parameter.ParameterName));
                                viewInfo.ReportParameterDictionary.Add(parameter.ParameterName, parameter);
                                break;
                            }
                        case eState.REPORT_FILTER:
                            {
                                viewInfo.ReportFilterLines.Add(line.Trim());
                                break;
                            }
                        case eState.REPORT_FILTER_SPECIAL:
                            {
                                viewInfo.ReportFilterSpecialLines.Add(line.Trim());
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    string msg = string.Format("Error in viewscript [{0}] ({1})", viewInfo.Name, ex.Message);
                    throw new ArgumentException(msg, ex);
                }
            }
            if (state != eState.NOTHING)
            {
                throw new InvalidScriptException("Fehlendes " + END_VIEW + " Tag.");
            }
            return viewInfo;
        }

        /// <summary>
        ///   Gets the next state which should be procceded.
        /// </summary>
        /// <param name="rowNumber"> The row number. </param>
        /// <param name="line"> The line. </param>
        /// <returns> </returns>
        private static eState GetNextState(int rowNumber, string line)
        {
            switch (line.Trim().ToUpper())
            {
                case BEGIN_VIEW:
                    return eState.VIEWNAME;
                case END_VIEW:
                    return eState.FinalizeView;
                case OPTIMIZE:
                    return eState.OPTIMIZE;
                case LANG:
                    return eState.LANG;
                case DICTIONARY:
                    return eState.DICTIONARY;
                case KEYS:
                    return eState.KEYS;
                case VIEW:
                    return eState.VIEW;
                case PROCEDURE:
                    return eState.PROCEDURE;
                case PROCEDURE_PARAMS:
                    return eState.PROCEDURE_PARAMS;
                case PROCEDURE_PARAM_REFERENCE:
                    return eState.PROCEDURE_PARAM_REFERENCE;
                case PROCEDURE_TEMP:
                    return eState.PROCEDURE_TEMP;
                case TEMP_STATEMENTS:
                    return eState.TEMP_STATEMENTS;
                case REPORT_PARAMS:
                    return eState.REPORT_PARAMS;
                case REPORT_FILTER:
                    return eState.REPORT_FILTER;
                case REPORT_FILTER_SPECIAL:
                    return eState.REPORT_FILTER_SPECIAL;
                case OBJECTTYPE:
                    return eState.OBJECTTYPE;
                default:
                    throw new InvalidScriptException("Unexpected identifier in line " + rowNumber + ": " + line);
            }
        }

        /// <summary>
        ///   Ignores the line.
        /// </summary>
        /// <param name="sLine"> The s line. </param>
        /// <returns> </returns>
        private static bool IgnoreLine(string sLine)
        {
            if (sLine.Length == 0)
                return true;
            if (sLine.StartsWith("-- ##"))
                return false;
            if (sLine.StartsWith("#"))
                return true;
            if (sLine.StartsWith("-- "))
                return true;
            return false;
        }

        private static List<Index> GetIndizesFromViewInfo(List<string> indizes)
        {
            var output = new List<Index>();
            foreach (var i in indizes)
            {
                if (Regex.IsMatch(i, " *--"))
                    continue;
                var index = new Index(i);
                if (!output.Contains(index)) output.Add(index);
            }
            return output;
        }

        #region [ Helper methods ]

        /// <summary>
        ///   Ensures the default language to be instrted into the viewInfo
        /// </summary>
        /// <param name="viewInfo"> viewInfo to be ensured </param>
        private static void EnsureDefaultLanguage(ViewInfo viewInfo)
        {
            if (viewInfo.Languages.Count == 0) viewInfo.Languages.Add(Language.DefaultLanguage.CountryCode);
        }

        #endregion [ Helper methods ]

        #region Nested type: eState

        private enum eState
        {
            NOTHING,
            VIEWNAME,
            OPTIMIZE,
            LANG,
            DICTIONARY,
            KEYS,
            VIEW,
            PROCEDURE,
            PROCEDURE_PARAMS,
            PROCEDURE_TEMP,
            FinalizeView,
            TEMP_STATEMENTS,
            REPORT_PARAMS,
            REPORT_FILTER,
            REPORT_FILTER_SPECIAL,
            PROCEDURE_PARAM_REFERENCE,
            OBJECTTYPE,
        }

        #endregion
    }
}