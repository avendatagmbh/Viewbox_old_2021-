using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb.Internal;
using AV.Log;
using DbAccess;
using Utils;
using ViewBuilderBusiness.Structures.Config;
using log4net;

namespace ViewBuilderBusiness.MetadataUpdate
{
    public class UpdateSapMetadata
    {
        private const string tableToFind = "dd03t";
        internal static ILog log = LogHelper.GetLogger();

        #region [ Commands ]

        private const string insertColumnTextCommand =
            @"INSERT INTO {2}.column_texts (id, ref_id, country_code, text)
            (
                SELECT (SELECT MAX(id) + 1 FROM {2}.column_texts), c.id AS ref_id, '{0}' AS country_code, d.DDTEXT AS value FROM {2}.columns c 
                INNER JOIN {2}.tables t ON t.id = c.table_id
                INNER JOIN {3}.{4} d ON d.TABNAME = t.name AND d.FIELDNAME = c.name AND d.DDLANGUAGE = '{1}'
                WHERE t.database = '{3}' AND d.DDTEXT != ''
                AND c.id NOT IN (SELECT ref_id FROM {2}.column_texts WHERE country_code = '{0}')
                LIMIT 1
            );";

        private const string updateColumnTextCommand =
            @"UPDATE {2}.column_texts ct
            JOIN
            ( SELECT c.id, '{0}' AS country_code, d.DDTEXT FROM {2}.columns c 
                INNER JOIN {2}.tables t ON t.id = c.table_id
                INNER JOIN {3}.{4} d ON d.TABNAME = t.name AND d.FIELDNAME = c.name AND d.DDLANGUAGE = '{1}'
                WHERE t.database = '{3}' AND d.DDTEXT != ''
                AND c.id IN (SELECT ref_id FROM {2}.column_texts WHERE country_code = '{0}')
            ) AS x
                     ON x.id = ct.ref_id AND x.country_code = ct.country_code
            SET 
            ct.text = x.DDTEXT";

        private const string insertTableTextCommand =
            @"INSERT INTO {2}.table_texts (id, ref_id, country_code, text)
            (
                SELECT (SELECT MAX(id) + 1 FROM {2}.table_texts), t.id AS ref_id, '{0}' AS country_code, d.DDTEXT AS value FROM {2}.tables t
                INNER JOIN {3}.{4} d ON d.TABNAME = t.name AND d.DDLANGUAGE = '{1}'
                WHERE t.database = '{3}' AND d.DDTEXT != ''
                AND t.id NOT IN (SELECT ref_id FROM {2}.table_texts WHERE country_code = '{0}')
                LIMIT 1
            );";

        private const string updateTableTextCommand =
            @"UPDATE {2}.table_texts tt
            JOIN
            ( SELECT t.id, '{0}' AS country_code, d.DDTEXT FROM {2}.tables t
                INNER JOIN {3}.{4} d ON d.TABNAME = t.name AND d.DDLANGUAGE = '{1}'
                WHERE t.database = '{3}' AND d.DDTEXT != ''
                AND t.id IN (SELECT ref_id FROM {2}.table_texts WHERE country_code = '{0}')
            ) AS x
                        ON x.id = tt.ref_id AND x.country_code = tt.country_code
            SET 
            tt.text = x.DDTEXT";

        #endregion [ Commands ]

        #region [ Static methods ]

        public static void UpdateDescriptions(ProfileConfig profile, IDatabase conn, ProgressCalculator progress = null)
        {
            using (NDC.Push(LogHelper.GetNamespaceContext()))
            {
                try
                {
                    IEnumerable<string> databases = profile.ViewboxDb.Tables.Select(t => t.Database).Distinct();
                    if (progress != null)
                    {
                        progress.SetStep(0);
                        progress.SetWorkSteps(databases.Count()*Language.Languages.Count*2, false);
                    }
                    foreach (string database in databases)
                    {
                        if (IsSapDataBase(database, profile))
                        {
                            //using (IDatabase conn = profile.ConnectionManager.GetConnection()) {
                            foreach (
                                KeyValuePair<string, Tuple<string, string>> languageDescription in
                                    Language.LanguageDescriptions)
                            {
                                string sapLanguageCode =
                                    SapLanguageCodeHelper.GetSapLanguageCode(languageDescription.Key);
                                string info = string.Format("Inserting column texts from SAP (language: {0})",
                                                            languageDescription.Key);
                                if (progress != null)
                                {
                                    progress.Description = info;
                                }
                                log.Info(info);
                                // insert not existing column text entries
                                string insertColumnText = string.Format(insertColumnTextCommand, languageDescription.Key,
                                                                        sapLanguageCode,
                                                                        profile.ViewboxDb.DB.DbConfig.DbName,
                                                                        profile.DbConfig.DbName, tableToFind);
                                conn.ExecuteNonQuery(insertColumnText);
                                log.Info(string.Format("Updating column texts from SAP (language: {0})",
                                                       languageDescription.Key));
                                // update existing column text  entries
                                conn.ExecuteNonQuery(string.Format(updateColumnTextCommand, languageDescription.Key,
                                                                   sapLanguageCode, profile.ViewboxDb.DB.DbConfig.DbName,
                                                                   profile.DbConfig.DbName, tableToFind));
                                if (progress != null && progress.CancellationPending)
                                {
                                    return;
                                }
                                info = string.Format("Insert table texts from SAP (language: {0})",
                                                     languageDescription.Key);
                                if (progress != null)
                                {
                                    progress.Description = info;
                                    progress.StepDone();
                                }
                                log.Info(info);
                                // insert not existing table text entries
                                string insertTableText = string.Format(insertTableTextCommand, languageDescription.Key,
                                                                       sapLanguageCode,
                                                                       profile.ViewboxDb.DB.DbConfig.DbName,
                                                                       profile.DbConfig.DbName, tableToFind);
                                conn.ExecuteNonQuery(insertTableText);
                                log.Info(string.Format("Updating table texts from SAP (language: {0})",
                                                       languageDescription.Key));
                                // update existing table text  entries
                                conn.ExecuteNonQuery(string.Format(updateTableTextCommand, languageDescription.Key,
                                                                   sapLanguageCode, profile.ViewboxDb.DB.DbConfig.DbName,
                                                                   profile.DbConfig.DbName, tableToFind));
                                if (progress != null)
                                {
                                    progress.StepDone();
                                }
                            }
                            //}
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                    throw;
                }
            }
        }

        #endregion [ Static methods ]

        #region [ Helper methods ]

        private static bool IsSapDataBase(string database, ProfileConfig profile)
        {
            try
            {
                using (IDatabase conn = profile.ConnectionManager.GetConnection())
                {
                    return conn.TableExists(tableToFind);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                throw;
            }
        }

        #endregion [ Helper methods ]
    }
}