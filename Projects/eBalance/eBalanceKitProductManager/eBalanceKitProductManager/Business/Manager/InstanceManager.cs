using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitProductManager.Business.Structures.DbMapping;
using System.Collections.ObjectModel;

namespace eBalanceKitProductManager.Business.Manager {
    
    internal static class InstanceManager {

        static InstanceManager() {
            Instances = new ObservableCollection<Instance>();
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                var tmp = conn.DbMapping.LoadSorted<Instance>("company_name");
                foreach (var instance in tmp) {
                    Instances.Add(instance);
                }
            }
        }

        public static ObservableCollection<Instance> Instances { get; private set; }

        public static Instance AddInstance() {
            Instance instance = new Instance();
            instance.SerialNumber = GenerateSerial();
            instance.Timestamp = System.DateTime.Now;
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.DbMapping.Save(instance);
            }
            Instances.Add(instance);
            return instance;
        }

        public static void DeleteInstance(Instance instance) {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.DbMapping.Delete(instance);
            }
            Instances.Remove(instance);
        }

        private static string GenerateSerial() {
            string tmpSerial = Guid.NewGuid().ToString();
            System.Security.Cryptography.MD5 csp = new System.Security.Cryptography.MD5CryptoServiceProvider();
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            string hash = ByteArrayToString(csp.ComputeHash(enc.GetBytes(tmpSerial)));
            csp.Dispose();

            string serial = string.Empty;
            for (int i = 0; i < 32; i += 2) {
                serial += hash[i];
                if (i == 6) serial += "-";
                if (i == 14) serial += "-";
                if (i == 22) serial += "-";
            }

            return serial;

        }

        private static string ByteArrayToString(byte[] arrInput) {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length * 2);
            for (i = 0; i < arrInput.Length; i++) {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }



        public static void SaveInstances() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                foreach (var instance in Instances) conn.DbMapping.Save(instance);
            }
        }
    }
}
