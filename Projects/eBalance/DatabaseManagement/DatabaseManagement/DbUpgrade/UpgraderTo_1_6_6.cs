// -----------------------------------------------------------
// Created by Benjamin Held - 26.06.2011 13:54:19
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;
using DbAccess.Structures;

namespace DatabaseManagement.DbUpgrade {
    class UpgraderTo_1_6_6 {
        class TupleCreater {
            public string BaseString{get; set;}
            private List<Tuple<string, string>> columnAssignments;
            //private List<string> Columns;
            public TupleCreater Parent {get;set;}
            public int Id { get; set; }

            public TupleCreater() {
                this.Parent = null;
                this.columnAssignments = new List<Tuple<string, string>>();

            }
            public void Add(string left, string right){
                //Columns.Add(left);

                columnAssignments.Add(new Tuple<string,string>(left, right));
            }

            public List<Tuple<string, string>> ColumnAssignments() { return columnAssignments; }

        }


        int id = 1;
        int topLevelParent = 1;
        IDatabase readConn;
        IDatabase writeConn;

        public UpgraderTo_1_6_6(IDatabase conn) {
            this.readConn = conn;
        }

        void CreateTuples(List<TupleCreater> tuples) {
            TupleCreater tuple = new TupleCreater { BaseString = "genInfo.company.id.idNo" };
            tuple.Add("id_number_hrn", "genInfo.company.id.idNo.type.companyId.HRN");
            tuple.Add("id_number_uid", "genInfo.company.id.idNo.type.companyId.UID");
            tuple.Add("id_number_st13", "genInfo.company.id.idNo.type.companyId.ST13");
            tuple.Add("id_number_stid", "genInfo.company.id.idNo.type.companyId.STID");
            tuple.Add("id_number_stwid", "genInfo.company.id.idNo.type.companyId.STWID");
            tuple.Add("id_number_bf4", "genInfo.company.id.idNo.type.companyId.BF4");
            tuple.Add("id_number_bkn", "genInfo.company.id.idNo.type.companyId.BKN");
            tuple.Add("id_number_bun", "genInfo.company.id.idNo.type.companyId.BUN");
            tuple.Add("id_number_in", "genInfo.company.id.idNo.type.companyId.IN");
            tuple.Add("id_number_en", "genInfo.company.id.idNo.type.companyId.EN");
            tuple.Add("id_number_sn", "genInfo.company.id.idNo.type.companyId.SN");
            tuple.Add("id_number_s", "genInfo.company.id.idNo.type.companyId.S");
            tuples.Add(tuple);

            tuple = new TupleCreater { BaseString = "genInfo.company.id.stockExch" };
            tuple.Add("stock_exch_city", "genInfo.company.id.stockExch.city");
            tuple.Add("stock_exch_ticker", "genInfo.company.id.stockExch.ticker");
            tuple.Add("stock_exch_market", "genInfo.company.id.stockExch.market");
            tuple.Add("stock_exch_type_of_security", "genInfo.company.id.stockExch.typeOfSecurity");
            tuple.Add("stock_exch_security_code", "genInfo.company.id.stockExch.securityCode");
            tuple.Add("stock_exch_sc_entry", "genInfo.company.id.stockExch.securityCode.entry");
            tuple.Add("stock_exch_sc_type", "genInfo.company.id.stockExch.securityCode.type");
            tuples.Add(tuple);

            tuple = new TupleCreater { BaseString = "genInfo.company.id.industry" };
            tuple.Add("industry_key_type", "genInfo.company.id.industry.keyType");
            tuple.Add("industry_key_entry", "genInfo.company.id.industry.keyEntry");
            tuple.Add("industry_key_name", "genInfo.company.id.industry.name");
            tuples.Add(tuple);
        }

        void AddShareHolderColumns(TupleCreater shareHolder, TupleCreater shareHolderKey) {
            shareHolder.Add("name", "genInfo.company.id.shareholder.name");
            shareHolder.Add("current_number", "genInfo.company.id.shareholder.currentnumber");
            shareHolder.Add("signer_id", "genInfo.company.id.shareholder.signerId");
            shareHolder.Add("tax_number", "genInfo.company.id.shareholder.taxnumber");
            shareHolder.Add("tax_id", "genInfo.company.id.shareholder.taxid");
            shareHolder.Add("wid", "genInfo.company.id.shareholder.WID");
            shareHolder.Add("profit_divide_key", "genInfo.company.id.shareholder.ProfitDivideKey");
            shareHolder.Add("pdk_date_of_underyear_change", "genInfo.company.id.shareholder.ProfitDivideKey.dateOfunderyearChange");
            shareHolder.Add("pdk_formerkey", "genInfo.company.id.shareholder.ProfitDivideKey.formerkey");
            shareHolder.Add("legal_status", "genInfo.company.id.shareholder.legalStatus");
            shareHolder.Add("special_balance_requiered", "genInfo.company.id.shareholder.SpecialBalanceRequiered");
            shareHolder.Add("extension_requiered", "genInfo.company.id.shareholder.extensionRequiered");

            shareHolderKey.Add("sdk_numerator", "genInfo.company.id.shareholder.ShareDivideKey.numerator");
            shareHolderKey.Add("sdk_denominator", "genInfo.company.id.shareholder.ShareDivideKey.denominator");
            
        }
        void AddContactPersonColumns(TupleCreater contact) {
            contact.Add("name", "genInfo.company.id.contactAddress.person");
            contact.Add("name", "genInfo.company.id.contactAddress.person.name");
            contact.Add("dept", "genInfo.company.id.contactAddress.person.dept");
            contact.Add("function", "genInfo.company.id.contactAddress.person.function");
            contact.Add("phone", "genInfo.company.id.contactAddress.person.phone");
            contact.Add("fax", "genInfo.company.id.contactAddress.person.fax");
            contact.Add("email", "genInfo.company.id.contactAddress.person.eMail");
        }

        void CreateColumnAssignments(List<Tuple<string, string>> columnAssignments) {
            columnAssignments.Add(new Tuple<string, string>("name", "genInfo.company.id.name"));
            columnAssignments.Add(new Tuple<string, string>("former_name", "genInfo.company.id.name.formerName"));
            columnAssignments.Add(new Tuple<string, string>("last_name_change", "genInfo.company.id.name.dateOfLastChange"));
            columnAssignments.Add(new Tuple<string, string>("legal_status", "genInfo.company.id.legalStatus"));
            columnAssignments.Add(new Tuple<string, string>("former_status", "genInfo.company.id.legalStatus.formerStatus"));
            columnAssignments.Add(new Tuple<string, string>("last_status_change", "genInfo.company.id.legalStatus.dateOfLastChange"));
            columnAssignments.Add(new Tuple<string, string>("foundation_date", "genInfo.company.id.FoundationDate"));
            columnAssignments.Add(new Tuple<string, string>("last_tax_audit", "genInfo.company.id.lastTaxAudit"));
            columnAssignments.Add(new Tuple<string, string>("size_class", "genInfo.company.id.sizeClass"));
            columnAssignments.Add(new Tuple<string, string>("business", "genInfo.company.id.business"));
            //columnAssignments.Add(new Tuple<string, string>("industry_key_type", "genInfo.company.id.industry.keyType"));
            //columnAssignments.Add(new Tuple<string, string>("industry_key_entry", "genInfo.company.id.industry.keyEntry"));
            //columnAssignments.Add(new Tuple<string, string>("industry_key_name", "genInfo.company.id.industry.name"));
            columnAssignments.Add(new Tuple<string, string>("company_status", "genInfo.company.id.CompanyStatus"));
            columnAssignments.Add(new Tuple<string, string>("company_internet_description", "genInfo.company.id.internet.description"));
            columnAssignments.Add(new Tuple<string, string>("company_internet_url", "genInfo.company.id.internet.url"));
            columnAssignments.Add(new Tuple<string, string>("company_internet_url", "genInfo.company.id.internet"));
            columnAssignments.Add(new Tuple<string, string>("coming_from", "genInfo.company.id.comingfrom"));
            columnAssignments.Add(new Tuple<string, string>("company_logo", "genInfo.company.id.companyLogo"));
            columnAssignments.Add(new Tuple<string, string>("user_specific", "genInfo.company.userSpecific"));
            columnAssignments.Add(new Tuple<string, string>("inc_Type", "genInfo.company.id.Incorporation.Type"));
            columnAssignments.Add(new Tuple<string, string>("inc_prefix", "genInfo.company.id.Incorporation.prefix"));
            columnAssignments.Add(new Tuple<string, string>("inc_section", "genInfo.company.id.Incorporation.section"));
            columnAssignments.Add(new Tuple<string, string>("inc_number", "genInfo.company.id.Incorporation.number"));
            columnAssignments.Add(new Tuple<string, string>("inc_suffix", "genInfo.company.id.Incorporation.suffix"));
            columnAssignments.Add(new Tuple<string, string>("inc_court", "genInfo.company.id.Incorporation.court"));
            columnAssignments.Add(new Tuple<string, string>("inc_date_of_first_registration", "genInfo.company.id.Incorporation.dateOfFirstRegistration"));
            columnAssignments.Add(new Tuple<string, string>("loc_street", "genInfo.company.id.location.street"));
            columnAssignments.Add(new Tuple<string, string>("loc_house_number", "genInfo.company.id.location.houseNo"));
            columnAssignments.Add(new Tuple<string, string>("loc_zip", "genInfo.company.id.location.zipCode"));
            columnAssignments.Add(new Tuple<string, string>("loc_city", "genInfo.company.id.location.city"));
            columnAssignments.Add(new Tuple<string, string>("loc_iso", "genInfo.company.id.location.country.isoCode"));
            columnAssignments.Add(new Tuple<string, string>("id_number_hrn", "genInfo.company.id.idNo.type.companyId.HRN"));
            columnAssignments.Add(new Tuple<string, string>("id_number_uid", "genInfo.company.id.idNo.type.companyId.UID"));
            columnAssignments.Add(new Tuple<string, string>("id_number_st13", "genInfo.company.id.idNo.type.companyId.ST13"));
            columnAssignments.Add(new Tuple<string, string>("id_number_stid", "genInfo.company.id.idNo.type.companyId.STID"));
            columnAssignments.Add(new Tuple<string, string>("id_number_stwid", "genInfo.company.id.idNo.type.companyId.STWID"));
            columnAssignments.Add(new Tuple<string, string>("id_number_bf4", "genInfo.company.id.idNo.type.companyId.BF4"));
            columnAssignments.Add(new Tuple<string, string>("id_number_bkn", "genInfo.company.id.idNo.type.companyId.BKN"));
            columnAssignments.Add(new Tuple<string, string>("id_number_bun", "genInfo.company.id.idNo.type.companyId.BUN"));
            columnAssignments.Add(new Tuple<string, string>("id_number_in", "genInfo.company.id.idNo.type.companyId.IN"));
            columnAssignments.Add(new Tuple<string, string>("id_number_en", "genInfo.company.id.idNo.type.companyId.EN"));
            columnAssignments.Add(new Tuple<string, string>("id_number_sn", "genInfo.company.id.idNo.type.companyId.SN"));
            columnAssignments.Add(new Tuple<string, string>("id_number_s", "genInfo.company.id.idNo.type.companyId.S"));
            columnAssignments.Add(new Tuple<string, string>("stock_exch_city", "genInfo.company.id.stockExch.city"));
            columnAssignments.Add(new Tuple<string, string>("stock_exch_ticker", "genInfo.company.id.stockExch.ticker"));
            columnAssignments.Add(new Tuple<string, string>("stock_exch_market", "genInfo.company.id.stockExch.market"));
            columnAssignments.Add(new Tuple<string, string>("stock_exch_type_of_security", "genInfo.company.id.stockExch.typeOfSecurity"));
            columnAssignments.Add(new Tuple<string, string>("stock_exch_security_code", "genInfo.company.id.stockExch.securityCode"));
            columnAssignments.Add(new Tuple<string, string>("stock_exch_sc_entry", "genInfo.company.id.stockExch.securityCode.entry"));
            columnAssignments.Add(new Tuple<string, string>("stock_exch_sc_type", "genInfo.company.id.stockExch.securityCode.type"));

        }

        private object handleValueException(string elementId, object value) {
            if (elementId == "genInfo.company.id.shareholder.SpecialBalanceRequiered" || elementId == "genInfo.company.id.shareholder.extensionRequiered") {
                Boolean boolValue = Convert.ToBoolean(value);
                return boolValue ? "True" : "False";
                //if (boolValue != null) return boolValue ? "True" : "False";
                //int intValue = value as int;
                //return intValue ? "True" : "False";
            }
            return value;
        }

        private int AddEntryToCompanyTable(int company, object parent, string elementId, object value) {
            value = handleValueException(elementId, value);
            DbColumnValues values = new DbColumnValues();
            values["parent_id"] = parent;
            values["company_id"] = company;
            values["xbrl_elem_id"] = elementId;
            //ConvertTo necessary for SQLServer
            values["value"] = Convert.ToString(value);
            //values["id"] = id;
            SetDefaultValues(values);

            writeConn.InsertInto("values_gcd_company", values);
            return id++;
        }

        void SetDefaultValues(DbColumnValues values) {
            values["cb_value_other"] = null;
            values["is_manual_value"] = 0;
            values["flag1"] = 0;
            values["flag2"] = 0;
            values["flag3"] = 0;
            values["flag4"] = 0;

        }

        void AddTuple(TupleCreater tuple, int company, DbDataReader reader, Boolean addParent = true) {
            int parent = topLevelParent;
            if(tuple.Parent != null) parent = tuple.Parent.Id;

            int currentId = parent;
            if(addParent)
                currentId = AddEntryToCompanyTable(company, parent, tuple.BaseString, null);
            tuple.Id = AddEntryToCompanyTable(company, currentId, tuple.BaseString, null);
            foreach (var columnAssignment in tuple.ColumnAssignments()) {
                var value = reader[columnAssignment.Item1];
                AddEntryToCompanyTable(company, tuple.Id, columnAssignment.Item2, value);
            }
        }

        public void MigrateCompaniesTable() {
            readConn.DropTableIfExists("values_gcd_company");
            DatabaseCreator.Instance.CreateTable("1.1.6", readConn.DbConfig.DbType, readConn, "values_gcd_company");
            
            List<string> processedCompanyIds = new List<string>();

            List<Tuple<string, string>> columnAssignments = new List<Tuple<string, string>>();
            CreateColumnAssignments(columnAssignments);

            List<TupleCreater> tuples = new List<TupleCreater>();
            CreateTuples(tuples);
            
            using (writeConn = ConnectionManager.CreateConnection(readConn.DbConfig)) {
                writeConn.Open();
                writeConn.BeginTransaction();
                foreach (string companyId in readConn.GetColumnStringValues("SELECT " + readConn.Enquote("id") + " FROM " + readConn.Enquote("companies"))) {
                    int cId = Convert.ToInt32(companyId);
                    topLevelParent = AddEntryToCompanyTable(cId, null, null, null);
                    // ignore already processed companies
                    if (processedCompanyIds.Contains(companyId)) continue;
                    else processedCompanyIds.Add(companyId);

                    var reader = readConn.ExecuteReader("SELECT * FROM " + readConn.Enquote("companies") + " WHERE ID = " + companyId);
                    try {
                        reader.Read();
                        foreach (var tuple in columnAssignments) {
                            var value = reader[tuple.Item1];
                            if (value is DBNull) continue;
                            AddEntryToCompanyTable(cId, topLevelParent, tuple.Item2, value);
                        }
                        foreach (var tupleCreater in tuples) {
                            AddTuple(tupleCreater, cId, (DbDataReader)reader);
                        }
                        



                    } finally {
                        if (reader != null && !reader.IsClosed) {
                            reader.Close();
                        }
                    }
                    //shareholder
                    TupleCreater shareHolderParent = new TupleCreater();
                    //Add toplevel shareholder
                    shareHolderParent.Id = AddEntryToCompanyTable(cId, topLevelParent, "genInfo.company.id.shareholder", null);
                    reader = readConn.ExecuteReader("SELECT * FROM " + readConn.Enquote("shareholders") + " WHERE company_id = " + companyId);
                    try {

                        while (reader.Read()) {
                            TupleCreater tuple = new TupleCreater { BaseString = "genInfo.company.id.shareholder", Parent = shareHolderParent };
                            TupleCreater shareHolderKey = new TupleCreater { BaseString = "genInfo.company.id.shareholder.ShareDivideKey", Parent = tuple };

                            AddShareHolderColumns(tuple, shareHolderKey);
                            AddTuple(tuple, cId, (DbDataReader)reader, false);
                            AddTuple(shareHolderKey, cId, (DbDataReader)reader);
                        }

                    } finally {
                        if (reader != null && !reader.IsClosed) {
                            reader.Close();
                        }
                    }
                    //Contact persons
                    TupleCreater contactParent = new TupleCreater();
                    //Add toplevel shareholder
                    contactParent.Id = AddEntryToCompanyTable(cId, topLevelParent, "genInfo.company.id.contactAddress", null);
                    reader = readConn.ExecuteReader("SELECT * FROM " + readConn.Enquote("company_contacts") + " WHERE company_id = " + companyId);
                    try {

                        while (reader.Read()) {
                            TupleCreater tuple = new TupleCreater { BaseString = "genInfo.company.id.contactAddress", Parent = contactParent };

                            AddContactPersonColumns(tuple);
                            AddTuple(tuple, cId, (DbDataReader)reader, false);
                        }

                    } finally {
                        if (reader != null && !reader.IsClosed) {
                            reader.Close();
                        }
                    }

                }
                writeConn.CommitTransaction();
                //foreach (string shareholder in readConn.GetColumnStringValues("SELECT " + readConn.Enquote("id") + " FROM " + readConn.Enquote("shareholders"))) {
                //    DbDataReader reader = readConn.ExecuteReader("SELECT * FROM " + readConn.Enquote("shareholders") + " WHERE ID = " + shareholder);

                //    try {
                //        reader.Read();
                //        TupleCreater shareHolderParent = new TupleCreater();
                //        TupleCreater tuple = new TupleCreater { BaseString = "genInfo.company.id.shareholder", Parent = shareHolderParent };
                //        tuple.Add("genInfo.company.id.shareholder.name")
                        


                //    } finally {
                //        if (reader != null && !reader.IsClosed) {
                //            reader.Close();
                //        }
                //    }

                //}
            }
        }

    }
}
