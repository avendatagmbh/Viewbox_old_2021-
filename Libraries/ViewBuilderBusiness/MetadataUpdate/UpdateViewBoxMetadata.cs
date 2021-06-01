using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using SystemDb;
using SystemDb.Internal;
using AV.Log;
using DbAccess;
using Utils;
using ViewBuilderBusiness.Persist;
using ViewBuilderBusiness.Resources;
using ViewBuilderBusiness.Structures.Config;
using log4net;

namespace ViewBuilderBusiness.MetadataUpdate
{
    public class UpdateViewBoxMetadata
    {
        internal static ILog log = LogHelper.GetLogger();

        public static void UpdateLanguages(ProfileConfig profile, IDatabase conn, ProgressCalculator progress = null)
        {
            using (NDC.Push(LogHelper.GetNamespaceContext()))
            {
                try
                {
                    //using (IDatabase conn = profile.ViewboxDb.ConnectionManager.GetConnection()) {
                    Dictionary<int, string> languageKeys = new Dictionary<int, string>();
                    using (
                        IDataReader rdr =
                            conn.ExecuteReader("SELECT id," + conn.Enquote("country_code") + " FROM languages"))
                    {
                        while (rdr.Read())
                        {
                            languageKeys.Add(rdr.GetInt32(0), rdr[1].ToString());
                        }
                    }
                    if (progress != null)
                    {
                        progress.SetStep(0);
                        progress.SetWorkSteps(1, false);
                    }
                    foreach (KeyValuePair<string, CultureInfo> language in Language.Languages)
                    {
                        bool exists = languageKeys.Any(l => l.Value.ToLower() == language.Key);

                        if (!exists)
                        {
                            log.Info(string.Format("Inserting language {0}", language.Key));
                            int maxId = 1;
                            string maxIdCommnd = "SELECT IFNULL(MAX(id),0)+1 FROM languages";
                            using (IDataReader rdr = conn.ExecuteReader(maxIdCommnd))
                            {
                                if (rdr.Read())
                                {
                                    maxId = rdr.GetInt32(0);
                                }
                            }
                            string motto = SystemDb.Resources.Resources.ResourceManager.GetString("motto",
                                                                                                  new CultureInfo(
                                                                                                      language.Key));
                            string languageName = SystemDb.Resources.Resources.ResourceManager.GetString(
                                "languageName", new CultureInfo(language.Key));
                            string insertLngCommand =
                                string.Format(
                                    "INSERT INTO languages (id, country_code, language_name, language_motto) VALUES ({0}, @lng, @lngName, @lngMotto)",
                                    maxId);
                            InsertLanguage(insertLngCommand, language.Key, languageName, motto, conn);
                        }
                        //SystemDb.Resources.Resources.motto
                    }
                    //}
                    if (progress != null)
                    {
                        progress.StepDone();
                    }
                    if (progress != null && progress.CancellationPending)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                    {
                        log.Error(ex.Message, ex);
                    }
                    throw;
                }
            }
        }

        public static void UpdatePropertyTexts(ProfileConfig profile, IDatabase conn, ProgressCalculator progress = null)
        {
            using (NDC.Push(LogHelper.GetNamespaceContext()))
            {
                try
                {
                    //using (IDatabase conn = profile.ViewboxDb.ConnectionManager.GetConnection()) {
                    Dictionary<int, string> propertyKeys = new Dictionary<int, string>();
                    using (IDataReader rdr = conn.ExecuteReader("SELECT id," + conn.Enquote("key") + " FROM properties")
                        )
                    {
                        while (rdr.Read())
                        {
                            propertyKeys.Add(rdr.GetInt32(0), rdr[1].ToString());
                        }
                    }
                    if (progress != null)
                    {
                        progress.SetStep(0);
                        progress.SetWorkSteps(Language.Languages.Count*propertyKeys.Count, false);
                    }
                    foreach (KeyValuePair<string, CultureInfo> language in Language.Languages)
                    {
                        foreach (KeyValuePair<int, string> propertyKey in propertyKeys)
                        {
                            string text = Resource.ResourceManager.GetString(propertyKey.Value, language.Value);
                            string name = Resource.ResourceManager.GetString(propertyKey.Value + "_name", language.Value);
                            if (string.IsNullOrWhiteSpace(text))
                                throw new NotImplementedException(
                                    string.Format("Property text for property [{0}] is missing in the resource.",
                                                  propertyKey));
                            if (string.IsNullOrWhiteSpace(name))
                                throw new NotImplementedException(
                                    string.Format("Property name for property [{0}] is missing in the resource.",
                                                  propertyKey));
                            string info = string.Format("Inserting property texts (language: {0})", language.Key);
                            if (progress != null)
                            {
                                progress.Description = info;
                            }
                            bool exists =
                                Convert.ToInt32(
                                    conn.ExecuteScalar("SELECT COUNT(" + conn.Enquote("id") +
                                                       ") FROM property_texts WHERE " + conn.Enquote("ref_id") + " = " +
                                                       propertyKey.Key + " AND " + conn.Enquote("country_code") + " = '" +
                                                       language.Key + "'")) > 0;
                            if (exists)
                            {
                                log.Info(string.Format("Updating property texts (language: {0}, property: {1})",
                                                       language.Key, propertyKey));
                                string updatePropCommand =
                                    string.Format(
                                        "UPDATE property_texts SET {2} = @name, {3} = @value WHERE {0} = @refId AND {1} = @lng",
                                        conn.Enquote("ref_id"), conn.Enquote("country_code"), conn.Enquote("name"),
                                        conn.Enquote("text"));
                                InsertUpdatePropertyText(updatePropCommand, name, text, propertyKey.Key, language.Key,
                                                         conn);
                            }
                            else
                            {
                                log.Info(string.Format("Inserting property texts (language: {0}, property: {1})",
                                                       language.Key, propertyKey));
                                string insertPropCommand =
                                    string.Format("INSERT INTO property_texts ({0},{1},{2},{3},{4}) ({5}) ",
                                                  conn.Enquote("id"), conn.Enquote("ref_id"),
                                                  conn.Enquote("country_code"), conn.Enquote("name"),
                                                  conn.Enquote("text"),
                                                  "SELECT IFNULL(MAX(id),0) + 1, @refId, @lng, @name, @value FROM property_texts");
                                InsertUpdatePropertyText(insertPropCommand, name, text, propertyKey.Key, language.Key,
                                                         conn);
                            }
                            if (progress != null)
                            {
                                progress.StepDone();
                            }
                            if (progress != null && progress.CancellationPending)
                            {
                                return;
                            }
                        }
                    }
                    //}
                }
                catch (Exception ex)
                {
                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                    {
                        log.Error(ex.Message, ex);
                    }
                    throw;
                }
            }
        }

        public static void UpdateOptimizationTexts(ProfileConfig profile, IDatabase conn,
                                                   ProgressCalculator progress = null)
        {
            using (NDC.Push(LogHelper.GetNamespaceContext()))
            {
                try
                {
                    //using (IDatabase conn = profile.ViewboxDb.ConnectionManager.GetConnection()) {
                    string updateOptGroupCommand =
                        string.Format(
                            "UPDATE optimization_group_texts SET {0} = @value WHERE country_code = @lng AND ref_id IN (SELECT id FROM optimization_groups WHERE {1} = @type )",
                            conn.Enquote("text"), conn.Enquote("type"));
                    string insertOptGroupCommand =
                        string.Format(
                            "INSERT INTO optimization_group_texts (id, ref_id, country_code, {0}) SELECT (SELECT IFNULL(MAX(id),0)+1 FROM optimization_group_texts), (SELECT IFNULL(MAX(id),0) FROM optimization_groups WHERE {1} = @type ), @lng, @value",
                            conn.Enquote("text"), conn.Enquote("type"));
                    //string insertOptGroupCommand = string.Format("INSERT INTO optimization_group_texts (id, ref_id, country_code, {0}) SELECT (SELECT MAX(id)+1 FROM optimization_group_texts), id, @lng, @value FROM optimization_groups WHERE {1} = @type", conn.Enquote("text"), conn.Enquote("type"));
                    if (progress != null)
                    {
                        progress.SetStep(0);
                        progress.SetWorkSteps(Language.Languages.Count, false);
                    }
                    foreach (KeyValuePair<string, CultureInfo> language in Language.Languages)
                    {
                        string info = string.Format("Inserting optimization texts (language: {0})", language.Key);
                        if (progress != null)
                        {
                            progress.Description = info;
                        }
                        log.Info(info);
                        // get optimizations for which text on the given language is not exsists 
                        string textNotExistsCommand =
                            string.Format(
                                "SELECT id, value FROM optimizations as opt WHERE not exists (SELECT id FROM optimization_texts WHERE ref_id = opt.id AND country_code = '{0}')",
                                language.Key);
                        List<Tuple<int, string>> textNotExists = new List<Tuple<int, string>>();
                        using (IDataReader rdr = conn.ExecuteReader(textNotExistsCommand))
                        {
                            while (rdr.Read())
                            {
                                textNotExists.Add(new Tuple<int, string>(rdr.GetInt32(0),
                                                                         rdr[1] == DBNull.Value
                                                                             ? null
                                                                             : rdr.GetString(1)));
                            }
                        }
                        log.Info(string.Format("Inserting missing optimization text (language: {0})", language.Key));
                        foreach (Tuple<int, string> textNotExist in textNotExists)
                        {
                            string insertCommand =
                                string.Format("INSERT INTO optimization_texts ({0},{1},{2},{3}) ({4}) ",
                                              conn.Enquote("id"), conn.Enquote("ref_id"), conn.Enquote("country_code"),
                                              conn.Enquote("text"),
                                              string.Format(
                                                  "SELECT IFNULL(MAX(id),0) + 1, @refId, @lng, @value FROM optimization_texts"));
                            InsertOptimizationText(insertCommand, textNotExist.Item2, textNotExist.Item1, language.Key,
                                                   conn);
                        }
                        // get optimization texts where the value is NULL (it means "all") 
                        List<int> nullIds = new List<int>();
                        using (
                            IDataReader rdr =
                                conn.ExecuteReader("SELECT " + conn.Enquote("id") + " FROM optimization_texts WHERE " +
                                                   conn.Enquote("text") + " IS NULL AND " + conn.Enquote("country_code") +
                                                   " = '" + language.Key + "'"))
                        {
                            while (rdr.Read())
                            {
                                nullIds.Add(rdr.GetInt32(0));
                            }
                        }
                        log.Info(string.Format("Updating \"All\" optimization text (language: {0})", language.Key));
                        foreach (int nullId in nullIds)
                        {
                            string text = Resource.ResourceManager.GetString("All", language.Value);
                            UpdateOptimizationText(
                                string.Format("UPDATE optimization_texts SET {0} = @value WHERE id = @id",
                                              conn.Enquote("text")), text, nullId, conn);
                        }
                        if (progress != null && progress.CancellationPending)
                        {
                            return;
                        }
                        log.Info(string.Format("Updating optimization group texts (language: {0})", language.Key));
                        foreach (OptimizationType type in Enum.GetValues(typeof (OptimizationType)))
                        {
                            string groupText = string.Empty;
                            switch (type)
                            {
                                case OptimizationType.None:
                                case OptimizationType.NotSet:
                                    continue;
                                case OptimizationType.System:
                                    groupText = Resource.ResourceManager.GetString("optSystem", language.Value);
                                    break;
                                case OptimizationType.SplitTable:
                                    groupText = Resource.ResourceManager.GetString("optCompanyCode", language.Value);
                                    break;
                                case OptimizationType.SortColumn:
                                    groupText = Resource.ResourceManager.GetString("optFinancialYear", language.Value);
                                    break;
                                case OptimizationType.IndexTable:
                                    groupText = Resource.ResourceManager.GetString("optClient", language.Value);
                                    break;
                                default:
                                    throw new NotImplementedException(
                                        string.Format("Updating optimization texts for {0} is not implemented!", type));
                            }
                            int result = InsertUpdateOptimizationGroupText(updateOptGroupCommand, groupText,
                                                                           language.Key, type, conn);
                            // insert if nothing was updated
                            if (result == 0)
                            {
                                InsertUpdateOptimizationGroupText(insertOptGroupCommand, groupText, language.Key, type,
                                                                  conn);
                            }
                        }
                        if (progress != null)
                        {
                            progress.StepDone();
                        }
                        if (progress != null && progress.CancellationPending)
                        {
                            return;
                        }
                    }
                    //}
                }
                catch (Exception ex)
                {
                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                    {
                        log.Error(ex.Message, ex);
                    }
                    throw;
                }
            }
        }

        public static void UpdateCategoryTexts(ProfileConfig profile, IDatabase conn, ProgressCalculator progress = null)
        {
            using (NDC.Push(LogHelper.GetNamespaceContext()))
            {
                try
                {
                    //using (IDatabase conn = profile.ViewboxDb.ConnectionManager.GetConnection()) {
                    if (progress != null)
                    {
                        progress.SetStep(0);
                        progress.SetWorkSteps(Language.Languages.Count, false);
                    }
                    foreach (KeyValuePair<string, CultureInfo> language in Language.Languages)
                    {
                        string text = Resource.ResourceManager.GetString("Category_General", language.Value);

                        if (string.IsNullOrWhiteSpace(text))
                            throw new NotImplementedException("General category text is missing in the resource.");
                        string info = string.Format("Updating category texts (language: {0})", language.Key);
                        if (progress != null)
                        {
                            progress.Description = info;
                        }
                        // DEVNOTE: only one default category exists with id = 0
                        bool exists =
                            Convert.ToInt32(
                                conn.ExecuteScalar("SELECT COUNT(" + conn.Enquote("id") + ") FROM category_texts WHERE " +
                                                   conn.Enquote("ref_id") + " = 0 AND " + conn.Enquote("country_code") +
                                                   " = '" + language.Key + "'")) > 0;
                        if (exists)
                        {
                            log.Info(info);
                            string updateCategoryCommand =
                                string.Format(
                                    "UPDATE category_texts SET {2} = @value WHERE {0} = @refId AND {1} = @lng",
                                    conn.Enquote("ref_id"), conn.Enquote("country_code"), conn.Enquote("text"));
                            InsertUpdateCategoryText(updateCategoryCommand, text, 0, language.Key, conn);
                        }
                        else
                        {
                            log.Info(info);
                            string insertCategoryCommand =
                                string.Format("INSERT INTO category_texts ({0},{1},{2},{3}) ({4}) ",
                                              conn.Enquote("id"), conn.Enquote("ref_id"), conn.Enquote("country_code"),
                                              conn.Enquote("text"),
                                              "SELECT IFNULL(MAX(id),0) + 1, @refId, @lng, @value FROM category_texts");
                            InsertUpdateCategoryText(insertCategoryCommand, text, 0, language.Key, conn);
                        }
                        if (progress != null)
                        {
                            progress.StepDone();
                        }
                        if (progress != null && progress.CancellationPending)
                        {
                            return;
                        }
                    }
                    //}
                }
                catch (Exception ex)
                {
                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                    {
                        log.Error(ex.Message, ex);
                    }
                    throw;
                }
            }
        }

        public static void UpdateIssueAndParameterLanguageTexts(ProfileConfig profile, IDatabase conn,
                                                                ProgressCalculator progress = null,
                                                                string tableNameToUpdate = null)
        {
            string fileName_param_texts = @"Resources\parameters_translated.csv";
            string fileName_issue_texts = @"Resources\issues_translated.csv";
            char textDelimiter = '"';
            char separator = ';';
            // DEVNOTE: the file encoding must be UTF8 !!!
            Encoding encoding = Encoding.UTF8;
            using (NDC.Push(LogHelper.GetNamespaceContext()))
            {
                try
                {
                    Dictionary<string, int> languageTextColumnMapping = new Dictionary<string, int>();
                    languageTextColumnMapping.Add(Language.Languages["en"].IetfLanguageTag, 6);
                    languageTextColumnMapping.Add(Language.Languages["de"].IetfLanguageTag, 7);
                    languageTextColumnMapping.Add(Language.Languages["fr"].IetfLanguageTag, 8);
                    languageTextColumnMapping.Add(Language.Languages["it"].IetfLanguageTag, 9);
                    languageTextColumnMapping.Add(Language.Languages["es"].IetfLanguageTag, 10);
                    Dictionary<LanguageMetadataEnum, int> metadataColumnMapping =
                        new Dictionary<LanguageMetadataEnum, int>();
                    metadataColumnMapping.Add(LanguageMetadataEnum.DatabaseName, 5);
                    metadataColumnMapping.Add(LanguageMetadataEnum.TableName, 3);
                    metadataColumnMapping.Add(LanguageMetadataEnum.ColumnName, 4);
                    metadataColumnMapping.Add(LanguageMetadataEnum.IssueName, 1);
                    metadataColumnMapping.Add(LanguageMetadataEnum.ParameterName, 2);
                    CsvReader reader = new CsvReader(fileName_param_texts)
                                           {
                                               Separator = separator,
                                               HeadlineInFirstRow = true,
                                               StringsOptionallyEnclosedBy = textDelimiter
                                           };
                    DataTable data = reader.GetCsvData(10000, encoding);
                    data.TableName = fileName_param_texts;
                    if (progress != null)
                    {
                        progress.SetStep(0);
                        progress.SetWorkSteps(Language.Languages.Count + 1, false);
                    }
                    ImportParameterText(conn, data, languageTextColumnMapping, metadataColumnMapping, profile, progress,
                                        tableNameToUpdate);
                    if (progress != null && progress.CancellationPending)
                    {
                        return;
                    }
                    languageTextColumnMapping = new Dictionary<string, int>();
                    languageTextColumnMapping.Add(Language.Languages["en"].IetfLanguageTag, 3);
                    languageTextColumnMapping.Add(Language.Languages["de"].IetfLanguageTag, 4);
                    languageTextColumnMapping.Add(Language.Languages["fr"].IetfLanguageTag, 5);
                    languageTextColumnMapping.Add(Language.Languages["it"].IetfLanguageTag, 6);
                    languageTextColumnMapping.Add(Language.Languages["es"].IetfLanguageTag, 7);
                    metadataColumnMapping = new Dictionary<LanguageMetadataEnum, int>();
                    metadataColumnMapping.Add(LanguageMetadataEnum.IssueName, 1);
                    CsvReader readerIssue = new CsvReader(fileName_issue_texts)
                                                {
                                                    Separator = separator,
                                                    HeadlineInFirstRow = true,
                                                    StringsOptionallyEnclosedBy = textDelimiter
                                                };
                    DataTable dataIssue = readerIssue.GetCsvData(10000, encoding);
                    dataIssue.TableName = fileName_issue_texts;
                    if (progress != null)
                    {
                        progress.SetStep(0);
                        progress.SetWorkSteps(dataIssue.Rows.Count, false);
                    }
                    ImportIssueText(conn, dataIssue, languageTextColumnMapping, metadataColumnMapping, profile, progress,
                                    tableNameToUpdate);
                }
                catch (Exception ex)
                {
                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                    {
                        log.Error(ex.Message, ex);
                    }
                    throw;
                }
            }
        }

        public static void UpdateObjectTypesTexts(ProfileConfig profile, IDatabase db,
                                                  ProgressCalculator progress = null)
        {
            // insert all known object types
            //using (IDatabase db = profile.ViewboxDb.ConnectionManager.GetConnection()) {
            if (progress != null)
            {
                progress.SetStep(0);
                progress.SetWorkSteps(Enum.GetValues(typeof (ObjectTypeEnum)).Length, false);
            }
            foreach (ObjectTypeEnum objType in Enum.GetValues(typeof (ObjectTypeEnum)))
            {
                string info = string.Format("Updating object type texts ({0})", objType.ToString());
                if (progress != null)
                {
                    progress.Description = info;
                }
                Dictionary<string, string> langDescr = new Dictionary<string, string>();
                foreach (KeyValuePair<string, CultureInfo> language in Language.Languages)
                {
                    string text = Resource.ResourceManager.GetString(objType.ToString(), language.Value);
                    if (string.IsNullOrWhiteSpace(text))
                        throw new NotImplementedException(
                            string.Format("Resource is missing for object type [{0}] on language [{1}]",
                                          objType.ToString(), language.Key));
                    langDescr.Add(language.Key, text);
                }
                profile.ViewboxDb.SaveObjectType(db, objType.ToString(), langDescr, true);
                if (progress != null)
                {
                    progress.StepDone();
                }
                if (progress != null && progress.CancellationPending)
                {
                    return;
                }
            }
            //}
        }

        #region [ Private methods ]

        private static void ImportIssueText(IDatabase conn, DataTable data,
                                            Dictionary<string, int> languageTextColumnMapping,
                                            Dictionary<LanguageMetadataEnum, int> metadataColumnMapping,
                                            ProfileConfig profile, ProgressCalculator progress = null,
                                            string tableNameToUpdate = null)
        {
            string status = string.Empty;
            const string updateCommand =
                @"UPDATE table_texts SET {0} = @value WHERE country_code = @lng AND ref_id = 
                                            (SELECT t.id FROM tables t
                                            WHERE t.{1} = @issueName)";
            const string insertCommand =
                @"INSERT table_texts (id,ref_id,country_code,{0}) (
                                            SELECT 
                                                (SELECT IFNULL(MAX(id),0)+1 FROM table_texts), 
                                                t.id,
                                                @lng,
                                                @value
                                            FROM tables t
                                            WHERE t.{1} = @issueName
                                            )";
            using (NDC.Push(LogHelper.GetNamespaceContext()))
            {
                try
                {
                    //using (IDatabase conn = profile.ViewboxDb.ConnectionManager.GetConnection()) {
                    status = string.Format("Updating issue descriptions from file [{0}]", data.TableName);
                    log.Info(status);
                    if (progress != null) progress.Description = status;
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        DataRow row = data.Rows[i];
                        string issueName = row[metadataColumnMapping[LanguageMetadataEnum.IssueName]].ToString();
                        if (tableNameToUpdate == null ||
                            string.Compare(issueName, tableNameToUpdate, StringComparison.InvariantCultureIgnoreCase) ==
                            0)
                        {
                            foreach (KeyValuePair<string, int> keyValuePair in languageTextColumnMapping)
                            {
                                string lng = keyValuePair.Key;
                                string text = row[keyValuePair.Value].ToString();
                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    if (
                                        InsertUpdateIssueText(
                                            string.Format(updateCommand, conn.Enquote("text"), conn.Enquote("name")),
                                            text, lng, issueName, conn) == 0)
                                        InsertUpdateIssueText(
                                            string.Format(insertCommand, conn.Enquote("text"), conn.Enquote("name")),
                                            text, lng, issueName, conn);
                                }
                            }
                        }
                        if (progress != null)
                        {
                            progress.StepDone();
                        }
                    }
                    //}
                }
                catch (Exception ex)
                {
                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                    {
                        log.Error(ex.Message, ex);
                    }
                    throw;
                }
            }
        }

        private static void ImportParameterText(IDatabase conn, DataTable data,
                                                Dictionary<string, int> languageTextColumnMapping,
                                                Dictionary<LanguageMetadataEnum, int> metadataColumnMapping,
                                                ProfileConfig profile, ProgressCalculator progress = null,
                                                string tableNameToUpdate = null)
        {
            string status = string.Empty;
            using (NDC.Push(LogHelper.GetNamespaceContext()))
            {
                try
                {
                    const string updateCommand =
                        @"UPDATE parameter_texts SET {0} = @value WHERE country_code = @lng AND ref_id = 
                                                    (SELECT p.id FROM parameter p
                                                    INNER JOIN tables t ON t.id = p.issue_id
                                                    WHERE t.{1} = @issueName AND p.{1} = @paramName AND t.{2} = @databaseName)";
                    const string insertCommand =
                        @"INSERT parameter_texts (id,ref_id,country_code,{0}) (
                                                    SELECT 
                                                        (SELECT IFNULL(MAX(id),0)+1 FROM parameter_texts), 
                                                        p.id,
                                                        @lng,
                                                        @value
                                                    FROM parameter p
                                                         INNER JOIN tables t ON t.id = p.issue_id
                                                         WHERE t.{1} = @issueName AND p.{1} = @paramName AND t.{2} = @databaseName
                                                    )";
                    const string getParamsWithMissingDesctrption =
                        @"SELECT p.id, p.table_name, p.column_name FROM parameter p 
                                                                            WHERE 1 > (SELECT COUNT(id) FROM parameter_texts pt WHERE pt.ref_id = p.id AND pt.country_code = '{0}')";
                    const string insertTextForParamsWithMissingDesctrption =
                        @"INSERT parameter_texts (id,ref_id,country_code,{0})
                                                                                SELECT 
                                                                                (SELECT IFNULL(MAX(id),0)+1 FROM parameter_texts), 
                                                                                @refId,
                                                                                @lng,
                                                                                @value";
                    // lng, tableName, columnName, text
                    Dictionary<string, Dictionary<string, Dictionary<string, string>>> tableColumnLanguageText =
                        new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
                    //using (IDatabase conn = profile.ViewboxDb.ConnectionManager.GetConnection()) {
                    status = string.Format("Updating issue parameter descriptions from file [{0}]", data.TableName);
                    log.Info(status);
                    if (progress != null) progress.Description = status;
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        DataRow row = data.Rows[i];
                        string issueName = row[metadataColumnMapping[LanguageMetadataEnum.IssueName]].ToString();
                        string paramName = row[metadataColumnMapping[LanguageMetadataEnum.ParameterName]].ToString();
                        string databaseName = row[metadataColumnMapping[LanguageMetadataEnum.DatabaseName]].ToString();
                        string tableName = row[metadataColumnMapping[LanguageMetadataEnum.TableName]].ToString();
                        string columnName = row[metadataColumnMapping[LanguageMetadataEnum.ColumnName]].ToString();
                        //if(string.IsNullOrWhiteSpace(tableName)) tableName = issueName + "_fakeTable" + i;
                        //if (string.IsNullOrWhiteSpace(columnName)) columnName = issueName + "_fakeColumn" + i;
                        if (tableNameToUpdate == null ||
                            string.Compare(issueName, tableNameToUpdate, StringComparison.InvariantCultureIgnoreCase) ==
                            0)
                        {
                            foreach (KeyValuePair<string, int> keyValuePair in languageTextColumnMapping)
                            {
                                string lng = keyValuePair.Key;
                                string text = row[keyValuePair.Value].ToString();
                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    if (
                                        InsertUpdateParameterText(
                                            string.Format(updateCommand, conn.Enquote("text"), conn.Enquote("name"),
                                                          conn.Enquote("database")), text, lng, issueName, paramName,
                                            conn, profile.DbConfig.DbName) == 0)
                                        InsertUpdateParameterText(
                                            string.Format(insertCommand, conn.Enquote("text"), conn.Enquote("name"),
                                                          conn.Enquote("database")), text, lng, issueName, paramName,
                                            conn, profile.DbConfig.DbName);
                                    if (!tableColumnLanguageText.ContainsKey(lng))
                                        tableColumnLanguageText[lng] =
                                            new Dictionary<string, Dictionary<string, string>>();
                                    if (!tableColumnLanguageText[lng].ContainsKey(tableName))
                                        tableColumnLanguageText[lng][tableName] = new Dictionary<string, string>();
                                    tableColumnLanguageText[lng][tableName][columnName] = text;
                                }
                            }
                        }
                    }
                    if (progress != null && progress.CancellationPending)
                    {
                        return;
                    }
                    if (progress != null)
                    {
                        progress.StepDone();
                    }
                    // Fallback: updating parameters which has no description of a language and has description in the imput file based on column mapping
                    status = string.Format("Updating missing issue parameter descriptions based on column name [{0}]",
                                           data.TableName);
                    log.Info(status);
                    if (progress != null) progress.Description = status;
                    foreach (KeyValuePair<string, CultureInfo> lng in Language.Languages)
                    {
                        // tableName, columnName, parameter Id
                        Dictionary<string, Dictionary<string, int>> paramTextNotExistsOnLanguage =
                            new Dictionary<string, Dictionary<string, int>>();
                        string language = lng.Key;
                        using (
                            IDataReader rdr =
                                conn.ExecuteReader(string.Format(getParamsWithMissingDesctrption, language)))
                        {
                            while (rdr.Read())
                            {
                                string tableName = rdr.IsDBNull(1) ? null : rdr.GetString(1);
                                string columnName = rdr.IsDBNull(2) ? null : rdr.GetString(2);
                                if (!string.IsNullOrWhiteSpace(tableName) && !string.IsNullOrWhiteSpace(columnName))
                                {
                                    int id = rdr.GetInt32(0);
                                    if (!paramTextNotExistsOnLanguage.ContainsKey(tableName))
                                        paramTextNotExistsOnLanguage[tableName] = new Dictionary<string, int>();
                                    paramTextNotExistsOnLanguage[tableName][columnName] = id;
                                }
                            }
                        }
                        if (tableColumnLanguageText.ContainsKey(lng.Key))
                        {
                            foreach (
                                KeyValuePair<string, Dictionary<string, string>> tableColumnText in
                                    tableColumnLanguageText[lng.Key])
                            {
                                string tableName = tableColumnText.Key;
                                // if there is column in the table for which we have no description on this language
                                if (paramTextNotExistsOnLanguage.ContainsKey(tableName))
                                {
                                    foreach (
                                        KeyValuePair<string, int> columnParamIdMapping in
                                            paramTextNotExistsOnLanguage[tableName])
                                    {
                                        string columnName = columnParamIdMapping.Key;
                                        // if we have description for the column
                                        if (tableColumnText.Value.ContainsKey(columnName))
                                        {
                                            string text = tableColumnText.Value[columnName];
                                            if (!string.IsNullOrWhiteSpace(text))
                                            {
                                                InsertMissingParamText(
                                                    string.Format(insertTextForParamsWithMissingDesctrption,
                                                                  conn.Enquote("text")), text, language,
                                                    columnParamIdMapping.Value, conn);
                                            }
                                        }
                                    }
                                }
                                if (progress != null)
                                {
                                    progress.StepDone();
                                }
                            }
                        }
                        if (progress != null && progress.CancellationPending)
                        {
                            return;
                        }
                    }
                    //}
                }
                catch (Exception ex)
                {
                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                    {
                        log.Error(ex.Message, ex);
                    }
                    throw;
                }
            }
        }

        private static int InsertLanguage(string command, string lng, string name, string motto, IDatabase conn)
        {
            int retVal = 0;
            using (IDbCommand cmd = conn.GetDbCommand())
            {
                //using (cmd.Connection = conn.GetDbConnection()) 
                cmd.Connection = conn.Connection;
                {
                    //cmd.Connection.ConnectionString = conn.ConnectionString;
                    //cmd.Connection.Open();
                    cmd.CommandText = string.Format(command);
                    IDbDataParameter pLng = cmd.CreateParameter();
                    IDbDataParameter pName = cmd.CreateParameter();
                    IDbDataParameter pMotto = cmd.CreateParameter();
                    pLng.ParameterName = "@lng";
                    pLng.Value = lng;
                    pName.ParameterName = "@lngName";
                    pName.Value = name;
                    pMotto.ParameterName = "@lngMotto";
                    pMotto.Value = motto;
                    cmd.Parameters.Add(pLng);
                    cmd.Parameters.Add(pName);
                    cmd.Parameters.Add(pMotto);
                    retVal = cmd.ExecuteNonQuery();
                }
            }
            return retVal;
        }

        private static int InsertUpdateIssueText(string command, string value, string lng, string issueName,
                                                 IDatabase conn)
        {
            int retVal = 0;
            using (IDbCommand cmd = conn.GetDbCommand())
            {
                //using (cmd.Connection = conn.GetDbConnection()) 
                cmd.Connection = conn.Connection;
                {
                    //cmd.Connection.ConnectionString = conn.ConnectionString;
                    //cmd.Connection.Open();
                    cmd.CommandText = string.Format(command);
                    IDbDataParameter pValue = cmd.CreateParameter();
                    IDbDataParameter pLng = cmd.CreateParameter();
                    IDbDataParameter pIssueName = cmd.CreateParameter();
                    pValue.ParameterName = "@value";
                    pValue.Value = value;
                    pLng.ParameterName = "@lng";
                    pLng.Value = lng;
                    pIssueName.ParameterName = "@issueName";
                    pIssueName.Value = issueName;
                    cmd.Parameters.Add(pValue);
                    cmd.Parameters.Add(pLng);
                    cmd.Parameters.Add(pIssueName);
                    retVal = cmd.ExecuteNonQuery();
                }
            }
            return retVal;
        }

        private static int InsertMissingParamText(string command, string value, string lng, int refId, IDatabase conn)
        {
            int retVal = 0;
            using (IDbCommand cmd = conn.GetDbCommand())
            {
                //using (cmd.Connection = conn.GetDbConnection())
                cmd.Connection = conn.Connection;
                {
                    //cmd.Connection.ConnectionString = conn.ConnectionString;
                    //cmd.Connection.Open();
                    cmd.CommandText = string.Format(command);
                    IDbDataParameter pValue = cmd.CreateParameter();
                    IDbDataParameter pLng = cmd.CreateParameter();
                    IDbDataParameter pRefId = cmd.CreateParameter();
                    pValue.ParameterName = "@value";
                    pValue.Value = value;
                    pLng.ParameterName = "@lng";
                    pLng.Value = lng;
                    pRefId.ParameterName = "@refId";
                    pRefId.Value = refId;
                    cmd.Parameters.Add(pValue);
                    cmd.Parameters.Add(pLng);
                    cmd.Parameters.Add(pRefId);
                    retVal = cmd.ExecuteNonQuery();
                }
            }
            return retVal;
        }

        private static int InsertUpdateParameterText(string command, string value, string lng, string issueName,
                                                     string paramName, IDatabase conn, string databaseName)
        {
            int retVal = 0;
            using (IDbCommand cmd = conn.GetDbCommand())
            {
                //using (cmd.Connection = conn.GetDbConnection()) 
                cmd.Connection = conn.Connection;
                {
                    //cmd.Connection.ConnectionString = conn.ConnectionString;
                    //cmd.Connection.Open();
                    cmd.CommandText = string.Format(command);
                    IDbDataParameter pValue = cmd.CreateParameter();
                    IDbDataParameter pLng = cmd.CreateParameter();
                    IDbDataParameter pIssueName = cmd.CreateParameter();
                    IDbDataParameter pParamName = cmd.CreateParameter();
                    IDbDataParameter pDatabaseName = cmd.CreateParameter();
                    pValue.ParameterName = "@value";
                    pValue.Value = value;
                    pLng.ParameterName = "@lng";
                    pLng.Value = lng;
                    pIssueName.ParameterName = "@issueName";
                    pIssueName.Value = issueName;
                    pParamName.ParameterName = "@paramName";
                    pParamName.Value = paramName;
                    pDatabaseName.ParameterName = "@databaseName";
                    pDatabaseName.Value = databaseName;
                    cmd.Parameters.Add(pValue);
                    cmd.Parameters.Add(pLng);
                    cmd.Parameters.Add(pIssueName);
                    cmd.Parameters.Add(pParamName);
                    cmd.Parameters.Add(pDatabaseName);
                    retVal = cmd.ExecuteNonQuery();
                }
            }
            return retVal;
        }

        private static int InsertUpdatePropertyText(string updatePropCommand, string name, string text, int refId,
                                                    string lng, IDatabase conn)
        {
            int retVal = 0;
            using (IDbCommand cmd = conn.GetDbCommand())
            {
                //using (cmd.Connection = conn.GetDbConnection())
                cmd.Connection = conn.Connection;
                {
                    //cmd.Connection.ConnectionString = conn.ConnectionString;
                    //cmd.Connection.Open();
                    cmd.CommandText = string.Format(updatePropCommand);
                    IDbDataParameter pText = cmd.CreateParameter();
                    IDbDataParameter pName = cmd.CreateParameter();
                    IDbDataParameter pRefId = cmd.CreateParameter();
                    IDbDataParameter pLng = cmd.CreateParameter();
                    pText.ParameterName = "@value";
                    pText.Value = text;
                    pName.ParameterName = "@name";
                    pName.Value = name;
                    pRefId.ParameterName = "@refId";
                    pRefId.Value = refId;
                    pLng.ParameterName = "@lng";
                    pLng.Value = lng;
                    cmd.Parameters.Add(pText);
                    cmd.Parameters.Add(pName);
                    cmd.Parameters.Add(pRefId);
                    cmd.Parameters.Add(pLng);
                    retVal = cmd.ExecuteNonQuery();
                }
            }
            return retVal;
        }

        private static int InsertUpdateCategoryText(string updateCategoryCommand, string text, int refId, string lng,
                                                    IDatabase conn)
        {
            int retVal = 0;
            using (IDbCommand cmd = conn.GetDbCommand())
            {
                //using (cmd.Connection = conn.GetDbConnection()) 
                cmd.Connection = conn.Connection;
                {
                    //cmd.Connection.ConnectionString = conn.ConnectionString;
                    //cmd.Connection.Open();
                    cmd.CommandText = string.Format(updateCategoryCommand);
                    IDbDataParameter pText = cmd.CreateParameter();
                    IDbDataParameter pRefId = cmd.CreateParameter();
                    IDbDataParameter pLng = cmd.CreateParameter();
                    pText.ParameterName = "@value";
                    pText.Value = text;
                    pRefId.ParameterName = "@refId";
                    pRefId.Value = refId;
                    pLng.ParameterName = "@lng";
                    pLng.Value = lng;
                    cmd.Parameters.Add(pText);
                    cmd.Parameters.Add(pRefId);
                    cmd.Parameters.Add(pLng);
                    retVal = cmd.ExecuteNonQuery();
                }
            }
            return retVal;
        }

        private static int InsertOptimizationText(string insertOptCommand, string text, int refId, string lng,
                                                  IDatabase conn)
        {
            int retVal = 0;
            using (IDbCommand cmd = conn.GetDbCommand())
            {
                //using (cmd.Connection = conn.GetDbConnection())
                cmd.Connection = conn.Connection;
                {
                    //cmd.Connection.ConnectionString = conn.ConnectionString;
                    //cmd.Connection.Open();
                    //cmd.CommandTimeout = 0;
                    cmd.CommandText = string.Format(insertOptCommand);
                    IDbDataParameter pText = cmd.CreateParameter();
                    IDbDataParameter pRefId = cmd.CreateParameter();
                    IDbDataParameter pLng = cmd.CreateParameter();
                    pText.ParameterName = "@value";
                    pText.Value = text;
                    pRefId.ParameterName = "@refId";
                    pRefId.Value = refId;
                    pLng.ParameterName = "@lng";
                    pLng.Value = lng;
                    cmd.Parameters.Add(pText);
                    cmd.Parameters.Add(pRefId);
                    cmd.Parameters.Add(pLng);
                    retVal = cmd.ExecuteNonQuery();
                }
            }
            return retVal;
        }

        private static int UpdateOptimizationText(string updateOptCommand, string text, int id, IDatabase conn)
        {
            int retVal = 0;
            using (IDbCommand cmd = conn.GetDbCommand())
            {
                //using (cmd.Connection = conn.GetDbConnection())
                cmd.Connection = conn.Connection;
                {
                    //cmd.Connection.ConnectionString = conn.ConnectionString;
                    //cmd.Connection.Open();
                    cmd.CommandText = string.Format(updateOptCommand);
                    IDbDataParameter pText = cmd.CreateParameter();
                    IDbDataParameter pId = cmd.CreateParameter();
                    pText.ParameterName = "@value";
                    pText.Value = text;
                    pId.ParameterName = "@id";
                    pId.Value = id;
                    cmd.Parameters.Add(pText);
                    cmd.Parameters.Add(pId);
                    retVal = cmd.ExecuteNonQuery();
                }
            }
            return retVal;
        }

        private static int InsertUpdateOptimizationGroupText(string updateOptGroupCommand, string groupText, string lng,
                                                             OptimizationType type, IDatabase conn)
        {
            int retVal = 0;
            using (IDbCommand cmd = conn.GetDbCommand())
            {
                //using (cmd.Connection = conn.GetDbConnection()) 
                cmd.Connection = conn.Connection;
                {
                    //cmd.Connection.ConnectionString = conn.ConnectionString;
                    //cmd.Connection.Open();
                    cmd.CommandText = string.Format(updateOptGroupCommand);
                    IDbDataParameter pText = cmd.CreateParameter();
                    IDbDataParameter pLng = cmd.CreateParameter();
                    IDbDataParameter pType = cmd.CreateParameter();
                    pText.ParameterName = "@value";
                    pText.Value = groupText;
                    pLng.ParameterName = "@lng";
                    pLng.Value = lng;
                    pType.ParameterName = "@type";
                    pType.Value = (int) type;
                    cmd.Parameters.Add(pText);
                    cmd.Parameters.Add(pLng);
                    cmd.Parameters.Add(pType);
                    retVal = cmd.ExecuteNonQuery();
                }
            }
            return retVal;
        }

        #endregion [ Private methods ]

        #region Nested type: LanguageMetadataEnum

        private enum LanguageMetadataEnum
        {
            ColumnName = 0,
            TableName = 1,
            DatabaseName = 2,
            IssueName = 3,
            ParameterName = 4,
        }

        #endregion
    }
}