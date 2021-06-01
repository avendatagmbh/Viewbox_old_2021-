using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using MySql.Data.MySqlClient;
using BelegeArchivePDFGenerator.DataModels;
using System.Collections.Concurrent;

namespace BelegeArchivePDFGenerator
{
    class DbSelect
    {
        public static List<string> ShowDatabases(string connStr)
        {
            List<string> dbList = new List<string>();
            using (var conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    string query = "show databases;";
                    MySqlCommand myCommand = new MySqlCommand(query, conn);
                    using (var myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            dbList.Add(myReader.GetString("Database"));
                        }
                    }
                    return dbList;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.Number + " - " + ex.Message);
                }
            }
            return null;
        }

        public static ConcurrentBag<DataModel> SelectRowsFrom(string tableName, int timeOut, int limit)
        {
            ConcurrentBag<DataModel> retVals = new ConcurrentBag<DataModel>();

            using (var conn = new MySqlConnection(DbConnection.connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT *";
                    /*foreach (var colName in colNames)
                    {
                        query += "`,";
                    }
                    query = query.Remove(query.Length - 1);*/
                    query += " FROM " + tableName + " LIMIT " + limit.ToString() + ";";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.CommandTimeout = timeOut;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DataModel row = new DataModel();
                            row.Name = reader.GetValue(1) != DBNull.Value ? reader.GetString(1) : "";
                            row.Address = reader.GetValue(2) != DBNull.Value ? reader.GetString(2) : "";
                            row.Kunden_nr = reader.GetValue(3) != DBNull.Value ? reader.GetString(3) : "";
                            row.Verstags_konto = reader.GetValue(4) != DBNull.Value ? reader.GetString(4) : "";
                            row.Referenz_nummber = reader.GetValue(5) != DBNull.Value ? reader.GetString(5) : "";
                            row.Strom_rechnung = reader.GetValue(6) != DBNull.Value ? reader.GetString(6) : "";
                            row.abrechnun_date = reader.GetValue(7) != DBNull.Value ? reader.GetString(7) : "";
                            row.zahlpunkt = reader.GetValue(8) != DBNull.Value ? reader.GetString(8) : "";
                            row.verbrauchersstelle = reader.GetValue(9) != DBNull.Value ? reader.GetString(9) : "";
                            row.rechnung_date = reader.GetValue(10) != DBNull.Value ? reader.GetString(10) : "";
                            row.falligkeits_date = reader.GetValue(11) != DBNull.Value ? reader.GetString(11) : "";
                            row.leistung_ver = reader.GetValue(12) != DBNull.Value ? reader.GetString(12) : "";
                            row.leistung_preis = reader.GetValue(13) != DBNull.Value ? reader.GetString(13) : "";
                            row.leistung_sum = reader.GetValue(14) != DBNull.Value ? reader.GetString(14) : "";
                            row.ablesung = reader.GetValue(15) != DBNull.Value ? reader.GetString(15) : "";
                            row.wirkarbeit_ht_ver = reader.GetValue(16) != DBNull.Value ? reader.GetString(16) : "";
                            row.wirkarbeit_ht_preis = reader.GetValue(17) != DBNull.Value ? reader.GetString(17) : "";
                            row.wirkarbeit_ht_sum = reader.GetValue(18) != DBNull.Value ? reader.GetString(18) : "";
                            row.wirkarbeit_nt_ver = reader.GetValue(19) != DBNull.Value ? reader.GetString(19) : "";
                            row.wirkarbeit_nt_preis = reader.GetValue(20) != DBNull.Value ? reader.GetString(20) : "";
                            row.wirkarbeit_nt_sum = reader.GetValue(21) != DBNull.Value ? reader.GetString(21) : "";
                            row.gutschrift_messpreis = reader.GetValue(22) != DBNull.Value ? reader.GetString(22) : "";
                            row.messung = reader.GetValue(23) != DBNull.Value ? reader.GetString(23) : "";
                            row.abrechnung = reader.GetValue(24) != DBNull.Value ? reader.GetString(24) : "";
                            row.messstellenbetrieb = reader.GetValue(25) != DBNull.Value ? reader.GetString(25) : ""; 
                            row.eeg_ver = reader.GetValue(26) != DBNull.Value ? reader.GetString(26) : "";
                            row.eeg_preis = reader.GetValue(27) != DBNull.Value ? reader.GetString(27) : "";
                            row.eeg_sum = reader.GetValue(28) != DBNull.Value ? reader.GetString(28) : "";
                            row.kwkg_ver = reader.GetValue(29) != DBNull.Value ? reader.GetString(29) : "";
                            row.kwkg_preis = reader.GetValue(30) != DBNull.Value ? reader.GetString(30) : "";
                            row.kwkg_sum = reader.GetValue(31) != DBNull.Value ? reader.GetString(31) : "";
                            row.strumsteuer = reader.GetValue(32) != DBNull.Value ? reader.GetString(32) : "";
                            row.wirkaibeit_ver = reader.GetValue(33) != DBNull.Value ? reader.GetString(33) : "";
                            row.wirkaibeit_preis = reader.GetValue(34) != DBNull.Value ? reader.GetString(34) : "";
                            row.wirkaibeit_sum = reader.GetValue(35) != DBNull.Value ? reader.GetString(35) : "";
                            row.rechnungsbetrag = reader.GetValue(36) != DBNull.Value ? reader.GetString(36) : "";
                            row.zahlnummer = reader.GetValue(37) != DBNull.Value ? reader.GetString(37) : "";

                            retVals.Add(row); 
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.Number + " - " + ex.Message);
                }
            }
            return retVals;
        }
    }
}
