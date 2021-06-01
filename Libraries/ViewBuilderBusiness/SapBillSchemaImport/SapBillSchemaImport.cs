using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SystemDb.Internal;
using AV.Log;
using DbAccess;
using DbAccess.Structures;
using Utils;
using ViewBuilderBusiness.Structures.Config;
using log4net;

namespace ViewBuilderBusiness.SapBillSchemaImport
{
    public class BilRow
    {
        public BilRow()
        {
            Childs = new List<BilRow>();
        }

        public BilRow ParentRow { get; set; }
        public List<BilRow> Childs { get; set; }
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public bool Debit { get; set; }
        public bool Credit { get; set; }
        public int Level { get; set; }
        public string AccountFrom { get; set; }
        public string AccountTo { get; set; }
        public string Type { get; set; }
        public BilRow HighestGroup { get; set; }
        public string AdditionalInformation { get; set; }
        public string AccountStructure { get; set; }

        public bool IsAdditional(string info)
        {
            return AdditionalInformation == info || (ParentRow != null && ParentRow.IsAdditional(info));
        }
    }

    public class SapBillSchemaImportFile : NotifyPropertyChangedBase
    {
        private string _accountsStructure = "";
        private string _filePath;
        private string _filePreview;
        private bool _selected;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                OnPropertyChanged("Selected");
            }
        }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged("FilePath");
                if (String.IsNullOrEmpty(AccountsStructure))
                {
                    DetermineAccountsStructure();
                    if (String.IsNullOrEmpty(AccountsStructure))
                        AccountsStructure = FileName.ToUpper().Replace(".TXT", "");
                }
            }
        }

        public string FileName
        {
            get { return String.IsNullOrEmpty(FilePath) ? "" : Path.GetFileName(FilePath); }
        }

        public string AccountsStructure
        {
            get { return _accountsStructure; }
            set
            {
                _accountsStructure = value;
                OnPropertyChanged("AccountsStructure");
                if (!String.IsNullOrWhiteSpace(_accountsStructure))
                    Selected = true;
            }
        }

        public string FilePreview
        {
            get { return _filePreview; }
            set
            {
                _filePreview = value;
                OnPropertyChanged("FilePreview");
            }
        }

        public override string ToString()
        {
            return AccountsStructure + " " +
                   (String.IsNullOrEmpty(FileName) ? "" : FileName.ToLower().Replace(".txt", ""));
        }

        private void DetermineAccountsStructure()
        {
            Regex rangeRexexp = new Regex(@"^[ |-]*(.{4})(\S+)\s+-\s+(\S+)\s+([X_\|]{3})\s*(.*)$",
                                          RegexOptions.IgnoreCase);
            FilePreview = "";
            using (StreamReader oReader = new StreamReader(FilePath, Encoding.GetEncoding(1252)))
            {
                while (!oReader.EndOfStream)
                {
                    string sLine = oReader.ReadLine();
                    FilePreview += Environment.NewLine + sLine;
                    if (rangeRexexp.IsMatch(sLine))
                    {
                        MatchCollection m = rangeRexexp.Matches(sLine);
                        if (m.Count > 0 && m[0].Groups.Count > 3)
                        {
                            AccountsStructure = m[0].Groups[1].Value;
                            return;
                        }
                    }
                }
            }
        }
    }

    public class SapBillSchemaImport
    {
        private const string balanceViewPartialName = "_BalanceViewPartial";
        private const string sapBalanceIssueName = "sap_balance";
        private const string bgvBilanzstruktur = "_bgv_Bilanzstruktur";
        internal static ILog log = LogHelper.GetLogger();
        private static bool FoundNotAssigned;
        private readonly IList<SapBillSchemaImportFile> Files;
        private readonly ProfileConfig Profile;
        private readonly ProgressCalculator Progress;
        internal ILog _log = LogHelper.GetLogger();

        public SapBillSchemaImport(IList<SapBillSchemaImportFile> files, ProfileConfig profile,
                                   ProgressCalculator progress = null)
        {
            Profile = profile;
            Progress = progress;
            Files = files;
        }

        public static bool FunctionEnabled(ProfileConfig profile)
        {
            using (NDC.Push(LogHelper.GetNamespaceContext()))
            {
                try
                {
                    //TODO: check SAP
                    using (IDatabase conn = profile.ConnectionManager.GetConnection())
                    {
                        if (!conn.TableExists("t011t"))
                            return false;
                    }
                    using (IDatabase conn = profile.ViewboxDb.ConnectionManager.GetConnection())
                    {
                        if (conn.TableExists("issue_extensions"))
                        {
                            int c =
                                Convert.ToInt32(
                                    conn.ExecuteScalar(string.Format("SELECT count(*) FROM {0} WHERE {1} like {2}",
                                                                     conn.Enquote("issue_extensions"),
                                                                     conn.Enquote("command"),
                                                                     conn.GetValueString(sapBalanceIssueName))));
                            return c > 0;
                        }
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
            return false;
        }

        public void DoWork2()
        {
            int in_type_id;
            int in_pvon_id;
            int in_pbis_id;
            string sap_balance_database;
            string sap_balance_table;
            List<DbColumnValues> data = new List<DbColumnValues> {};
            using (NDC.Push(LogHelper.GetNamespaceContext()))
            {
                try
                {
                    if (Profile == null || Files == null)
                        throw new NoNullAllowedException();
                    if (Files.Any(itm => itm.Selected && String.IsNullOrEmpty(itm.AccountsStructure)))
                        throw new Exception("Please set Account structure in each selected file");
                    if (Progress != null) Progress.Description = "Sap Bill Schema Import...";
                    if (Progress != null) Progress.SetWorkSteps(12 + Files.Count(itm => itm.Selected), false);
                    if (Progress != null) Progress.Description = string.Format("Connecting...");
                    using (IDatabase conn = Profile.ViewboxDb.ConnectionManager.GetConnection())
                    {
                        if (!conn.IsOpen) conn.Open();

                        #region set up prereguirements

                        if (!conn.TableExists("schemes"))
                            throw new Exception("Missing schemes table from viewbox database");
                        if (Progress != null) Progress.Description = "Check balance view partial...";
                        if (Progress != null) Progress.StepDone();
                        // look up for balance view partial. determine id
                        int balanceViewId;
                        bool partialViewExists =
                            Convert.ToInt32(
                                conn.ExecuteScalar(string.Format("SELECT count(*) FROM {0} WHERE {1} like {2}",
                                                                 conn.Enquote("schemes"), conn.Enquote("partial"),
                                                                 conn.GetValueString(balanceViewPartialName)))) > 0;
                        if (partialViewExists)
                        {
                            balanceViewId =
                                Convert.ToInt32(
                                    conn.ExecuteScalar(string.Format("SELECT id FROM {0} WHERE {1} like {2}",
                                                                     conn.Enquote("schemes"), conn.Enquote("partial"),
                                                                     conn.GetValueString(balanceViewPartialName))));
                        }
                        else
                        {
                            balanceViewId =
                                Convert.ToInt32(
                                    conn.ExecuteScalar(string.Format("SELECT COALESCE(max(id),0)+1 FROM {0}",
                                                                     conn.Enquote("schemes"))));
                            DbColumnValues values = new DbColumnValues();
                            values["id"] = balanceViewId;
                            values["partial"] = balanceViewPartialName;
                            conn.InsertInto("schemes", values);
                        }
                        if (!conn.TableExists("issue_extensions"))
                            throw new Exception("Missing issue_extensions table from viewbox database");
                        bool issue_extensions_Exists = Convert.ToInt32(
                            conn.ExecuteScalar(
                                string.Format(
                                    "SELECT count(*) FROM {0} e WHERE {1} like {2} and (select {3} from {4} t where t.id = e.ref_id) = {5}",
                                    conn.Enquote("issue_extensions"), conn.Enquote("command"),
                                    conn.GetValueString(sapBalanceIssueName), conn.Enquote("database"),
                                    conn.Enquote("tables"), conn.GetValueString(Profile.DbConfig.DbName)
                                    ))) > 0;
                        if (!issue_extensions_Exists)
                            throw new Exception("For this function you must create the " + sapBalanceIssueName +
                                                " viewscript first.");

                        #endregion

                        #region Get information sap_balance issue

                        if (Progress != null && Progress.CancellationPending) return;
                        if (Progress != null) Progress.Description = "Get information sap_balance issue...";
                        if (Progress != null) Progress.StepDone();
                        // get information about sap_balance issue from viewbox
                        int sap_balance_id;
                        int issue_extensions_id;
                        using (
                            IDataReader rdr =
                                conn.ExecuteReader(
                                    string.Format(
                                        "SELECT ref_id, id FROM {0} e WHERE {1} like {2} and (select {3} from {4} t where t.id = e.ref_id) = {5}",
                                        conn.Enquote("issue_extensions"),
                                        conn.Enquote("command"),
                                        conn.GetValueString(sapBalanceIssueName),
                                        conn.Enquote("database"), conn.Enquote("tables"),
                                        conn.GetValueString(Profile.DbConfig.DbName)
                                        )))
                        {
                            if (!rdr.Read())
                                throw new Exception("There are no table information sap_balance_issue_id.");
                            sap_balance_id = rdr.GetInt32(0);
                            issue_extensions_id = rdr.GetInt32(1);
                        }

                        #endregion

                        #region Enable issue_extensions

                        if (Progress != null && Progress.CancellationPending) return;
                        if (Progress != null) Progress.Description = "Enable issue_extensions...";
                        if (Progress != null) Progress.StepDone();
                        {
                            DbColumnValues values = new DbColumnValues();
                            values["flag"] = 1;
                            conn.Update("issue_extensions", values,
                                        conn.Enquote("id") + "=" + conn.GetValueString(issue_extensions_id));
                        }

                        #endregion

                        #region Get information related tables

                        if (Progress != null && Progress.CancellationPending) return;
                        if (Progress != null) Progress.Description = "Get information related tables...";
                        if (Progress != null) Progress.StepDone();
                        // get information about sap_balance related table from viewbox
                        using (
                            IDataReader rdr =
                                conn.ExecuteReader(string.Format("SELECT {3}, {4} FROM {0} WHERE {1} = {2}",
                                                                 conn.Enquote("tables"), conn.Enquote("id"),
                                                                 conn.GetValueString(sap_balance_id),
                                                                 conn.Enquote("database"), conn.Enquote("name"))))
                        {
                            if (!rdr.Read())
                                throw new Exception("There are no table information sap_balance_issue_id.");
                            sap_balance_database = rdr.GetString(0);
                            sap_balance_table = rdr.GetString(1); // typically : "blg_bilanz_guv"
                        }

                        #endregion

                        #region Setup table scheme to balance view partial

                        if (Progress != null && Progress.CancellationPending) return;
                        if (Progress != null) Progress.Description = "Setup table scheme to balance view partial...";
                        if (Progress != null) Progress.StepDone();
                        //set up table scheme to balance view partial
                        {
                            DbColumnValues values = new DbColumnValues();
                            values["default_scheme"] = balanceViewId;
                            conn.Update("tables", values, conn.Enquote("id") + "=" + conn.GetValueString(sap_balance_id));
                        }

                        #endregion

                        #region Get id's of Parameters

                        if (Progress != null && Progress.CancellationPending) return;
                        if (Progress != null) Progress.Description = "Get parameter informations...";
                        if (Progress != null) Progress.StepDone();
                        using (
                            IDataReader rdr =
                                conn.ExecuteReader(
                                    string.Format("SELECT id FROM {0} WHERE {1} like {2} AND {3} like {4}",
                                                  conn.Enquote("parameter"),
                                                  conn.Enquote("name"),
                                                  conn.GetValueString("in_type"),
                                                  conn.Enquote("issue_id"),
                                                  conn.GetValueString(sap_balance_id)
                                        )))
                        {
                            if (!rdr.Read())
                                throw new Exception("There is no in_type parameter");
                            in_type_id = rdr.GetInt32(0);
                        }
                        using (
                            IDataReader rdr =
                                conn.ExecuteReader(
                                    string.Format("SELECT id FROM {0} WHERE {1} like {2} AND {3} like {4}",
                                                  conn.Enquote("parameter"),
                                                  conn.Enquote("name"),
                                                  conn.GetValueString("in_pvon"),
                                                  conn.Enquote("issue_id"),
                                                  conn.GetValueString(sap_balance_id)
                                        )))
                        {
                            if (!rdr.Read())
                                throw new Exception("There is no in_pvon parameter");
                            in_pvon_id = rdr.GetInt32(0);
                        }
                        using (
                            IDataReader rdr =
                                conn.ExecuteReader(
                                    string.Format("SELECT id FROM {0} WHERE {1} like {2} AND {3} like {4}",
                                                  conn.Enquote("parameter"),
                                                  conn.Enquote("name"),
                                                  conn.GetValueString("in_pbis"),
                                                  conn.Enquote("issue_id"),
                                                  conn.GetValueString(sap_balance_id)
                                        )))
                        {
                            if (!rdr.Read())
                                throw new Exception("There is no in_pbis parameter");
                            in_pbis_id = rdr.GetInt32(0);
                        }

                        #endregion

                        #region Clean up Parameter related tables

                        if (Progress != null && Progress.CancellationPending) return;
                        if (Progress != null) Progress.Description = "Clean up parameter informations...";
                        if (Progress != null) Progress.StepDone();
                        try
                        {
                            conn.ExecuteNonQuery(
                                String.Format(
                                    "DELETE FROM `collection_texts` WHERE `ref_id` IN (SELECT c.`id` FROM `collections` c LEFT JOIN `parameter_collections` pc ON c.`collection_id` = pc.`collection_id` WHERE pc.`parameter_id` IN ({0},{1},{2}))",
                                    conn.GetValueString(in_type_id),
                                    conn.GetValueString(in_pvon_id),
                                    conn.GetValueString(in_pbis_id)
                                    ));
                            conn.ExecuteNonQuery(
                                String.Format(
                                    "DELETE FROM `collections` WHERE `collection_id` IN (SELECT `collection_id` FROM `parameter_collections` WHERE `parameter_id` IN ({0},{1},{2}))",
                                    conn.GetValueString(in_type_id),
                                    conn.GetValueString(in_pvon_id),
                                    conn.GetValueString(in_pbis_id)
                                    ));
                            conn.ExecuteNonQuery(
                                String.Format("DELETE FROM `parameter_texts` WHERE `ref_id` IN ({0},{1},{2})",
                                              conn.GetValueString(in_type_id),
                                              conn.GetValueString(in_pvon_id),
                                              conn.GetValueString(in_pbis_id)
                                    ));
                            conn.ExecuteNonQuery(
                                String.Format(
                                    "DELETE FROM `parameter_collections` WHERE `parameter_id` IN ({0},{1},{2})",
                                    conn.GetValueString(in_type_id),
                                    conn.GetValueString(in_pvon_id),
                                    conn.GetValueString(in_pbis_id)
                                    ));
                            conn.ExecuteNonQuery(
                                String.Format(
                                    "DELETE FROM `user_issueparameter_history` WHERE `parameter_id` IN ({0},{1},{2})",
                                    conn.GetValueString(in_type_id),
                                    conn.GetValueString(in_pvon_id),
                                    conn.GetValueString(in_pbis_id)
                                    ));
                            conn.ExecuteNonQuery(
                                String.Format(
                                    "DELETE FROM `user_issue_storedprocedure_order_settings` WHERE `parameter_id` IN ({0},{1},{2})",
                                    conn.GetValueString(in_type_id),
                                    conn.GetValueString(in_pvon_id),
                                    conn.GetValueString(in_pbis_id)
                                    ));
                        }
                        catch (Exception ex)
                        {
                            using (NDC.Push(LogHelper.GetNamespaceContext()))
                            {
                                log.Error(ex.Message, ex);
                            }
                            throw;
                        }

                        #endregion
                    }

                    #region Import structure

                    string errors = "";
                    if (Progress != null && Progress.CancellationPending) return;
                    using (IDatabase conn = Profile.ConnectionManager.GetConnection())
                    {
                        if (Progress != null && Progress.CancellationPending) return;
                        if (Progress != null) Progress.Description = string.Format("Import structure...");
                        if (Progress != null) Progress.StepDone();
                        foreach (SapBillSchemaImportFile f in Files.Where(itm => itm.Selected))
                        {
                            if (Progress != null && Progress.CancellationPending) return;
                            if (Progress != null) Progress.Description = string.Format("Processed schema: {0}", f);
                            if (Progress != null) Progress.StepDone();
                            try
                            {
                                using (StreamReader oReader = new StreamReader(f.FilePath, Encoding.GetEncoding(1252)))
                                {
                                    ImportStructure2(conn, oReader, f.AccountsStructure);
                                    f.Selected = false;
                                }
                            }
                            catch (Exception)
                            {
                                errors += Environment.NewLine + f.FilePath;
                            }
                        }
                    }

                    #endregion

                    #region Load parameter data

                    if (Progress != null && Progress.CancellationPending) return;
                    if (Progress != null) Progress.StepDone();
                    using (IDatabase conn = Profile.ConnectionManager.GetConnection())
                    {
                        if (!conn.IsOpen) conn.Open();
                        if (Progress != null) Progress.Description = "Load parameter data...";
                        List<string> saplangs =
                            Language.LanguageDescriptions.Select(
                                languageDescription => SapLanguageCodeHelper.GetSapLanguageCode(languageDescription.Key))
                                .ToList();
                        //SPRAS = 'D'
                        using (
                            IDataReader rdr =
                                conn.ExecuteReader(
                                    string.Format(
                                        "SELECT SPRAS, VERSN, VSTXT FROM T011T WHERE SPRAS IN ({0}) AND VERSN COLLATE latin1_german1_ci IN (SELECT DISTINCT BilStr FROM {1})",
                                        "'" + String.Join("', '", saplangs) + "'", conn.Enquote(bgvBilanzstruktur))))
                        {
                            while (rdr.Read())
                            {
                                DbColumnValues v = new DbColumnValues();
                                v["spras"] = rdr.IsDBNull(0) ? "" : rdr.GetString(0);
                                v["versn"] = rdr.IsDBNull(1) ? "" : rdr.GetString(1);
                                v["vstxt"] = rdr.IsDBNull(2) ? "" : rdr.GetString(2);
                                data.Add(v);
                            }
                        }
                    }

                    #endregion

                    #region Insert Parameter related info

                    using (IDatabase conn = Profile.ViewboxDb.ConnectionManager.GetConnection())
                    {
                        if (!conn.IsOpen) conn.Open();
                        if (Progress != null) Progress.Description = "Build parameter data...";
                        if (Progress != null) Progress.StepDone();

                        #region in_pvon in_pbis

                        {
                            int collection_id;
                            using (
                                IDataReader rdr =
                                    conn.ExecuteReader(
                                        string.Format("SELECT COALESCE(max(collection_id),0)+1 FROM collections")))
                            {
                                if (!rdr.Read())
                                    throw new Exception("Error during get collection_id");
                                collection_id = rdr.GetInt32(0);
                            }
                            {
                                DbColumnValues values = new DbColumnValues();
                                values["parameter_id"] = in_pvon_id; // <<<<<<<<<<<
                                values["collection_id"] = collection_id;
                                conn.InsertInto("parameter_collections", values);
                            }
                            {
                                DbColumnValues values = new DbColumnValues();
                                values["parameter_id"] = in_pbis_id; // <<<<<<<<<<<
                                values["collection_id"] = collection_id;
                                conn.InsertInto("parameter_collections", values);
                            }
                            for (int index = 1; index <= 16; index++)
                            {
                                string indexValue = String.Format("{0:0#}", index);
                                int current_collections_id = -1;
                                using (
                                    IDataReader rdr =
                                        conn.ExecuteReader(
                                            string.Format(
                                                "SELECT `id` FROM `collections` WHERE `collection_id`={0} AND value={1}",
                                                conn.GetValueString(collection_id), conn.GetValueString(indexValue))))
                                {
                                    if (rdr.Read())
                                        current_collections_id = rdr.GetInt32(0);
                                }
                                if (current_collections_id == -1)
                                {
                                    DbColumnValues values = new DbColumnValues();
                                    values["value"] = indexValue; // <<<<<<<<<<<
                                    values["collection_id"] = collection_id;
                                    conn.InsertInto("collections", values);
                                    current_collections_id = conn.GetLastInsertId();
                                }
                            }
                            for (int index = 1; index <= 16; index++)
                            {
                                string indexValue = String.Format("{0:0#}", index);
                                int current_collections_id = -1;
                                using (
                                    IDataReader rdr =
                                        conn.ExecuteReader(
                                            string.Format(
                                                "SELECT `id` FROM `collections` WHERE `collection_id`={0} AND value={1}",
                                                conn.GetValueString(collection_id), conn.GetValueString(indexValue))))
                                {
                                    if (rdr.Read())
                                        current_collections_id = rdr.GetInt32(0);
                                }
                                if (current_collections_id == -1)
                                {
                                    DbColumnValues values = new DbColumnValues();
                                    values["value"] = indexValue; // <<<<<<<<<<<
                                    values["collection_id"] = collection_id;
                                    conn.InsertInto("collections", values);
                                    current_collections_id = conn.GetLastInsertId();
                                }
                            }
                        }

                        #endregion

                        #region in_type

                        {
                            int collection_id;
                            using (
                                IDataReader rdr =
                                    conn.ExecuteReader(
                                        string.Format("SELECT COALESCE(max(collection_id),0)+1 FROM collections")))
                            {
                                if (!rdr.Read())
                                    throw new Exception("Error during get collection_id");
                                collection_id = rdr.GetInt32(0);
                            }
                            {
                                DbColumnValues values = new DbColumnValues();
                                values["parameter_id"] = in_type_id;
                                values["collection_id"] = collection_id;
                                conn.InsertInto("parameter_collections", values);
                            }
                            foreach (DbColumnValues d in data)
                            {
                                int current_collections_id = -1;
                                using (
                                    IDataReader rdr =
                                        conn.ExecuteReader(
                                            string.Format(
                                                "SELECT `id` FROM `collections` WHERE `collection_id`={0} AND value={1}",
                                                conn.GetValueString(collection_id), conn.GetValueString(d["versn"]))))
                                {
                                    if (rdr.Read())
                                        current_collections_id = rdr.GetInt32(0);
                                }
                                if (current_collections_id == -1)
                                {
                                    DbColumnValues values = new DbColumnValues();
                                    values["value"] = d["versn"];
                                    values["collection_id"] = collection_id;
                                    conn.InsertInto("collections", values);
                                    current_collections_id = conn.GetLastInsertId();
                                }
                                {
                                    DbColumnValues values = new DbColumnValues();
                                    values["ref_id"] = current_collections_id;
                                    values["country_code"] =
                                        SapLanguageCodeHelper.GetViewBuilderLanguageCode(d["spras"].ToString());
                                    values["text"] = d["vstxt"];
                                    conn.InsertInto("collection_texts", values);
                                }
                            }
                        }

                        #endregion

                        // sap_balance issue_id
                        // 2db parameter  collectino
                        // in_type < versn
                        // in_pvon, in_pbis < 01...16 (string)
                        // collection text (en) < vstxt
                    }

                    #endregion

                    #region Update issue and parameter language texts

                    using (IDatabase conn = Profile.ViewboxDb.ConnectionManager.GetConnection())
                    {
                        if (!conn.IsOpen) conn.Open();
                        if (Progress != null && Progress.CancellationPending) return;
                        if (Progress != null) Progress.Description = "Update issue and parameter language texts...";
                        if (Progress != null) Progress.StepDone();
                        /*  MetadataUpdate.UpdateViewBoxMetadata.UpdateLanguages(Profile, conn, Progress);
                        MetadataUpdate.UpdateViewBoxMetadata.UpdateIssueAndParameterLanguageTexts(Profile, conn, Progress, sap_balance_table);*/
                    }

                    #endregion

                    if (Progress != null) Progress.Description = "Sap Bill Schema Import finished.";
                    if (Progress != null) Progress.StepDone();
                    if (!String.IsNullOrEmpty(errors))
                        throw new Exception("There was errors during importing Sap Bill Schema. Files: " + errors);
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

        private void ImportStructure2(IDatabase db, StreamReader oReader, string txtAccountsStructure)
        {
            string structureName = String.Empty;
            List<BilRow> rows = null;
            using (NDC.Push(LogHelper.GetNamespaceContext()))
            {
                try
                {
                    #region Determine account structure

                    while (!oReader.EndOfStream)
                    {
                        string sLine = oReader.ReadLine();
                        if (!String.IsNullOrEmpty(sLine))
                        {
                            structureName = sLine.Trim().Split(new[] { ' ' })[0];
                            break;
                        }
                    }

                    #endregion Determine account structure

                    rows = GetStructure(oReader, txtAccountsStructure);

                    string sqlCreateTable = "CREATE TABLE IF NOT EXISTS " + bgvBilanzstruktur + " (" +
                                            "`ID` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT," +
                                            "`BilStr` VARCHAR(10) NOT NULL," +
                                            "`Parent` INTEGER UNSIGNED NOT NULL," +
                                            "`Ebene` INTEGER UNSIGNED NOT NULL," +
                                            "`Nummer` VARCHAR(10) NOT NULL," +
                                            "`Titel` VARCHAR(255) NOT NULL," +
                                            "`Konto` VARCHAR(20) NOT NULL," +
                                            "`KontoVon` VARCHAR(20) NOT NULL," +
                                            "`KontoBis` VARCHAR(20) NOT NULL," +
                                            "`Typ` VARCHAR(1) NOT NULL," +
                                            "`Soll` BOOLEAN NOT NULL," +
                                            "`Haben` BOOLEAN NOT NULL," +
                                            "`HighestGroupId` INTEGER UNSIGNED NOT NULL," +
                                            "`AdditionalInformation` VARCHAR(3)," +
                                            "`AccountStructure` VARCHAR(4)," +
                                            "PRIMARY KEY (`ID`, `BilStr`))" +
                                            "ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 COLLATE=latin1_german1_ci " +
                                            "COMMENT = 'Bilanzstruktur für View-Erstellung';";
                    db.ExecuteNonQuery(sqlCreateTable);
                    db.ExecuteNonQuery("DELETE FROM " + bgvBilanzstruktur + " WHERE BilStr = '" + structureName + "'");
                    string sqlInsertPrefix =
                        "INSERT INTO " + bgvBilanzstruktur + " " +
                        "(" +
                        "bilStr," +
                        "ID," +
                        "Parent," +
                        "Ebene," +
                        "Nummer," +
                        "Titel," +
                        "Konto," +
                        "KontoVon," +
                        "KontoBis," +
                        "Typ," +
                        "Soll," +
                        "Haben," +
                        "HighestGroupId," +
                        "AdditionalInformation," +
                        "AccountStructure" +
                        ") VALUES ";
                    string sqlInsert = string.Empty;
                    int nRow = 0;
                    if (rows == null)
                        return;
                    var treeee = GetTree(rows);
                    foreach (BilRow oBilRow in treeee)
                    {
                        if (oBilRow.Id > 0)
                        {
                            if (sqlInsert.Length > 0) sqlInsert += ",";
                            sqlInsert +=
                                "(" +
                                db.GetValueString(structureName) + "," +
                                oBilRow.Id + "," +
                                (oBilRow.ParentRow == null ? 0 : oBilRow.ParentRow.Id) + "," +
                                oBilRow.Level + "," +
                                db.GetValueString(oBilRow.ShortName) + "," +
                                db.GetValueString(oBilRow.Name) + "," +
                                db.GetValueString(oBilRow.Account) + "," +
                                db.GetValueString(oBilRow.AccountFrom) + "," +
                                db.GetValueString(oBilRow.AccountTo) + "," +
                                db.GetValueString(oBilRow.Type) + "," +
                                (oBilRow.Debit ? "True" : "False") + "," +
                                (oBilRow.Credit ? "True" : "False") + "," +
                                (oBilRow.HighestGroup == null ? 0 : oBilRow.HighestGroup.Id) + "," +
                                (oBilRow.AdditionalInformation != null
                                     ? "'" + oBilRow.AdditionalInformation + "'"
                                     : "NULL") + "," +
                                (oBilRow.AccountStructure != null ? "'" + oBilRow.AccountStructure + "'" : "NULL") +
                                ")";
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine("Eintrag mit ID 0 gefunden!");
                        }
                        if (++nRow%100 == 0)
                        {
                            db.ExecuteNonQuery(sqlInsertPrefix + sqlInsert);
                            sqlInsert = string.Empty;
                        }
                    }
                    if (sqlInsert.Length > 0)
                    {
                        db.ExecuteNonQuery(sqlInsertPrefix + sqlInsert);
                    }
                }
                catch (Exception ex)
                {
                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                    {
                        _log.Error(ex.Message, ex);
                    }
                    throw;
                }
            }
        }

        public static List<BilRow> GetStructure(StreamReader oReader, string txtAccountsStructure)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            string accStructure = String.Empty;
            List<BilRow> ret = new List<BilRow>();
            int index = 1;
            Regex levelRexexp = new Regex(@"([ \|-]*)(.*)", RegexOptions.IgnoreCase);
            Regex rangeRexexp = new Regex(@"^(.{4})([\S]*[\d]+[\S]*)+\s-\s([\S]*[\d]+[\S]*)+\s+([X_\|]{3})(.*)$",
                                          RegexOptions.IgnoreCase);
            Regex singleRexexp = new Regex(@"^([\S]*[\d]+[\S]*)(.*)$", RegexOptions.IgnoreCase);
            Regex hasNumberRexexp = new Regex(@"([0-9]+)", RegexOptions.IgnoreCase);

            BilRow oldRow = null;
            while (!oReader.EndOfStream)
            {
                string sLine = oReader.ReadLine();
                string content = sLine.Replace("|", "").Replace("-", "").Trim();
                if (String.IsNullOrEmpty(sLine) || String.IsNullOrEmpty(content))
                    continue;
                if (!levelRexexp.IsMatch(sLine))
                    continue;
                var levelMatch = levelRexexp.Matches(sLine);
                if (levelMatch.Count == 0 || levelMatch[0].Groups.Count < 3 ||
                    levelMatch[0].Groups[1].Value.Length%4 != 0)
                    continue;
                int level = levelMatch[0].Groups[1].Length/4;
                sLine = levelMatch[0].Groups[2].Value;

                BilRow row = new BilRow();
                row.Level = level;
                row.Name = content.Trim();
                row.ShortName = "";
                row.Account = "";
                row.AccountTo = "";
                row.AccountFrom = "";
                if (rangeRexexp.IsMatch(sLine))
                {
                    var match = rangeRexexp.Match(sLine);
                    row.AccountStructure = match.Groups[1].Value.Trim();
                    row.AccountFrom = match.Groups[2].Value.Trim();
                    row.AccountTo = match.Groups[3].Value.Trim();
                    row.Type = "B";
                }
                else if (singleRexexp.IsMatch(sLine))
                {
                    var match = singleRexexp.Match(sLine);
                    row.AccountFrom = "";
                    row.AccountTo = "";
                    row.Account = match.Groups[1].Value.Trim();
                    if (".".Equals(row.Account))
                        row.Account = "";
                    row.ShortName = row.Account;
                }
                while (oldRow != null && oldRow.Level >= level)
                {
                    oldRow = oldRow.ParentRow;
                }
                row.ParentRow = oldRow;
                if (row.ParentRow != null)
                    row.HighestGroup = row.ParentRow.HighestGroup ?? row.ParentRow;

                #region Debit or Credit

                if (sLine.Contains("X|X"))
                {
                    row.Debit = true;
                    row.Credit = true;
                }
                else if (sLine.Contains("_|X"))
                {
                    row.Debit = false;
                    row.Credit = true;
                }
                else if (sLine.Contains("X|_"))
                {
                    row.Debit = true;
                    row.Credit = false;
                }
                else
                {
                    row.Debit = false;
                    row.Credit = false;
                }

                #endregion

                if (String.IsNullOrEmpty(row.Type))
                {
                    if (!sLine.Contains("-----") && !sLine.Contains(txtAccountsStructure))
                    {
                        row.Type = "H";
                        if (!row.Debit && !row.Credit)
                        {
                            row.Debit = true;
                            row.Credit = true;
                        }
                    }
                    else
                    {
                        row.Type = "K";
                    }
                }
                string gLine = sLine.ToLower().Replace(" ", "").Replace("-", "");
                if (gLine.EndsWith("bilanzergebnisgewinn")) row.AdditionalInformation = "H";
                if (gLine.EndsWith("bilanzergebnisverlust")) row.AdditionalInformation = "S";
                if (gLine.EndsWith("gewinnundverlustrechnung") || gLine.EndsWith("incomestatement"))
                {
                    row.AdditionalInformation = "GUV";
                }
                if (gLine.EndsWith("ergebnisverwendung") || gLine.EndsWith("appropriatedresult"))
                    row.AdditionalInformation = "E";
                if (gLine.EndsWith("finanzergebnis")) row.AdditionalInformation = "F";
                if (gLine.EndsWith("guvergebnis")) row.AdditionalInformation = "G";
                if (row.Type == "K" && !row.Name.ToLower().Contains("zugeordnet") &&
                    !row.Name.ToLower().Contains("not assigned") && String.IsNullOrEmpty(row.AdditionalInformation) &&
                    !hasNumberRexexp.IsMatch(row.Account))
                    continue;
                if (row.Type == "B" && !row.Name.ToLower().Contains("zugeordnet") &&
                    !row.Name.ToLower().Contains("not assigned") && String.IsNullOrEmpty(row.AdditionalInformation) &&
                    (!hasNumberRexexp.IsMatch(row.AccountFrom) || !hasNumberRexexp.IsMatch(row.AccountTo)))
                    continue;
                row.Id = index++;
                if (row.ParentRow == null)
                    ret.Add(row);
                else
                    row.ParentRow.Childs.Add(row);
                oldRow = row;
            }
            watch.Stop();
            Debug.WriteLine(watch.ElapsedMilliseconds);
            FoundNotAssigned = false;
            foreach (BilRow row in ret.ToList())
            {
                if (ClearableRow(row))
                    ret.Remove(row);
            }
            return ret;
        }

        private static bool ClearableRow(BilRow row)
        {
            if (!FoundNotAssigned &&
                (row.Name.ToLower().Contains("zugeordnet") || row.Name.ToLower().Contains("not assigned")))
            {
                FoundNotAssigned = false;
                row.Account = "";
                return false;
            }
            if (row.Type == "B" || !String.IsNullOrEmpty(row.AdditionalInformation))
                return false;
            foreach (BilRow child in row.Childs.ToList())
                if (ClearableRow(child))
                    row.Childs.Remove(child);
                else
                    return false;
            return true;
        }

        private static IEnumerable<BilRow> GetTree(IEnumerable<BilRow> rows)
        {
            foreach (BilRow row in rows)
            {
                yield return row;
                foreach (BilRow child in GetTree(row.Childs))
                {
                    yield return child;
                }
            }
        }
    }
}