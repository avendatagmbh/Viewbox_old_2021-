/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-09-30      adapted from other project
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Utils {

    /// <summary>
    /// Utility class for string functions.
    /// </summary>
    public static class StringUtils {

        /// <summary>
        /// Returns the hash value for the specified password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <returns></returns>
        public static string GetPasswordHash(string password, string salt) {
            System.Security.Cryptography.SHA1 cspSha512 = // workaround - SHA512 not supported for WinXP
                new System.Security.Cryptography.SHA1CryptoServiceProvider();

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            string hash = ByteArrayToString(cspSha512.ComputeHash(enc.GetBytes(password + salt)));
            
            cspSha512.Dispose();
            
            return hash;
        }

        /// <summary>
        /// Verifies if the specified password/salt combination fits to the given hash value.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="hashValue">The hash value.</param>
        /// <returns></returns>
        public static bool VerifyPassword(string password, string salt, string hashValue) {
            return hashValue.Equals(GetPasswordHash(password, salt));
        }

        /// <summary>
        /// Encrypts the string.
        /// </summary>
        /// <param name="clearText">The clear text.</param>
        /// <param name="Key">The key.</param>
        /// <param name="IV">The IV.</param>
        /// <returns></returns>
        private static byte[] EncryptString(byte[] clearText, byte[] Key, byte[] IV) {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearText, 0, clearText.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }

        /// <summary>
        /// Encrypts the string.
        /// </summary>
        /// <param name="clearText">The clear text.</param>
        /// <param name="Password">The password.</param>
        /// <returns></returns>
        public static string EncryptString(string clearText, string Password) {
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] encryptedData = EncryptString(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Encrypts the string.
        /// </summary>
        /// <param name="clearText">The clear text.</param>
        /// <param name="Password">The password.</param>
        /// <returns></returns>
        public static string EncryptString(string clearText, string salt, string Password) {
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText + salt);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] encryptedData = EncryptString(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Decrypts the string.
        /// </summary>
        /// <param name="cipherData">The cipher data.</param>
        /// <param name="Key">The key.</param>
        /// <param name="IV">The IV.</param>
        /// <returns></returns>
        private static byte[] DecryptString(byte[] cipherData, byte[] Key, byte[] IV) {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key;
            alg.IV = IV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        /// <summary>
        /// Decrypts the string.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="Password">The password.</param>
        /// <returns></returns>
        public static string DecryptString(string cipherText, string Password) {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] decryptedData = DecryptString(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return System.Text.Encoding.Unicode.GetString(decryptedData);
        }

        /// <summary>
        /// Decrypts the string.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <param name="Password">The password.</param>
        /// <returns></returns>
        public static string DecryptString(string cipherText, string salt, string Password) {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] decryptedData = DecryptString(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));

            string sDecrypted = System.Text.Encoding.Unicode.GetString(decryptedData);
            return sDecrypted.Substring(0, sDecrypted.Length - salt.Length);
        }


        /// <summary>
        /// Returs the hexadecimal representation for the specified byte array.
        /// </summary>
        /// <param name="arrInput">The byte array.</param>
        /// <returns></returns>
        public static string ByteArrayToString(byte[] arrInput) {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length * 2);
            for (i = 0; i < arrInput.Length; i++) {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }

        public static string CreateKey(string company, string serial) {
            string tmp = 
                company.ToLower().Replace(" ", "") + 
                serial.ToLower().Replace(" ", "");
            
            string tmpKey = Utils.StringUtils.EncryptString(tmp, "dog82cg!&f2§gksovFGJHOETvwt8$631g");

            System.Security.Cryptography.MD5 csp = new System.Security.Cryptography.MD5CryptoServiceProvider();
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            string hash = Utils.StringUtils.ByteArrayToString(csp.ComputeHash(enc.GetBytes(tmpKey)));
            csp.Dispose();

            string key = string.Empty;
            for (int i = 0; i < 32; i += 2) {
                key += hash[i];
                if (i == 6) key += "-";
                if (i == 14) key += "-";
                if (i == 22) key += "-";
            }
            return key;
        }

        public static string Left(string value, int maxLength) { return value.Length < maxLength ? value : value.Substring(0, maxLength); }

        public static string FormatTimeSpan(TimeSpan duration) {
            if (duration.Days != 0)
                return duration.Days + "d " + duration.Hours + "h " + duration.Minutes + "m " + duration.Seconds + "s";

            if (duration.Hours != 0)
                return duration.Hours + "h " + duration.Minutes + "m " + duration.Seconds + "s";

            if (duration.Minutes != 0)
                return duration.Minutes + "m " + duration.Seconds + "s";

            return duration.Seconds + "s";
        }

        private readonly static string[] _disallowedChars = {"#", "\"", "/", ";", "!", "?", "%", "^", "`", "=", "~", "<", ">", "|", "," };

        public static string EscapeFileName(string filename) {
            string ret = filename;
            foreach (var disallowedChar in _disallowedChars) {
                ret = ret.Replace(disallowedChar, "_");
            }
            return ret;
        }

    }
}
