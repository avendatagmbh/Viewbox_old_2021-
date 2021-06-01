// -----------------------------------------------------------
// Created by Benjamin Held - 26.06.2011 13:54:38
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using DbAccess;

namespace DatabaseManagement.DbUpgrade {
    class DatabaseCreator {
        struct IndexInformation{
            public List<KeyValuePair<string, List<string>>> indices;
            public string tableName;
        }
        struct TableInformation {
            public List<string> tableCreation;
            public List<string> tableNames;
            public List<IndexInformation> indexInfo;
        }

        Dictionary<string, Dictionary<string,TableInformation> > versionToTableInformation;
        private static DatabaseCreator instance;

        List<string> ExtractTableNames(string database, List<string> tables) {
            if (tables == null) return null;
            List<string> result = new List<string>();
            char []delim = new char[2] {'`', '`'};
            if(database == "SQLServer"){
                delim[0] = '[';
                delim[1] = ']';
            }

            if (database.ToLower().Equals("oracle")) delim[0] = delim[1] = '\"';
            
            foreach (var table in tables) {
                int first = table.IndexOf(delim[0])+1;
                int last = table.IndexOf(delim[1], first);
                result.Add(table.Substring(first, (last-first)));
            }

            return result;
        }

        private void AddVersion(string version,TableInformation mysql, TableInformation sqlite, TableInformation sqlserver, TableInformation oracle) {
            mysql.tableNames = ExtractTableNames("MySQL", mysql.tableCreation);
            sqlite.tableNames = ExtractTableNames("SQLite", sqlite.tableCreation);
            sqlserver.tableNames = ExtractTableNames("SQLServer", sqlserver.tableCreation);
            
            oracle.tableNames = ExtractTableNames("Oracle", oracle.tableCreation);

            sqlite.indexInfo = mysql.indexInfo;
            sqlserver.indexInfo = mysql.indexInfo;

            Dictionary<string, TableInformation> tableInformationDict = new Dictionary<string, TableInformation>();
            tableInformationDict["MySQL"] = mysql;
            tableInformationDict["SQLite"] = sqlite;
            tableInformationDict["SQLServer"] = sqlserver;
            tableInformationDict["Oracle"] = oracle;

            versionToTableInformation.Add(version, tableInformationDict);

        }

        private DatabaseCreator() {
            versionToTableInformation = new Dictionary<string, Dictionary<string, TableInformation>>();
            {
                #region version 100
                TableInformation version100_mysql = new TableInformation();
                version100_mysql.tableCreation = new List<string> {
                    "CREATE TABLE `accounts`(`id` INT NOT NULL AUTO_INCREMENT,`balance_list_id` INT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(64) NOT NULL,`balance` decimal(20,2) NOT NULL,`assigned_element_id` TEXT,`dc_flag` VARCHAR(1),PRIMARY KEY(`id`),FOREIGN KEY `fk_4d204b5c56d748a6949af769dbfc0ff4`(`balance_list_id`) REFERENCES `balance_lists`(`id`) ON DELETE CASCADE ON UPDATE CASCADE)",
                    "CREATE TABLE `financial_years`(`id` INT NOT NULL AUTO_INCREMENT,`company_id` INT NOT NULL,`fyear` INT,`is_enabled` TINYINT(1),`fiscal_year_begin` DATETIME,`fiscal_year_end` DATETIME,`bal_sheet_closing_date` DATETIME,PRIMARY KEY(`id`),FOREIGN KEY `fk_effe3ef1711f42469b2ff0d35d0eb019`(`company_id`) REFERENCES `companies`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE)",
                    "CREATE TABLE `document_rights`(`id` INT NOT NULL AUTO_INCREMENT,`document_id` INT NOT NULL,`user_id` INT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `company_contacts`(`id` INT NOT NULL AUTO_INCREMENT,`company_id` INT NOT NULL,`name` VARCHAR(64),`dept` VARCHAR(64),`function` VARCHAR(64),`phone` VARCHAR(64),`fax` VARCHAR(64),`email` VARCHAR(64),PRIMARY KEY(`id`),FOREIGN KEY `fk_4a172545d6e24a9bb44839b48f95c3e8`(`company_id`) REFERENCES `companies`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE)",
                    "CREATE TABLE `documents`(`id` INT NOT NULL AUTO_INCREMENT,`name` VARCHAR(256) NOT NULL,`comment` TEXT,`company_id` INT,`system_id` INT,`financial_year_id` INT,`creator_id` INT,`creation_date` DATETIME,`last_modifier_id` INT,`last_modified_date` DATETIME,`balance_list_id` INT,PRIMARY KEY(`id`),FOREIGN KEY `fk_24963f6402e04c2db99bff8297a72117`(`balance_list_id`) REFERENCES `balance_lists`(`id`) ON DELETE CASCADE ON UPDATE CASCADE)",
                    "CREATE TABLE `assignment_template_heads`(`id` INT NOT NULL AUTO_INCREMENT,`name` VARCHAR(64),`account_structure` VARCHAR(128),`comment` TEXT,`creator_id` INT,`last_modifier_id` INT,`creation_date` DATETIME,PRIMARY KEY(`id`))",
                    "CREATE TABLE `users`(`id` INT NOT NULL AUTO_INCREMENT,`username` VARCHAR(64),`fullname` VARCHAR(256),`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` TINYINT(1),`is_admin` TINYINT(1),PRIMARY KEY(`id`),UNIQUE KEY uk_e02b1e4b8d1842adb7fab16597069460(`username`))",
                    "CREATE TABLE `info`(`id` INT NOT NULL AUTO_INCREMENT,`key` VARCHAR(64) NOT NULL,`value` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_f2d7f3d2f8914ec7b8cd5f1fc6a75248(`key`))",
                    "CREATE TABLE `companies`(`Id` INT NOT NULL AUTO_INCREMENT,`name` VARCHAR(256) NOT NULL,`former_name` TEXT,`last_name_change` DATETIME,`legal_status` VARCHAR(256),`former_status` VARCHAR(256),`last_status_change` DATETIME,`foundation_date` DATETIME,`last_tax_audit` INT,`size_class` VARCHAR(256),`business` VARCHAR(256),`industry_key_type` VARCHAR(256),`industry_key_type_other` VARCHAR(64),`industry_key_entry` VARCHAR(64),`industry_key_name` VARCHAR(64),`company_status` VARCHAR(256),`company_internet_description` VARCHAR(256),`company_internet_url` VARCHAR(256),`coming_from` VARCHAR(256),`company_logo` VARCHAR(256),`user_specific` TEXT,`inc_Type` VARCHAR(256),`inc_prefix` VARCHAR(64),`inc_section` VARCHAR(64),`inc_number` VARCHAR(64),`inc_suffix` VARCHAR(64),`inc_court` VARCHAR(64),`inc_date_of_first_registration` DATETIME,`loc_street` VARCHAR(64),`loc_house_number` VARCHAR(10),`loc_zip` VARCHAR(10),`loc_city` VARCHAR(64),`loc_iso` VARCHAR(3),`id_number_hrn` VARCHAR(20),`id_number_uid` VARCHAR(20),`id_number_st13` VARCHAR(13),`id_number_stid` VARCHAR(20),`id_number_stwid` VARCHAR(20),`id_number_bf4` VARCHAR(4),`id_number_bkn` VARCHAR(20),`id_number_bun` VARCHAR(20),`id_number_in` VARCHAR(20),`id_number_en` VARCHAR(20),`id_number_sn` VARCHAR(20),`id_number_s` VARCHAR(20),`stock_exch_city` VARCHAR(64),`stock_exch_ticker` VARCHAR(64),`stock_exch_market` VARCHAR(64),`stock_exch_type_of_security` VARCHAR(64),`stock_exch_security_code` VARCHAR(64),`stock_exch_sc_entry` VARCHAR(64),`stock_exch_sc_type` VARCHAR(64),PRIMARY KEY(`Id`))",
                    "CREATE TABLE `values_gcd`(`id` INT NOT NULL AUTO_INCREMENT,`document_id` INT,`parent_id` INT,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`flag1` TINYINT(1) NOT NULL,`flag2` TINYINT(1) NOT NULL,`flag3` TINYINT(1) NOT NULL,`flag4` TINYINT(1) NOT NULL,PRIMARY KEY(`id`),FOREIGN KEY `fk_10ee5f92b018470c8628d5d563140930`(`document_id`) REFERENCES `documents`(`id`) ON DELETE CASCADE ON UPDATE CASCADE,FOREIGN KEY `fk_0299c72979604bb58bd388bc9642f3ac`(`parent_id`) REFERENCES `values_gcd`(`id`))",
                    "CREATE TABLE `values_gaap`(`id` INT NOT NULL AUTO_INCREMENT,`document_id` INT,`parent_id` INT,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`flag1` TINYINT(1) NOT NULL,`flag2` TINYINT(1) NOT NULL,`flag3` TINYINT(1) NOT NULL,`flag4` TINYINT(1) NOT NULL,PRIMARY KEY(`id`),FOREIGN KEY `fk_31cefcb7bc8248b0a9eb842defa2c987`(`document_id`) REFERENCES `documents`(`id`) ON DELETE CASCADE ON UPDATE CASCADE,FOREIGN KEY `fk_a5b5a8cf6e0049ca82287c43cba9c111`(`parent_id`) REFERENCES `values_gaap`(`id`))",
                    "CREATE TABLE `shareholders`(`id` INT NOT NULL AUTO_INCREMENT,`company_id` INT NOT NULL,`name` VARCHAR(80) NOT NULL,`current_number` VARCHAR(20),`signer_id` VARCHAR(20),`tax_number` VARCHAR(13),`tax_id` VARCHAR(11),`wid` VARCHAR(20),`profit_divide_key` VARCHAR(20),`pdk_date_of_underyear_change` DATETIME,`pdk_formerkey` VARCHAR(64),`legal_status` VARCHAR(256),`sdk_numerator` INT,`sdk_denominator` INT,`special_balance_requiered` TINYINT(1),`extension_requiered` TINYINT(1),PRIMARY KEY(`id`),FOREIGN KEY `fk_576b0a80434147258a639c6fa08fed30`(`company_id`) REFERENCES `companies`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE)",
                    "CREATE TABLE `balance_lists`(`id` INT NOT NULL AUTO_INCREMENT,`imported_from_id` INT,`import_date` DATETIME,`source` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `systems`(`id` INT NOT NULL AUTO_INCREMENT,`name` VARCHAR(64) NOT NULL,`comment` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_7a977d0bf8e646d5b730de6455c47316(`name`))",
                    "CREATE TABLE `assignment_template_lines`(`id` INT NOT NULL AUTO_INCREMENT,`template_id` INT,`account_number` VARCHAR(32),`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`),FOREIGN KEY `fk_444a8397c1ec4ea0aae01d310a24693a`(`template_id`) REFERENCES `assignment_template_heads`(`id`) ON DELETE CASCADE ON UPDATE CASCADE)",
                };

                TableInformation version100_sqlite = new TableInformation();
                version100_sqlite.tableCreation = new List<string> {
                    "CREATE TABLE `accounts`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`balance_list_id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`balance` REAL NOT NULL,`assigned_element_id` TEXT,`dc_flag` TEXT,FOREIGN KEY (`balance_list_id`) REFERENCES `balance_lists`(`id`) ON DELETE CASCADE ON UPDATE CASCADE)",
                    "CREATE TABLE `financial_years`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`company_id` INTEGER NOT NULL,`fyear` INTEGER,`is_enabled` INTEGER,`fiscal_year_begin` TEXT,`fiscal_year_end` TEXT,`bal_sheet_closing_date` TEXT,FOREIGN KEY (`company_id`) REFERENCES `companies`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE)",
                    "CREATE TABLE `document_rights`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`document_id` INTEGER NOT NULL,`user_id` INTEGER NOT NULL)",
                    "CREATE TABLE `company_contacts`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`company_id` INTEGER NOT NULL,`name` TEXT,`dept` TEXT,`function` TEXT,`phone` TEXT,`fax` TEXT,`email` TEXT,FOREIGN KEY (`company_id`) REFERENCES `companies`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE)",
                    "CREATE TABLE `documents`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`name` TEXT NOT NULL,`comment` TEXT,`company_id` INTEGER,`system_id` INTEGER,`financial_year_id` INTEGER,`creator_id` INTEGER,`creation_date` TEXT,`last_modifier_id` INTEGER,`last_modified_date` TEXT,`balance_list_id` INTEGER,FOREIGN KEY (`balance_list_id`) REFERENCES `balance_lists`(`id`) ON DELETE CASCADE ON UPDATE CASCADE)",
                    "CREATE TABLE `assignment_template_heads`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`name` TEXT,`account_structure` TEXT,`comment` TEXT,`creator_id` INTEGER,`last_modifier_id` INTEGER,`creation_date` TEXT)",
                    "CREATE TABLE `users`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`username` TEXT,`fullname` TEXT,`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` INTEGER,`is_admin` INTEGER,UNIQUE (`username`))",
                    "CREATE TABLE `info`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`key` TEXT NOT NULL,`value` TEXT,UNIQUE (`key`))",
                    "CREATE TABLE `companies`(`Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`name` TEXT NOT NULL,`former_name` TEXT,`last_name_change` TEXT,`legal_status` TEXT,`former_status` TEXT,`last_status_change` TEXT,`foundation_date` TEXT,`last_tax_audit` INTEGER,`size_class` TEXT,`business` TEXT,`industry_key_type` TEXT,`industry_key_type_other` TEXT,`industry_key_entry` TEXT,`industry_key_name` TEXT,`company_status` TEXT,`company_internet_description` TEXT,`company_internet_url` TEXT,`coming_from` TEXT,`company_logo` TEXT,`user_specific` TEXT,`inc_Type` TEXT,`inc_prefix` TEXT,`inc_section` TEXT,`inc_number` TEXT,`inc_suffix` TEXT,`inc_court` TEXT,`inc_date_of_first_registration` TEXT,`loc_street` TEXT,`loc_house_number` TEXT,`loc_zip` TEXT,`loc_city` TEXT,`loc_iso` TEXT,`id_number_hrn` TEXT,`id_number_uid` TEXT,`id_number_st13` TEXT,`id_number_stid` TEXT,`id_number_stwid` TEXT,`id_number_bf4` TEXT,`id_number_bkn` TEXT,`id_number_bun` TEXT,`id_number_in` TEXT,`id_number_en` TEXT,`id_number_sn` TEXT,`id_number_s` TEXT,`stock_exch_city` TEXT,`stock_exch_ticker` TEXT,`stock_exch_market` TEXT,`stock_exch_type_of_security` TEXT,`stock_exch_security_code` TEXT,`stock_exch_sc_entry` TEXT,`stock_exch_sc_type` TEXT)",
                    "CREATE TABLE `values_gcd`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`document_id` INTEGER,`parent_id` INTEGER,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`flag1` INTEGER NOT NULL,`flag2` INTEGER NOT NULL,`flag3` INTEGER NOT NULL,`flag4` INTEGER NOT NULL,FOREIGN KEY (`document_id`) REFERENCES `documents`(`id`) ON DELETE CASCADE ON UPDATE CASCADE,FOREIGN KEY (`parent_id`) REFERENCES `values_gcd`(`id`))",
                    "CREATE TABLE `values_gaap`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`document_id` INTEGER,`parent_id` INTEGER,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`flag1` INTEGER NOT NULL,`flag2` INTEGER NOT NULL,`flag3` INTEGER NOT NULL,`flag4` INTEGER NOT NULL,FOREIGN KEY (`document_id`) REFERENCES `documents`(`id`) ON DELETE CASCADE ON UPDATE CASCADE,FOREIGN KEY (`parent_id`) REFERENCES `values_gaap`(`id`))",
                    "CREATE TABLE `shareholders`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`company_id` INTEGER NOT NULL,`name` TEXT NOT NULL,`current_number` TEXT,`signer_id` TEXT,`tax_number` TEXT,`tax_id` TEXT,`wid` TEXT,`profit_divide_key` TEXT,`pdk_date_of_underyear_change` TEXT,`pdk_formerkey` TEXT,`legal_status` TEXT,`sdk_numerator` INTEGER,`sdk_denominator` INTEGER,`special_balance_requiered` INTEGER,`extension_requiered` INTEGER,FOREIGN KEY (`company_id`) REFERENCES `companies`(`Id`) ON DELETE CASCADE ON UPDATE CASCADE)",
                    "CREATE TABLE `balance_lists`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`imported_from_id` INTEGER,`import_date` TEXT,`source` TEXT)",
                    "CREATE TABLE `systems`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`name` TEXT NOT NULL,`comment` TEXT,UNIQUE (`name`))",
                    "CREATE TABLE `assignment_template_lines`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`template_id` INTEGER,`account_number` TEXT,`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,FOREIGN KEY (`template_id`) REFERENCES `assignment_template_heads`(`id`) ON DELETE CASCADE ON UPDATE CASCADE)",
                };

                TableInformation version100_sqlserver = new TableInformation();
                version100_sqlserver.tableCreation = new List<string> {
                    //"CREATE TABLE [balance_lists]([id] INT NOT NULL IDENTITY,[imported_from_id] INT,[import_date] DATETIME,[source] TEXT,PRIMARY KEY([id]))",
                    //"CREATE TABLE [companies]([Id] INT NOT NULL IDENTITY,[name] VARCHAR(256) NOT NULL,[former_name] TEXT,[last_name_change] DATETIME,[legal_status] VARCHAR(256),[former_status] VARCHAR(256),[last_status_change] DATETIME,[foundation_date] DATETIME,[last_tax_audit] INT,[size_class] VARCHAR(256),[business] VARCHAR(256),[industry_key_type] VARCHAR(256),[industry_key_type_other] VARCHAR(64),[industry_key_entry] VARCHAR(64),[industry_key_name] VARCHAR(64),[company_status] VARCHAR(256),[company_internet_description] VARCHAR(256),[company_internet_url] VARCHAR(256),[coming_from] VARCHAR(256),[company_logo] VARCHAR(256),[user_specific] TEXT,[inc_Type] VARCHAR(256),[inc_prefix] VARCHAR(64),[inc_section] VARCHAR(64),[inc_number] VARCHAR(64),[inc_suffix] VARCHAR(64),[inc_court] VARCHAR(64),[inc_date_of_first_registration] DATETIME,[loc_street] VARCHAR(64),[loc_house_number] VARCHAR(10),[loc_zip] VARCHAR(10),[loc_city] VARCHAR(64),[loc_iso] VARCHAR(3),[id_number_hrn] VARCHAR(20),[id_number_uid] VARCHAR(20),[id_number_st13] VARCHAR(13),[id_number_stid] VARCHAR(20),[id_number_stwid] VARCHAR(20),[id_number_bf4] VARCHAR(4),[id_number_bkn] VARCHAR(20),[id_number_bun] VARCHAR(20),[id_number_in] VARCHAR(20),[id_number_en] VARCHAR(20),[id_number_sn] VARCHAR(20),[id_number_s] VARCHAR(20),[stock_exch_city] VARCHAR(64),[stock_exch_ticker] VARCHAR(64),[stock_exch_market] VARCHAR(64),[stock_exch_type_of_security] VARCHAR(64),[stock_exch_security_code] VARCHAR(64),[stock_exch_sc_entry] VARCHAR(64),[stock_exch_sc_type] VARCHAR(64),PRIMARY KEY([Id]))",
                    //"CREATE TABLE [accounts]([id] INT NOT NULL IDENTITY,[balance_list_id] INT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(64) NOT NULL,[balance] decimal(20,2) NOT NULL,[assigned_element_id] TEXT,[dc_flag] VARCHAR(1),PRIMARY KEY([id]),FOREIGN KEY ([balance_list_id]) REFERENCES [balance_lists]([id]) ON DELETE CASCADE ON UPDATE CASCADE)",
                    //"CREATE TABLE [financial_years]([id] INT NOT NULL IDENTITY,[company_id] INT NOT NULL,[fyear] INT,[is_enabled] TINYINT,[fiscal_year_begin] DATETIME,[fiscal_year_end] DATETIME,[bal_sheet_closing_date] DATETIME,PRIMARY KEY([id]),FOREIGN KEY ([company_id]) REFERENCES [companies]([Id]) ON DELETE CASCADE ON UPDATE CASCADE)",
                    //"CREATE TABLE [document_rights]([id] INT NOT NULL IDENTITY,[document_id] INT NOT NULL,[user_id] INT NOT NULL,PRIMARY KEY([id]))",
                    //"CREATE TABLE [company_contacts]([id] INT NOT NULL IDENTITY,[company_id] INT NOT NULL,[name] VARCHAR(64),[dept] VARCHAR(64),[function] VARCHAR(64),[phone] VARCHAR(64),[fax] VARCHAR(64),[email] VARCHAR(64),PRIMARY KEY([id]),FOREIGN KEY ([company_id]) REFERENCES [companies]([Id]) ON DELETE CASCADE ON UPDATE CASCADE)",
                    //"CREATE TABLE [documents]([id] INT NOT NULL IDENTITY,[name] VARCHAR(256) NOT NULL,[comment] TEXT,[company_id] INT,[system_id] INT,[financial_year_id] INT,[creator_id] INT,[creation_date] DATETIME,[last_modifier_id] INT,[last_modified_date] DATETIME,[balance_list_id] INT,PRIMARY KEY([id]),FOREIGN KEY ([balance_list_id]) REFERENCES [balance_lists]([id]) ON DELETE CASCADE ON UPDATE CASCADE)",
                    //"CREATE TABLE [assignment_template_heads]([id] INT NOT NULL IDENTITY,[name] VARCHAR(64),[account_structure] VARCHAR(128),[comment] TEXT,[creator_id] INT,[last_modifier_id] INT,[creation_date] DATETIME,PRIMARY KEY([id]))",
                    //"CREATE TABLE [users]([id] INT NOT NULL IDENTITY,[username] VARCHAR(64),[fullname] VARCHAR(256),[password] TEXT NOT NULL,[key] TEXT NOT NULL,[is_active] TINYINT,[is_admin] TINYINT,PRIMARY KEY([id]),UNIQUE ([username]))",
                    //"CREATE TABLE [info]([id] INT NOT NULL IDENTITY,[key] VARCHAR(64) NOT NULL,[value] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([key]))",
                    //"CREATE TABLE [values_gcd]([id] INT NOT NULL IDENTITY,[document_id] INT,[parent_id] INT,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[flag1] TINYINT NOT NULL,[flag2] TINYINT NOT NULL,[flag3] TINYINT NOT NULL,[flag4] TINYINT NOT NULL,PRIMARY KEY([id]),FOREIGN KEY ([document_id]) REFERENCES [documents]([id]) ON DELETE CASCADE ON UPDATE CASCADE,FOREIGN KEY ([parent_id]) REFERENCES [values_gcd]([id]))",
                    //"CREATE TABLE [values_gaap]([id] INT NOT NULL IDENTITY,[document_id] INT,[parent_id] INT,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[flag1] TINYINT NOT NULL,[flag2] TINYINT NOT NULL,[flag3] TINYINT NOT NULL,[flag4] TINYINT NOT NULL,PRIMARY KEY([id]),FOREIGN KEY ([document_id]) REFERENCES [documents]([id]) ON DELETE CASCADE ON UPDATE CASCADE,FOREIGN KEY ([parent_id]) REFERENCES [values_gaap]([id]))",
                    //"CREATE TABLE [shareholders]([id] INT NOT NULL IDENTITY,[company_id] INT NOT NULL,[name] VARCHAR(80) NOT NULL,[current_number] VARCHAR(20),[signer_id] VARCHAR(20),[tax_number] VARCHAR(13),[tax_id] VARCHAR(11),[wid] VARCHAR(20),[profit_divide_key] VARCHAR(20),[pdk_date_of_underyear_change] DATETIME,[pdk_formerkey] VARCHAR(64),[legal_status] VARCHAR(256),[sdk_numerator] INT,[sdk_denominator] INT,[special_balance_requiered] TINYINT,[extension_requiered] TINYINT,PRIMARY KEY([id]),FOREIGN KEY ([company_id]) REFERENCES [companies]([Id]) ON DELETE CASCADE ON UPDATE CASCADE)",
                    //"CREATE TABLE [systems]([id] INT NOT NULL IDENTITY,[name] VARCHAR(64) NOT NULL,[comment] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([name]))",
                    //"CREATE TABLE [assignment_template_lines]([id] INT NOT NULL IDENTITY,[template_id] INT,[account_number] VARCHAR(32),[account_name] TEXT,[debit_element_id] TEXT,[credit_element_id] TEXT,PRIMARY KEY([id]),FOREIGN KEY ([template_id]) REFERENCES [assignment_template_heads]([id]) ON DELETE CASCADE ON UPDATE CASCADE)",
                    "CREATE TABLE [balance_lists]([id] INT NOT NULL IDENTITY,[imported_from_id] INT,[import_date] DATETIME,[source] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [companies]([Id] INT NOT NULL IDENTITY,[name] VARCHAR(256) NOT NULL,[former_name] TEXT,[last_name_change] DATETIME,[legal_status] VARCHAR(256),[former_status] VARCHAR(256),[last_status_change] DATETIME,[foundation_date] DATETIME,[last_tax_audit] INT,[size_class] VARCHAR(256),[business] VARCHAR(256),[industry_key_type] VARCHAR(256),[industry_key_type_other] VARCHAR(64),[industry_key_entry] VARCHAR(64),[industry_key_name] VARCHAR(64),[company_status] VARCHAR(256),[company_internet_description] VARCHAR(256),[company_internet_url] VARCHAR(256),[coming_from] VARCHAR(256),[company_logo] VARCHAR(256),[user_specific] TEXT,[inc_Type] VARCHAR(256),[inc_prefix] VARCHAR(64),[inc_section] VARCHAR(64),[inc_number] VARCHAR(64),[inc_suffix] VARCHAR(64),[inc_court] VARCHAR(64),[inc_date_of_first_registration] DATETIME,[loc_street] VARCHAR(64),[loc_house_number] VARCHAR(10),[loc_zip] VARCHAR(10),[loc_city] VARCHAR(64),[loc_iso] VARCHAR(3),[id_number_hrn] VARCHAR(20),[id_number_uid] VARCHAR(20),[id_number_st13] VARCHAR(13),[id_number_stid] VARCHAR(20),[id_number_stwid] VARCHAR(20),[id_number_bf4] VARCHAR(4),[id_number_bkn] VARCHAR(20),[id_number_bun] VARCHAR(20),[id_number_in] VARCHAR(20),[id_number_en] VARCHAR(20),[id_number_sn] VARCHAR(20),[id_number_s] VARCHAR(20),[stock_exch_city] VARCHAR(64),[stock_exch_ticker] VARCHAR(64),[stock_exch_market] VARCHAR(64),[stock_exch_type_of_security] VARCHAR(64),[stock_exch_security_code] VARCHAR(64),[stock_exch_sc_entry] VARCHAR(64),[stock_exch_sc_type] VARCHAR(64),PRIMARY KEY([Id]))",
                    "CREATE TABLE [accounts]([id] INT NOT NULL IDENTITY,[balance_list_id] INT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(64) NOT NULL,[balance] decimal(20,2) NOT NULL,[assigned_element_id] TEXT,[dc_flag] VARCHAR(1),PRIMARY KEY([id]))",
                    "CREATE TABLE [financial_years]([id] INT NOT NULL IDENTITY,[company_id] INT NOT NULL,[fyear] INT,[is_enabled] TINYINT,[fiscal_year_begin] DATETIME,[fiscal_year_end] DATETIME,[bal_sheet_closing_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [document_rights]([id] INT NOT NULL IDENTITY,[document_id] INT NOT NULL,[user_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [company_contacts]([id] INT NOT NULL IDENTITY,[company_id] INT NOT NULL,[name] VARCHAR(64),[dept] VARCHAR(64),[function] VARCHAR(64),[phone] VARCHAR(64),[fax] VARCHAR(64),[email] VARCHAR(64),PRIMARY KEY([id]))",
                    "CREATE TABLE [documents]([id] INT NOT NULL IDENTITY,[name] VARCHAR(256) NOT NULL,[comment] TEXT,[company_id] INT,[system_id] INT,[financial_year_id] INT,[creator_id] INT,[creation_date] DATETIME,[last_modifier_id] INT,[last_modified_date] DATETIME,[balance_list_id] INT,PRIMARY KEY([id]),)",
                    "CREATE TABLE [assignment_template_heads]([id] INT NOT NULL IDENTITY,[name] VARCHAR(64),[account_structure] VARCHAR(128),[comment] TEXT,[creator_id] INT,[last_modifier_id] INT,[creation_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [users]([id] INT NOT NULL IDENTITY,[username] VARCHAR(64),[fullname] VARCHAR(256),[password] TEXT NOT NULL,[key] TEXT NOT NULL,[is_active] TINYINT,[is_admin] TINYINT,PRIMARY KEY([id]),UNIQUE ([username]))",
                    "CREATE TABLE [info]([id] INT NOT NULL IDENTITY,[key] VARCHAR(64) NOT NULL,[value] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([key]))",
                    "CREATE TABLE [values_gcd]([id] INT NOT NULL IDENTITY,[document_id] INT,[parent_id] INT,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[flag1] TINYINT NOT NULL,[flag2] TINYINT NOT NULL,[flag3] TINYINT NOT NULL,[flag4] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gaap]([id] INT NOT NULL IDENTITY,[document_id] INT,[parent_id] INT,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[flag1] TINYINT NOT NULL,[flag2] TINYINT NOT NULL,[flag3] TINYINT NOT NULL,[flag4] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [shareholders]([id] INT NOT NULL IDENTITY,[company_id] INT NOT NULL,[name] VARCHAR(80) NOT NULL,[current_number] VARCHAR(20),[signer_id] VARCHAR(20),[tax_number] VARCHAR(13),[tax_id] VARCHAR(11),[wid] VARCHAR(20),[profit_divide_key] VARCHAR(20),[pdk_date_of_underyear_change] DATETIME,[pdk_formerkey] VARCHAR(64),[legal_status] VARCHAR(256),[sdk_numerator] INT,[sdk_denominator] INT,[special_balance_requiered] TINYINT,[extension_requiered] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [systems]([id] INT NOT NULL IDENTITY,[name] VARCHAR(64) NOT NULL,[comment] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([name]))",
                    "CREATE TABLE [assignment_template_lines]([id] INT NOT NULL IDENTITY,[template_id] INT,[account_number] VARCHAR(32),[account_name] TEXT,[debit_element_id] TEXT,[credit_element_id] TEXT,PRIMARY KEY([id]))",
                };

                var version100_oracle = new TableInformation();
                AddVersion("1.0.0", version100_mysql, version100_sqlite, version100_sqlserver,version100_oracle);
                #endregion
            }

            {
                #region version116

                TableInformation version116_mysql = new TableInformation();
                version116_mysql.tableCreation = new List<string> {
                    "CREATE TABLE `values_gcd`(`parent_id` INT,`document_id` INT,`id` INT NOT NULL AUTO_INCREMENT,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`flag1` TINYINT(1) NOT NULL,`flag2` TINYINT(1) NOT NULL,`flag3` TINYINT(1) NOT NULL,`flag4` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_heads`(`id` INT NOT NULL AUTO_INCREMENT,`name` VARCHAR(64),`account_structure` VARCHAR(128),`comment` TEXT,`creator_id` INT,`last_modifier_id` INT,`creation_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `accounts`(`id` INT NOT NULL AUTO_INCREMENT,`balance_list_id` INT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(64) NOT NULL,`balance` decimal(20,2) NOT NULL,`assigned_element_id` TEXT,`dc_flag` VARCHAR(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `documents`(`id` INT NOT NULL AUTO_INCREMENT,`name` VARCHAR(256) NOT NULL,`comment` TEXT,`company_id` INT,`system_id` INT,`financial_year_id` INT,`creator_id` INT,`creation_date` DATETIME,`last_modifier_id` INT,`last_modified_date` DATETIME,`balance_list_id` INT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_net`(`id` INT NOT NULL AUTO_INCREMENT,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gaap`(`parent_id` INT,`document_id` INT,`id` INT NOT NULL AUTO_INCREMENT,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`flag1` TINYINT(1) NOT NULL,`flag2` TINYINT(1) NOT NULL,`flag3` TINYINT(1) NOT NULL,`flag4` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `users`(`id` INT NOT NULL AUTO_INCREMENT,`username` VARCHAR(64),`fullname` VARCHAR(256),`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` TINYINT(1),`is_admin` TINYINT(1),PRIMARY KEY(`id`),UNIQUE KEY uk_2534d85032464ba68e10aec024e8ec64(`username`)) ENGINE = InnoDb",
                    "CREATE TABLE `systems`(`id` INT NOT NULL AUTO_INCREMENT,`name` VARCHAR(64),`comment` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_daffa76c10c14a21b2530e3c2afae5fa(`name`)) ENGINE = InnoDb",
                    "CREATE TABLE `info`(`id` INT NOT NULL AUTO_INCREMENT,`key` VARCHAR(64) NOT NULL,`value` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_5b58176309774a05a5d4c23fade8a30a(`key`)) ENGINE = InnoDb",
                    "CREATE TABLE `financial_years`(`id` INT NOT NULL AUTO_INCREMENT,`company_id` INT NOT NULL,`fyear` INT,`is_enabled` TINYINT(1),`fiscal_year_begin` DATETIME,`fiscal_year_end` DATETIME,`bal_sheet_closing_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `document_rights`(`id` INT NOT NULL AUTO_INCREMENT,`document_id` INT NOT NULL,`user_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `balance_lists`(`id` INT NOT NULL AUTO_INCREMENT,`imported_from_id` INT,`import_date` DATETIME,`source` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd_company`(`parent_id` INT,`company_id` INT,`id` INT NOT NULL AUTO_INCREMENT,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`flag1` TINYINT(1) NOT NULL,`flag2` TINYINT(1) NOT NULL,`flag3` TINYINT(1) NOT NULL,`flag4` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_changes_equity_statement`(`id` INT NOT NULL AUTO_INCREMENT,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INT NOT NULL AUTO_INCREMENT,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_lines`(`id` INT NOT NULL AUTO_INCREMENT,`template_id` INT,`account_number` VARCHAR(32),`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_gross_short`(`id` INT NOT NULL AUTO_INCREMENT,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `companies`(`Id` INT NOT NULL AUTO_INCREMENT,`name` VARCHAR(256),PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                };

                TableInformation version116_sqlite = new TableInformation();
                version116_sqlite.tableCreation = new List<string> {
                    "CREATE TABLE `values_gcd`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`flag1` INTEGER NOT NULL,`flag2` INTEGER NOT NULL,`flag3` INTEGER NOT NULL,`flag4` INTEGER NOT NULL)",
                    "CREATE TABLE `assignment_template_heads`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`name` TEXT,`account_structure` TEXT,`comment` TEXT,`creator_id` INTEGER,`last_modifier_id` INTEGER,`creation_date` TEXT)",
                    "CREATE TABLE `accounts`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`balance_list_id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`balance` NUMERIC NOT NULL,`assigned_element_id` TEXT,`dc_flag` TEXT)",
                    "CREATE TABLE `documents`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`name` TEXT NOT NULL,`comment` TEXT,`company_id` INTEGER,`system_id` INTEGER,`financial_year_id` INTEGER,`creator_id` INTEGER,`creation_date` TEXT,`last_modifier_id` INTEGER,`last_modified_date` TEXT,`balance_list_id` INTEGER)",
                    "CREATE TABLE `hypercube_ass_net`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT)",
                    "CREATE TABLE `values_gaap`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`flag1` INTEGER NOT NULL,`flag2` INTEGER NOT NULL,`flag3` INTEGER NOT NULL,`flag4` INTEGER NOT NULL)",
                    "CREATE TABLE `users`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`username` TEXT,`fullname` TEXT,`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` INTEGER,`is_admin` INTEGER,UNIQUE (`username`))",
                    "CREATE TABLE `systems`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`name` TEXT,`comment` TEXT,UNIQUE (`name`))",
                    "CREATE TABLE `info`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`key` TEXT NOT NULL,`value` TEXT,UNIQUE (`key`))",
                    "CREATE TABLE `financial_years`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`company_id` INTEGER NOT NULL,`fyear` INTEGER,`is_enabled` INTEGER,`fiscal_year_begin` TEXT,`fiscal_year_end` TEXT,`bal_sheet_closing_date` TEXT)",
                    "CREATE TABLE `document_rights`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`document_id` INTEGER NOT NULL,`user_id` INTEGER NOT NULL)",
                    "CREATE TABLE `balance_lists`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`imported_from_id` INTEGER,`import_date` TEXT,`source` TEXT)",
                    "CREATE TABLE `values_gcd_company`(`parent_id` INTEGER,`company_id` INTEGER,`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`flag1` INTEGER NOT NULL,`flag2` INTEGER NOT NULL,`flag3` INTEGER NOT NULL,`flag4` INTEGER NOT NULL)",
                    "CREATE TABLE `hypercube_changes_equity_statement`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT)",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT)",
                    "CREATE TABLE `assignment_template_lines`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`template_id` INTEGER,`account_number` TEXT,`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT)",
                    "CREATE TABLE `hypercube_ass_gross_short`(`id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT)",
                    "CREATE TABLE `companies`(`Id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`name` TEXT)",
                };

                TableInformation version116_sqlserver = new TableInformation();
                version116_sqlserver.tableCreation = new List<string> {
                    "CREATE TABLE [values_gcd]([parent_id] INT,[document_id] INT,[id] INT NOT NULL IDENTITY,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[flag1] TINYINT NOT NULL,[flag2] TINYINT NOT NULL,[flag3] TINYINT NOT NULL,[flag4] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_heads]([id] INT NOT NULL IDENTITY,[name] VARCHAR(64),[account_structure] VARCHAR(128),[comment] TEXT,[creator_id] INT,[last_modifier_id] INT,[creation_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [accounts]([id] INT NOT NULL IDENTITY,[balance_list_id] INT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(64) NOT NULL,[balance] decimal(20,2) NOT NULL,[assigned_element_id] TEXT,[dc_flag] VARCHAR(1),PRIMARY KEY([id]))",
                    "CREATE TABLE [documents]([id] INT NOT NULL IDENTITY,[name] VARCHAR(256) NOT NULL,[comment] TEXT,[company_id] INT,[system_id] INT,[financial_year_id] INT,[creator_id] INT,[creation_date] DATETIME,[last_modifier_id] INT,[last_modified_date] DATETIME,[balance_list_id] INT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_net]([id] INT NOT NULL IDENTITY,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gaap]([parent_id] INT,[document_id] INT,[id] INT NOT NULL IDENTITY,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[flag1] TINYINT NOT NULL,[flag2] TINYINT NOT NULL,[flag3] TINYINT NOT NULL,[flag4] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [users]([id] INT NOT NULL IDENTITY,[username] VARCHAR(64),[fullname] VARCHAR(256),[password] TEXT NOT NULL,[key] TEXT NOT NULL,[is_active] TINYINT,[is_admin] TINYINT,PRIMARY KEY([id]),UNIQUE ([username]))",
                    "CREATE TABLE [systems]([id] INT NOT NULL IDENTITY,[name] VARCHAR(64),[comment] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([name]))",
                    "CREATE TABLE [info]([id] INT NOT NULL IDENTITY,[key] VARCHAR(64) NOT NULL,[value] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([key]))",
                    "CREATE TABLE [financial_years]([id] INT NOT NULL IDENTITY,[company_id] INT NOT NULL,[fyear] INT,[is_enabled] TINYINT,[fiscal_year_begin] DATETIME,[fiscal_year_end] DATETIME,[bal_sheet_closing_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [document_rights]([id] INT NOT NULL IDENTITY,[document_id] INT NOT NULL,[user_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [balance_lists]([id] INT NOT NULL IDENTITY,[imported_from_id] INT,[import_date] DATETIME,[source] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gcd_company]([parent_id] INT,[company_id] INT,[id] INT NOT NULL IDENTITY,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[flag1] TINYINT NOT NULL,[flag2] TINYINT NOT NULL,[flag3] TINYINT NOT NULL,[flag4] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_changes_equity_statement]([id] INT NOT NULL IDENTITY,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_gross]([id] INT NOT NULL IDENTITY,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_lines]([id] INT NOT NULL IDENTITY,[template_id] INT,[account_number] VARCHAR(32),[account_name] TEXT,[debit_element_id] TEXT,[credit_element_id] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_gross_short]([id] INT NOT NULL IDENTITY,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [companies]([Id] INT NOT NULL IDENTITY,[name] VARCHAR(256),PRIMARY KEY([Id]))",
                };

                //version116_mysql.tableNames = ExtractTableNames("MySQL", version116_mysql.tableCreation);
                //version116_sqlite.tableNames = ExtractTableNames("SQLite", version116_sqlite.tableCreation);
                //version116_sqlserver.tableNames = ExtractTableNames("SQLServer", version116_sqlserver.tableCreation);
                //tableInformationDict["MySQL"] = version116_mysql;
                //tableInformationDict["SQLite"] = version116_sqlite;
                //tableInformationDict["SQLServer"] = version116_sqlserver;

                //versionToTableInformation.Add("1.1.6", tableInformationDict);
                var version116_oracle = new TableInformation();
                AddVersion("1.1.6", version116_mysql, version116_sqlite, version116_sqlserver,version116_oracle);
                #endregion
            }

            {
                #region version119

                TableInformation version119_mysql = new TableInformation();
                version119_mysql.tableCreation = new List<string> {
                    "CREATE TABLE `documents`(`id` INT NOT NULL,`name` VARCHAR(256) NOT NULL,`comment` TEXT,`company_id` INT,`system_id` INT,`financial_year_id` INT,`creator_id` INT,`creation_date` DATETIME,`last_modifier_id` INT,`last_modified_date` DATETIME,`balance_list_id` INT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    //"CREATE TABLE `hypercube_ass_gross_short`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_lines`(`id` INT NOT NULL,`template_id` INT,`account_number` VARCHAR(32),`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `users`(`id` INT NOT NULL,`username` VARCHAR(64),`fullname` VARCHAR(256),`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` TINYINT(1),`is_admin` TINYINT(1),PRIMARY KEY(`id`),UNIQUE KEY uk_3ce62e6a657c4e598f8631f971350cd9(`username`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`document_id` INT NOT NULL,`type` INT NOT NULL,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `info`(`id` INT NOT NULL,`key` VARCHAR(64) NOT NULL,`value` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_06f6619dbc84405c97d6c342134948b1(`key`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd_company`(`parent_id` BIGINT,`company_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `companies`(`Id` INT NOT NULL,`name` VARCHAR(256),PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`account_structure` VARCHAR(128),`comment` TEXT,`creator_id` INT,`last_modifier_id` INT,`creation_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `systems`(`id` INT NOT NULL,`name` VARCHAR(64),`comment` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_ca3aff1fe487439484d96095a2eb6d22(`name`)) ENGINE = InnoDb",
                    "CREATE TABLE `accounts`(`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`balance` decimal(20,2) NOT NULL,`assigned_element_id` TEXT,`dc_flag` VARCHAR(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_element_info`(`id` BIGINT NOT NULL,`template_id` INT,`element_id` TEXT,`auto_compute_enabled` TINYINT(1),`supress_warning_messages` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_lines`(`id` BIGINT NOT NULL,`document_id` INT NOT NULL,`header_id` INT NOT NULL,`value_id` BIGINT,`transfer_value` decimal(20,2),`transfer_value_previous_year` decimal(20,2),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `financial_years`(`id` INT NOT NULL,`company_id` INT NOT NULL,`fyear` INT,`is_enabled` TINYINT(1),`fiscal_year_begin` DATETIME,`fiscal_year_end` DATETIME,`bal_sheet_closing_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `document_rights`(`id` BIGINT NOT NULL,`document_id` INT NOT NULL,`user_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `balance_lists`(`id` INT NOT NULL,`imported_from_id` INT,`import_date` DATETIME,`source` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gaap`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    //"CREATE TABLE `hypercube_changes_equity_statement`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    //"CREATE TABLE `hypercube_ass_net`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                };

                #region indices
                List<IndexInformation> indexInfo = new List<IndexInformation>();
                IndexInformation info = new IndexInformation();
                info.tableName = "documents";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_documents_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_gross_short";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_gross_short_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_assignment_template_lines_template_id",
                                                                        new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "users";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd_company";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "companies";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "systems";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_element_info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_assignment_template_element_info_template_id",
                                                           new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "financial_years";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_financial_years_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "document_rights";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_gross";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_gross_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "balance_lists";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gaap";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_changes_equity_statement";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_hypercube_changes_equity_statement_document_id",
                                                           new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_net";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_net_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);
                version119_mysql.indexInfo = indexInfo;
                #endregion

                TableInformation version119_sqlite = new TableInformation();
                version119_sqlite.tableCreation = new List<string> {
                    "CREATE TABLE `documents`(`id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`company_id` INTEGER,`system_id` INTEGER,`financial_year_id` INTEGER,`creator_id` INTEGER,`creation_date` TEXT,`last_modifier_id` INTEGER,`last_modified_date` TEXT,`balance_list_id` INTEGER,PRIMARY KEY(`id`))",
                    //"CREATE TABLE `hypercube_ass_gross_short`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_lines`(`id` INTEGER NOT NULL,`template_id` INTEGER,`account_number` TEXT,`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `users`(`id` INTEGER NOT NULL,`username` TEXT,`fullname` TEXT,`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` INTEGER,`is_admin` INTEGER,PRIMARY KEY(`id`),UNIQUE (`username`))",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INTEGER NOT NULL,`name` TEXT,`document_id` INTEGER NOT NULL,`type` INTEGER NOT NULL,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `info`(`id` INTEGER NOT NULL,`key` TEXT NOT NULL,`value` TEXT,PRIMARY KEY(`id`),UNIQUE (`key`))",
                    "CREATE TABLE `values_gcd_company`(`parent_id` INTEGER,`company_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `companies`(`Id` INTEGER NOT NULL,`name` TEXT,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `assignment_template_heads`(`id` INTEGER NOT NULL,`name` TEXT,`account_structure` TEXT,`comment` TEXT,`creator_id` INTEGER,`last_modifier_id` INTEGER,`creation_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `systems`(`id` INTEGER NOT NULL,`name` TEXT,`comment` TEXT,PRIMARY KEY(`id`),UNIQUE (`name`))",
                    //"CREATE TABLE `accounts`(`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`balance` REAL NOT NULL,`assigned_element_id` TEXT,`dc_flag` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `accounts`(`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`balance` NUMERIC NOT NULL,`assigned_element_id` TEXT,`dc_flag` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_element_info`(`id` INTEGER NOT NULL,`template_id` INTEGER,`element_id` TEXT,`auto_compute_enabled` INTEGER,`supress_warning_messages` INTEGER NOT NULL,`send_account_balances` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    //"CREATE TABLE `transfer_hbst_lines`(`id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`header_id` INTEGER NOT NULL,`value_id` INTEGER,`transfer_value` REAL,`transfer_value_previous_year` REAL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `transfer_hbst_lines`(`id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`header_id` INTEGER NOT NULL,`value_id` INTEGER,`transfer_value` NUMERIC,`transfer_value_previous_year` NUMERIC,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gcd`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `financial_years`(`id` INTEGER NOT NULL,`company_id` INTEGER NOT NULL,`fyear` INTEGER,`is_enabled` INTEGER,`fiscal_year_begin` TEXT,`fiscal_year_end` TEXT,`bal_sheet_closing_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `document_rights`(`id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`user_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `balance_lists`(`id` INTEGER NOT NULL,`imported_from_id` INTEGER,`import_date` TEXT,`source` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gaap`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    //"CREATE TABLE `hypercube_changes_equity_statement`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    //"CREATE TABLE `hypercube_ass_net`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                };

                TableInformation version119_sqlserver = new TableInformation();
                version119_sqlserver.tableCreation = new List<string> {
                    "CREATE TABLE [documents]([id] INT NOT NULL,[name] VARCHAR(256) NOT NULL,[comment] TEXT,[company_id] INT,[system_id] INT,[financial_year_id] INT,[creator_id] INT,[creation_date] DATETIME,[last_modifier_id] INT,[last_modified_date] DATETIME,[balance_list_id] INT,PRIMARY KEY([id]))",
                    //"CREATE TABLE [hypercube_ass_gross_short]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_lines]([id] INT NOT NULL,[template_id] INT,[account_number] VARCHAR(32),[account_name] TEXT,[debit_element_id] TEXT,[credit_element_id] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [users]([id] INT NOT NULL,[username] VARCHAR(64),[fullname] VARCHAR(256),[password] TEXT NOT NULL,[key] TEXT NOT NULL,[is_active] TINYINT,[is_admin] TINYINT,PRIMARY KEY([id]),UNIQUE ([username]))",
                    "CREATE TABLE [transfer_hbst_heads]([id] INT NOT NULL,[name] VARCHAR(64),[document_id] INT NOT NULL,[type] INT NOT NULL,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [info]([id] INT NOT NULL,[key] VARCHAR(64) NOT NULL,[value] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([key]))",
                    "CREATE TABLE [values_gcd_company]([parent_id] INT,[company_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [companies]([Id] INT NOT NULL,[name] VARCHAR(256),PRIMARY KEY([Id]))",
                    "CREATE TABLE [assignment_template_heads]([id] INT NOT NULL,[name] VARCHAR(64),[account_structure] VARCHAR(128),[comment] TEXT,[creator_id] INT,[last_modifier_id] INT,[creation_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [systems]([id] INT NOT NULL,[name] VARCHAR(64),[comment] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([name]))",
                    "CREATE TABLE [accounts]([id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[balance] decimal(20,2) NOT NULL,[assigned_element_id] TEXT,[dc_flag] VARCHAR(1),PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_element_info]([id] BIGINT NOT NULL,[template_id] INT,[element_id] TEXT,[auto_compute_enabled] TINYINT,[supress_warning_messages] TINYINT NOT NULL,[send_account_balances] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [transfer_hbst_lines]([id] BIGINT NOT NULL,[document_id] INT NOT NULL,[header_id] INT NOT NULL,[value_id] BIGINT,[transfer_value] decimal(20,2),[transfer_value_previous_year] decimal(20,2),PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gcd]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [financial_years]([id] INT NOT NULL,[company_id] INT NOT NULL,[fyear] INT,[is_enabled] TINYINT,[fiscal_year_begin] DATETIME,[fiscal_year_end] DATETIME,[bal_sheet_closing_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [document_rights]([id] BIGINT NOT NULL,[document_id] INT NOT NULL,[user_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_gross]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [balance_lists]([id] INT NOT NULL,[imported_from_id] INT,[import_date] DATETIME,[source] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gaap]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    //"CREATE TABLE [hypercube_changes_equity_statement]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    //"CREATE TABLE [hypercube_ass_net]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                };

                //version119_sqlite.indexInfo = version119_mysql.indexInfo;
                //version119_sqlserver.indexInfo = version119_mysql.indexInfo;

                //Dictionary<string, TableInformation> tableInformationDict = new Dictionary<string, TableInformation>();
                //version119_mysql.tableNames = ExtractTableNames("MySQL", version119_mysql.tableCreation);
                //version119_sqlite.tableNames = ExtractTableNames("SQLite", version119_sqlite.tableCreation);
                //version119_sqlserver.tableNames = ExtractTableNames("SQLServer", version119_sqlserver.tableCreation);
                //tableInformationDict["MySQL"] = version119_mysql;
                //tableInformationDict["SQLite"] = version119_sqlite;
                //tableInformationDict["SQLServer"] = version119_sqlserver;
                //versionToTableInformation.Add("1.1.9", tableInformationDict);
                var version119_oracle = new TableInformation();
                AddVersion("1.1.9", version119_mysql, version119_sqlite, version119_sqlserver,version119_oracle);
                #endregion
            }

            {
                #region version 120

                #region indices
                List<IndexInformation> indexInfo = new List<IndexInformation>();
                IndexInformation info = new IndexInformation();
                info.tableName = "documents";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_gross_short";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_gross_short_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_assignment_template_lines_template_id",
                                                                        new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "users";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "taxonomy_ids";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd_company";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "companies";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "splitted_accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_splitted_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_account_groups_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "systems";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "financial_years";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_financial_years_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "document_rights";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_gross";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_gross_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_element_info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_assignment_template_element_info_template_id",
                                                           new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gaap";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "balance_lists";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_balance_lists_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_changes_equity_statement";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_hypercube_changes_equity_statement_document_id",
                                                           new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_net";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_net_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                #endregion

                //Changed tables: accounts, account_groups, balance_lists, documents, splitted_accounts, taxonomy_ids
                TableInformation version120_mysql = new TableInformation();
                version120_mysql.indexInfo = indexInfo;

                version120_mysql.tableCreation = new List<string> {
                    "CREATE TABLE `documents`(`id` INT NOT NULL,`name` VARCHAR(256) NOT NULL,`comment` TEXT,`company_id` INT,`system_id` INT,`financial_year_id` INT,`creator_id` INT,`creation_date` DATETIME,`last_modifier_id` INT,`last_modified_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_gross_short`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_lines`(`id` INT NOT NULL,`template_id` INT,`account_number` VARCHAR(32),`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `users`(`id` INT NOT NULL,`username` VARCHAR(64),`fullname` VARCHAR(256),`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` TINYINT(1),`is_admin` TINYINT(1),PRIMARY KEY(`id`),UNIQUE KEY uk_ed2d553654294a0abebb7109bf6fab99(`username`)) ENGINE = InnoDb",
                    "CREATE TABLE `taxonomy_ids`(`id` INT NOT NULL,`taxonomy_id` TEXT NOT NULL,`number` VARCHAR(10) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`document_id` INT NOT NULL,`type` INT NOT NULL,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `info`(`id` INT NOT NULL,`key` VARCHAR(64) NOT NULL,`value` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_f9d310e530bf44099d92572d3401f770(`key`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd_company`(`parent_id` BIGINT,`company_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `companies`(`Id` INT NOT NULL,`name` VARCHAR(256),PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `accounts`(`amount` decimal(20,2) NOT NULL,`group_id` BIGINT,`dc_flag` VARCHAR(1),`additional_info` TEXT,`id` BIGINT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`assigned_element_id` INT,`balance_list_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `splitted_accounts`(`amount` decimal(20,2) NOT NULL,`amount_percent` decimal(20,2) NOT NULL,`id` BIGINT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`assigned_element_id` INT,`balance_list_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `account_groups`(`id` BIGINT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`assigned_element_id` INT,`balance_list_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`account_structure` VARCHAR(128),`comment` TEXT,`creator_id` INT,`last_modifier_id` INT,`creation_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `systems`(`id` INT NOT NULL,`name` VARCHAR(64),`comment` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_5aad42d1496d42ffb90729be8815f1f0(`name`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_lines`(`id` BIGINT NOT NULL,`document_id` INT NOT NULL,`header_id` INT NOT NULL,`value_id` BIGINT,`transfer_value` decimal(20,2),`transfer_value_previous_year` decimal(20,2),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `financial_years`(`id` INT NOT NULL,`company_id` INT NOT NULL,`fyear` INT,`is_enabled` TINYINT(1),`fiscal_year_begin` DATETIME,`fiscal_year_end` DATETIME,`bal_sheet_closing_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `document_rights`(`id` BIGINT NOT NULL,`document_id` INT NOT NULL,`user_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_element_info`(`id` BIGINT NOT NULL,`template_id` INT,`element_id` TEXT,`auto_compute_enabled` TINYINT(1),`supress_warning_messages` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gaap`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `balance_lists`(`document_id` INT,`id` INT NOT NULL,`comment` TEXT,`name` VARCHAR(40),`imported_from_id` INT,`import_date` DATETIME,`source` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_changes_equity_statement`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_net`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                };

                TableInformation version120_sqlite = new TableInformation();
                version120_sqlite.tableCreation = new List<string> {
                    "CREATE TABLE `documents`(`id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`company_id` INTEGER,`system_id` INTEGER,`financial_year_id` INTEGER,`creator_id` INTEGER,`creation_date` TEXT,`last_modifier_id` INTEGER,`last_modified_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_gross_short`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_lines`(`id` INTEGER NOT NULL,`template_id` INTEGER,`account_number` TEXT,`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `users`(`id` INTEGER NOT NULL,`username` TEXT,`fullname` TEXT,`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` INTEGER,`is_admin` INTEGER,PRIMARY KEY(`id`),UNIQUE (`username`))",
                    "CREATE TABLE `taxonomy_ids`(`id` INTEGER NOT NULL,`taxonomy_id` TEXT NOT NULL,`number` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INTEGER NOT NULL,`name` TEXT,`document_id` INTEGER NOT NULL,`type` INTEGER NOT NULL,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `info`(`id` INTEGER NOT NULL,`key` TEXT NOT NULL,`value` TEXT,PRIMARY KEY(`id`),UNIQUE (`key`))",
                    "CREATE TABLE `values_gcd_company`(`parent_id` INTEGER,`company_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `companies`(`Id` INTEGER NOT NULL,`name` TEXT,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `accounts`(`amount` NUMERIC NOT NULL,`group_id` INTEGER,`dc_flag` TEXT,`additional_info` TEXT,`id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`assigned_element_id` INTEGER,`balance_list_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `splitted_accounts`(`amount` NUMERIC NOT NULL,`amount_percent` NUMERIC NOT NULL,`id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`assigned_element_id` INTEGER,`balance_list_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `account_groups`(`id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`assigned_element_id` INTEGER,`balance_list_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_heads`(`id` INTEGER NOT NULL,`name` TEXT,`account_structure` TEXT,`comment` TEXT,`creator_id` INTEGER,`last_modifier_id` INTEGER,`creation_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `systems`(`id` INTEGER NOT NULL,`name` TEXT,`comment` TEXT,PRIMARY KEY(`id`),UNIQUE (`name`))",
                    "CREATE TABLE `transfer_hbst_lines`(`id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`header_id` INTEGER NOT NULL,`value_id` INTEGER,`transfer_value` NUMERIC,`transfer_value_previous_year` NUMERIC,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gcd`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `financial_years`(`id` INTEGER NOT NULL,`company_id` INTEGER NOT NULL,`fyear` INTEGER,`is_enabled` INTEGER,`fiscal_year_begin` TEXT,`fiscal_year_end` TEXT,`bal_sheet_closing_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `document_rights`(`id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`user_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_element_info`(`id` INTEGER NOT NULL,`template_id` INTEGER,`element_id` TEXT,`auto_compute_enabled` INTEGER,`supress_warning_messages` INTEGER NOT NULL,`send_account_balances` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gaap`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `balance_lists`(`document_id` INTEGER,`id` INTEGER NOT NULL,`comment` TEXT,`name` TEXT,`imported_from_id` INTEGER,`import_date` TEXT,`source` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_changes_equity_statement`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_net`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                };

                TableInformation version120_sqlserver = new TableInformation();
                version120_sqlserver.tableCreation = new List<string> {
                    "CREATE TABLE [documents]([id] INT NOT NULL,[name] VARCHAR(256) NOT NULL,[comment] TEXT,[company_id] INT,[system_id] INT,[financial_year_id] INT,[creator_id] INT,[creation_date] DATETIME,[last_modifier_id] INT,[last_modified_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_gross_short]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_lines]([id] INT NOT NULL,[template_id] INT,[account_number] VARCHAR(32),[account_name] TEXT,[debit_element_id] TEXT,[credit_element_id] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [users]([id] INT NOT NULL,[username] VARCHAR(64),[fullname] VARCHAR(256),[password] TEXT NOT NULL,[key] TEXT NOT NULL,[is_active] TINYINT,[is_admin] TINYINT,PRIMARY KEY([id]),UNIQUE ([username]))",
                    "CREATE TABLE [taxonomy_ids]([id] INT NOT NULL,[taxonomy_id] TEXT NOT NULL,[number] VARCHAR(10) NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [transfer_hbst_heads]([id] INT NOT NULL,[name] VARCHAR(64),[document_id] INT NOT NULL,[type] INT NOT NULL,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [info]([id] INT NOT NULL,[key] VARCHAR(64) NOT NULL,[value] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([key]))",
                    "CREATE TABLE [values_gcd_company]([parent_id] INT,[company_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [companies]([Id] INT NOT NULL,[name] VARCHAR(256),PRIMARY KEY([Id]))",
                    "CREATE TABLE [accounts]([amount] decimal(20,2) NOT NULL,[group_id] BIGINT,[dc_flag] VARCHAR(1),[additional_info] TEXT,[id] BIGINT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[assigned_element_id] INT,[balance_list_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [splitted_accounts]([amount] decimal(20,2) NOT NULL,[amount_percent] decimal(20,2) NOT NULL,[id] BIGINT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[assigned_element_id] INT,[balance_list_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [account_groups]([id] BIGINT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[assigned_element_id] INT,[balance_list_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_heads]([id] INT NOT NULL,[name] VARCHAR(64),[account_structure] VARCHAR(128),[comment] TEXT,[creator_id] INT,[last_modifier_id] INT,[creation_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [systems]([id] INT NOT NULL,[name] VARCHAR(64),[comment] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([name]))",
                    "CREATE TABLE [transfer_hbst_lines]([id] BIGINT NOT NULL,[document_id] INT NOT NULL,[header_id] INT NOT NULL,[value_id] BIGINT,[transfer_value] decimal(20,2),[transfer_value_previous_year] decimal(20,2),PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gcd]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [financial_years]([id] INT NOT NULL,[company_id] INT NOT NULL,[fyear] INT,[is_enabled] TINYINT,[fiscal_year_begin] DATETIME,[fiscal_year_end] DATETIME,[bal_sheet_closing_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [document_rights]([id] BIGINT NOT NULL,[document_id] INT NOT NULL,[user_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_gross]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_element_info]([id] BIGINT NOT NULL,[template_id] INT,[element_id] TEXT,[auto_compute_enabled] TINYINT,[supress_warning_messages] TINYINT NOT NULL,[send_account_balances] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gaap]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [balance_lists]([document_id] INT,[id] INT NOT NULL,[comment] TEXT,[name] VARCHAR(40),[imported_from_id] INT,[import_date] DATETIME,[source] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_changes_equity_statement]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_net]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                };
                var version120_oracle = new TableInformation();
                AddVersion("1.2.0", version120_mysql, version120_sqlite, version120_sqlserver,version120_oracle);
                #endregion
            }

            {
                #region version 121

                #region indices
                List<IndexInformation> indexInfo = new List<IndexInformation>();
                IndexInformation info = new IndexInformation();
                info.tableName = "documents";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_gross_short";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_gross_short_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_assignment_template_lines_template_id",
                                                                        new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "users";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "taxonomy_ids";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd_company";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "companies";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "send_log";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "splitted_accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_splitted_accounts_split_group_id",
                                                                        new List<string> {"split_group_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_splitted_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_account_groups_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "split_account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_split_account_groups_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_split_account_groups_account_id",
                                                                        new List<string> {"account_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "systems";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "financial_years";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_financial_years_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "document_rights";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_gross";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_gross_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_element_info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_assignment_template_element_info_template_id",
                                                           new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gaap";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "balance_lists";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_balance_lists_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_changes_equity_statement";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_hypercube_changes_equity_statement_document_id",
                                                           new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_net";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_net_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);
                #endregion

                //Changed tables: accounts, account_groups, balance_lists, documents, splitted_accounts, taxonomy_ids
                TableInformation version121_mysql = new TableInformation();
                version121_mysql.indexInfo = indexInfo;

                version121_mysql.tableCreation = new List<string> {
                    "CREATE TABLE `documents`(`id` INT NOT NULL,`name` VARCHAR(256) NOT NULL,`comment` TEXT,`company_id` INT,`system_id` INT,`financial_year_id` INT,`creator_id` INT,`creation_date` DATETIME,`last_modifier_id` INT,`last_modified_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_gross_short`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_lines`(`id` INT NOT NULL,`template_id` INT,`account_number` VARCHAR(32),`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `users`(`id` INT NOT NULL,`username` VARCHAR(64),`fullname` VARCHAR(256),`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` TINYINT(1),`is_admin` TINYINT(1),PRIMARY KEY(`id`),UNIQUE KEY uk_343a6d5b78bc4a918b6a2a6b5d3efd7a(`username`)) ENGINE = InnoDb",
                    "CREATE TABLE `taxonomy_ids`(`id` INT NOT NULL,`taxonomy_id` TEXT NOT NULL,`number` VARCHAR(10) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`document_id` INT NOT NULL,`type` INT NOT NULL,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `info`(`id` INT NOT NULL,`key` VARCHAR(64) NOT NULL,`value` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_4a033e10dec242a7b8f8a0bff13b62ec(`key`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd_company`(`parent_id` BIGINT,`company_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `companies`(`Id` INT NOT NULL,`name` VARCHAR(256),PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `accounts`(`amount` decimal(20,2) NOT NULL,`id` BIGINT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`assigned_element_id` INT,`comment` TEXT,`balance_list_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `send_log`(`id` INT NOT NULL,`send_data` LONGTEXT NOT NULL,`report_info` LONGTEXT NOT NULL,`user_id` INT NOT NULL,`send_error` INT NOT NULL,`result_message` TEXT NOT NULL,`timestamp` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `splitted_accounts`(`amount` decimal(20,2) NOT NULL,`amount_percent` decimal(20,2),`split_group_id` BIGINT,`id` BIGINT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`assigned_element_id` INT,`comment` TEXT,`balance_list_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `account_groups`(`id` BIGINT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`assigned_element_id` INT,`comment` TEXT,`balance_list_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`account_structure` VARCHAR(128),`comment` TEXT,`creator_id` INT,`last_modifier_id` INT,`creation_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `split_account_groups`(`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`account_id` BIGINT NOT NULL,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `systems`(`id` INT NOT NULL,`name` VARCHAR(64),`comment` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_3650dfee9b33450a9f1dbad320591a08(`name`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_lines`(`id` BIGINT NOT NULL,`document_id` INT NOT NULL,`header_id` INT NOT NULL,`value_id` BIGINT,`transfer_value` decimal(20,2),`transfer_value_previous_year` decimal(20,2),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `financial_years`(`id` INT NOT NULL,`company_id` INT NOT NULL,`fyear` INT,`is_enabled` TINYINT(1),`fiscal_year_begin` DATETIME,`fiscal_year_end` DATETIME,`bal_sheet_closing_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `document_rights`(`id` BIGINT NOT NULL,`document_id` INT NOT NULL,`user_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_element_info`(`id` BIGINT NOT NULL,`template_id` INT,`element_id` TEXT,`auto_compute_enabled` TINYINT(1),`supress_warning_messages` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gaap`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `balance_lists`(`document_id` INT,`id` INT NOT NULL,`comment` TEXT,`name` VARCHAR(40),`imported_from_id` INT,`import_date` DATETIME,`source` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_changes_equity_statement`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_net`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                };

                TableInformation version121_sqlite = new TableInformation();
                version121_sqlite.tableCreation = new List<string> {
                    "CREATE TABLE `documents`(`id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`company_id` INTEGER,`system_id` INTEGER,`financial_year_id` INTEGER,`creator_id` INTEGER,`creation_date` TEXT,`last_modifier_id` INTEGER,`last_modified_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_gross_short`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_lines`(`id` INTEGER NOT NULL,`template_id` INTEGER,`account_number` TEXT,`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `users`(`id` INTEGER NOT NULL,`username` TEXT,`fullname` TEXT,`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` INTEGER,`is_admin` INTEGER,PRIMARY KEY(`id`),UNIQUE (`username`))",
                    "CREATE TABLE `taxonomy_ids`(`id` INTEGER NOT NULL,`taxonomy_id` TEXT NOT NULL,`number` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INTEGER NOT NULL,`name` TEXT,`document_id` INTEGER NOT NULL,`type` INTEGER NOT NULL,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `info`(`id` INTEGER NOT NULL,`key` TEXT NOT NULL,`value` TEXT,PRIMARY KEY(`id`),UNIQUE (`key`))",
                    "CREATE TABLE `values_gcd_company`(`parent_id` INTEGER,`company_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `companies`(`Id` INTEGER NOT NULL,`name` TEXT,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `accounts`(`amount` NUMERIC NOT NULL,`id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,`balance_list_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `send_log`(`id` INTEGER NOT NULL,`send_data` TEXT NOT NULL,`report_info` TEXT NOT NULL,`user_id` INTEGER NOT NULL,`send_error` INTEGER NOT NULL,`result_message` TEXT NOT NULL,`timestamp` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `splitted_accounts`(`amount` NUMERIC NOT NULL,`amount_percent` NUMERIC,`split_group_id` INTEGER,`id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,`balance_list_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `account_groups`(`id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,`balance_list_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_heads`(`id` INTEGER NOT NULL,`name` TEXT,`account_structure` TEXT,`comment` TEXT,`creator_id` INTEGER,`last_modifier_id` INTEGER,`creation_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `split_account_groups`(`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`account_id` INTEGER NOT NULL,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `systems`(`id` INTEGER NOT NULL,`name` TEXT,`comment` TEXT,PRIMARY KEY(`id`),UNIQUE (`name`))",
                    "CREATE TABLE `transfer_hbst_lines`(`id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`header_id` INTEGER NOT NULL,`value_id` INTEGER,`transfer_value` NUMERIC,`transfer_value_previous_year` NUMERIC,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gcd`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `financial_years`(`id` INTEGER NOT NULL,`company_id` INTEGER NOT NULL,`fyear` INTEGER,`is_enabled` INTEGER,`fiscal_year_begin` TEXT,`fiscal_year_end` TEXT,`bal_sheet_closing_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `document_rights`(`id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`user_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_element_info`(`id` INTEGER NOT NULL,`template_id` INTEGER,`element_id` TEXT,`auto_compute_enabled` INTEGER,`supress_warning_messages` INTEGER NOT NULL,`send_account_balances` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gaap`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `balance_lists`(`document_id` INTEGER,`id` INTEGER NOT NULL,`comment` TEXT,`name` TEXT,`imported_from_id` INTEGER,`import_date` TEXT,`source` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_changes_equity_statement`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_net`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                };

                TableInformation version121_sqlserver = new TableInformation();
                version121_sqlserver.tableCreation = new List<string> {
                    "CREATE TABLE [documents]([id] INT NOT NULL,[name] VARCHAR(256) NOT NULL,[comment] TEXT,[company_id] INT,[system_id] INT,[financial_year_id] INT,[creator_id] INT,[creation_date] DATETIME,[last_modifier_id] INT,[last_modified_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_gross_short]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_lines]([id] INT NOT NULL,[template_id] INT,[account_number] VARCHAR(32),[account_name] TEXT,[debit_element_id] TEXT,[credit_element_id] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [users]([id] INT NOT NULL,[username] VARCHAR(64),[fullname] VARCHAR(256),[password] TEXT NOT NULL,[key] TEXT NOT NULL,[is_active] TINYINT,[is_admin] TINYINT,PRIMARY KEY([id]),UNIQUE ([username]))",
                    "CREATE TABLE [taxonomy_ids]([id] INT NOT NULL,[taxonomy_id] TEXT NOT NULL,[number] VARCHAR(10) NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [transfer_hbst_heads]([id] INT NOT NULL,[name] VARCHAR(64),[document_id] INT NOT NULL,[type] INT NOT NULL,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [info]([id] INT NOT NULL,[key] VARCHAR(64) NOT NULL,[value] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([key]))",
                    "CREATE TABLE [values_gcd_company]([parent_id] INT,[company_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [companies]([Id] INT NOT NULL,[name] VARCHAR(256),PRIMARY KEY([Id]))",
                    "CREATE TABLE [accounts]([amount] decimal(20,2) NOT NULL,[id] BIGINT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[assigned_element_id] INT,[comment] TEXT,[balance_list_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [send_log]([id] INT NOT NULL,[send_data] TEXT NOT NULL,[report_info] TEXT NOT NULL,[user_id] INT NOT NULL,[send_error] INT NOT NULL,[result_message] TEXT NOT NULL,[timestamp] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [splitted_accounts]([amount] decimal(20,2) NOT NULL,[amount_percent] decimal(20,2),[split_group_id] INT,[id] BIGINT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[assigned_element_id] INT,[comment] TEXT,[balance_list_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [account_groups]([id] BIGINT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[assigned_element_id] INT,[comment] TEXT,[balance_list_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_heads]([id] INT NOT NULL,[name] VARCHAR(64),[account_structure] VARCHAR(128),[comment] TEXT,[creator_id] INT,[last_modifier_id] INT,[creation_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [split_account_groups]([id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[account_id] INT NOT NULL,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [systems]([id] INT NOT NULL,[name] VARCHAR(64),[comment] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([name]))",
                    "CREATE TABLE [transfer_hbst_lines]([id] BIGINT NOT NULL,[document_id] INT NOT NULL,[header_id] INT NOT NULL,[value_id] BIGINT,[transfer_value] decimal(20,2),[transfer_value_previous_year] decimal(20,2),PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gcd]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [financial_years]([id] INT NOT NULL,[company_id] INT NOT NULL,[fyear] INT,[is_enabled] TINYINT,[fiscal_year_begin] DATETIME,[fiscal_year_end] DATETIME,[bal_sheet_closing_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [document_rights]([id] BIGINT NOT NULL,[document_id] INT NOT NULL,[user_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_gross]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_element_info]([id] BIGINT NOT NULL,[template_id] INT,[element_id] TEXT,[auto_compute_enabled] TINYINT,[supress_warning_messages] TINYINT NOT NULL,[send_account_balances] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gaap]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [balance_lists]([document_id] INT,[id] INT NOT NULL,[comment] TEXT,[name] VARCHAR(40),[imported_from_id] INT,[import_date] DATETIME,[source] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_changes_equity_statement]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_net]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                };
                var version121_oracle = new TableInformation();
                AddVersion("1.2.1", version121_mysql, version121_sqlite, version121_sqlserver, version121_oracle);
                #endregion
            }

            {
                #region version 130
                //Created tables: log_admin, log_main, log_send, log_value_change, split_account_groups, account_groups, split_accounts
                TableInformation version130_mysql = new TableInformation();

                #region indices
                List<IndexInformation> indexInfo = new List<IndexInformation>();
                IndexInformation info = new IndexInformation();
                info.tableName = "documents";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_gross_short";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_gross_short_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_assignment_template_lines_template_id",
                                                                        new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "users";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "taxonomy_ids";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd_company";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "companies";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "split_account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_split_account_groups_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "splitted_accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_splitted_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "systems";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_account_groups_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "balance_lists";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_balance_lists_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_admin";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_send";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_report_1_value_change";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_report_1";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "financial_years";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_financial_years_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "document_rights";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_gross";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_gross_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_element_info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_assignment_template_element_info_template_id",
                                                           new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gaap";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_changes_equity_statement";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_hypercube_changes_equity_statement_document_id",
                                                           new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_net";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_net_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                #endregion

                version130_mysql.indexInfo = indexInfo;
                version130_mysql.tableCreation = new List<string> {
                    "CREATE TABLE `documents`(`id` INT NOT NULL,`name` VARCHAR(256) NOT NULL,`comment` TEXT,`company_id` INT,`system_id` INT,`financial_year_id` INT,`creator_id` INT,`creation_date` DATETIME,`last_modifier_id` INT,`last_modified_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_gross_short`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_lines`(`id` INT NOT NULL,`template_id` INT,`account_number` VARCHAR(32),`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `users`(`id` INT NOT NULL,`username` VARCHAR(64),`fullname` VARCHAR(256),`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` TINYINT(1),`is_admin` TINYINT(1),`is_deleted` TINYINT(1),PRIMARY KEY(`id`),UNIQUE KEY uk_6fa77da5b2bb4603a8d3e4cf2003b78b(`username`)) ENGINE = InnoDb",
                    "CREATE TABLE `taxonomy_ids`(`id` INT NOT NULL,`taxonomy_id` TEXT NOT NULL,`number` VARCHAR(10) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`document_id` INT NOT NULL,`type` INT NOT NULL,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `info`(`id` INT NOT NULL,`key` VARCHAR(64) NOT NULL,`value` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_a9ebc2d77c044163994cd7939171ee8c(`key`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd_company`(`parent_id` BIGINT,`company_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `companies`(`Id` INT NOT NULL,`name` VARCHAR(256),PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `accounts`(`amount` decimal(20,2) NOT NULL,`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`assigned_element_id` INT,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `split_account_groups`(`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`account_id` BIGINT NOT NULL,`comment` TEXT,`value_input_mode` INT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `splitted_accounts`(`amount` decimal(20,2) NOT NULL,`amount_percent` decimal(20,2),`split_group_id` BIGINT,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`assigned_element_id` INT,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `systems`(`id` INT NOT NULL,`name` VARCHAR(64),`comment` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_bde83911dc074bfa82341aba250f4e40(`name`)) ENGINE = InnoDb",
                    "CREATE TABLE `account_groups`(`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`assigned_element_id` INT,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`account_structure` VARCHAR(128),`comment` TEXT,`creator_id` INT,`last_modifier_id` INT,`creation_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `balance_lists`(`id` INT NOT NULL,`document_id` INT,`comment` TEXT,`name` VARCHAR(40),`imported_from_id` INT,`import_date` DATETIME,`source` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_admin`(`id` BIGINT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`content_type` INT NOT NULL,`action_type` INT NOT NULL,`reference_id` BIGINT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`info` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_send`(`id` INT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`document_id` BIGINT NOT NULL,`send_data` LONGTEXT NOT NULL,`report_info` LONGTEXT NOT NULL,`send_error` INT NOT NULL,`result_message` TEXT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_report_1_value_change`(`id` BIGINT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`taxonomy_id` INT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`reference_id` BIGINT NOT NULL,`value_type` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_report_1`(`id` BIGINT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`content_type` INT NOT NULL,`action_type` INT NOT NULL,`reference_id` BIGINT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`info` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_lines`(`id` BIGINT NOT NULL,`document_id` INT NOT NULL,`header_id` INT NOT NULL,`value_id` BIGINT,`transfer_value` decimal(20,2),`transfer_value_previous_year` decimal(20,2),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `financial_years`(`id` INT NOT NULL,`company_id` INT NOT NULL,`fyear` INT,`is_enabled` TINYINT(1),`fiscal_year_begin` DATETIME,`fiscal_year_end` DATETIME,`bal_sheet_closing_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `document_rights`(`id` BIGINT NOT NULL,`document_id` INT NOT NULL,`user_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_element_info`(`id` BIGINT NOT NULL,`template_id` INT,`element_id` TEXT,`auto_compute_enabled` TINYINT(1),`supress_warning_messages` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gaap`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_changes_equity_statement`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_net`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                };
                TableInformation version130_sqlserver = new TableInformation();
                version130_sqlserver.tableCreation = new List<string> {
                    "CREATE TABLE [documents]([id] INT NOT NULL,[name] VARCHAR(256) NOT NULL,[comment] TEXT,[company_id] INT,[system_id] INT,[financial_year_id] INT,[creator_id] INT,[creation_date] DATETIME,[last_modifier_id] INT,[last_modified_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_gross_short]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_lines]([id] INT NOT NULL,[template_id] INT,[account_number] VARCHAR(32),[account_name] TEXT,[debit_element_id] TEXT,[credit_element_id] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [users]([id] INT NOT NULL,[username] VARCHAR(64),[fullname] VARCHAR(256),[password] TEXT NOT NULL,[key] TEXT NOT NULL,[is_active] TINYINT,[is_admin] TINYINT,[is_deleted] TINYINT,PRIMARY KEY([id]),UNIQUE ([username]))",
                    "CREATE TABLE [taxonomy_ids]([id] INT NOT NULL,[taxonomy_id] TEXT NOT NULL,[number] VARCHAR(10) NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [transfer_hbst_heads]([id] INT NOT NULL,[name] VARCHAR(64),[document_id] INT NOT NULL,[type] INT NOT NULL,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [info]([id] INT NOT NULL,[key] VARCHAR(64) NOT NULL,[value] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([key]))",
                    "CREATE TABLE [values_gcd_company]([parent_id] INT,[company_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [companies]([Id] INT NOT NULL,[name] VARCHAR(256),PRIMARY KEY([Id]))",
                    "CREATE TABLE [accounts]([amount] decimal(20,2) NOT NULL,[id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[assigned_element_id] INT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [split_account_groups]([id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[account_id] BIGINT NOT NULL,[comment] TEXT,[value_input_mode] INT,PRIMARY KEY([id]))",
                    "CREATE TABLE [splitted_accounts]([amount] decimal(20,2) NOT NULL,[amount_percent] decimal(20,2),[split_group_id] BIGINT,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[assigned_element_id] INT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [systems]([id] INT NOT NULL,[name] VARCHAR(64),[comment] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([name]))",
                    "CREATE TABLE [account_groups]([id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[assigned_element_id] INT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_heads]([id] INT NOT NULL,[name] VARCHAR(64),[account_structure] VARCHAR(128),[comment] TEXT,[creator_id] INT,[last_modifier_id] INT,[creation_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [balance_lists]([id] INT NOT NULL,[document_id] INT,[comment] TEXT,[name] VARCHAR(40),[imported_from_id] INT,[import_date] DATETIME,[source] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_admin]([id] BIGINT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[content_type] INT NOT NULL,[action_type] INT NOT NULL,[reference_id] BIGINT NOT NULL,[old_value] TEXT,[new_value] TEXT,[info] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_send]([id] INT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[document_id] BIGINT NOT NULL,[send_data] TEXT NOT NULL,[report_info] TEXT NOT NULL,[send_error] INT NOT NULL,[result_message] TEXT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_report_1_value_change]([id] BIGINT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[taxonomy_id] INT NOT NULL,[old_value] TEXT,[new_value] TEXT,[reference_id] BIGINT NOT NULL,[value_type] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_report_1]([id] BIGINT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[content_type] INT NOT NULL,[action_type] INT NOT NULL,[reference_id] BIGINT NOT NULL,[old_value] TEXT,[new_value] TEXT,[info] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [transfer_hbst_lines]([id] BIGINT NOT NULL,[document_id] INT NOT NULL,[header_id] INT NOT NULL,[value_id] BIGINT,[transfer_value] decimal(20,2),[transfer_value_previous_year] decimal(20,2),PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gcd]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [financial_years]([id] INT NOT NULL,[company_id] INT NOT NULL,[fyear] INT,[is_enabled] TINYINT,[fiscal_year_begin] DATETIME,[fiscal_year_end] DATETIME,[bal_sheet_closing_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [document_rights]([id] BIGINT NOT NULL,[document_id] INT NOT NULL,[user_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_gross]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_element_info]([id] BIGINT NOT NULL,[template_id] INT,[element_id] TEXT,[auto_compute_enabled] TINYINT,[supress_warning_messages] TINYINT NOT NULL,[send_account_balances] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gaap]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_changes_equity_statement]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_net]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                };
                TableInformation version130_sqlite = new TableInformation();
                version130_sqlite.tableCreation = new List<string> {
                    "CREATE TABLE `documents`(`id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`company_id` INTEGER,`system_id` INTEGER,`financial_year_id` INTEGER,`creator_id` INTEGER,`creation_date` TEXT,`last_modifier_id` INTEGER,`last_modified_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_gross_short`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_lines`(`id` INTEGER NOT NULL,`template_id` INTEGER,`account_number` TEXT,`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `users`(`id` INTEGER NOT NULL,`username` TEXT,`fullname` TEXT,`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` INTEGER,`is_admin` INTEGER,`is_deleted` INTEGER,PRIMARY KEY(`id`),UNIQUE (`username`))",
                    "CREATE TABLE `taxonomy_ids`(`id` INTEGER NOT NULL,`taxonomy_id` TEXT NOT NULL,`number` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INTEGER NOT NULL,`name` TEXT,`document_id` INTEGER NOT NULL,`type` INTEGER NOT NULL,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `info`(`id` INTEGER NOT NULL,`key` TEXT NOT NULL,`value` TEXT,PRIMARY KEY(`id`),UNIQUE (`key`))",
                    "CREATE TABLE `values_gcd_company`(`parent_id` INTEGER,`company_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `companies`(`Id` INTEGER NOT NULL,`name` TEXT,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `accounts`(`amount` NUMERIC NOT NULL,`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `split_account_groups`(`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`account_id` INTEGER NOT NULL,`comment` TEXT,`value_input_mode` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `splitted_accounts`(`amount` NUMERIC NOT NULL,`amount_percent` NUMERIC,`split_group_id` INTEGER,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `systems`(`id` INTEGER NOT NULL,`name` TEXT,`comment` TEXT,PRIMARY KEY(`id`),UNIQUE (`name`))",
                    "CREATE TABLE `account_groups`(`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_heads`(`id` INTEGER NOT NULL,`name` TEXT,`account_structure` TEXT,`comment` TEXT,`creator_id` INTEGER,`last_modifier_id` INTEGER,`creation_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `balance_lists`(`id` INTEGER NOT NULL,`document_id` INTEGER,`comment` TEXT,`name` TEXT,`imported_from_id` INTEGER,`import_date` TEXT,`source` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_admin`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`content_type` INTEGER NOT NULL,`action_type` INTEGER NOT NULL,`reference_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`info` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_send`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`send_data` TEXT NOT NULL,`report_info` TEXT NOT NULL,`send_error` INTEGER NOT NULL,`result_message` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_report_1_value_change`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`taxonomy_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`reference_id` INTEGER NOT NULL,`value_type` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_report_1`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`content_type` INTEGER NOT NULL,`action_type` INTEGER NOT NULL,`reference_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`info` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `transfer_hbst_lines`(`id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`header_id` INTEGER NOT NULL,`value_id` INTEGER,`transfer_value` NUMERIC,`transfer_value_previous_year` NUMERIC,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gcd`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `financial_years`(`id` INTEGER NOT NULL,`company_id` INTEGER NOT NULL,`fyear` INTEGER,`is_enabled` INTEGER,`fiscal_year_begin` TEXT,`fiscal_year_end` TEXT,`bal_sheet_closing_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `document_rights`(`id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`user_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_element_info`(`id` INTEGER NOT NULL,`template_id` INTEGER,`element_id` TEXT,`auto_compute_enabled` INTEGER,`supress_warning_messages` INTEGER NOT NULL,`send_account_balances` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gaap`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_changes_equity_statement`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_net`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                };
                var version130_oracle = new TableInformation();
                AddVersion("1.3.0", version130_mysql, version130_sqlite, version130_sqlserver, version130_oracle);
                #endregion
            }

            {
                #region version 131
                //Changed table: users (deleted UNIQUE_KEY(username))
                TableInformation version131_mysql = new TableInformation();

                #region indices
                List<IndexInformation> indexInfo = new List<IndexInformation>();
                IndexInformation info = new IndexInformation();
                info.tableName = "documents";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_gross_short";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_gross_short_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_assignment_template_lines_template_id",
                                                                        new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "users";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "taxonomy_ids";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd_company";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "companies";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "split_account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_split_account_groups_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "splitted_accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_splitted_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "systems";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_account_groups_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "balance_lists";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_balance_lists_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_admin";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_send";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_report_1_value_change";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_report_1";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "financial_years";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_financial_years_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "document_rights";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_gross";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_gross_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_element_info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_assignment_template_element_info_template_id",
                                                           new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gaap";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_changes_equity_statement";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_hypercube_changes_equity_statement_document_id",
                                                           new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_net";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_net_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                #endregion

                version131_mysql.indexInfo = indexInfo;
                version131_mysql.tableCreation = new List<string> {
                    "CREATE TABLE `documents`(`id` INT NOT NULL,`name` VARCHAR(256) NOT NULL,`comment` TEXT,`company_id` INT,`system_id` INT,`financial_year_id` INT,`creator_id` INT,`creation_date` DATETIME,`last_modifier_id` INT,`last_modified_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_gross_short`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_lines`(`id` INT NOT NULL,`template_id` INT,`account_number` VARCHAR(32),`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `users`(`id` INTEGER NOT NULL,`username` TEXT,`fullname` TEXT,`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` INTEGER,`is_admin` INTEGER,`is_deleted` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `taxonomy_ids`(`id` INT NOT NULL,`taxonomy_id` TEXT NOT NULL,`number` VARCHAR(10) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`document_id` INT NOT NULL,`type` INT NOT NULL,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `info`(`id` INT NOT NULL,`key` VARCHAR(64) NOT NULL,`value` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_a9ebc2d77c044163994cd7939171ee8c(`key`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd_company`(`parent_id` BIGINT,`company_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `companies`(`Id` INT NOT NULL,`name` VARCHAR(256),PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `accounts`(`amount` decimal(20,2) NOT NULL,`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`assigned_element_id` INT,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `split_account_groups`(`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`account_id` BIGINT NOT NULL,`comment` TEXT,`value_input_mode` INT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `splitted_accounts`(`amount` decimal(20,2) NOT NULL,`amount_percent` decimal(20,2),`split_group_id` BIGINT,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`assigned_element_id` INT,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `systems`(`id` INT NOT NULL,`name` VARCHAR(64),`comment` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_bde83911dc074bfa82341aba250f4e40(`name`)) ENGINE = InnoDb",
                    "CREATE TABLE `account_groups`(`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`assigned_element_id` INT,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`account_structure` VARCHAR(128),`comment` TEXT,`creator_id` INT,`last_modifier_id` INT,`creation_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `balance_lists`(`id` INT NOT NULL,`document_id` INT,`comment` TEXT,`name` VARCHAR(40),`imported_from_id` INT,`import_date` DATETIME,`source` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_admin`(`id` BIGINT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`content_type` INT NOT NULL,`action_type` INT NOT NULL,`reference_id` BIGINT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`info` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_send`(`id` INT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`document_id` BIGINT NOT NULL,`send_data` LONGTEXT NOT NULL,`report_info` LONGTEXT NOT NULL,`send_error` INT NOT NULL,`result_message` TEXT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_report_1_value_change`(`id` BIGINT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`taxonomy_id` INT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`reference_id` BIGINT NOT NULL,`value_type` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_report_1`(`id` BIGINT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`content_type` INT NOT NULL,`action_type` INT NOT NULL,`reference_id` BIGINT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`info` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_lines`(`id` BIGINT NOT NULL,`document_id` INT NOT NULL,`header_id` INT NOT NULL,`value_id` BIGINT,`transfer_value` decimal(20,2),`transfer_value_previous_year` decimal(20,2),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `financial_years`(`id` INT NOT NULL,`company_id` INT NOT NULL,`fyear` INT,`is_enabled` TINYINT(1),`fiscal_year_begin` DATETIME,`fiscal_year_end` DATETIME,`bal_sheet_closing_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `document_rights`(`id` BIGINT NOT NULL,`document_id` INT NOT NULL,`user_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_element_info`(`id` BIGINT NOT NULL,`template_id` INT,`element_id` TEXT,`auto_compute_enabled` TINYINT(1),`supress_warning_messages` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gaap`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`is_manual_value` TINYINT(1) NOT NULL,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_changes_equity_statement`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_net`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                };
                TableInformation version131_sqlserver = new TableInformation();
                version131_sqlserver.tableCreation = new List<string> {
                    "CREATE TABLE [documents]([id] INT NOT NULL,[name] VARCHAR(256) NOT NULL,[comment] TEXT,[company_id] INT,[system_id] INT,[financial_year_id] INT,[creator_id] INT,[creation_date] DATETIME,[last_modifier_id] INT,[last_modified_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_gross_short]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_lines]([id] INT NOT NULL,[template_id] INT,[account_number] VARCHAR(32),[account_name] TEXT,[debit_element_id] TEXT,[credit_element_id] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [users]([id] INT NOT NULL,[username] VARCHAR(64),[fullname] VARCHAR(256),[password] TEXT NOT NULL,[key] TEXT NOT NULL,[is_active] TINYINT,[is_admin] TINYINT,[is_deleted] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [taxonomy_ids]([id] INT NOT NULL,[taxonomy_id] TEXT NOT NULL,[number] VARCHAR(10) NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [transfer_hbst_heads]([id] INT NOT NULL,[name] VARCHAR(64),[document_id] INT NOT NULL,[type] INT NOT NULL,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [info]([id] INT NOT NULL,[key] VARCHAR(64) NOT NULL,[value] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([key]))",
                    "CREATE TABLE [values_gcd_company]([parent_id] INT,[company_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [companies]([Id] INT NOT NULL,[name] VARCHAR(256),PRIMARY KEY([Id]))",
                    "CREATE TABLE [accounts]([amount] decimal(20,2) NOT NULL,[id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[assigned_element_id] INT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [split_account_groups]([id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[account_id] BIGINT NOT NULL,[comment] TEXT,[value_input_mode] INT,PRIMARY KEY([id]))",
                    "CREATE TABLE [splitted_accounts]([amount] decimal(20,2) NOT NULL,[amount_percent] decimal(20,2),[split_group_id] BIGINT,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[assigned_element_id] INT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [systems]([id] INT NOT NULL,[name] VARCHAR(64),[comment] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([name]))",
                    "CREATE TABLE [account_groups]([id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[assigned_element_id] INT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_heads]([id] INT NOT NULL,[name] VARCHAR(64),[account_structure] VARCHAR(128),[comment] TEXT,[creator_id] INT,[last_modifier_id] INT,[creation_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [balance_lists]([id] INT NOT NULL,[document_id] INT,[comment] TEXT,[name] VARCHAR(40),[imported_from_id] INT,[import_date] DATETIME,[source] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_admin]([id] BIGINT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[content_type] INT NOT NULL,[action_type] INT NOT NULL,[reference_id] BIGINT NOT NULL,[old_value] TEXT,[new_value] TEXT,[info] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_send]([id] INT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[document_id] BIGINT NOT NULL,[send_data] TEXT NOT NULL,[report_info] TEXT NOT NULL,[send_error] INT NOT NULL,[result_message] TEXT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_report_1_value_change]([id] BIGINT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[taxonomy_id] INT NOT NULL,[old_value] TEXT,[new_value] TEXT,[reference_id] BIGINT NOT NULL,[value_type] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_report_1]([id] BIGINT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[content_type] INT NOT NULL,[action_type] INT NOT NULL,[reference_id] BIGINT NOT NULL,[old_value] TEXT,[new_value] TEXT,[info] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [transfer_hbst_lines]([id] BIGINT NOT NULL,[document_id] INT NOT NULL,[header_id] INT NOT NULL,[value_id] BIGINT,[transfer_value] decimal(20,2),[transfer_value_previous_year] decimal(20,2),PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gcd]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [financial_years]([id] INT NOT NULL,[company_id] INT NOT NULL,[fyear] INT,[is_enabled] TINYINT,[fiscal_year_begin] DATETIME,[fiscal_year_end] DATETIME,[bal_sheet_closing_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [document_rights]([id] BIGINT NOT NULL,[document_id] INT NOT NULL,[user_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_gross]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_element_info]([id] BIGINT NOT NULL,[template_id] INT,[element_id] TEXT,[auto_compute_enabled] TINYINT,[supress_warning_messages] TINYINT NOT NULL,[send_account_balances] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gaap]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[is_manual_value] TINYINT NOT NULL,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_changes_equity_statement]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_net]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                };
                TableInformation version131_sqlite = new TableInformation();
                version131_sqlite.tableCreation = new List<string> {
                    "CREATE TABLE `documents`(`id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`company_id` INTEGER,`system_id` INTEGER,`financial_year_id` INTEGER,`creator_id` INTEGER,`creation_date` TEXT,`last_modifier_id` INTEGER,`last_modified_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_gross_short`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_lines`(`id` INTEGER NOT NULL,`template_id` INTEGER,`account_number` TEXT,`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `users`(`id` INTEGER NOT NULL,`username` TEXT,`fullname` TEXT,`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` INTEGER,`is_admin` INTEGER,`is_deleted` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `taxonomy_ids`(`id` INTEGER NOT NULL,`taxonomy_id` TEXT NOT NULL,`number` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INTEGER NOT NULL,`name` TEXT,`document_id` INTEGER NOT NULL,`type` INTEGER NOT NULL,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `info`(`id` INTEGER NOT NULL,`key` TEXT NOT NULL,`value` TEXT,PRIMARY KEY(`id`),UNIQUE (`key`))",
                    "CREATE TABLE `values_gcd_company`(`parent_id` INTEGER,`company_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `companies`(`Id` INTEGER NOT NULL,`name` TEXT,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `accounts`(`amount` NUMERIC NOT NULL,`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `split_account_groups`(`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`account_id` INTEGER NOT NULL,`comment` TEXT,`value_input_mode` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `splitted_accounts`(`amount` NUMERIC NOT NULL,`amount_percent` NUMERIC,`split_group_id` INTEGER,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `systems`(`id` INTEGER NOT NULL,`name` TEXT,`comment` TEXT,PRIMARY KEY(`id`),UNIQUE (`name`))",
                    "CREATE TABLE `account_groups`(`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_heads`(`id` INTEGER NOT NULL,`name` TEXT,`account_structure` TEXT,`comment` TEXT,`creator_id` INTEGER,`last_modifier_id` INTEGER,`creation_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `balance_lists`(`id` INTEGER NOT NULL,`document_id` INTEGER,`comment` TEXT,`name` TEXT,`imported_from_id` INTEGER,`import_date` TEXT,`source` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_admin`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`content_type` INTEGER NOT NULL,`action_type` INTEGER NOT NULL,`reference_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`info` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_send`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`send_data` TEXT NOT NULL,`report_info` TEXT NOT NULL,`send_error` INTEGER NOT NULL,`result_message` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_report_1_value_change`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`taxonomy_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`reference_id` INTEGER NOT NULL,`value_type` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_report_1`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`content_type` INTEGER NOT NULL,`action_type` INTEGER NOT NULL,`reference_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`info` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `transfer_hbst_lines`(`id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`header_id` INTEGER NOT NULL,`value_id` INTEGER,`transfer_value` NUMERIC,`transfer_value_previous_year` NUMERIC,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gcd`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `financial_years`(`id` INTEGER NOT NULL,`company_id` INTEGER NOT NULL,`fyear` INTEGER,`is_enabled` INTEGER,`fiscal_year_begin` TEXT,`fiscal_year_end` TEXT,`bal_sheet_closing_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `document_rights`(`id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`user_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_element_info`(`id` INTEGER NOT NULL,`template_id` INTEGER,`element_id` TEXT,`auto_compute_enabled` INTEGER,`supress_warning_messages` INTEGER NOT NULL,`send_account_balances` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gaap`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`is_manual_value` INTEGER NOT NULL,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_changes_equity_statement`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_net`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                };
                var version131_oracle = new TableInformation();
                AddVersion("1.3.1", version131_mysql, version131_sqlite, version131_sqlserver, version131_oracle);
                #endregion
            }

            {
                #region version 140
                //Changed table: users (deleted UNIQUE_KEY(username))
                TableInformation version140_mysql = new TableInformation();

                #region indices
                List<IndexInformation> indexInfo = new List<IndexInformation>();
                IndexInformation info = new IndexInformation();
                info.tableName = "templates_balance_list";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "templates_account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "systems";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "templates_account_splittings";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "templates_balance_list";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gaap";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "financial_years";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_financial_years_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd_company";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "splitted_accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_splitted_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "users";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "documents";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_element_info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_assignment_template_element_info_template_id",
                                                           new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "taxonomy_ids";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "split_account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_split_account_groups_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "companies";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_net";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_net_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "balance_lists";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_balance_lists_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_account_groups_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_admin";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_send";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "assignment_template_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_assignment_template_lines_template_id",
                                                                        new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "roles";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "rights";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("rights_content",
                                                                        new List<string>
                                                                        {"role_id", "content_type", "ref_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "user_roles";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_user_roles_1",
                                                                        new List<string> {"user_id", "role_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                #endregion

                version140_mysql.indexInfo = indexInfo;
                version140_mysql.tableCreation = new List<string> {
                    "CREATE TABLE `templates_balance_list`(`Id` INT NOT NULL,`name` VARCHAR(64) NOT NULL,`comment` TEXT,`creator_id` INT NOT NULL,`timestamp` DATETIME NOT NULL,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `templates_account_groups`(`Id` INT NOT NULL,`name` VARCHAR(64) NOT NULL,`comment` TEXT,`creator_id` INT NOT NULL,`timestamp` DATETIME NOT NULL,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `systems`(`id` INT NOT NULL,`name` VARCHAR(64),`comment` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_250efb8cd67044b8bff0a4105e05268f(`name`)) ENGINE = InnoDb",
                    "CREATE TABLE `templates_account_splittings`(`Id` INT NOT NULL,`name` VARCHAR(64) NOT NULL,`comment` TEXT,`creator_id` INT NOT NULL,`timestamp` DATETIME NOT NULL,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `templates_balance_list`(`Id` INT NOT NULL,`name` VARCHAR(64) NOT NULL,`comment` TEXT,`creator_id` INT NOT NULL,`timestamp` DATETIME NOT NULL,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gaap`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `financial_years`(`id` INT NOT NULL,`company_id` INT NOT NULL,`fyear` INT,`is_enabled` TINYINT(1),`fiscal_year_begin` DATETIME,`fiscal_year_end` DATETIME,`bal_sheet_closing_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`account_structure` VARCHAR(128),`comment` TEXT,`creator_id` INT,`last_modifier_id` INT,`creation_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd_company`(`parent_id` BIGINT,`company_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `splitted_accounts`(`amount` decimal(20,2) NOT NULL,`amount_percent` decimal(20,2),`split_group_id` BIGINT,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`assigned_element_id` INT,`comment` TEXT,`hidden` TINYINT(1) NOT NULL,`sort_index` VARCHAR(64) NOT NULL,`in_tray` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `users`(`id` INT NOT NULL,`username` VARCHAR(64),`fullname` VARCHAR(256),`salt` VARCHAR(64),`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` TINYINT(1),`is_admin` TINYINT(1),`is_deleted` TINYINT(1),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `info`(`id` INT NOT NULL,`key` VARCHAR(64) NOT NULL,`value` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_c5ee0b71808b4813907941a2d519f8a6(`key`)) ENGINE = InnoDb",
                    "CREATE TABLE `documents`(`id` INT NOT NULL,`name` VARCHAR(256) NOT NULL,`comment` TEXT,`company_id` INT,`system_id` INT,`financial_year_id` INT,`owner_id` INT,`creation_date` DATETIME,`last_modifier_id` INT,`last_modified_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`xbrl_elem_id` VARCHAR(128),`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_element_info`(`id` BIGINT NOT NULL,`template_id` INT,`element_id` TEXT,`auto_compute_enabled` TINYINT(1),`supress_warning_messages` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `taxonomy_ids`(`id` INT NOT NULL,`taxonomy_id` TEXT NOT NULL,`number` VARCHAR(10) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `split_account_groups`(`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`account_id` BIGINT NOT NULL,`comment` TEXT,`value_input_mode` INT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_lines`(`id` BIGINT NOT NULL,`document_id` INT NOT NULL,`header_id` INT NOT NULL,`value_id` BIGINT,`transfer_value` decimal(20,2),`transfer_value_previous_year` decimal(20,2),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `companies`(`Id` INT NOT NULL,`owner_id` INT,`name` VARCHAR(256),PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_net`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `accounts`(`amount` decimal(20,2) NOT NULL,`group_id` BIGINT,`user_defined` TINYINT(1) NOT NULL,`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`assigned_element_id` INT,`comment` TEXT,`hidden` TINYINT(1) NOT NULL,`sort_index` VARCHAR(64) NOT NULL,`in_tray` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `balance_lists`(`id` INT NOT NULL,`document_id` INT,`comment` TEXT,`name` VARCHAR(40),`imported_from_id` INT,`import_date` DATETIME,`source` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `account_groups`(`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`assigned_element_id` INT,`comment` TEXT,`hidden` TINYINT(1) NOT NULL,`sort_index` VARCHAR(64) NOT NULL,`in_tray` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_admin`(`id` BIGINT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`content_type` INT NOT NULL,`action_type` INT NOT NULL,`reference_id` BIGINT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`info` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_send`(`id` INT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`document_id` BIGINT NOT NULL,`send_data` LONGTEXT NOT NULL,`report_info` LONGTEXT NOT NULL,`send_error` INT NOT NULL,`result_message` TEXT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `assignment_template_lines`(`id` INT NOT NULL,`template_id` INT,`account_number` VARCHAR(32),`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `roles`(`id` INT NOT NULL,`name` VARCHAR(256),`comment` TEXT,`user_id` INT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `rights`(`id` INT NOT NULL,`role_id` INT NOT NULL,`content_type` INT NOT NULL,`ref_id` INT NOT NULL,`rights` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `user_roles`(`id` INT NOT NULL,`user_id` INT NOT NULL,`role_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`document_id` INT NOT NULL,`type` INT NOT NULL,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_report_1_value_change`(`id` BIGINT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`taxonomy_id` INT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`reference_id` BIGINT NOT NULL,`value_type` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_report_1`(`id` BIGINT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`content_type` INT NOT NULL,`action_type` INT NOT NULL,`reference_id` BIGINT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`info` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                };

                TableInformation version140_sqlserver = new TableInformation();
                version140_sqlserver.tableCreation = new List<string> {
                    "CREATE TABLE [templates_balance_list]([Id] INT NOT NULL,[name] VARCHAR(64) NOT NULL,[comment] TEXT,[creator_id] INT NOT NULL,[timestamp] DATETIME NOT NULL,PRIMARY KEY([Id]))",
                    "CREATE TABLE [templates_account_groups]([Id] INT NOT NULL,[name] VARCHAR(64) NOT NULL,[comment] TEXT,[creator_id] INT NOT NULL,[timestamp] DATETIME NOT NULL,PRIMARY KEY([Id]))",
                    "CREATE TABLE [systems]([id] INT NOT NULL,[name] VARCHAR(64),[comment] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([name]))",
                    "CREATE TABLE [templates_account_splittings]([Id] INT NOT NULL,[name] VARCHAR(64) NOT NULL,[comment] TEXT,[creator_id] INT NOT NULL,[timestamp] DATETIME NOT NULL,PRIMARY KEY([Id]))",
                    "CREATE TABLE [templates_balance_list]([Id] INT NOT NULL,[name] VARCHAR(64) NOT NULL,[comment] TEXT,[creator_id] INT NOT NULL,[timestamp] DATETIME NOT NULL,PRIMARY KEY([Id]))",
                    "CREATE TABLE [values_gaap]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [financial_years]([id] INT NOT NULL,[company_id] INT NOT NULL,[fyear] INT,[is_enabled] TINYINT,[fiscal_year_begin] DATETIME,[fiscal_year_end] DATETIME,[bal_sheet_closing_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_heads]([id] INT NOT NULL,[name] VARCHAR(64),[account_structure] VARCHAR(128),[comment] TEXT,[creator_id] INT,[last_modifier_id] INT,[creation_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gcd_company]([parent_id] INT,[company_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [splitted_accounts]([amount] decimal(20,2) NOT NULL,[amount_percent] decimal(20,2),[split_group_id] BIGINT,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[assigned_element_id] INT,[comment] TEXT,[hidden] TINYINT NOT NULL,[sort_index] VARCHAR(64) NOT NULL,[in_tray] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [users]([id] INT NOT NULL,[username] VARCHAR(64),[fullname] VARCHAR(256),[salt] VARCHAR(64),[password] TEXT NOT NULL,[key] TEXT NOT NULL,[is_active] TINYINT,[is_admin] TINYINT,[is_deleted] TINYINT,PRIMARY KEY([id]))",
                    "CREATE TABLE [info]([id] INT NOT NULL,[key] VARCHAR(64) NOT NULL,[value] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([key]))",
                    "CREATE TABLE [documents]([id] INT NOT NULL,[name] VARCHAR(256) NOT NULL,[comment] TEXT,[company_id] INT,[system_id] INT,[financial_year_id] INT,[owner_id] INT,[creation_date] DATETIME,[last_modifier_id] INT,[last_modified_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gcd]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[xbrl_elem_id] VARCHAR(128),[value] TEXT,[cb_value_other] TEXT,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_element_info]([id] BIGINT NOT NULL,[template_id] INT,[element_id] TEXT,[auto_compute_enabled] TINYINT,[supress_warning_messages] TINYINT NOT NULL,[send_account_balances] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [taxonomy_ids]([id] INT NOT NULL,[taxonomy_id] TEXT NOT NULL,[number] VARCHAR(10) NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [split_account_groups]([id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[account_id] BIGINT NOT NULL,[comment] TEXT,[value_input_mode] INT,PRIMARY KEY([id]))",
                    "CREATE TABLE [transfer_hbst_lines]([id] BIGINT NOT NULL,[document_id] INT NOT NULL,[header_id] INT NOT NULL,[value_id] BIGINT,[transfer_value] decimal(20,2),[transfer_value_previous_year] decimal(20,2),PRIMARY KEY([id]))",
                    "CREATE TABLE [companies]([Id] INT NOT NULL,[owner_id] INT,[name] VARCHAR(256),PRIMARY KEY([Id]))",
                    "CREATE TABLE [hypercube_ass_net]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [accounts]([amount] decimal(20,2) NOT NULL,[group_id] BIGINT,[user_defined] TINYINT NOT NULL,[id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[assigned_element_id] INT,[comment] TEXT,[hidden] TINYINT NOT NULL,[sort_index] VARCHAR(64) NOT NULL,[in_tray] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [balance_lists]([id] INT NOT NULL,[document_id] INT,[comment] TEXT,[name] VARCHAR(40),[imported_from_id] INT,[import_date] DATETIME,[source] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [account_groups]([number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[assigned_element_id] INT,[comment] TEXT,[hidden] TINYINT NOT NULL,[sort_index] VARCHAR(64) NOT NULL,[in_tray] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_admin]([id] BIGINT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[content_type] INT NOT NULL,[action_type] INT NOT NULL,[reference_id] BIGINT NOT NULL,[old_value] TEXT,[new_value] TEXT,[info] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_send]([id] INT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[document_id] BIGINT NOT NULL,[send_data] TEXT NOT NULL,[report_info] TEXT NOT NULL,[send_error] INT NOT NULL,[result_message] TEXT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [assignment_template_lines]([id] INT NOT NULL,[template_id] INT,[account_number] VARCHAR(32),[account_name] TEXT,[debit_element_id] TEXT,[credit_element_id] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [roles]([id] INT NOT NULL,[name] VARCHAR(256),[comment] TEXT,[user_id] INT,PRIMARY KEY([id]))",
                    "CREATE TABLE [rights]([id] INT NOT NULL,[role_id] INT NOT NULL,[content_type] INT NOT NULL,[ref_id] INT NOT NULL,[rights] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [user_roles]([id] INT NOT NULL,[user_id] INT NOT NULL,[role_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [transfer_hbst_heads]([id] INT NOT NULL,[name] VARCHAR(64),[document_id] INT NOT NULL,[type] INT NOT NULL,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_report_1_value_change]([id] BIGINT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[taxonomy_id] INT NOT NULL,[old_value] TEXT,[new_value] TEXT,[reference_id] BIGINT NOT NULL,[value_type] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_report_1]([id] BIGINT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[content_type] INT NOT NULL,[action_type] INT NOT NULL,[reference_id] BIGINT NOT NULL,[old_value] TEXT,[new_value] TEXT,[info] TEXT,PRIMARY KEY([id]))",
                };
                TableInformation version140_sqlite = new TableInformation();
                version140_sqlite.tableCreation = new List<string> {
                    "CREATE TABLE `templates_balance_list`(`Id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`creator_id` INTEGER NOT NULL,`timestamp` TEXT NOT NULL,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `templates_account_groups`(`Id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`creator_id` INTEGER NOT NULL,`timestamp` TEXT NOT NULL,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `systems`(`id` INTEGER NOT NULL,`name` TEXT,`comment` TEXT,PRIMARY KEY(`id`),UNIQUE (`name`))",
                    "CREATE TABLE `templates_account_splittings`(`Id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`creator_id` INTEGER NOT NULL,`timestamp` TEXT NOT NULL,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `templates_balance_list`(`Id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`creator_id` INTEGER NOT NULL,`timestamp` TEXT NOT NULL,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `values_gaap`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `financial_years`(`id` INTEGER NOT NULL,`company_id` INTEGER NOT NULL,`fyear` INTEGER,`is_enabled` INTEGER,`fiscal_year_begin` TEXT,`fiscal_year_end` TEXT,`bal_sheet_closing_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_heads`(`id` INTEGER NOT NULL,`name` TEXT,`account_structure` TEXT,`comment` TEXT,`creator_id` INTEGER,`last_modifier_id` INTEGER,`creation_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gcd_company`(`parent_id` INTEGER,`company_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `splitted_accounts`(`amount` NUMERIC NOT NULL,`amount_percent` NUMERIC,`split_group_id` INTEGER,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,`hidden` INTEGER NOT NULL,`sort_index` TEXT NOT NULL,`in_tray` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `users`(`id` INTEGER NOT NULL,`username` TEXT,`fullname` TEXT,`salt` TEXT,`password` TEXT NOT NULL,`key` TEXT NOT NULL,`is_active` INTEGER,`is_admin` INTEGER,`is_deleted` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `info`(`id` INTEGER NOT NULL,`key` TEXT NOT NULL,`value` TEXT,PRIMARY KEY(`id`),UNIQUE (`key`))",
                    "CREATE TABLE `documents`(`id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`company_id` INTEGER,`system_id` INTEGER,`financial_year_id` INTEGER,`owner_id` INTEGER,`creation_date` TEXT,`last_modifier_id` INTEGER,`last_modified_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gcd`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`xbrl_elem_id` TEXT,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_element_info`(`id` INTEGER NOT NULL,`template_id` INTEGER,`element_id` TEXT,`auto_compute_enabled` INTEGER,`supress_warning_messages` INTEGER NOT NULL,`send_account_balances` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `taxonomy_ids`(`id` INTEGER NOT NULL,`taxonomy_id` TEXT NOT NULL,`number` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `split_account_groups`(`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`account_id` INTEGER NOT NULL,`comment` TEXT,`value_input_mode` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `transfer_hbst_lines`(`id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`header_id` INTEGER NOT NULL,`value_id` INTEGER,`transfer_value` NUMERIC,`transfer_value_previous_year` NUMERIC,PRIMARY KEY(`id`))",
                    "CREATE TABLE `companies`(`Id` INTEGER NOT NULL,`owner_id` INTEGER,`name` TEXT,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `accounts`(`amount` NUMERIC NOT NULL,`group_id` INTEGER,`user_defined` INTEGER NOT NULL,`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,`hidden` INTEGER NOT NULL,`sort_index` TEXT NOT NULL,`in_tray` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `balance_lists`(`id` INTEGER NOT NULL,`document_id` INTEGER,`comment` TEXT,`name` TEXT,`imported_from_id` INTEGER,`import_date` TEXT,`source` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `account_groups`(`number` TEXT NOT NULL,`name` TEXT NOT NULL,`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,`hidden` INTEGER NOT NULL,`sort_index` TEXT NOT NULL,`in_tray` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_admin`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`content_type` INTEGER NOT NULL,`action_type` INTEGER NOT NULL,`reference_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`info` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_send`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`send_data` TEXT NOT NULL,`report_info` TEXT NOT NULL,`send_error` INTEGER NOT NULL,`result_message` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `assignment_template_lines`(`id` INTEGER NOT NULL,`template_id` INTEGER,`account_number` TEXT,`account_name` TEXT,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `roles`(`id` INTEGER NOT NULL,`name` TEXT,`comment` TEXT,`user_id` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `rights`(`id` INTEGER NOT NULL,`role_id` INTEGER NOT NULL,`content_type` INTEGER NOT NULL,`ref_id` INTEGER NOT NULL,`rights` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `user_roles`(`id` INTEGER NOT NULL,`user_id` INTEGER NOT NULL,`role_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INTEGER NOT NULL,`name` TEXT,`document_id` INTEGER NOT NULL,`type` INTEGER NOT NULL,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_report_1_value_change`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`taxonomy_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`reference_id` INTEGER NOT NULL,`value_type` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_report_1`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`content_type` INTEGER NOT NULL,`action_type` INTEGER NOT NULL,`reference_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`info` TEXT,PRIMARY KEY(`id`))",
                };
                var version140_oracle = new TableInformation();
                AddVersion("1.4.0", version140_mysql, version140_sqlite, version140_sqlserver, version140_oracle);
                #endregion

            }
            {
                #region version 150
                TableInformation version150_mysql = new TableInformation();

                #region indices 150
                List<IndexInformation> indexInfo = new List<IndexInformation>();
                IndexInformation info = new IndexInformation();
                info.tableName = "templates_account_splittings";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "taxonomy_ids";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_taxonomy_ids__taxonomy_id",
                                                                        new List<string> {"taxonomy_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "financial_years";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_financial_years_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "templates_balance_list";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "templates_account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "templates_balance_list";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "systems";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "companies";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "roles";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "rights";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("rights_content",
                                                                        new List<string>
                                                                        {"role_id", "content_type", "ref_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "user_roles";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_user_roles_1",
                                                                        new List<string> {"user_id", "role_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "mapping_template_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_mapping_template_lines_template_id",
                                                                        new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "splitted_accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_splitted_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "taxonomy_info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx__taxonomy_info__name",
                                                                        new List<string> {"name"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gaap";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "transfer_hbst_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_net";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_net_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "mapping_template_element_info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_mapping_template_element_info_template_id",
                                                           new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "documents";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_changes_equity_statement";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_hypercube_changes_equity_statement_document_id",
                                                           new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_gross";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_ass_gross_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "mapping_template_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_ass_gross_short";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>(
                                     "fki_hypercube_ass_gross_short_document_id", new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "balance_lists";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_balance_lists_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "split_account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_split_account_groups_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd_company";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_account_groups_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "users";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_admin";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_send";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_report_1_value_change";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_report_1";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                #endregion

                version150_mysql.indexInfo = indexInfo;
                version150_mysql.tableCreation = new List<string> {
                    "CREATE TABLE `templates_account_splittings`(`Id` INT NOT NULL,`name` VARCHAR(64) NOT NULL,`comment` TEXT,`creator_id` INT NOT NULL,`timestamp` DATETIME NOT NULL,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_lines`(`id` BIGINT NOT NULL,`document_id` INT NOT NULL,`header_id` INT NOT NULL,`value_id` BIGINT,`transfer_value` decimal(20,2),`transfer_value_previous_year` decimal(20,2),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `taxonomy_ids`(`id` INT NOT NULL,`taxonomy_id` INT,`xbrl_element_id` TEXT NOT NULL,`number` VARCHAR(10) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `financial_years`(`id` INT NOT NULL,`company_id` INT NOT NULL,`fyear` INT,`is_enabled` TINYINT(1),`fiscal_year_begin` DATETIME,`fiscal_year_end` DATETIME,`bal_sheet_closing_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `templates_balance_list`(`Id` INT NOT NULL,`name` VARCHAR(64) NOT NULL,`comment` TEXT,`creator_id` INT NOT NULL,`timestamp` DATETIME NOT NULL,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `templates_account_groups`(`Id` INT NOT NULL,`name` VARCHAR(64) NOT NULL,`comment` TEXT,`creator_id` INT NOT NULL,`timestamp` DATETIME NOT NULL,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `templates_balance_list`(`Id` INT NOT NULL,`name` VARCHAR(64) NOT NULL,`comment` TEXT,`creator_id` INT NOT NULL,`timestamp` DATETIME NOT NULL,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `systems`(`id` INT NOT NULL,`name` VARCHAR(128),`comment` VARCHAR(256),PRIMARY KEY(`id`),UNIQUE KEY uk_0e4f119a2b9d40ada71018ae3b316c02(`name`)) ENGINE = InnoDb",
                    "CREATE TABLE `companies`(`Id` INT NOT NULL,`name` VARCHAR(256),`owner_id` INT,`taxonomy_info_id` INT,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `roles`(`id` INT NOT NULL,`name` VARCHAR(256),`comment` TEXT,`user_id` INT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `rights`(`id` INT NOT NULL,`role_id` INT NOT NULL,`content_type` INT NOT NULL,`ref_id` INT NOT NULL,`rights` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `user_roles`(`id` INT NOT NULL,`user_id` INT NOT NULL,`role_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `mapping_template_lines`(`id` INT NOT NULL,`template_id` INT,`account_number` VARCHAR(32),`account_name` TEXT,`is_account_of_exchange` TINYINT(1),`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `splitted_accounts`(`amount` decimal(20,2) NOT NULL,`amount_percent` decimal(20,2),`split_group_id` BIGINT,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`assigned_element_id` INT,`comment` TEXT,`hidden` TINYINT(1) NOT NULL,`sort_index` VARCHAR(64) NOT NULL,`in_tray` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `taxonomy_info`(`id` INT NOT NULL,`name` VARCHAR(128) NOT NULL,`path` TEXT NOT NULL,`filename` TEXT NOT NULL,`type` INT NOT NULL,`version` VARCHAR(64) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gaap`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`elem_id` INT,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`document_id` INT NOT NULL,`type` INT NOT NULL,`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_net`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `mapping_template_element_info`(`id` BIGINT NOT NULL,`template_id` INT,`element_id` TEXT,`auto_compute_enabled` TINYINT(1),`supress_warning_messages` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `documents`(`id` INT NOT NULL,`name` VARCHAR(256) NOT NULL,`comment` TEXT,`company_id` INT,`system_id` INT,`financial_year_id` INT,`owner_id` INT,`gcd_taxonomy_info_id` INT,`main_taxonomy_info_id` INT,`creation_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_changes_equity_statement`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `mapping_template_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`account_structure` VARCHAR(128),`comment` TEXT,`creator_id` INT,`last_modifier_id` INT,`creation_date` DATETIME,`modify_date` DATETIME,`taxonomy_info_id` INT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`elem_id` INT,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_ass_gross_short`(`id` INT NOT NULL,`document_id` INT,`dim1_xbrl_elem_id` VARCHAR(128),`dim2_xbrl_elem_id` VARCHAR(128),`dim3_xbrl_elem_id` VARCHAR(128),`value` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `accounts`(`amount` decimal(20,2) NOT NULL,`group_id` BIGINT,`user_defined` TINYINT(1) NOT NULL,`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`assigned_element_id` INT,`comment` TEXT,`hidden` TINYINT(1) NOT NULL,`sort_index` VARCHAR(64) NOT NULL,`in_tray` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `balance_lists`(`id` INT NOT NULL,`document_id` INT,`comment` TEXT,`name` VARCHAR(40),`imported_from_id` INT,`import_date` DATETIME,`source` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `split_account_groups`(`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`account_id` BIGINT NOT NULL,`comment` TEXT,`value_input_mode` INT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd_company`(`parent_id` BIGINT,`company_id` INT,`id` BIGINT NOT NULL,`elem_id` INT,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `account_groups`(`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`assigned_element_id` INT,`comment` TEXT,`hidden` TINYINT(1) NOT NULL,`sort_index` VARCHAR(64) NOT NULL,`in_tray` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `users`(`id` INT NOT NULL,`username` VARCHAR(128),`fullname` VARCHAR(256),`salt` VARCHAR(64),`password` TEXT NOT NULL,`is_active` TINYINT(1),`is_admin` TINYINT(1),`is_deleted` TINYINT(1),`is_domain_user` TINYINT(1),`domain` VARCHAR(256),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `info`(`id` INT NOT NULL,`key` VARCHAR(64) NOT NULL,`value` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_15fe997cc13445d99291afb4d33071a6(`key`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_admin`(`id` BIGINT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`content_type` INT NOT NULL,`action_type` INT NOT NULL,`reference_id` BIGINT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`info` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_send`(`id` INT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`document_id` BIGINT NOT NULL,`send_data` LONGTEXT NOT NULL,`report_info` LONGTEXT NOT NULL,`send_error` INT NOT NULL,`result_message` TEXT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_report_1_value_change`(`id` BIGINT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`taxonomy_id` INT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`reference_id` BIGINT NOT NULL,`value_type` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_report_1`(`id` BIGINT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`content_type` INT NOT NULL,`action_type` INT NOT NULL,`reference_id` BIGINT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`info` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                };



                TableInformation version150_sqlserver = new TableInformation();
                version150_sqlserver.tableCreation = new List<string> {
                    "CREATE TABLE [templates_account_splittings]([Id] INT NOT NULL,[name] VARCHAR(64) NOT NULL,[comment] TEXT,[creator_id] INT NOT NULL,[timestamp] DATETIME NOT NULL,PRIMARY KEY([Id]))",
                    "CREATE TABLE [transfer_hbst_lines]([id] BIGINT NOT NULL,[document_id] INT NOT NULL,[header_id] INT NOT NULL,[value_id] BIGINT,[transfer_value] decimal(20,2),[transfer_value_previous_year] decimal(20,2),PRIMARY KEY([id]))",
                    "CREATE TABLE [taxonomy_ids]([id] INT NOT NULL,[taxonomy_id] INT,[xbrl_element_id] TEXT NOT NULL,[number] VARCHAR(10) NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [financial_years]([id] INT NOT NULL,[company_id] INT NOT NULL,[fyear] INT,[is_enabled] TINYINT,[fiscal_year_begin] DATETIME,[fiscal_year_end] DATETIME,[bal_sheet_closing_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [templates_balance_list]([Id] INT NOT NULL,[name] VARCHAR(64) NOT NULL,[comment] TEXT,[creator_id] INT NOT NULL,[timestamp] DATETIME NOT NULL,PRIMARY KEY([Id]))",
                    "CREATE TABLE [templates_account_groups]([Id] INT NOT NULL,[name] VARCHAR(64) NOT NULL,[comment] TEXT,[creator_id] INT NOT NULL,[timestamp] DATETIME NOT NULL,PRIMARY KEY([Id]))",
                    "CREATE TABLE [templates_balance_list]([Id] INT NOT NULL,[name] VARCHAR(64) NOT NULL,[comment] TEXT,[creator_id] INT NOT NULL,[timestamp] DATETIME NOT NULL,PRIMARY KEY([Id]))",
                    "CREATE TABLE [systems]([id] INT NOT NULL,[name] VARCHAR(128),[comment] VARCHAR(256),PRIMARY KEY([id]),UNIQUE ([name]))",
                    "CREATE TABLE [companies]([Id] INT NOT NULL,[name] VARCHAR(256),[owner_id] INT,[taxonomy_info_id] INT,PRIMARY KEY([Id]))",
                    "CREATE TABLE [roles]([id] INT NOT NULL,[name] VARCHAR(256),[comment] TEXT,[user_id] INT,PRIMARY KEY([id]))",
                    "CREATE TABLE [rights]([id] INT NOT NULL,[role_id] INT NOT NULL,[content_type] INT NOT NULL,[ref_id] INT NOT NULL,[rights] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [user_roles]([id] INT NOT NULL,[user_id] INT NOT NULL,[role_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [mapping_template_lines]([id] INT NOT NULL,[template_id] INT,[account_number] VARCHAR(32),[account_name] TEXT,[is_account_of_exchange] TINYINT,[debit_element_id] TEXT,[credit_element_id] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [splitted_accounts]([amount] decimal(20,2) NOT NULL,[amount_percent] decimal(20,2),[split_group_id] BIGINT,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[assigned_element_id] INT,[comment] TEXT,[hidden] TINYINT NOT NULL,[sort_index] VARCHAR(64) NOT NULL,[in_tray] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [taxonomy_info]([id] INT NOT NULL,[name] VARCHAR(128) NOT NULL,[path] TEXT NOT NULL,[filename] TEXT NOT NULL,[type] INT NOT NULL,[version] VARCHAR(64) NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gaap]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[elem_id] INT,[value] TEXT,[cb_value_other] TEXT,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [transfer_hbst_heads]([id] INT NOT NULL,[name] VARCHAR(64),[document_id] INT NOT NULL,[type] INT NOT NULL,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_net]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [mapping_template_element_info]([id] BIGINT NOT NULL,[template_id] INT,[element_id] TEXT,[auto_compute_enabled] TINYINT,[supress_warning_messages] TINYINT NOT NULL,[send_account_balances] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [documents]([id] INT NOT NULL,[name] VARCHAR(256) NOT NULL,[comment] TEXT,[company_id] INT,[system_id] INT,[financial_year_id] INT,[owner_id] INT,[gcd_taxonomy_info_id] INT,[main_taxonomy_info_id] INT,[creation_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_changes_equity_statement]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_gross]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [mapping_template_heads]([id] INT NOT NULL,[name] VARCHAR(64),[account_structure] VARCHAR(128),[comment] TEXT,[creator_id] INT,[last_modifier_id] INT,[creation_date] DATETIME,[modify_date] DATETIME,[taxonomy_info_id] INT,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gcd]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[elem_id] INT,[value] TEXT,[cb_value_other] TEXT,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_ass_gross_short]([id] INT NOT NULL,[document_id] INT,[dim1_xbrl_elem_id] VARCHAR(128),[dim2_xbrl_elem_id] VARCHAR(128),[dim3_xbrl_elem_id] VARCHAR(128),[value] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [accounts]([amount] decimal(20,2) NOT NULL,[group_id] BIGINT,[user_defined] TINYINT NOT NULL,[id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[assigned_element_id] INT,[comment] TEXT,[hidden] TINYINT NOT NULL,[sort_index] VARCHAR(64) NOT NULL,[in_tray] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [balance_lists]([id] INT NOT NULL,[document_id] INT,[comment] TEXT,[name] VARCHAR(40),[imported_from_id] INT,[import_date] DATETIME,[source] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [split_account_groups]([id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[account_id] BIGINT NOT NULL,[comment] TEXT,[value_input_mode] INT,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gcd_company]([parent_id] INT,[company_id] INT,[id] BIGINT NOT NULL,[elem_id] INT,[value] TEXT,[cb_value_other] TEXT,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [account_groups]([number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[assigned_element_id] INT,[comment] TEXT,[hidden] TINYINT NOT NULL,[sort_index] VARCHAR(64) NOT NULL,[in_tray] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [users]([id] INT NOT NULL,[username] VARCHAR(128),[fullname] VARCHAR(256),[salt] VARCHAR(64),[password] TEXT NOT NULL,[is_active] TINYINT,[is_admin] TINYINT,[is_deleted] TINYINT,[is_domain_user] TINYINT,[domain] VARCHAR(256),PRIMARY KEY([id]))",
                    "CREATE TABLE [info]([id] INT NOT NULL,[key] VARCHAR(64) NOT NULL,[value] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([key]))",
                    "CREATE TABLE [log_admin]([id] BIGINT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[content_type] INT NOT NULL,[action_type] INT NOT NULL,[reference_id] BIGINT NOT NULL,[old_value] TEXT,[new_value] TEXT,[info] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_send]([id] INT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[document_id] BIGINT NOT NULL,[send_data] TEXT NOT NULL,[report_info] TEXT NOT NULL,[send_error] INT NOT NULL,[result_message] TEXT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_report_1_value_change]([id] BIGINT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[taxonomy_id] INT NOT NULL,[old_value] TEXT,[new_value] TEXT,[reference_id] BIGINT NOT NULL,[value_type] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_report_1]([id] BIGINT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[content_type] INT NOT NULL,[action_type] INT NOT NULL,[reference_id] BIGINT NOT NULL,[old_value] TEXT,[new_value] TEXT,[info] TEXT,PRIMARY KEY([id]))",
                };


                TableInformation version150_sqlite = new TableInformation();
                version150_sqlite.tableCreation = new List<string> {
                    "CREATE TABLE `templates_account_splittings`(`Id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`creator_id` INTEGER NOT NULL,`timestamp` TEXT NOT NULL,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `transfer_hbst_lines`(`id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`header_id` INTEGER NOT NULL,`value_id` INTEGER,`transfer_value` NUMERIC,`transfer_value_previous_year` NUMERIC,PRIMARY KEY(`id`))",
                    "CREATE TABLE `taxonomy_ids`(`id` INTEGER NOT NULL,`taxonomy_id` INTEGER,`xbrl_element_id` TEXT NOT NULL,`number` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `financial_years`(`id` INTEGER NOT NULL,`company_id` INTEGER NOT NULL,`fyear` INTEGER,`is_enabled` INTEGER,`fiscal_year_begin` TEXT,`fiscal_year_end` TEXT,`bal_sheet_closing_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `templates_balance_list`(`Id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`creator_id` INTEGER NOT NULL,`timestamp` TEXT NOT NULL,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `templates_account_groups`(`Id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`creator_id` INTEGER NOT NULL,`timestamp` TEXT NOT NULL,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `templates_balance_list`(`Id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`creator_id` INTEGER NOT NULL,`timestamp` TEXT NOT NULL,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `systems`(`id` INTEGER NOT NULL,`name` TEXT,`comment` TEXT,PRIMARY KEY(`id`),UNIQUE (`name`))",
                    "CREATE TABLE `companies`(`Id` INTEGER NOT NULL,`name` TEXT,`taxonomy_info_id` INTEGER,`owner_id` INTEGER,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `roles`(`id` INTEGER NOT NULL,`name` TEXT,`comment` TEXT,`user_id` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `rights`(`id` INTEGER NOT NULL,`role_id` INTEGER NOT NULL,`content_type` INTEGER NOT NULL,`ref_id` INTEGER NOT NULL,`rights` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `user_roles`(`id` INTEGER NOT NULL,`user_id` INTEGER NOT NULL,`role_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `mapping_template_lines`(`id` INTEGER NOT NULL,`template_id` INTEGER,`account_number` TEXT,`account_name` TEXT,`is_account_of_exchange` INTEGER,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `splitted_accounts`(`amount` NUMERIC NOT NULL,`amount_percent` NUMERIC,`split_group_id` INTEGER,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,`hidden` INTEGER NOT NULL,`sort_index` TEXT NOT NULL,`in_tray` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `taxonomy_info`(`id` INTEGER NOT NULL,`name` TEXT NOT NULL,`path` TEXT NOT NULL,`filename` TEXT NOT NULL,`type` INTEGER NOT NULL,`version` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gaap`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`elem_id` INTEGER,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `transfer_hbst_heads`(`id` INTEGER NOT NULL,`name` TEXT,`document_id` INTEGER NOT NULL,`type` INTEGER NOT NULL,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_net`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `mapping_template_element_info`(`id` INTEGER NOT NULL,`template_id` INTEGER,`element_id` TEXT,`auto_compute_enabled` INTEGER,`supress_warning_messages` INTEGER NOT NULL,`send_account_balances` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `documents`(`id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`company_id` INTEGER,`system_id` INTEGER,`financial_year_id` INTEGER,`owner_id` INTEGER,`gcd_taxonomy_info_id` INTEGER,`main_taxonomy_info_id` INTEGER,`creation_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_changes_equity_statement`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_gross`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `mapping_template_heads`(`id` INTEGER NOT NULL,`name` TEXT,`account_structure` TEXT,`comment` TEXT,`creator_id` INTEGER,`last_modifier_id` INTEGER,`creation_date` TEXT,`modify_date` TEXT,`taxonomy_info_id` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gcd`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`elem_id` INTEGER,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_ass_gross_short`(`id` INTEGER NOT NULL,`document_id` INTEGER,`dim1_xbrl_elem_id` TEXT,`dim2_xbrl_elem_id` TEXT,`dim3_xbrl_elem_id` TEXT,`value` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `accounts`(`amount` NUMERIC NOT NULL,`group_id` INTEGER,`user_defined` INTEGER NOT NULL,`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,`hidden` INTEGER NOT NULL,`sort_index` TEXT NOT NULL,`in_tray` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `balance_lists`(`id` INTEGER NOT NULL,`document_id` INTEGER,`comment` TEXT,`name` TEXT,`imported_from_id` INTEGER,`import_date` TEXT,`source` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `split_account_groups`(`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`account_id` INTEGER NOT NULL,`comment` TEXT,`value_input_mode` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gcd_company`(`parent_id` INTEGER,`company_id` INTEGER,`id` INTEGER NOT NULL,`elem_id` INTEGER,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `account_groups`(`number` TEXT NOT NULL,`name` TEXT NOT NULL,`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,`hidden` INTEGER NOT NULL,`sort_index` TEXT NOT NULL,`in_tray` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `users`(`id` INTEGER NOT NULL,`username` TEXT,`fullname` TEXT,`salt` TEXT,`password` TEXT NOT NULL,`is_active` INTEGER,`is_admin` INTEGER,`is_deleted` INTEGER,`is_domain_user` INTEGER,`domain` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `info`(`id` INTEGER NOT NULL,`key` TEXT NOT NULL,`value` TEXT,PRIMARY KEY(`id`),UNIQUE (`key`))",
                    "CREATE TABLE `log_admin`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`content_type` INTEGER NOT NULL,`action_type` INTEGER NOT NULL,`reference_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`info` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_send`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`send_data` TEXT NOT NULL,`report_info` TEXT NOT NULL,`send_error` INTEGER NOT NULL,`result_message` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_report_1_value_change`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`taxonomy_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`reference_id` INTEGER NOT NULL,`value_type` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_report_1`(`id` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`content_type` INTEGER NOT NULL,`action_type` INTEGER NOT NULL,`reference_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`info` TEXT,PRIMARY KEY(`id`))",
                };


                var version150_oracle = new TableInformation();
                version150_oracle.tableCreation = new List<string> {
                    "CREATE TABLE \"templates_account_splittings\"(\"Id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(64) NOT NULL ,\"comment\" CLOB,\"creator_id\" NUMBER(10,0) NOT NULL ,\"timestamp\" TIMESTAMP NOT NULL ,CONSTRAINT pk_cc8712cb5bb34ffd903f71088b PRIMARY KEY(\"Id\")) INITRANS 5 ",
                    "CREATE TABLE \"transfer_hbst_lines\"(\"id\" NUMBER(19,0) NOT NULL ,\"document_id\" NUMBER(10,0) NOT NULL ,\"header_id\" NUMBER(10,0) NOT NULL ,\"value_id\" NUMBER(19,0),\"transfer_value\" FLOAT(24),\"transfer_value_previous_year\" FLOAT(24),CONSTRAINT pk_0e3b30da06cf4ccaa7f2199266 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"taxonomy_ids\"(\"id\" NUMBER(10,0) NOT NULL ,\"taxonomy_id\" NUMBER(10,0),\"xbrl_element_id\" CLOB NOT NULL ,\"number\" VARCHAR2(10) NOT NULL ,CONSTRAINT pk_210929f3dbf14f379ad0da3657 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"financial_years\"(\"id\" NUMBER(10,0) NOT NULL ,\"company_id\" NUMBER(10,0) NOT NULL ,\"fyear\" NUMBER(10,0),\"is_enabled\" NUMBER(1,0),\"fiscal_year_begin\" TIMESTAMP,\"fiscal_year_end\" TIMESTAMP,\"bal_sheet_closing_date\" TIMESTAMP,CONSTRAINT pk_004c4177c14c4d5ca5c1ccc24d PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"templates_balance_list\"(\"Id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(64) NOT NULL ,\"comment\" CLOB,\"creator_id\" NUMBER(10,0) NOT NULL ,\"timestamp\" TIMESTAMP NOT NULL ,CONSTRAINT pk_69a2f265bbd54649b190a6be58 PRIMARY KEY(\"Id\")) INITRANS 5 ",
                    "CREATE TABLE \"templates_account_groups\"(\"Id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(64) NOT NULL ,\"comment\" CLOB,\"creator_id\" NUMBER(10,0) NOT NULL ,\"timestamp\" TIMESTAMP NOT NULL ,CONSTRAINT pk_fb7f497fb7264756adf9410793 PRIMARY KEY(\"Id\")) INITRANS 5 ",
                    "CREATE TABLE \"templates_balance_list\"(\"Id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(64) NOT NULL ,\"comment\" CLOB,\"creator_id\" NUMBER(10,0) NOT NULL ,\"timestamp\" TIMESTAMP NOT NULL ,CONSTRAINT pk_895fa044fe8b487a90df8fafc3 PRIMARY KEY(\"Id\")) INITRANS 5 ",
                    "CREATE TABLE \"systems\"(\"id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(128),\"comment\" VARCHAR2(256),CONSTRAINT pk_93801db9565c47c8b92d28e122 PRIMARY KEY(\"id\"),CONSTRAINT uk_3d70307495464f45bcad954a28 UNIQUE(\"name\")) INITRANS 5 ",
                    "CREATE TABLE \"companies\"(\"Id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(256),\"owner_id\" NUMBER(10,0),\"taxonomy_info_id\" NUMBER(10,0),CONSTRAINT pk_3418f7dfd2fb45b0b939f600cc PRIMARY KEY(\"Id\")) INITRANS 5 ",
                    "CREATE TABLE \"roles\"(\"id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(256),\"comment\" CLOB,\"user_id\" NUMBER(10,0),CONSTRAINT pk_523960938d664e52bc92d986c8 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"rights\"(\"id\" NUMBER(10,0) NOT NULL ,\"role_id\" NUMBER(10,0) NOT NULL ,\"content_type\" NUMBER NOT NULL ,\"ref_id\" NUMBER(10,0) NOT NULL ,\"rights\" NUMBER(10,0) NOT NULL ,CONSTRAINT pk_3bfa3a706ac44d7cbb124c769e PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"user_roles\"(\"id\" NUMBER(10,0) NOT NULL ,\"user_id\" NUMBER(10,0) NOT NULL ,\"role_id\" NUMBER(10,0) NOT NULL ,CONSTRAINT pk_c0c8df128ecb4b5c8ee19386a5 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"mapping_template_lines\"(\"id\" NUMBER(10,0) NOT NULL ,\"template_id\" NUMBER(10,0),\"account_number\" VARCHAR2(32),\"account_name\" CLOB,\"is_account_of_exchange\" NUMBER(1,0),\"debit_element_id\" CLOB,\"credit_element_id\" CLOB,CONSTRAINT pk_9a136f2fa4834988855a7c24b0 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"splitted_accounts\"(\"amount\" FLOAT(24) NOT NULL ,\"amount_percent\" FLOAT(24),\"split_group_id\" NUMBER(19,0),\"number\" VARCHAR2(32) NOT NULL ,\"name\" VARCHAR2(128) NOT NULL ,\"id\" NUMBER(19,0) NOT NULL ,\"balance_list_id\" NUMBER(10,0) NOT NULL ,\"assigned_element_id\" NUMBER(10,0),\"comment\" CLOB,\"hidden\" NUMBER(1,0) NOT NULL ,\"sort_index\" VARCHAR2(64) NOT NULL ,\"in_tray\" NUMBER(1,0) NOT NULL ,CONSTRAINT pk_bc76f4817d3f4aac8f9269ce2c PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"taxonomy_info\"(\"id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(128) NOT NULL ,\"path\" CLOB NOT NULL ,\"filename\" CLOB NOT NULL ,\"type\" NUMBER NOT NULL ,\"version\" VARCHAR2(64) NOT NULL ,CONSTRAINT pk_898310b5b18646f9877414dd51 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"values_gaap\"(\"parent_id\" NUMBER(19,0),\"document_id\" NUMBER(10,0),\"id\" NUMBER(19,0) NOT NULL ,\"elem_id\" NUMBER(10,0),\"value\" CLOB,\"cb_value_other\" CLOB,\"supress_warning_messages\" NUMBER(1,0) NOT NULL ,\"auto_computation_enabled\" NUMBER(1,0) NOT NULL ,\"send_account_balances\" NUMBER(1,0),\"comment\" CLOB,CONSTRAINT pk_f11bedf09cad4201aceb8cf3fa PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"transfer_hbst_heads\"(\"id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(64),\"document_id\" NUMBER(10,0) NOT NULL ,\"type\" NUMBER NOT NULL ,\"comment\" CLOB,CONSTRAINT pk_78d12e913be3485ba376896f70 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"hypercube_ass_net\"(\"id\" NUMBER(10,0) NOT NULL ,\"document_id\" NUMBER(10,0),\"dim1_xbrl_elem_id\" VARCHAR2(128),\"dim2_xbrl_elem_id\" VARCHAR2(128),\"dim3_xbrl_elem_id\" VARCHAR2(128),\"value\" CLOB,CONSTRAINT pk_e9eb84dfaad84d37b307cab30e PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"mapping_template_element_info\"(\"id\" NUMBER(19,0) NOT NULL ,\"template_id\" NUMBER(10,0),\"element_id\" CLOB,\"auto_compute_enabled\" NUMBER(1,0),\"supress_warning_messages\" NUMBER(1,0) NOT NULL ,\"send_account_balances\" NUMBER(1,0) NOT NULL ,CONSTRAINT pk_a1f1a88dc49f47219f0179a137 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"documents\"(\"id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(256) NOT NULL ,\"comment\" CLOB,\"company_id\" NUMBER(10,0),\"system_id\" NUMBER(10,0),\"financial_year_id\" NUMBER(10,0),\"owner_id\" NUMBER(10,0),\"gcd_taxonomy_info_id\" NUMBER(10,0),\"main_taxonomy_info_id\" NUMBER(10,0),\"creation_date\" TIMESTAMP,CONSTRAINT pk_e143ef3187944af4b6e0f5d884 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    //"CREATE TABLE \"hypercube_changes_equity_statement\"(\"id\" NUMBER(10,0) NOT NULL ,\"document_id\" NUMBER(10,0),\"dim1_xbrl_elem_id\" VARCHAR2(128),\"dim2_xbrl_elem_id\" VARCHAR2(128),\"dim3_xbrl_elem_id\" VARCHAR2(128),\"value\" CLOB,CONSTRAINT pk_fba2762f2fda43aca5bd1955eb PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"hypercube_ass_gross\"(\"id\" NUMBER(10,0) NOT NULL ,\"document_id\" NUMBER(10,0),\"dim1_xbrl_elem_id\" VARCHAR2(128),\"dim2_xbrl_elem_id\" VARCHAR2(128),\"dim3_xbrl_elem_id\" VARCHAR2(128),\"value\" CLOB,CONSTRAINT pk_dad404a0c23e45758ed7f3d1e9 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"mapping_template_heads\"(\"id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(64),\"account_structure\" VARCHAR2(128),\"comment\" CLOB,\"creator_id\" NUMBER(10,0),\"creation_date\" TIMESTAMP,\"last_modifier_id\" NUMBER(10,0),\"modify_date\" TIMESTAMP,\"taxonomy_info_id\" NUMBER(10,0),CONSTRAINT pk_68c94c0ee2e94574b264d4e93f PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"values_gcd\"(\"parent_id\" NUMBER(19,0),\"document_id\" NUMBER(10,0),\"id\" NUMBER(19,0) NOT NULL ,\"elem_id\" NUMBER(10,0),\"value\" CLOB,\"cb_value_other\" CLOB,\"supress_warning_messages\" NUMBER(1,0) NOT NULL ,\"auto_computation_enabled\" NUMBER(1,0) NOT NULL ,\"send_account_balances\" NUMBER(1,0),\"comment\" CLOB,CONSTRAINT pk_7eae8e6875ae4bd09808ccb333 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"hypercube_ass_gross_short\"(\"id\" NUMBER(10,0) NOT NULL ,\"document_id\" NUMBER(10,0),\"dim1_xbrl_elem_id\" VARCHAR2(128),\"dim2_xbrl_elem_id\" VARCHAR2(128),\"dim3_xbrl_elem_id\" VARCHAR2(128),\"value\" CLOB,CONSTRAINT pk_480ac36ab251478982d231f809 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"accounts\"(\"amount\" FLOAT(24) NOT NULL ,\"group_id\" NUMBER(19,0),\"user_defined\" NUMBER(1,0) NOT NULL ,\"id\" NUMBER(19,0) NOT NULL ,\"balance_list_id\" NUMBER(10,0) NOT NULL ,\"number\" VARCHAR2(32) NOT NULL ,\"name\" VARCHAR2(128) NOT NULL ,\"assigned_element_id\" NUMBER(10,0),\"comment\" CLOB,\"hidden\" NUMBER(1,0) NOT NULL ,\"sort_index\" VARCHAR2(64) NOT NULL ,\"in_tray\" NUMBER(1,0) NOT NULL ,CONSTRAINT pk_f11e0f726f0a4b18bb2d792644 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"balance_lists\"(\"id\" NUMBER(10,0) NOT NULL ,\"document_id\" NUMBER(10,0),\"comment\" CLOB,\"name\" VARCHAR2(40),\"imported_from_id\" NUMBER(10,0),\"import_date\" TIMESTAMP,\"source\" CLOB,CONSTRAINT pk_1c9de6fc7c5344609155bb936c PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"split_account_groups\"(\"id\" NUMBER(19,0) NOT NULL ,\"balance_list_id\" NUMBER(10,0) NOT NULL ,\"account_id\" NUMBER(19,0) NOT NULL ,\"comment\" CLOB,\"value_input_mode\" NUMBER,CONSTRAINT pk_6d3e717352404006ac0eef7764 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"values_gcd_company\"(\"parent_id\" NUMBER(19,0),\"company_id\" NUMBER(10,0),\"id\" NUMBER(19,0) NOT NULL ,\"elem_id\" NUMBER(10,0),\"value\" CLOB,\"cb_value_other\" CLOB,\"supress_warning_messages\" NUMBER(1,0) NOT NULL ,\"auto_computation_enabled\" NUMBER(1,0) NOT NULL ,\"send_account_balances\" NUMBER(1,0),\"comment\" CLOB,CONSTRAINT pk_03cba27dad904c5c866e73bcf9 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"account_groups\"(\"number\" VARCHAR2(32) NOT NULL ,\"name\" VARCHAR2(128) NOT NULL ,\"id\" NUMBER(19,0) NOT NULL ,\"balance_list_id\" NUMBER(10,0) NOT NULL ,\"assigned_element_id\" NUMBER(10,0),\"comment\" CLOB,\"hidden\" NUMBER(1,0) NOT NULL ,\"sort_index\" VARCHAR2(64) NOT NULL ,\"in_tray\" NUMBER(1,0) NOT NULL ,CONSTRAINT pk_5a90059b9a7e416394ef660cf8 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"users\"(\"id\" NUMBER(10,0) NOT NULL ,\"username\" VARCHAR2(128),\"fullname\" VARCHAR2(256),\"salt\" VARCHAR2(64),\"password\" CLOB NOT NULL ,\"is_active\" NUMBER(1,0),\"is_admin\" NUMBER(1,0),\"is_deleted\" NUMBER(1,0),\"is_domain_user\" NUMBER(1,0),\"domain\" VARCHAR2(64),CONSTRAINT pk_1ec170ee805c4e93b921cde058 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"info\"(\"id\" NUMBER(10,0) NOT NULL ,\"key\" VARCHAR2(64) NOT NULL ,\"value\" VARCHAR2(64),CONSTRAINT pk_434b64110bfc4b2f9167159f73 PRIMARY KEY(\"id\"),CONSTRAINT uk_cd087f494063477ab06b17857b UNIQUE(\"key\")) INITRANS 5 ",
                    "CREATE TABLE \"log_admin\"(\"id\" NUMBER(19,0) NOT NULL ,\"timestamp\" TIMESTAMP,\"user_id\" NUMBER(10,0) NOT NULL ,\"content_type\" NUMBER NOT NULL ,\"action_type\" NUMBER NOT NULL ,\"reference_id\" NUMBER(19,0) NOT NULL ,\"old_value\" CLOB,\"new_value\" CLOB,\"info\" CLOB,CONSTRAINT pk_3ca8bfe5468a477a952c7c9ec6 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"log_send\"(\"id\" NUMBER(10,0) NOT NULL ,\"timestamp\" TIMESTAMP,\"user_id\" NUMBER(10,0) NOT NULL ,\"document_id\" NUMBER(19,0) NOT NULL ,\"send_data\" CLOB NOT NULL ,\"report_info\" CLOB NOT NULL ,\"send_error\" NUMBER NOT NULL ,\"result_message\" CLOB NOT NULL ,CONSTRAINT pk_db963b76b5df4aa3b515544738 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"log_report_1_value_change\"(\"id\" NUMBER(19,0) NOT NULL ,\"timestamp\" TIMESTAMP,\"user_id\" NUMBER(10,0) NOT NULL ,\"taxonomy_id\" NUMBER(10,0) NOT NULL ,\"old_value\" CLOB,\"new_value\" CLOB,\"reference_id\" NUMBER(19,0) NOT NULL ,\"value_type\" NUMBER NOT NULL ,CONSTRAINT pk_6717ed5a36f54c9289e9c0b073 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"log_report_1\"(\"id\" NUMBER(19,0) NOT NULL ,\"timestamp\" TIMESTAMP,\"user_id\" NUMBER(10,0) NOT NULL ,\"content_type\" NUMBER NOT NULL ,\"action_type\" NUMBER NOT NULL ,\"reference_id\" NUMBER(19,0) NOT NULL ,\"old_value\" CLOB,\"new_value\" CLOB,\"info\" CLOB,CONSTRAINT pk_9050837be4374df5b93382c521 PRIMARY KEY(\"id\")) INITRANS 5 ",
                };
                AddVersion("1.5.0", version150_mysql, version150_sqlite, version150_sqlserver,version150_oracle);
                AddVersion("1.5.9", version150_mysql, version150_sqlite, version150_sqlserver, version150_oracle);
                #endregion
                
            }

            {

                #region version 1.6.0 

                #region indices
                List<IndexInformation> indexInfo = new List<IndexInformation>();

                #region indices
                IndexInformation info = new IndexInformation();
                info.tableName = "values_gaap";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercubes";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_hypercubes1",
                                                                        new List<string> {"document_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_hypercubes2",
                                                                        new List<string>
                                                                        {"taxonomy_id", "cube_element_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercubes_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "templates_account_profile";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "federal_gazette_info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "GlobalOptions";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_dimensions";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_hypercubes",
                                                                        new List<string>
                                                                        {"taxonomy_id", "cube_element_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gaap_fg";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_fg_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gaap_fg_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "companies";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "mapping_template_lines";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_mapping_template_lines_template_id",
                                                                        new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "templates_balance_list";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "templates_account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "templates_account_splittings";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "templates_balance_list";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_report_1";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "balance_lists";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_balance_lists_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_account_groups_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "splitted_accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_splitted_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "reconciliation_reflist_items";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_reconciliation_reflist_items",
                                                                        new List<string> {"reconciliation_reflist_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_reconciliation_refelist_items_elementid",
                                                                        new List<string> {"element_id"}));
                info.indices.Add(
                    new KeyValuePair<string, List<string>>(
                        "fki_reconciliation_reflist_items_reconciliation_reflist_id",
                        new List<string> {"reconciliation_reflist_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "reconciliations";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_reconciliations",
                                                                        new List<string> {"document_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_reconciliations_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hyper_cube_dimension_ordinals";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_hc_dimension_ordinals",
                                                                        new List<string>
                                                                        {"taxonomy_id", "cube_element_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "users";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "systems";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "values_gcd_company";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_parent_id",
                                                                        new List<string> {"parent_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_values_gcd_company_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "documents";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "mapping_template_element_info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>(
                                     "fki_mapping_template_element_info_template_id", new List<string> {"template_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "reconciliation_transactions";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_reconciliation_trans",
                                                                        new List<string> {"document_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_reconciliation_transactions_document_id",
                                                                        new List<string> {"document_id"}));
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_reconciliation_transactions_reconciliation_id",
                                                           new List<string> {"reconciliation_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "taxonomy_ids";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_taxonomy_ids__taxonomy_id",
                                                                        new List<string> {"taxonomy_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "split_account_groups";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_split_account_groups_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_items";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_items_hypercube_id",
                                                                        new List<string> {"hypercube_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_items_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_import_templates";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_hypercube_import_templates_taxid_cubeid",
                                                                        new List<string>
                                                                        {"taxonomy_id", "cube_element_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_hypercube_import_templates_creation_user",
                                                                        new List<string> {"creation_user"}));
                info.indices.Add(
                    new KeyValuePair<string, List<string>>("fki_hypercube_import_templates_modification_user",
                                                           new List<string> {"modification_user"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "mapping_template_heads";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "roles";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "rights";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("rights_content",
                                                                        new List<string>
                                                                        {"role_id", "content_type", "ref_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "user_roles";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_user_roles_1",
                                                                        new List<string> {"user_id", "role_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "reconciliation_reflists";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_reconciliation_reflists_document_user",
                                                                        new List<string> {"document_id", "user_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_reconciliation_reflists_document_id",
                                                                        new List<string> {"document_id"}));
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_reconciliation_reflists_user_id",
                                                                        new List<string> {"user_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_send";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "hypercube_dimension_keys";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx_hypercube_dimension_keys_taxid_cubeid",
                                                                        new List<string>
                                                                        {"taxonomy_id", "cube_element_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_admin";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "financial_years";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_financial_years_company_id",
                                                                        new List<string> {"company_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "virtual_accounts";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_virtual_accounts_balance_list_id",
                                                                        new List<string> {"balance_list_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "balance_lists";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("fki_balance_lists_document_id",
                                                                        new List<string> {"document_id"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "taxonomy_info";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                info.indices.Add(new KeyValuePair<string, List<string>>("idx__taxonomy_info__name",
                                                                        new List<string> {"name"}));
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "log_report_1_value_change";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "report_federal_gazette";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);

                info = new IndexInformation();
                info.tableName = "upgrade_information";
                info.indices = new List<KeyValuePair<string, List<string>>>();
                indexInfo.Add(info);
                #endregion

                #endregion


                var version160_sqlite = new TableInformation();
                version160_sqlite.tableCreation = new List<string> {
                    "CREATE TABLE `documents`(`id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`company_id` INTEGER,`system_id` INTEGER,`financial_year_id` INTEGER,`owner_id` INTEGER,`gcd_taxonomy_info_id` INTEGER,`main_taxonomy_info_id` INTEGER,`creation_date` TEXT,`reconciliation_mode` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `global_search_history`(`id` INTEGER NOT NULL,`user_id` INTEGER NOT NULL,`history_list` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_items`(`hypercube_id` INTEGER,`document_id` INTEGER,`dimension_key_id` INTEGER NOT NULL,`value` TEXT,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `audit_correction_transaction`(`document_id` INTEGER,`head_id` INTEGER,`transaction_type` INTEGER,`element_id` INTEGER,`value` NUMERIC,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `templates_balance_list`(`Id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`creator_id` INTEGER NOT NULL,`timestamp` TEXT NOT NULL,`template` TEXT NOT NULL,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `audit_correction_set`(`set_id` INTEGER NOT NULL,`correction_id` INTEGER NOT NULL,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `mapping_template_lines`(`id` INTEGER NOT NULL,`template_id` INTEGER,`account_number` TEXT,`account_name` TEXT,`is_account_of_exchange` INTEGER,`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_import_templates`(`template_id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,`taxonomy_id` INTEGER,`cube_element_id` INTEGER,`template_name` TEXT,`xml_assignment` TEXT,`comment` TEXT,`creation_user` INTEGER NOT NULL,`cration_date` TEXT NOT NULL,`modification_user` INTEGER,`inverse_assignment` INTEGER NOT NULL,`modification_date` TEXT,`encoding` TEXT NOT NULL,`seperator` TEXT NOT NULL,`delimiter` TEXT NOT NULL,`template_csv` TEXT NOT NULL,`dimension_order` TEXT)",
                    "CREATE TABLE `templates_account_groups`(`Id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`creator_id` INTEGER NOT NULL,`timestamp` TEXT NOT NULL,`template` TEXT NOT NULL,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `templates_account_splittings`(`Id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`creator_id` INTEGER NOT NULL,`timestamp` TEXT NOT NULL,`template` TEXT NOT NULL,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `templates_balance_list`(`Id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`creator_id` INTEGER NOT NULL,`timestamp` TEXT NOT NULL,`template` TEXT NOT NULL,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `templates_account_profile`(`Id` INTEGER NOT NULL,`name` TEXT NOT NULL,`comment` TEXT,`creator_id` INTEGER NOT NULL,`timestamp` TEXT NOT NULL,`template` TEXT NOT NULL,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `log_report_1`(`content_type` INTEGER NOT NULL,`action_type` INTEGER NOT NULL,`reference_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`info` TEXT,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `users`(`id` INTEGER NOT NULL,`username` TEXT,`fullname` TEXT,`salt` TEXT,`password` TEXT NOT NULL,`is_active` INTEGER,`is_admin` INTEGER,`last_login` TEXT,`is_companyadmin` INTEGER,`assigned_companies` TEXT,`is_deleted` INTEGER,`is_domain_user` INTEGER,`domain` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `taxonomy_ids`(`id` INTEGER NOT NULL,`taxonomy_id` INTEGER,`xbrl_element_id` TEXT NOT NULL,`number` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `mapping_template_element_info`(`id` INTEGER NOT NULL,`template_id` INTEGER,`element_id` TEXT,`auto_compute_enabled` INTEGER,`supress_warning_messages` INTEGER NOT NULL,`send_account_balances` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `taxonomy_info`(`id` INTEGER NOT NULL,`name` TEXT NOT NULL,`path` TEXT NOT NULL,`filename` TEXT NOT NULL,`type` INTEGER NOT NULL,`version` TEXT NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `info`(`id` INTEGER NOT NULL,`key` TEXT NOT NULL,`value` TEXT,PRIMARY KEY(`id`),UNIQUE (`key`))",
                    "CREATE TABLE `values_gcd_company`(`parent_id` INTEGER,`company_id` INTEGER,`id` INTEGER NOT NULL,`elem_id` INTEGER,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `companies`(`Id` INTEGER NOT NULL,`name` TEXT,`owner_id` INTEGER,`taxonomy_info_id` INTEGER,PRIMARY KEY(`Id`))",
                    "CREATE TABLE `reconciliation_reflist_items`(`reconciliation_reflist_id` INTEGER,`element_id` INTEGER,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `GlobalOptions`(`id` INTEGER NOT NULL,`user_id` INTEGER,`option` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_report_1_value_change`(`taxonomy_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`reference_id` INTEGER NOT NULL,`value_type` INTEGER NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `accounts`(`amount` NUMERIC NOT NULL,`group_id` INTEGER,`user_defined` INTEGER NOT NULL,`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,`hidden` INTEGER NOT NULL,`sort_index` TEXT NOT NULL,`in_tray` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `split_account_groups`(`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`account_id` INTEGER NOT NULL,`comment` TEXT,`value_input_mode` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `splitted_accounts`(`amount` NUMERIC NOT NULL,`amount_percent` NUMERIC,`split_group_id` INTEGER,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,`hidden` INTEGER NOT NULL,`sort_index` TEXT NOT NULL,`in_tray` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `account_groups`(`number` TEXT NOT NULL,`name` TEXT NOT NULL,`id` INTEGER NOT NULL,`balance_list_id` INTEGER NOT NULL,`assigned_element_id` INTEGER,`comment` TEXT,`hidden` INTEGER NOT NULL,`sort_index` TEXT NOT NULL,`in_tray` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `upgrade_information`(`id` INTEGER NOT NULL,`upgrade_available_from` TEXT NOT NULL,`version_string` TEXT NOT NULL,`ordinal` NUMERIC NOT NULL,`resource_name` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `reconciliation_transactions`(`document_id` INTEGER,`reconciliation_id` INTEGER,`transaction_type` INTEGER,`element_id` INTEGER,`value` NUMERIC,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `balance_lists`(`id` INTEGER NOT NULL,`document_id` INTEGER,`comment` TEXT,`name` TEXT,`imported_from_id` INTEGER,`import_date` TEXT,`source` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_admin`(`content_type` INTEGER NOT NULL,`action_type` INTEGER NOT NULL,`reference_id` INTEGER NOT NULL,`old_value` TEXT,`new_value` TEXT,`info` TEXT,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_dimension_keys`(`taxonomy_id` INTEGER,`cube_element_id` INTEGER,`primary_dimension_id` INTEGER NOT NULL,`dimension_id` INTEGER NOT NULL,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercubes`(`document_id` INTEGER,`taxonomy_id` INTEGER,`cube_element_id` INTEGER,`comment` TEXT,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `mapping_template_heads`(`id` INTEGER NOT NULL,`name` TEXT,`account_structure` TEXT,`comment` TEXT,`creator_id` INTEGER,`creation_date` TEXT,`last_modifier_id` INTEGER,`modify_date` TEXT,`taxonomy_info_id` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `account_reflists`(`document_id` INTEGER NOT NULL,`user_id` INTEGER NOT NULL,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hypercube_dimensions`(`taxonomy_id` INTEGER,`cube_element_id` INTEGER,`explicit_member_element_id_1` INTEGER,`explicit_member_element_id_2` INTEGER,`explicit_member_element_id_3` INTEGER,`explicit_member_element_id_4` INTEGER,`explicit_member_element_id_5` INTEGER,`explicit_member_element_id_6` INTEGER,`explicit_member_element_id_7` INTEGER,`explicit_member_element_id_8` INTEGER,`explicit_member_element_id_9` INTEGER,`explicit_member_element_id_10` INTEGER,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `report_federal_gazette`(`id` INTEGER NOT NULL,`name` TEXT,`comment` TEXT,`company_id` INTEGER NOT NULL,`document_id` INTEGER NOT NULL,`owner_id` INTEGER NOT NULL,`gaap_taxonomy_info_id` TEXT NOT NULL,`creation_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `systems`(`id` INTEGER NOT NULL,`name` TEXT,`comment` TEXT,PRIMARY KEY(`id`),UNIQUE (`name`))",
                    "CREATE TABLE `roles`(`id` INTEGER NOT NULL,`name` TEXT,`comment` TEXT,`user_id` INTEGER,PRIMARY KEY(`id`))",
                    "CREATE TABLE `rights`(`id` INTEGER NOT NULL,`role_id` INTEGER NOT NULL,`content_type` INTEGER NOT NULL,`ref_id` INTEGER NOT NULL,`rights` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `user_roles`(`id` INTEGER NOT NULL,`user_id` INTEGER NOT NULL,`role_id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gaap_fg`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`elem_id` INTEGER,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `federal_gazette_info`(`id` INTEGER NOT NULL,`notes` INTEGER,`assets` INTEGER,`balance` INTEGER,`income_use` INTEGER,`income_statement` INTEGER,`management_report` INTEGER,`start_date` TEXT,`end_date` TEXT,`exemption_decision` INTEGER,`exemption_note` INTEGER,`loss_assumption` INTEGER,`oder_type` INTEGER,`publication_area` INTEGER,`publication_category` INTEGER,`publication_type` INTEGER,`publication_description` INTEGER,`company_type` INTEGER,`company_size` TEXT,`company_name` TEXT,`company_sign` TEXT,`registration_type` TEXT,`legal_form` TEXT,`domicile` TEXT,`company_registration_type` TEXT,`registeration_court` TEXT,`registration_number` TEXT,`company_id` TEXT,`client_id` TEXT,`copmany_name_address` TEXT,`company_devision` TEXT,`company_street` TEXT,`company_postcode` TEXT,`company_city` TEXT,`company_State` TEXT,`salutation` TEXT,`title` TEXT,`firstname` TEXT,`lastname` TEXT,`telephone` TEXT,`cell` TEXT,`email` TEXT,`data_sent` INTEGER,`account_deleted` INTEGER,`ticket_id` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `hyper_cube_dimension_ordinals`(`taxonomy_id` INTEGER,`cube_element_id` INTEGER,`dimension_element_id` INTEGER NOT NULL,`ordinal` INTEGER NOT NULL,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `virtual_accounts`(`balance_list_id` INTEGER NOT NULL,`assigned_element_id` INTEGER,`taxonomy_source_position` TEXT,`amount` NUMERIC NOT NULL,`hidden` INTEGER NOT NULL,`number` TEXT NOT NULL,`name` TEXT NOT NULL,`group_id` INTEGER,`user_defined` INTEGER NOT NULL,`id` INTEGER NOT NULL,`sort_index` TEXT NOT NULL,`in_tray` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `balance_lists`(`id` INTEGER NOT NULL,`document_id` INTEGER,`comment` TEXT,`name` TEXT,`imported_from_id` INTEGER,`import_date` TEXT,`source` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gcd`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`elem_id` INTEGER,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `financial_years`(`id` INTEGER NOT NULL,`company_id` INTEGER NOT NULL,`fyear` INTEGER,`is_enabled` INTEGER,`fiscal_year_begin` TEXT,`fiscal_year_end` TEXT,`bal_sheet_closing_date` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `log_send`(`document_id` INTEGER NOT NULL,`send_data` TEXT NOT NULL,`report_info` TEXT NOT NULL,`send_error` INTEGER NOT NULL,`result_message` TEXT NOT NULL,`timestamp` TEXT,`user_id` INTEGER NOT NULL,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `reconciliation_reflists`(`document_id` INTEGER NOT NULL,`user_id` INTEGER NOT NULL,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `reconciliations`(`document_id` INTEGER,`type` INTEGER,`transfer_kind` INTEGER,`name` TEXT,`comment` TEXT,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `audit_correction`(`document_id` INTEGER,`name` TEXT,`comment` TEXT,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                    "CREATE TABLE `values_gaap`(`parent_id` INTEGER,`document_id` INTEGER,`id` INTEGER NOT NULL,`elem_id` INTEGER,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` INTEGER NOT NULL,`auto_computation_enabled` INTEGER NOT NULL,`send_account_balances` INTEGER,`comment` TEXT,PRIMARY KEY(`id`))",
                    "CREATE TABLE `account_reflist_items`(`account_reflist_id` INTEGER,`account_type` INTEGER,`account_id` INTEGER,`id` INTEGER NOT NULL,PRIMARY KEY(`id`))",
                };

                var version160_mysql = new TableInformation();

                version160_mysql.tableCreation = new List<string> {
                    "CREATE TABLE `documents`(`id` INT NOT NULL,`name` VARCHAR(256) NOT NULL,`comment` TEXT,`company_id` INT,`system_id` INT,`financial_year_id` INT,`owner_id` INT,`gcd_taxonomy_info_id` INT,`main_taxonomy_info_id` INT,`creation_date` DATETIME,`reconciliation_mode` INT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `global_search_history`(`id` INT NOT NULL,`user_id` INT NOT NULL,`history_list` TEXT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_items`(`hypercube_id` INT,`document_id` INT,`dimension_key_id` BIGINT NOT NULL,`value` TEXT,`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `audit_correction_transaction`(`document_id` INT,`head_id` BIGINT,`transaction_type` INT,`element_id` INT,`value` decimal(20,2),`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `templates_balance_list`(`Id` INT NOT NULL,`name` VARCHAR(64) NOT NULL,`comment` TEXT,`creator_id` INT NOT NULL,`timestamp` DATETIME NOT NULL,`template` LONGTEXT NOT NULL,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `audit_correction_set`(`set_id` BIGINT NOT NULL,`correction_id` BIGINT NOT NULL,`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `mapping_template_lines`(`id` INT NOT NULL,`template_id` INT,`account_number` VARCHAR(32),`account_name` TEXT,`is_account_of_exchange` TINYINT(1),`debit_element_id` TEXT,`credit_element_id` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_import_templates`(`template_id` INT NOT NULL AUTO_INCREMENT,`taxonomy_id` INT,`cube_element_id` INT,`template_name` VARCHAR(64),`xml_assignment` TEXT,`comment` TEXT,`creation_user` INT NOT NULL,`cration_date` DATETIME NOT NULL,`modification_user` INT,`inverse_assignment` TINYINT(1) NOT NULL,`modification_date` DATETIME,`encoding` VARCHAR(64) NOT NULL,`seperator` VARCHAR(64) NOT NULL,`delimiter` VARCHAR(64) NOT NULL,`template_csv` VARCHAR(200) NOT NULL,`dimension_order` VARCHAR(64),PRIMARY KEY(`template_id`)) ENGINE = InnoDb",
                    "CREATE TABLE `templates_account_groups`(`Id` INT NOT NULL,`name` VARCHAR(64) NOT NULL,`comment` TEXT,`creator_id` INT NOT NULL,`timestamp` DATETIME NOT NULL,`template` LONGTEXT NOT NULL,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `templates_account_splittings`(`Id` INT NOT NULL,`name` VARCHAR(64) NOT NULL,`comment` TEXT,`creator_id` INT NOT NULL,`timestamp` DATETIME NOT NULL,`template` LONGTEXT NOT NULL,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `templates_balance_list`(`Id` INT NOT NULL,`name` VARCHAR(64) NOT NULL,`comment` TEXT,`creator_id` INT NOT NULL,`timestamp` DATETIME NOT NULL,`template` LONGTEXT NOT NULL,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `templates_account_profile`(`Id` INT NOT NULL,`name` VARCHAR(64) NOT NULL,`comment` TEXT,`creator_id` INT NOT NULL,`timestamp` DATETIME NOT NULL,`template` LONGTEXT NOT NULL,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_report_1`(`content_type` INT NOT NULL,`action_type` INT NOT NULL,`reference_id` BIGINT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`info` TEXT,`timestamp` DATETIME,`user_id` INT NOT NULL,`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `users`(`id` INT NOT NULL,`username` VARCHAR(128),`fullname` VARCHAR(256),`salt` VARCHAR(64),`password` TEXT NOT NULL,`is_active` TINYINT(1),`is_admin` TINYINT(1),`last_login` DATETIME,`is_companyadmin` TINYINT(1),`assigned_companies` TEXT,`is_deleted` TINYINT(1),`is_domain_user` TINYINT(1),`domain` VARCHAR(64),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `taxonomy_ids`(`id` INT NOT NULL,`taxonomy_id` INT,`xbrl_element_id` TEXT NOT NULL,`number` VARCHAR(10) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `mapping_template_element_info`(`id` BIGINT NOT NULL,`template_id` INT,`element_id` TEXT,`auto_compute_enabled` TINYINT(1),`supress_warning_messages` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `taxonomy_info`(`id` INT NOT NULL,`name` VARCHAR(128) NOT NULL,`path` TEXT NOT NULL,`filename` TEXT NOT NULL,`type` INT NOT NULL,`version` VARCHAR(64) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `info`(`id` INT NOT NULL,`key` VARCHAR(64) NOT NULL,`value` VARCHAR(64),PRIMARY KEY(`id`),UNIQUE KEY uk_d82e7ae0c1f641f6bc9ce0c676139ff5(`key`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd_company`(`parent_id` BIGINT,`company_id` INT,`id` BIGINT NOT NULL,`elem_id` INT,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `companies`(`Id` INT NOT NULL,`name` VARCHAR(256),`owner_id` INT,`taxonomy_info_id` INT,PRIMARY KEY(`Id`)) ENGINE = InnoDb",
                    "CREATE TABLE `reconciliation_reflist_items`(`reconciliation_reflist_id` BIGINT,`element_id` INT,`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `GlobalOptions`(`id` INT NOT NULL,`user_id` INT,`option` LONGTEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_report_1_value_change`(`taxonomy_id` INT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`reference_id` BIGINT NOT NULL,`value_type` INT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `accounts`(`amount` decimal(20,2) NOT NULL,`group_id` BIGINT,`user_defined` TINYINT(1) NOT NULL,`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`assigned_element_id` INT,`comment` TEXT,`hidden` TINYINT(1) NOT NULL,`sort_index` VARCHAR(64) NOT NULL,`in_tray` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `split_account_groups`(`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`account_id` BIGINT NOT NULL,`comment` TEXT,`value_input_mode` INT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `splitted_accounts`(`amount` decimal(20,2) NOT NULL,`amount_percent` decimal(20,2),`split_group_id` BIGINT,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`assigned_element_id` INT,`comment` TEXT,`hidden` TINYINT(1) NOT NULL,`sort_index` VARCHAR(64) NOT NULL,`in_tray` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `account_groups`(`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`id` BIGINT NOT NULL,`balance_list_id` INT NOT NULL,`assigned_element_id` INT,`comment` TEXT,`hidden` TINYINT(1) NOT NULL,`sort_index` VARCHAR(64) NOT NULL,`in_tray` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `upgrade_information`(`id` INT NOT NULL,`upgrade_available_from` DATETIME NOT NULL,`version_string` VARCHAR(64) NOT NULL,`ordinal` decimal(20,2) NOT NULL,`resource_name` VARCHAR(64),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `reconciliation_transactions`(`document_id` INT,`reconciliation_id` BIGINT,`transaction_type` INT,`element_id` INT,`value` decimal(20,2),`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `balance_lists`(`id` INT NOT NULL,`document_id` INT,`comment` TEXT,`name` VARCHAR(40),`imported_from_id` INT,`import_date` DATETIME,`source` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_admin`(`content_type` INT NOT NULL,`action_type` INT NOT NULL,`reference_id` BIGINT NOT NULL,`old_value` LONGTEXT,`new_value` LONGTEXT,`info` TEXT,`timestamp` DATETIME,`user_id` INT NOT NULL,`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_dimension_keys`(`taxonomy_id` INT,`cube_element_id` INT,`primary_dimension_id` INT NOT NULL,`dimension_id` BIGINT NOT NULL,`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercubes`(`document_id` INT,`taxonomy_id` INT,`cube_element_id` INT,`comment` TEXT,`id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `mapping_template_heads`(`id` INT NOT NULL,`name` VARCHAR(64),`account_structure` VARCHAR(128),`comment` TEXT,`creator_id` INT,`creation_date` DATETIME,`last_modifier_id` INT,`modify_date` DATETIME,`taxonomy_info_id` INT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `account_reflists`(`document_id` INT NOT NULL,`user_id` INT NOT NULL,`id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hypercube_dimensions`(`taxonomy_id` INT,`cube_element_id` INT,`explicit_member_element_id_1` INT,`explicit_member_element_id_2` INT,`explicit_member_element_id_3` INT,`explicit_member_element_id_4` INT,`explicit_member_element_id_5` INT,`explicit_member_element_id_6` INT,`explicit_member_element_id_7` INT,`explicit_member_element_id_8` INT,`explicit_member_element_id_9` INT,`explicit_member_element_id_10` INT,`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `report_federal_gazette`(`id` INT NOT NULL,`name` VARCHAR(256),`comment` TEXT,`company_id` INT NOT NULL,`document_id` INT NOT NULL,`owner_id` INT NOT NULL,`gaap_taxonomy_info_id` VARCHAR(64) NOT NULL,`creation_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `systems`(`id` INT NOT NULL,`name` VARCHAR(128),`comment` VARCHAR(256),PRIMARY KEY(`id`),UNIQUE KEY uk_c8ac5b0be564408680ff0eb323218f7e(`name`)) ENGINE = InnoDb",
                    "CREATE TABLE `roles`(`id` INT NOT NULL,`name` VARCHAR(256),`comment` TEXT,`user_id` INT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `rights`(`id` INT NOT NULL,`role_id` INT NOT NULL,`content_type` INT NOT NULL,`ref_id` INT NOT NULL,`rights` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `user_roles`(`id` INT NOT NULL,`user_id` INT NOT NULL,`role_id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gaap_fg`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`elem_id` INT,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `federal_gazette_info`(`id` INT NOT NULL,`notes` TINYINT(1),`assets` TINYINT(1),`balance` TINYINT(1),`income_use` TINYINT(1),`income_statement` TINYINT(1),`management_report` TINYINT(1),`start_date` DATETIME,`end_date` DATETIME,`exemption_decision` TINYINT(1),`exemption_note` TINYINT(1),`loss_assumption` TINYINT(1),`oder_type` INT,`publication_area` INT,`publication_category` INT,`publication_type` INT,`publication_description` INT,`company_type` INT,`company_size` VARCHAR(64),`company_name` TEXT,`company_sign` VARCHAR(64),`registration_type` VARCHAR(64),`legal_form` VARCHAR(64),`domicile` VARCHAR(256),`company_registration_type` VARCHAR(256),`registeration_court` VARCHAR(64),`registration_number` VARCHAR(256),`company_id` VARCHAR(256),`client_id` VARCHAR(64),`copmany_name_address` TEXT,`company_devision` TEXT,`company_street` VARCHAR(256),`company_postcode` VARCHAR(64),`company_city` VARCHAR(256),`company_State` VARCHAR(256),`salutation` VARCHAR(64),`title` VARCHAR(64),`firstname` VARCHAR(256),`lastname` VARCHAR(256),`telephone` VARCHAR(64),`cell` VARCHAR(64),`email` VARCHAR(256),`data_sent` TINYINT(1),`account_deleted` TINYINT(1),`ticket_id` VARCHAR(64),PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `hyper_cube_dimension_ordinals`(`taxonomy_id` INT,`cube_element_id` INT,`dimension_element_id` INT NOT NULL,`ordinal` INT NOT NULL,`id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `virtual_accounts`(`balance_list_id` INT NOT NULL,`assigned_element_id` INT,`taxonomy_source_position` VARCHAR(64),`amount` decimal(20,2) NOT NULL,`hidden` TINYINT(1) NOT NULL,`number` VARCHAR(32) NOT NULL,`name` VARCHAR(128) NOT NULL,`group_id` BIGINT,`user_defined` TINYINT(1) NOT NULL,`id` BIGINT NOT NULL,`sort_index` VARCHAR(64) NOT NULL,`in_tray` TINYINT(1) NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `balance_lists`(`id` INT NOT NULL,`document_id` INT,`comment` TEXT,`name` VARCHAR(40),`imported_from_id` INT,`import_date` DATETIME,`source` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gcd`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`elem_id` INT,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `financial_years`(`id` INT NOT NULL,`company_id` INT NOT NULL,`fyear` INT,`is_enabled` TINYINT(1),`fiscal_year_begin` DATETIME,`fiscal_year_end` DATETIME,`bal_sheet_closing_date` DATETIME,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `log_send`(`document_id` BIGINT NOT NULL,`send_data` LONGTEXT NOT NULL,`report_info` LONGTEXT NOT NULL,`send_error` INT NOT NULL,`result_message` TEXT NOT NULL,`timestamp` DATETIME,`user_id` INT NOT NULL,`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `reconciliation_reflists`(`document_id` INT NOT NULL,`user_id` INT NOT NULL,`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `reconciliations`(`document_id` INT,`type` INT,`transfer_kind` INT,`name` TEXT,`comment` TEXT,`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `audit_correction`(`document_id` INT,`name` TEXT,`comment` TEXT,`id` BIGINT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `values_gaap`(`parent_id` BIGINT,`document_id` INT,`id` BIGINT NOT NULL,`elem_id` INT,`value` TEXT,`cb_value_other` TEXT,`supress_warning_messages` TINYINT(1) NOT NULL,`auto_computation_enabled` TINYINT(1) NOT NULL,`send_account_balances` TINYINT(1),`comment` TEXT,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                    "CREATE TABLE `account_reflist_items`(`account_reflist_id` INT,`account_type` INT,`account_id` BIGINT,`id` INT NOT NULL,PRIMARY KEY(`id`)) ENGINE = InnoDb",
                };

                var version160_oracle = new TableInformation();
                version160_oracle.tableCreation = new List<string> {
                    "CREATE TABLE \"info\"(\"id\" NUMBER(10,0) NOT NULL ,\"key\" VARCHAR2(64) NOT NULL ,\"value\" VARCHAR2(64),CONSTRAINT pk_b0fd0856c5564662a535adf18d PRIMARY KEY(\"id\"),CONSTRAINT uk_8df379b2a4bd4748899c1b57b4 UNIQUE(\"key\")) INITRANS 5 ",
                    "CREATE TABLE \"split_account_groups\"(\"id\" NUMBER(19,0) NOT NULL ,\"balance_list_id\" NUMBER(10,0) NOT NULL ,\"account_id\" NUMBER(19,0) NOT NULL ,\"comment\" CLOB,\"value_input_mode\" NUMBER,CONSTRAINT pk_7a56d626cb5e4899b10a17e898 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"templates_balance_list\"(\"Id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(64) NOT NULL ,\"comment\" CLOB,\"creator_id\" NUMBER(10,0) NOT NULL ,\"timestamp\" TIMESTAMP NOT NULL ,\"template\" CLOB NOT NULL ,CONSTRAINT pk_698731eaa0ea45af9189a51b8d PRIMARY KEY(\"Id\")) INITRANS 5 ",
                    "CREATE TABLE \"templates_account_groups\"(\"Id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(64) NOT NULL ,\"comment\" CLOB,\"creator_id\" NUMBER(10,0) NOT NULL ,\"timestamp\" TIMESTAMP NOT NULL ,\"template\" CLOB NOT NULL ,CONSTRAINT pk_7f84c6b3731d4ac1be2433f5cd PRIMARY KEY(\"Id\")) INITRANS 5 ",
                    "CREATE TABLE \"templates_account_splittings\"(\"Id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(64) NOT NULL ,\"comment\" CLOB,\"creator_id\" NUMBER(10,0) NOT NULL ,\"timestamp\" TIMESTAMP NOT NULL ,\"template\" CLOB NOT NULL ,CONSTRAINT pk_40e943d434d345c5b7fc4ea9eb PRIMARY KEY(\"Id\")) INITRANS 5 ",
                    "CREATE TABLE \"templates_balance_list\"(\"Id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(64) NOT NULL ,\"comment\" CLOB,\"creator_id\" NUMBER(10,0) NOT NULL ,\"timestamp\" TIMESTAMP NOT NULL ,\"template\" CLOB NOT NULL ,CONSTRAINT pk_39079ced1d2348ff89d35008c7 PRIMARY KEY(\"Id\")) INITRANS 5 ",
                    "CREATE TABLE \"templates_account_profile\"(\"Id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(64) NOT NULL ,\"comment\" CLOB,\"creator_id\" NUMBER(10,0) NOT NULL ,\"timestamp\" TIMESTAMP NOT NULL ,\"template\" CLOB NOT NULL ,CONSTRAINT pk_da47d3b728e842fca394253069 PRIMARY KEY(\"Id\")) INITRANS 5 ",
                    "CREATE TABLE \"reconciliations\"(\"document_id\" NUMBER(10,0),\"type\" NUMBER,\"transfer_kind\" NUMBER,\"name\" CLOB,\"comment\" CLOB,\"id\" NUMBER(19,0) NOT NULL ,CONSTRAINT pk_731a2118737c485dac9c56dd47 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"log_send\"(\"document_id\" NUMBER(19,0) NOT NULL ,\"send_data\" CLOB NOT NULL ,\"report_info\" CLOB NOT NULL ,\"send_error\" NUMBER NOT NULL ,\"result_message\" CLOB NOT NULL ,\"timestamp\" TIMESTAMP,\"user_id\" NUMBER(10,0) NOT NULL ,\"id\" NUMBER(19,0) NOT NULL ,CONSTRAINT pk_5a93dde4fe1146dbb31cb5ccfe PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"mapping_template_lines\"(\"id\" NUMBER(10,0) NOT NULL ,\"template_id\" NUMBER(10,0),\"account_number\" VARCHAR2(32),\"account_name\" CLOB,\"is_account_of_exchange\" NUMBER(1,0),\"debit_element_id\" CLOB,\"credit_element_id\" CLOB,CONSTRAINT pk_d37737a11228429d9562cd5543 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"balance_lists\"(\"id\" NUMBER(10,0) NOT NULL ,\"document_id\" NUMBER(10,0),\"comment\" CLOB,\"name\" VARCHAR2(40),\"imported_from_id\" NUMBER(10,0),\"import_date\" TIMESTAMP,\"source\" CLOB,CONSTRAINT pk_9ebf2d9e829d40ac8db4c0f380 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"accounts\"(\"amount\" FLOAT(24) NOT NULL ,\"group_id\" NUMBER(19,0),\"user_defined\" NUMBER(1,0) NOT NULL ,\"id\" NUMBER(19,0) NOT NULL ,\"balance_list_id\" NUMBER(10,0) NOT NULL ,\"number\" VARCHAR2(32) NOT NULL ,\"name\" VARCHAR2(128) NOT NULL ,\"assigned_element_id\" NUMBER(10,0),\"comment\" CLOB,\"hidden\" NUMBER(1,0) NOT NULL ,\"sort_index\" VARCHAR2(64) NOT NULL ,\"in_tray\" NUMBER(1,0) NOT NULL ,CONSTRAINT pk_fc4ea13fd964478f894f56e77f PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"account_groups\"(\"number\" VARCHAR2(32) NOT NULL ,\"name\" VARCHAR2(128) NOT NULL ,\"id\" NUMBER(19,0) NOT NULL ,\"balance_list_id\" NUMBER(10,0) NOT NULL ,\"assigned_element_id\" NUMBER(10,0),\"comment\" CLOB,\"hidden\" NUMBER(1,0) NOT NULL ,\"sort_index\" VARCHAR2(64) NOT NULL ,\"in_tray\" NUMBER(1,0) NOT NULL ,CONSTRAINT pk_554635fb2bf34383b23a63dd36 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"splitted_accounts\"(\"amount\" FLOAT(24) NOT NULL ,\"amount_percent\" FLOAT(24),\"split_group_id\" NUMBER(19,0),\"number\" VARCHAR2(32) NOT NULL ,\"name\" VARCHAR2(128) NOT NULL ,\"id\" NUMBER(19,0) NOT NULL ,\"balance_list_id\" NUMBER(10,0) NOT NULL ,\"assigned_element_id\" NUMBER(10,0),\"comment\" CLOB,\"hidden\" NUMBER(1,0) NOT NULL ,\"sort_index\" VARCHAR2(64) NOT NULL ,\"in_tray\" NUMBER(1,0) NOT NULL ,CONSTRAINT pk_f429e55db50540bd9cf8a53f69 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"account_reflist_items\"(\"account_reflist_id\" NUMBER(10,0),\"account_type\" NUMBER(10,0),\"account_id\" NUMBER(19,0),\"id\" NUMBER(10,0) NOT NULL ,CONSTRAINT pk_e16a69e2fce74c9f8778d6d15d PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"hypercube_dimension_keys\"(\"taxonomy_id\" NUMBER(10,0),\"cube_element_id\" NUMBER(10,0),\"primary_dimension_id\" NUMBER(10,0) NOT NULL ,\"dimension_id\" NUMBER(19,0) NOT NULL ,\"id\" NUMBER(19,0) NOT NULL ,CONSTRAINT pk_268db919ee5f4de0af19db0554 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"taxonomy_ids\"(\"id\" NUMBER(10,0) NOT NULL ,\"taxonomy_id\" NUMBER(10,0),\"xbrl_element_id\" CLOB NOT NULL ,\"number\" VARCHAR2(10) NOT NULL ,CONSTRAINT pk_cc9f675270794f1ab000dde0a3 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"mapping_template_element_info\"(\"id\" NUMBER(19,0) NOT NULL ,\"template_id\" NUMBER(10,0),\"element_id\" CLOB,\"auto_compute_enabled\" NUMBER(1,0),\"supress_warning_messages\" NUMBER(1,0) NOT NULL ,\"send_account_balances\" NUMBER(1,0) NOT NULL ,CONSTRAINT pk_af3727bede614f4b8befcbeb4a PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"log_admin\"(\"content_type\" NUMBER NOT NULL ,\"action_type\" NUMBER NOT NULL ,\"reference_id\" NUMBER(19,0) NOT NULL ,\"old_value\" CLOB,\"new_value\" CLOB,\"info\" CLOB,\"timestamp\" TIMESTAMP,\"user_id\" NUMBER(10,0) NOT NULL ,\"id\" NUMBER(19,0) NOT NULL ,CONSTRAINT pk_093624081eaa49dd9c86af1abc PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"documents\"(\"id\" NUMBER(10,0) NOT NULL, \"reconciliation_mode\" NUMBER,\"name\" VARCHAR2(256) NOT NULL ,\"comment\" CLOB,\"company_id\" NUMBER(10,0),\"system_id\" NUMBER(10,0),\"financial_year_id\" NUMBER(10,0),\"owner_id\" NUMBER(10,0),\"gcd_taxonomy_info_id\" NUMBER(10,0),\"main_taxonomy_info_id\" NUMBER(10,0),\"creation_date\" TIMESTAMP,CONSTRAINT pk_76a7fc7d4b5247afa7f024e49a PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"roles\"(\"id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(256),\"comment\" CLOB,\"user_id\" NUMBER(10,0),CONSTRAINT pk_7eb62e0a81ee405aa6c537c7d7 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"rights\"(\"id\" NUMBER(10,0) NOT NULL ,\"role_id\" NUMBER(10,0) NOT NULL ,\"content_type\" NUMBER NOT NULL ,\"ref_id\" NUMBER(10,0) NOT NULL ,\"rights\" NUMBER(10,0) NOT NULL ,CONSTRAINT pk_c9a70b93ad3f4ad88a2216c313 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"user_roles\"(\"id\" NUMBER(10,0) NOT NULL ,\"user_id\" NUMBER(10,0) NOT NULL ,\"role_id\" NUMBER(10,0) NOT NULL ,CONSTRAINT pk_ed1fd296625a4b5398a4ad138a PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"systems\"(\"id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(128),\"comment\" VARCHAR2(256),CONSTRAINT pk_0dd3da5903d444eeb56b05032b PRIMARY KEY(\"id\"),CONSTRAINT uk_d69b1548c53844caa1fed226eb UNIQUE(\"name\")) INITRANS 5 ",
                    "CREATE TABLE \"financial_years\"(\"id\" NUMBER(10,0) NOT NULL ,\"company_id\" NUMBER(10,0) NOT NULL ,\"fyear\" NUMBER(10,0),\"is_enabled\" NUMBER(1,0),\"fiscal_year_begin\" TIMESTAMP,\"fiscal_year_end\" TIMESTAMP,\"bal_sheet_closing_date\" TIMESTAMP,CONSTRAINT pk_c907d8330caf446e8560f66e46 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"account_reflists\"(\"document_id\" NUMBER(10,0) NOT NULL ,\"user_id\" NUMBER(10,0) NOT NULL ,\"id\" NUMBER(10,0) NOT NULL ,CONSTRAINT pk_2bbef8433c51427f875188e7e6 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"hyper_cube_dimension_ordinals\"(\"taxonomy_id\" NUMBER(10,0),\"cube_element_id\" NUMBER(10,0),\"dimension_element_id\" NUMBER(10,0) NOT NULL ,\"ordinal\" NUMBER(10,0) NOT NULL ,\"id\" NUMBER(10,0) NOT NULL ,CONSTRAINT pk_e882a87d506a4a09a5596c0c14 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"values_gaap\"(\"parent_id\" NUMBER(19,0),\"document_id\" NUMBER(10,0),\"id\" NUMBER(19,0) NOT NULL ,\"elem_id\" NUMBER(10,0),\"value\" CLOB,\"cb_value_other\" CLOB,\"supress_warning_messages\" NUMBER(1,0) NOT NULL ,\"auto_computation_enabled\" NUMBER(1,0) NOT NULL ,\"send_account_balances\" NUMBER(1,0),\"comment\" CLOB,CONSTRAINT pk_f02572530ba641819fac11f02d PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"mapping_template_heads\"(\"id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(64),\"account_structure\" VARCHAR2(128),\"comment\" CLOB,\"creator_id\" NUMBER(10,0),\"creation_date\" TIMESTAMP,\"last_modifier_id\" NUMBER(10,0),\"modify_date\" TIMESTAMP,\"taxonomy_info_id\" NUMBER(10,0),CONSTRAINT pk_f11eb3f31f204c128e2a9b8f65 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"report_federal_gazette\"(\"id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(256),\"comment\" CLOB,\"company_id\" NUMBER(10,0) NOT NULL ,\"document_id\" NUMBER(10,0) NOT NULL ,\"owner_id\" NUMBER(10,0) NOT NULL ,\"gaap_taxonomy_info_id\" VARCHAR2(64) NOT NULL ,\"creation_date\" TIMESTAMP,CONSTRAINT pk_9853fc41940440b8bfdba8b3c2 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"GlobalOptions\"(\"id\" NUMBER(10,0) NOT NULL ,\"user_id\" NUMBER(10,0),\"option\" CLOB,CONSTRAINT pk_3f96acf193ac4402a7dc34d710 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"audit_correction\"(\"document_id\" NUMBER(10,0),\"name\" CLOB,\"comment\" CLOB,\"type\" NUMBER,\"id\" NUMBER(19,0) NOT NULL ,CONSTRAINT pk_5ea9c060dc714a4f94ddea6584 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"hypercube_items\"(\"hypercube_id\" NUMBER(10,0),\"document_id\" NUMBER(10,0),\"dimension_key_id\" NUMBER(19,0) NOT NULL ,\"value\" CLOB,\"id\" NUMBER(19,0) NOT NULL ,CONSTRAINT pk_be931560588e4ddeb8943e5f09 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"hypercube_dimensions\"(\"taxonomy_id\" NUMBER(10,0),\"cube_element_id\" NUMBER(10,0),\"explicit_member_element_id_1\" NUMBER(10,0),\"explicit_member_element_id_2\" NUMBER(10,0),\"explicit_member_element_id_3\" NUMBER(10,0),\"explicit_member_element_id_4\" NUMBER(10,0),\"explicit_member_element_id_5\" NUMBER(10,0),\"explicit_member_element_id_6\" NUMBER(10,0),\"explicit_member_element_id_7\" NUMBER(10,0),\"explicit_member_element_id_8\" NUMBER(10,0),\"explicit_member_element_id_9\" NUMBER(10,0),\"explicit_member_element_id_10\" NUMBER(10,0),\"id\" NUMBER(19,0) NOT NULL ,CONSTRAINT pk_18e59af1fcfd4c0ea11ed33013 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"values_gcd\"(\"parent_id\" NUMBER(19,0),\"document_id\" NUMBER(10,0),\"id\" NUMBER(19,0) NOT NULL ,\"elem_id\" NUMBER(10,0),\"value\" CLOB,\"cb_value_other\" CLOB,\"supress_warning_messages\" NUMBER(1,0) NOT NULL ,\"auto_computation_enabled\" NUMBER(1,0) NOT NULL ,\"send_account_balances\" NUMBER(1,0),\"comment\" CLOB,CONSTRAINT pk_fd5d91dfbdc340ef81d1a699df PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"reconciliation_reflists\"(\"document_id\" NUMBER(10,0) NOT NULL ,\"user_id\" NUMBER(10,0) NOT NULL ,\"id\" NUMBER(19,0) NOT NULL ,CONSTRAINT pk_e9b1b295976c4c2788be353224 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"taxonomy_info\"(\"id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(128) NOT NULL ,\"path\" CLOB NOT NULL ,\"filename\" CLOB NOT NULL ,\"type\" NUMBER NOT NULL ,\"version\" VARCHAR2(64) NOT NULL ,CONSTRAINT pk_25075ef4e7a34f4dbd601b9e79 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"values_gcd_company\"(\"parent_id\" NUMBER(19,0),\"company_id\" NUMBER(10,0),\"id\" NUMBER(19,0) NOT NULL ,\"elem_id\" NUMBER(10,0),\"value\" CLOB,\"cb_value_other\" CLOB,\"supress_warning_messages\" NUMBER(1,0) NOT NULL ,\"auto_computation_enabled\" NUMBER(1,0) NOT NULL ,\"send_account_balances\" NUMBER(1,0),\"comment\" CLOB,CONSTRAINT pk_bd05d05bb61642f88dd8ebc2fc PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"values_gaap_fg\"(\"parent_id\" NUMBER(19,0),\"document_id\" NUMBER(10,0),\"id\" NUMBER(19,0) NOT NULL ,\"elem_id\" NUMBER(10,0),\"value\" CLOB,\"cb_value_other\" CLOB,\"supress_warning_messages\" NUMBER(1,0) NOT NULL ,\"auto_computation_enabled\" NUMBER(1,0) NOT NULL ,\"send_account_balances\" NUMBER(1,0),\"comment\" CLOB,CONSTRAINT pk_d0720c7ce90a4788b3eb8cef06 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"hypercubes\"(\"document_id\" NUMBER(10,0),\"taxonomy_id\" NUMBER(10,0),\"cube_element_id\" NUMBER(10,0),\"comment\" CLOB,\"id\" NUMBER(10,0) NOT NULL ,CONSTRAINT pk_c59d17ee76ae454b99ec70cb29 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"virtual_accounts\"(\"balance_list_id\" NUMBER(10,0) NOT NULL ,\"assigned_element_id\" NUMBER(10,0),\"taxonomy_source_position\" VARCHAR2(64),\"amount\" FLOAT(24) NOT NULL ,\"hidden\" NUMBER(1,0) NOT NULL ,\"number\" VARCHAR2(32) NOT NULL ,\"name\" VARCHAR2(128) NOT NULL ,\"group_id\" NUMBER(19,0),\"user_defined\" NUMBER(1,0) NOT NULL ,\"id\" NUMBER(19,0) NOT NULL ,\"sort_index\" VARCHAR2(64) NOT NULL ,\"in_tray\" NUMBER(1,0) NOT NULL ,CONSTRAINT pk_3d60f03dd4a34f05a7fd65b0d2 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"balance_lists\"(\"id\" NUMBER(10,0) NOT NULL ,\"document_id\" NUMBER(10,0),\"comment\" CLOB,\"name\" VARCHAR2(40),\"imported_from_id\" NUMBER(10,0),\"import_date\" TIMESTAMP,\"source\" CLOB,CONSTRAINT pk_049ab6f44936436898d0385a82 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"federal_gazette_info\"(\"id\" NUMBER(10,0) NOT NULL ,\"notes\" NUMBER(1,0),\"assets\" NUMBER(1,0),\"balance\" NUMBER(1,0),\"income_use\" NUMBER(1,0),\"income_statement\" NUMBER(1,0),\"management_report\" NUMBER(1,0),\"start_date\" TIMESTAMP,\"end_date\" TIMESTAMP,\"exemption_decision\" NUMBER(1,0),\"exemption_note\" NUMBER(1,0),\"loss_assumption\" NUMBER(1,0),\"oder_type\" NUMBER(10,0),\"publication_area\" NUMBER(10,0),\"publication_category\" NUMBER(10,0),\"publication_type\" NUMBER(10,0),\"publication_description\" NUMBER(10,0),\"company_type\" NUMBER(10,0),\"company_size\" VARCHAR2(64),\"company_name\" CLOB,\"company_sign\" VARCHAR2(64),\"registration_type\" VARCHAR2(64),\"legal_form\" VARCHAR2(64),\"domicile\" VARCHAR2(256),\"company_registration_type\" VARCHAR2(256),\"registeration_court\" VARCHAR2(64),\"registration_number\" VARCHAR2(256),\"company_id\" VARCHAR2(256),\"client_id\" VARCHAR2(64),\"copmany_name_address\" CLOB,\"company_devision\" CLOB,\"company_street\" VARCHAR2(256),\"company_postcode\" VARCHAR2(64),\"company_city\" VARCHAR2(256),\"company_State\" VARCHAR2(256),\"salutation\" VARCHAR2(64),\"title\" VARCHAR2(64),\"firstname\" VARCHAR2(256),\"lastname\" VARCHAR2(256),\"telephone\" VARCHAR2(64),\"cell\" VARCHAR2(64),\"email\" VARCHAR2(256),\"data_sent\" NUMBER(1,0),\"account_deleted\" NUMBER(1,0),\"ticket_id\" VARCHAR2(64),CONSTRAINT pk_2eed3ab3f71540ceaecf5da9bd PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"log_report_1\"(\"content_type\" NUMBER NOT NULL ,\"action_type\" NUMBER NOT NULL ,\"reference_id\" NUMBER(19,0) NOT NULL ,\"old_value\" CLOB,\"new_value\" CLOB,\"info\" CLOB,\"timestamp\" TIMESTAMP,\"user_id\" NUMBER(10,0) NOT NULL ,\"id\" NUMBER(19,0) NOT NULL ,CONSTRAINT pk_f04520a07150437f9d624757ea PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"log_report_1_value_change\"(\"taxonomy_id\" NUMBER(10,0) NOT NULL ,\"old_value\" CLOB,\"new_value\" CLOB,\"reference_id\" NUMBER(19,0) NOT NULL ,\"value_type\" NUMBER NOT NULL ,\"timestamp\" TIMESTAMP,\"user_id\" NUMBER(10,0) NOT NULL ,\"id\" NUMBER(19,0) NOT NULL ,CONSTRAINT pk_131b87b5fdef4c9f82ca74d998 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"reconciliation_transactions\"(\"document_id\" NUMBER(10,0),\"reconciliation_id\" NUMBER(19,0),\"transaction_type\" NUMBER,\"element_id\" NUMBER(10,0),\"value\" FLOAT(24),\"id\" NUMBER(19,0) NOT NULL ,CONSTRAINT pk_77c60b494ced4529a11455d2cc PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"audit_correction_transaction\"(\"document_id\" NUMBER(10,0),\"head_id\" NUMBER(19,0),\"transaction_type\" NUMBER,\"element_id\" NUMBER(10,0),\"value\" FLOAT(24),\"id\" NUMBER(19,0) NOT NULL ,CONSTRAINT pk_3da900d58e3b40a19d38b66e64 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"users\"(\"id\" NUMBER(10,0) NOT NULL ,\"username\" VARCHAR2(128),\"fullname\" VARCHAR2(256),\"salt\" VARCHAR2(64),\"password\" CLOB NOT NULL ,\"is_active\" NUMBER(1,0),\"is_admin\" NUMBER(1,0),\"is_companyadmin\" NUMBER(1,0),\"assigned_companies\" CLOB,\"is_deleted\" NUMBER(1,0),\"is_domain_user\" NUMBER(1,0),\"domain\" VARCHAR2(64),\"last_login\" TIMESTAMP,CONSTRAINT pk_93a138a791874199a28900b406 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"companies\"(\"Id\" NUMBER(10,0) NOT NULL ,\"name\" VARCHAR2(256),\"owner_id\" NUMBER(10,0),\"taxonomy_info_id\" NUMBER(10,0),CONSTRAINT pk_02c148c5ee8c48db82c6e4fce2 PRIMARY KEY(\"Id\")) INITRANS 5 ",
                    "CREATE TABLE \"reconciliation_reflist_items\"(\"reconciliation_reflist_id\" NUMBER(19,0),\"element_id\" NUMBER(10,0),\"id\" NUMBER(19,0) NOT NULL ,CONSTRAINT pk_47244c597b9146dc91929d9ac3 PRIMARY KEY(\"id\")) INITRANS 5 ",
                    "CREATE TABLE \"hypercube_import_templates\"(\"template_id\" NUMBER(10,0) NOT NULL ,\"taxonomy_id\" NUMBER(10,0),\"cube_element_id\" NUMBER(10,0),\"template_name\" VARCHAR2(64),\"xml_assignment\" CLOB,\"comment\" CLOB,\"creation_user\" NUMBER(10,0) NOT NULL ,\"cration_date\" TIMESTAMP NOT NULL ,\"modification_user\" NUMBER(10,0),\"inverse_assignment\" NUMBER(1,0) NOT NULL ,\"modification_date\" TIMESTAMP,\"encoding\" VARCHAR2(64) NOT NULL ,\"seperator\" VARCHAR2(64) NOT NULL ,\"delimiter\" VARCHAR2(64) NOT NULL ,\"template_csv\" VARCHAR2(200) NOT NULL ,\"dimension_order\" VARCHAR2(64),CONSTRAINT pk_d0ad79ab3a8f4d468cf55f79be PRIMARY KEY(\"template_id\")) INITRANS 5 ",
                    "CREATE TABLE \"upgrade_information\"(\"id\" NUMBER(10,0) NOT NULL ,\"upgrade_available_from\" TIMESTAMP NOT NULL ,\"version_string\" VARCHAR2(64) NOT NULL ,\"ordinal\" FLOAT(24) NOT NULL ,\"resource_name\" VARCHAR2(64),CONSTRAINT pk_8af34d3929ce4f40aa52353484 PRIMARY KEY(\"id\")) INITRANS 5 ",
                };

                var version160_sqlserver = new TableInformation();
                version160_sqlserver.tableCreation = new List<string> {
                    "CREATE TABLE [documents]([id] INT NOT NULL,[name] VARCHAR(256) NOT NULL,[comment] TEXT,[company_id] INT,[system_id] INT,[financial_year_id] INT,[owner_id] INT,[gcd_taxonomy_info_id] INT,[main_taxonomy_info_id] INT,[creation_date] DATETIME,[reconciliation_mode] INT,PRIMARY KEY([id]))",
                    "CREATE TABLE [global_search_history]([id] INT NOT NULL,[user_id] INT NOT NULL,[history_list] TEXT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_items]([hypercube_id] INT,[document_id] INT,[dimension_key_id] BIGINT NOT NULL,[value] TEXT,[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [audit_correction_transaction]([document_id] INT,[head_id] INT,[transaction_type] INT,[element_id] INT,[value] decimal(20,2),[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [templates_balance_list]([Id] INT NOT NULL,[name] VARCHAR(64) NOT NULL,[comment] TEXT,[creator_id] INT NOT NULL,[timestamp] DATETIME NOT NULL,[template] TEXT NOT NULL,PRIMARY KEY([Id]))",
                    "CREATE TABLE [audit_correction_set]([set_id] BIGINT NOT NULL,[correction_id] BIGINT NOT NULL,[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [mapping_template_lines]([id] INT NOT NULL,[template_id] INT,[account_number] VARCHAR(32),[account_name] TEXT,[is_account_of_exchange] TINYINT,[debit_element_id] TEXT,[credit_element_id] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_import_templates]([template_id] INT NOT NULL IDENTITY,[taxonomy_id] INT,[cube_element_id] INT,[template_name] VARCHAR(64),[xml_assignment] TEXT,[comment] TEXT,[creation_user] INT NOT NULL,[cration_date] DATETIME NOT NULL,[modification_user] INT,[inverse_assignment] TINYINT NOT NULL,[modification_date] DATETIME,[encoding] VARCHAR(64) NOT NULL,[seperator] VARCHAR(64) NOT NULL,[delimiter] VARCHAR(64) NOT NULL,[template_csv] VARCHAR(200) NOT NULL,[dimension_order] VARCHAR(64),PRIMARY KEY([template_id]))",
                    "CREATE TABLE [templates_account_groups]([Id] INT NOT NULL,[name] VARCHAR(64) NOT NULL,[comment] TEXT,[creator_id] INT NOT NULL,[timestamp] DATETIME NOT NULL,[template] TEXT NOT NULL,PRIMARY KEY([Id]))",
                    "CREATE TABLE [templates_account_splittings]([Id] INT NOT NULL,[name] VARCHAR(64) NOT NULL,[comment] TEXT,[creator_id] INT NOT NULL,[timestamp] DATETIME NOT NULL,[template] TEXT NOT NULL,PRIMARY KEY([Id]))",
                    "CREATE TABLE [templates_balance_list]([Id] INT NOT NULL,[name] VARCHAR(64) NOT NULL,[comment] TEXT,[creator_id] INT NOT NULL,[timestamp] DATETIME NOT NULL,[template] TEXT NOT NULL,PRIMARY KEY([Id]))",
                    "CREATE TABLE [templates_account_profile]([Id] INT NOT NULL,[name] VARCHAR(64) NOT NULL,[comment] TEXT,[creator_id] INT NOT NULL,[timestamp] DATETIME NOT NULL,[template] TEXT NOT NULL,PRIMARY KEY([Id]))",
                    "CREATE TABLE [log_report_1]([content_type] INT NOT NULL,[action_type] INT NOT NULL,[reference_id] BIGINT NOT NULL,[old_value] TEXT,[new_value] TEXT,[info] TEXT,[timestamp] DATETIME,[user_id] INT NOT NULL,[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [users]([id] INT NOT NULL,[username] VARCHAR(128),[fullname] VARCHAR(256),[salt] VARCHAR(64),[password] TEXT NOT NULL,[is_active] TINYINT,[is_admin] TINYINT,[last_login] DATETIME,[is_companyadmin] TINYINT,[assigned_companies] TEXT,[is_deleted] TINYINT,[is_domain_user] TINYINT,[domain] VARCHAR(64),PRIMARY KEY([id]))",
                    "CREATE TABLE [taxonomy_ids]([id] INT NOT NULL,[taxonomy_id] INT,[xbrl_element_id] TEXT NOT NULL,[number] VARCHAR(10) NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [mapping_template_element_info]([id] BIGINT NOT NULL,[template_id] INT,[element_id] TEXT,[auto_compute_enabled] TINYINT,[supress_warning_messages] TINYINT NOT NULL,[send_account_balances] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [taxonomy_info]([id] INT NOT NULL,[name] VARCHAR(128) NOT NULL,[path] TEXT NOT NULL,[filename] TEXT NOT NULL,[type] INT NOT NULL,[version] VARCHAR(64) NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [info]([id] INT NOT NULL,[key] VARCHAR(64) NOT NULL,[value] VARCHAR(64),PRIMARY KEY([id]),UNIQUE ([key]))",
                    "CREATE TABLE [values_gcd_company]([parent_id] INT,[company_id] INT,[id] BIGINT NOT NULL,[elem_id] INT,[value] TEXT,[cb_value_other] TEXT,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [companies]([Id] INT NOT NULL,[name] VARCHAR(256),[owner_id] INT,[taxonomy_info_id] INT,PRIMARY KEY([Id]))",
                    "CREATE TABLE [reconciliation_reflist_items]([reconciliation_reflist_id] INT,[element_id] INT,[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [GlobalOptions]([id] INT NOT NULL,[user_id] INT,[option] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_report_1_value_change]([taxonomy_id] INT NOT NULL,[old_value] TEXT,[new_value] TEXT,[reference_id] BIGINT NOT NULL,[value_type] INT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [accounts]([amount] decimal(20,2) NOT NULL,[group_id] BIGINT,[user_defined] TINYINT NOT NULL,[id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[assigned_element_id] INT,[comment] TEXT,[hidden] TINYINT NOT NULL,[sort_index] VARCHAR(64) NOT NULL,[in_tray] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [split_account_groups]([id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[account_id] BIGINT NOT NULL,[comment] TEXT,[value_input_mode] INT,PRIMARY KEY([id]))",
                    "CREATE TABLE [splitted_accounts]([amount] decimal(20,2) NOT NULL,[amount_percent] decimal(20,2),[split_group_id] BIGINT,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[assigned_element_id] INT,[comment] TEXT,[hidden] TINYINT NOT NULL,[sort_index] VARCHAR(64) NOT NULL,[in_tray] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [account_groups]([number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[id] BIGINT NOT NULL,[balance_list_id] INT NOT NULL,[assigned_element_id] INT,[comment] TEXT,[hidden] TINYINT NOT NULL,[sort_index] VARCHAR(64) NOT NULL,[in_tray] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [upgrade_information]([id] INT NOT NULL,[upgrade_available_from] DATETIME NOT NULL,[version_string] VARCHAR(64) NOT NULL,[ordinal] decimal(20,2) NOT NULL,[resource_name] VARCHAR(64),PRIMARY KEY([id]))",
                    "CREATE TABLE [reconciliation_transactions]([document_id] INT,[reconciliation_id] INT,[transaction_type] INT,[element_id] INT,[value] decimal(20,2),[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [balance_lists]([id] INT NOT NULL,[document_id] INT,[comment] TEXT,[name] VARCHAR(40),[imported_from_id] INT,[import_date] DATETIME,[source] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_admin]([content_type] INT NOT NULL,[action_type] INT NOT NULL,[reference_id] BIGINT NOT NULL,[old_value] TEXT,[new_value] TEXT,[info] TEXT,[timestamp] DATETIME,[user_id] INT NOT NULL,[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_dimension_keys]([taxonomy_id] INT,[cube_element_id] INT,[primary_dimension_id] INT NOT NULL,[dimension_id] BIGINT NOT NULL,[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercubes]([document_id] INT,[taxonomy_id] INT,[cube_element_id] INT,[comment] TEXT,[id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [mapping_template_heads]([id] INT NOT NULL,[name] VARCHAR(64),[account_structure] VARCHAR(128),[comment] TEXT,[creator_id] INT,[creation_date] DATETIME,[last_modifier_id] INT,[modify_date] DATETIME,[taxonomy_info_id] INT,PRIMARY KEY([id]))",
                    "CREATE TABLE [account_reflists]([document_id] INT NOT NULL,[user_id] INT NOT NULL,[id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [hypercube_dimensions]([taxonomy_id] INT,[cube_element_id] INT,[explicit_member_element_id_1] INT,[explicit_member_element_id_2] INT,[explicit_member_element_id_3] INT,[explicit_member_element_id_4] INT,[explicit_member_element_id_5] INT,[explicit_member_element_id_6] INT,[explicit_member_element_id_7] INT,[explicit_member_element_id_8] INT,[explicit_member_element_id_9] INT,[explicit_member_element_id_10] INT,[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [report_federal_gazette]([id] INT NOT NULL,[name] VARCHAR(256),[comment] TEXT,[company_id] INT NOT NULL,[document_id] INT NOT NULL,[owner_id] INT NOT NULL,[gaap_taxonomy_info_id] VARCHAR(64) NOT NULL,[creation_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [systems]([id] INT NOT NULL,[name] VARCHAR(128),[comment] VARCHAR(256),PRIMARY KEY([id]),UNIQUE ([name]))",
                    "CREATE TABLE [roles]([id] INT NOT NULL,[name] VARCHAR(256),[comment] TEXT,[user_id] INT,PRIMARY KEY([id]))",
                    "CREATE TABLE [rights]([id] INT NOT NULL,[role_id] INT NOT NULL,[content_type] INT NOT NULL,[ref_id] INT NOT NULL,[rights] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [user_roles]([id] INT NOT NULL,[user_id] INT NOT NULL,[role_id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gaap_fg]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[elem_id] INT,[value] TEXT,[cb_value_other] TEXT,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [federal_gazette_info]([id] INT NOT NULL,[notes] TINYINT,[assets] TINYINT,[balance] TINYINT,[income_use] TINYINT,[income_statement] TINYINT,[management_report] TINYINT,[start_date] DATETIME,[end_date] DATETIME,[exemption_decision] TINYINT,[exemption_note] TINYINT,[loss_assumption] TINYINT,[oder_type] INT,[publication_area] INT,[publication_category] INT,[publication_type] INT,[publication_description] INT,[company_type] INT,[company_size] VARCHAR(64),[company_name] TEXT,[company_sign] VARCHAR(64),[registration_type] VARCHAR(64),[legal_form] VARCHAR(64),[domicile] VARCHAR(256),[company_registration_type] VARCHAR(256),[registeration_court] VARCHAR(64),[registration_number] VARCHAR(256),[company_id] VARCHAR(256),[client_id] VARCHAR(64),[copmany_name_address] TEXT,[company_devision] TEXT,[company_street] VARCHAR(256),[company_postcode] VARCHAR(64),[company_city] VARCHAR(256),[company_State] VARCHAR(256),[salutation] VARCHAR(64),[title] VARCHAR(64),[firstname] VARCHAR(256),[lastname] VARCHAR(256),[telephone] VARCHAR(64),[cell] VARCHAR(64),[email] VARCHAR(256),[data_sent] TINYINT,[account_deleted] TINYINT,[ticket_id] VARCHAR(64),PRIMARY KEY([id]))",
                    "CREATE TABLE [hyper_cube_dimension_ordinals]([taxonomy_id] INT,[cube_element_id] INT,[dimension_element_id] INT NOT NULL,[ordinal] INT NOT NULL,[id] INT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [virtual_accounts]([balance_list_id] INT NOT NULL,[assigned_element_id] INT,[taxonomy_source_position] VARCHAR(64),[amount] decimal(20,2) NOT NULL,[hidden] TINYINT NOT NULL,[number] VARCHAR(32) NOT NULL,[name] VARCHAR(128) NOT NULL,[group_id] BIGINT,[user_defined] TINYINT NOT NULL,[id] BIGINT NOT NULL,[sort_index] VARCHAR(64) NOT NULL,[in_tray] TINYINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [balance_lists]([id] INT NOT NULL,[document_id] INT,[comment] TEXT,[name] VARCHAR(40),[imported_from_id] INT,[import_date] DATETIME,[source] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gcd]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[elem_id] INT,[value] TEXT,[cb_value_other] TEXT,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [financial_years]([id] INT NOT NULL,[company_id] INT NOT NULL,[fyear] INT,[is_enabled] TINYINT,[fiscal_year_begin] DATETIME,[fiscal_year_end] DATETIME,[bal_sheet_closing_date] DATETIME,PRIMARY KEY([id]))",
                    "CREATE TABLE [log_send]([document_id] BIGINT NOT NULL,[send_data] TEXT NOT NULL,[report_info] TEXT NOT NULL,[send_error] INT NOT NULL,[result_message] TEXT NOT NULL,[timestamp] DATETIME,[user_id] INT NOT NULL,[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [reconciliation_reflists]([document_id] INT NOT NULL,[user_id] INT NOT NULL,[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [reconciliations]([document_id] INT,[type] INT,[transfer_kind] INT,[name] TEXT,[comment] TEXT,[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [audit_correction]([document_id] INT,[name] TEXT,[comment] TEXT,[id] BIGINT NOT NULL,PRIMARY KEY([id]))",
                    "CREATE TABLE [values_gaap]([parent_id] INT,[document_id] INT,[id] BIGINT NOT NULL,[elem_id] INT,[value] TEXT,[cb_value_other] TEXT,[supress_warning_messages] TINYINT NOT NULL,[auto_computation_enabled] TINYINT NOT NULL,[send_account_balances] TINYINT,[comment] TEXT,PRIMARY KEY([id]))",
                    "CREATE TABLE [account_reflist_items]([account_reflist_id] INT,[account_type] INT,[account_id] BIGINT,[id] INT NOT NULL,PRIMARY KEY([id]))",
                };                

                AddVersion("1.6.0", version160_mysql, version160_sqlite, version160_sqlserver, version160_oracle);

                #endregion version 1.6.0
            }

        }

        /// <summary>
        /// Get the last version that has an entry in the versionToTableInformation.
        /// </summary>
        /// <param name="version">The current version.</param>
        /// <returns>The version that is contained in the versionToTableInformation.</returns>
        /// <Author>Sebastian Vetter</Author>
        private string GetLastDatabaseCreationVersion(string version) {
            var versionEntry = version;
            while (!versionToTableInformation.ContainsKey(versionEntry)) {
                versionEntry = eBalanceKitBase.VersionInfo.Instance.GetPreviousDbVersion(versionEntry);
            }
            return versionEntry;
        }

        public static DatabaseCreator Instance
                {
                    get
                    {
                        return instance ?? (instance = new DatabaseCreator());
                    }
                }

            public int GetTableCount(string version) {
            var availableDbVersion = version;
            try {
                availableDbVersion = GetLastDatabaseCreationVersion(version);
                TableInformation tableInfo = versionToTableInformation[availableDbVersion]["MySQL"];
                return tableInfo.tableNames.Count;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message + ", version: " + availableDbVersion);
                return 0;
            }
        }

        public List<string> GetTableNames(string version, string dbType) {
            var availableDbVersion = version;
            try {
                availableDbVersion = GetLastDatabaseCreationVersion(version);
                TableInformation tableInfo = versionToTableInformation[availableDbVersion][dbType];
                return tableInfo.tableNames;
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message + ", version: " + availableDbVersion);
                return null;
            }

        }

        public void CreateDatabase(string version, string dbType, IDatabase conn) {
            TableInformation tableInfo;
            //if (!dbType.ToLower().Equals("oracle"))
            var availableDbVersion = version;
            try {
                availableDbVersion = GetLastDatabaseCreationVersion(version);
                tableInfo = versionToTableInformation[availableDbVersion][dbType];
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message + ", version: " + availableDbVersion + ", dbType: " + dbType);
                return;
            }
            foreach (string table in tableInfo.tableNames)
                CreateTable(availableDbVersion, dbType, conn, table);
        }
        //unique key for oracle
        private string GenerateGuid() {
            var tmp = "pk_" + Guid.NewGuid().ToString("N");
            if (tmp.Length > 29)
                tmp = tmp.Substring(0, 29);
            return tmp;
        }
        //Creates a table for a specific version, allows to change the name of table
        public void CreateTable(string version, string dbType, IDatabase conn, string tableName, string underDifferentName=null) {
            TableInformation tableInfo;
            try {
                tableInfo = versionToTableInformation[version][dbType];
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message + ", version: " + version + ", dbType: " + dbType);
                return;
            }
            for(int i = 0; i < tableInfo.tableCreation.Count; ++i)
                if (tableInfo.tableNames[i] == tableName) {
                    try {
                        if (underDifferentName == null) {
                            conn.DropTableIfExists(tableInfo.tableNames[i]);
                            conn.ExecuteNonQuery(tableInfo.tableCreation[i]);
                        } else {
                            conn.DropTableIfExists(underDifferentName);
                            var tmp = tableInfo.tableCreation[i].Replace(tableInfo.tableNames[i], underDifferentName);
                            //dirty hack for oracle
                            if (dbType.ToLower().Equals("oracle") && tableName.Contains("value_change")) 
                                tmp = tmp.Replace("pk_6717ed5a36f54c9289e9c0b073", GenerateGuid());
                            if (dbType.ToLower().Equals("oracle") && tableName.Contains("log_report"))
                                tmp = tmp.Replace("pk_9050837be4374df5b93382c521", GenerateGuid());
                            
                            conn.ExecuteNonQuery(tmp);
                        }
                        return;
                      
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine(ex.Message + ", version: " + version + ", dbType: " + dbType);
                    }
                    foreach (var index in tableInfo.indexInfo[i].indices) {
                        try {
                            if(underDifferentName == null)
                                conn.CreateIndex(tableInfo.indexInfo[i].tableName, index.Key, index.Value);
                            else
                                conn.CreateIndex(underDifferentName, index.Key, index.Value);

                        } catch (Exception) {
                            //This exception can occur if the column already has an index, just ignore it.
                        }
                    }
                }
        }
    }
}
