// -----------------------------------------------------------
// Created by Benjamin Held - 26.07.2011 13:54:26
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
    class TransferValueUpdate {
        
        public static void UeberleitungsrechnungUpdate(IDatabase conn) {
            //Assumes the tables transfer_hbst_lines and transfer_hbst_heads exist.

            if (!conn.TableExists("values_gaap") || !conn.TableExists("transfer_hbst_heads") || !conn.TableExists("transfer_hbst_lines")) return;
            List<long?> docIds = getOneColumn(conn, "documents", "id", "");
            foreach (long? id in docIds) {
                if (id.HasValue)
                    copyUeberleitungswerte(conn, id.Value);
            }
        }

        private static Dictionary<string, long> CreateMappingTaxonomyToValueId(IDatabase conn, long docId) {
            Dictionary<string, long> mapping = new Dictionary<string, long>();
            var reader = conn.ExecuteReader("SELECT xbrl_elem_id,id FROM " + conn.Enquote("values_gaap") + " WHERE document_id=" + docId);
            try {
                while (reader.Read()) {
                    string elem_id = reader["xbrl_elem_id"] as string;
                    if (elem_id == null) continue;
                    long id = -1;

                    if (reader["id"] as long? != null)
                        id = ((long?)reader["id"]).Value;
                    else if (reader["id"] as int? != null)
                        id = ((int?)reader["id"]).Value;
                    if (!mapping.ContainsKey(elem_id))
                        mapping.Add(elem_id, id);
                }
            } finally {
                if (reader != null && !reader.IsClosed) {
                    reader.Close();
                }
            }

            return mapping;
        }

        private static void copyUeberleitungswerte(IDatabase conn, long docId) {
            List<long?> idList = getOneColumn(conn, "values_gaap", "id", "WHERE document_id=" + docId + " AND xbrl_elem_id IS NULL AND value IS NULL");
            if (idList.Count != 1) throw new Exception("Es gibt mehr als eine Root Id zu Dokument " + docId);
            long rootId = idList[0].Value;

            idList = getOneColumn(conn, "values_gaap", "id", "WHERE document_id=" + docId + " AND parent_id=" + rootId + " AND xbrl_elem_id=" + conn.GetSqlString("hbst.transfer"));
            //If there is no root element, we dont have to copy anything
            if (idList.Count == 0) return;
            if (idList.Count != 1) throw new Exception("Es gibt mehr als eine Root List Id zu Dokument " + docId);
            long rootListId = idList[0].Value;
            idList = getOneColumn(conn, "values_gaap", "id", "WHERE document_id=" + docId + " AND parent_id=" + rootListId + " AND xbrl_elem_id=" + conn.GetSqlString("hbst.transfer"));
            Dictionary<string, long> taxonomyIdMapping = CreateMappingTaxonomyToValueId(conn, docId);
            using (IDatabase writeConn = ConnectionManager.CreateConnection(conn.DbConfig)) {
                writeConn.Open();
                writeConn.BeginTransaction();
                try {
                    int transferHeadsId = (int)CalcNextID(conn, "transfer_hbst_heads", false);
                    long transferLinesId = CalcNextID(conn, "transfer_hbst_lines", true);

                    Dictionary<long, DbColumnValues> valueIdToColumns = new Dictionary<long, DbColumnValues>();
                    foreach (long id in idList) {
                        List<long?> transferIds = getOneColumn(conn, "values_gaap", "id", "WHERE document_id=" + docId + " AND parent_id=" + id);
                        Dictionary<string, Tuple<string, long>> transfer = ExtractTransferRelatedData(conn, transferIds);
                        DbColumnValues valuesTransfer = new DbColumnValues();
                        string name = "Überleitung_" + transferHeadsId;
                        valuesTransfer["id"] = transferHeadsId;
                        valuesTransfer["name"] = name;
                        valuesTransfer["document_id"] = docId;
                        valuesTransfer["type"] = 0;
                        switch (transfer["hbst.transfer.kind"].Item1) {
                            case "hbst.transfer.kind.reclassification":
                                valuesTransfer["type"] = 0;
                                break;
                            case "hbst.transfer.kind.changeValue":
                                valuesTransfer["type"] = 1;
                                break;
                            case "hbst.transfer.kind.reclassificationChangeValue":
                                valuesTransfer["type"] = 2;
                                break;
                        }
                        valuesTransfer["comment"] = transfer["hbst.transfer.comment"].Item1;
                        writeConn.InsertInto("transfer_hbst_heads", valuesTransfer);

                        List<long?> typeIds = getOneColumn(conn, "values_gaap", "id", "WHERE document_id=" + docId + " AND parent_id=" + transfer["hbst.transfer.bsAss"].Item2);
                        AddTransferData(conn, docId, typeIds, "hbst.transfer.bsAss", taxonomyIdMapping, ref transferLinesId, transferHeadsId, valueIdToColumns);

                        typeIds = getOneColumn(conn, "values_gaap", "id", "WHERE document_id=" + docId + " AND parent_id=" + transfer["hbst.transfer.bsEqLiab"].Item2);
                        AddTransferData(conn, docId, typeIds, "hbst.transfer.bsEqLiab", taxonomyIdMapping, ref transferLinesId, transferHeadsId, valueIdToColumns);

                        typeIds = getOneColumn(conn, "values_gaap", "id", "WHERE document_id=" + docId + " AND parent_id=" + transfer["hbst.transfer.isChangeNetIncome"].Item2);
                        AddTransferData(conn, docId, typeIds, "hbst.transfer.isChangeNetIncome", taxonomyIdMapping, ref transferLinesId, transferHeadsId, valueIdToColumns);
                        transferHeadsId++;
                    }
                    foreach (var columns in valueIdToColumns)
                        writeConn.InsertInto("transfer_hbst_lines", columns.Value);
                    writeConn.ExecuteNonQuery("DELETE FROM values_gaap WHERE " + conn.FunctionSubstring("xbrl_elem_id", 1, 4) + "=" + conn.GetSqlString("hbst") + " AND document_id=" + docId);
                    writeConn.CommitTransaction();
                } catch (Exception ex) {
                    writeConn.RollbackTransaction();
                    throw ex;
                }
            }
        }

        private static void AddTransferData(IDatabase conn, long docId, List<long?> ids, string type, Dictionary<string, long> taxonomyIdMapping, ref long transferLinesId,
            int transferHeadsId, Dictionary<long, DbColumnValues> valueIdToColumns) {
            foreach (long? id in ids) {
                List<long?> bsAssChildrenIds = getOneColumn(conn, "values_gaap", "id", "WHERE document_id=" + docId + " AND parent_id=" + id.Value);
                Dictionary<string, Tuple<string, long>> transferData = ExtractTransferRelatedData(conn, bsAssChildrenIds);

                string elemName = transferData[type + ".name"].Item1;

                long taxonomyId;
                if (elemName != null && taxonomyIdMapping.ContainsKey(elemName))
                    taxonomyId = taxonomyIdMapping[elemName];
                else {
                    System.Diagnostics.Debug.WriteLine("Could not find the key " + elemName + " in the taxonomy mapping.");
                    continue;
                }
                DbColumnValues valuesTransferData = new DbColumnValues();
                valuesTransferData["id"] = transferLinesId++;
                valuesTransferData["document_id"] = docId;
                valuesTransferData["header_id"] = transferHeadsId;
                valuesTransferData["value_id"] = taxonomyId;
                if (type == "hbst.transfer.bsAss" || type == "hbst.transfer.bsEqLiab") {
                    valuesTransferData["transfer_value"] = Convert.ToDouble(transferData[type + ".changeValueActualPeriod"].Item1);
                    valuesTransferData["transfer_value_previous_year"] = Convert.ToDouble(transferData[type + ".changeValuePreviousPeriod"].Item1);
                    if (valueIdToColumns.ContainsKey(taxonomyId)) {
                        double transferValue = (double)valueIdToColumns[taxonomyId]["transfer_value"] + Convert.ToDouble(transferData[type + ".changeValueActualPeriod"].Item1);
                        valueIdToColumns[taxonomyId]["transfer_value"] = transferValue;
                        double transferValuePrev = (double)valueIdToColumns[taxonomyId]["transfer_value_previous_year"] + Convert.ToDouble(transferData[type + ".changeValuePreviousPeriod"].Item1);
                        valueIdToColumns[taxonomyId]["transfer_value_previous_year"] = transferValuePrev;
                    } else valueIdToColumns[taxonomyId] = valuesTransferData;
                } else if (type == "hbst.transfer.isChangeNetIncome") {
                    valuesTransferData["transfer_value"] = Convert.ToDouble(transferData[type + ".changeValueActualPeriod"].Item1);
                    if (valueIdToColumns.ContainsKey(taxonomyId)) {
                        double transferValue = (double)valueIdToColumns[taxonomyId]["transfer_value"] + Convert.ToDouble(transferData[type + ".changeValueActualPeriod"].Item1);
                        valueIdToColumns[taxonomyId]["transfer_value"] = transferValue;
                    } else valueIdToColumns[taxonomyId] = valuesTransferData;

                }

            }
        }


        private static long CalcNextID(IDatabase conn, string table, bool useLong) {

            long? newIdValue;
            if (useLong)
                newIdValue = (long?)getOneColumn(conn, table, "MAX(id)", "")[0];
            else
                newIdValue = (int?)getOneColumn(conn, table, "MAX(id)", "")[0];
            long newId = 1;
            if (newIdValue.HasValue)
                newId = newIdValue.Value + 1;
            return newId;
        }
        private static Dictionary<string, Tuple<string, long>> ExtractTransferRelatedData(IDatabase conn, List<long?> ids) {
            Dictionary<string, Tuple<string, long>> transfer = new Dictionary<string, Tuple<string, long>>();
            foreach (long? transferId in ids) {
                var reader = conn.ExecuteReader("SELECT id,xbrl_elem_id,value FROM " + conn.Enquote("values_gaap") + " WHERE id=" + transferId.Value);
                try {
                    if (reader.Read()) {
                        string elem = reader["xbrl_elem_id"] as string;
                        string value = reader["value"] as string;
                        long id = (long)reader["id"];
                        transfer[elem] = new Tuple<string, long>(value, id);
                    }
                } finally {
                    if (reader != null && !reader.IsClosed) {
                        reader.Close();
                    }
                }
            }
            return transfer;
        }

        private static List<long?> getOneColumn(IDatabase conn, string table, string column, string where) {
            List<long?> result = new List<long?>();

            var reader = conn.ExecuteReader("SELECT " + column + " FROM " + conn.Enquote(table) + " " + where);
            try {
                while (reader.Read()) {
                    object t = reader[0];
                    if (reader[0] is DBNull)
                        result.Add(default(long));
                    else {
                        if (reader[0] as long? != null)
                            result.Add((long?)reader[0]);
                        else if (reader[0] as int? != null)
                            result.Add((int?)reader[0]);
                        else
                            result.Add(default(long));
                    }
                }
            } finally {
                if (reader != null && !reader.IsClosed) {
                    reader.Close();
                }
            }

            return result;
        }
    }
}
