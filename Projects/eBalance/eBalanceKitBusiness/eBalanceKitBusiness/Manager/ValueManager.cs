// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-07-28
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using DbAccess;
using Utils;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;


namespace eBalanceKitBusiness.Manager {
    public static class ValueManager {
        #region events
        public static event EventHandler<ErrorEventArgs> Error;

        private static void OnError(string message) { if (Error != null) Error(null, new ErrorEventArgs(message)); }
        #endregion events

        public static void SaveValues(IEnumerable<object> values) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.BeginTransaction();
                try {
                    foreach (var value in values) conn.DbMapping.Save(value);
                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        internal static void SaveValue(object value, LogManager.AddValueInfo info, bool useSeparateThread = true) {
            if (useSeparateThread)
                new Thread(SaveValueFun) { CurrentCulture = Thread.CurrentThread.CurrentCulture, CurrentUICulture = Thread.CurrentThread.CurrentUICulture }.Start(new Tuple<object, LogManager.AddValueInfo>(value, info));
            else SaveValueFun(value);
        }

        private static void SaveValueFun(object valueTuple) {
            var tuple = valueTuple as Tuple<object, LogManager.AddValueInfo>;
            using (IDatabase conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    Debug.Assert(tuple != null, "tuple != null");
                    conn.DbMapping.Save(tuple.Item1);

                    if (tuple.Item2 != null) LogManager.Instance.UpdateValue(tuple.Item2);

                } catch (Exception ex) {
                    OnError(ex.Message);
                    // TODO: handle error event
                }
            }
        }

        internal static void RemoveValues(IEnumerable<object> values) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.BeginTransaction();
                try {
                    foreach (var value in values) conn.DbMapping.Delete(value);

                    conn.CommitTransaction();
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message, ex);
                }
            }
        }
    }
}