using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Utils
{
	public static class StringUtils
	{
		private static readonly string[] DisallowedChars = new string[15]
		{
			"#", "\"", "/", ";", "!", "?", "%", "^", "`", "=",
			"~", "<", ">", "|", ","
		};

		public static string GetPasswordHash(string password, string salt)
		{
			SHA1 cspSha512 = new SHA1CryptoServiceProvider();
			ASCIIEncoding enc = new ASCIIEncoding();
			string result = ByteArrayToString(cspSha512.ComputeHash(enc.GetBytes(password + salt)));
			cspSha512.Dispose();
			return result;
		}

		public static bool VerifyPassword(string password, string salt, string hashValue)
		{
			return hashValue.Equals(GetPasswordHash(password, salt));
		}

		private static byte[] EncryptString(byte[] clearText, byte[] key, byte[] iv)
		{
			MemoryStream memoryStream = new MemoryStream();
			Rijndael alg = Rijndael.Create();
			alg.Key = key;
			alg.IV = iv;
			CryptoStream cryptoStream = new CryptoStream(memoryStream, alg.CreateEncryptor(), CryptoStreamMode.Write);
			cryptoStream.Write(clearText, 0, clearText.Length);
			cryptoStream.Close();
			return memoryStream.ToArray();
		}

		public static string EncryptString(string clearText, string password)
		{
			byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
			PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[13]
			{
				73, 118, 97, 110, 32, 77, 101, 100, 118, 101,
				100, 101, 118
			});
			return Convert.ToBase64String(EncryptString(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16)));
		}

		public static string EncryptString(string clearText, string salt, string password)
		{
			byte[] clearBytes = Encoding.Unicode.GetBytes(clearText + salt);
			PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[13]
			{
				73, 118, 97, 110, 32, 77, 101, 100, 118, 101,
				100, 101, 118
			});
			return Convert.ToBase64String(EncryptString(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16)));
		}

		private static byte[] DecryptString(byte[] cipherData, byte[] key, byte[] iv)
		{
			MemoryStream memoryStream = new MemoryStream();
			Rijndael alg = Rijndael.Create();
			alg.Key = key;
			alg.IV = iv;
			CryptoStream cryptoStream = new CryptoStream(memoryStream, alg.CreateDecryptor(), CryptoStreamMode.Write);
			cryptoStream.Write(cipherData, 0, cipherData.Length);
			cryptoStream.Close();
			return memoryStream.ToArray();
		}

		public static string DecryptString(string cipherText, string password)
		{
			byte[] cipherBytes = Convert.FromBase64String(cipherText);
			PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[13]
			{
				73, 118, 97, 110, 32, 77, 101, 100, 118, 101,
				100, 101, 118
			});
			byte[] decryptedData = DecryptString(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
			return Encoding.Unicode.GetString(decryptedData);
		}

		public static string DecryptString(string cipherText, string salt, string password)
		{
			byte[] cipherBytes = Convert.FromBase64String(cipherText);
			PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, new byte[13]
			{
				73, 118, 97, 110, 32, 77, 101, 100, 118, 101,
				100, 101, 118
			});
			byte[] decryptedData = DecryptString(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
			string sDecrypted = Encoding.Unicode.GetString(decryptedData);
			return sDecrypted.Substring(0, sDecrypted.Length - salt.Length);
		}

		public static string ByteArrayToString(byte[] arrInput)
		{
			StringBuilder sOutput = new StringBuilder(arrInput.Length * 2);
			for (int i = 0; i < arrInput.Length; i++)
			{
				sOutput.Append(arrInput[i].ToString("X2"));
			}
			return sOutput.ToString();
		}

		public static string CreateKey(string company, string serial)
		{
			string tmpKey = EncryptString(company.ToLower().Replace(" ", "") + serial.ToLower().Replace(" ", ""), "dog82cg!&f2§gksovFGJHOETvwt8$631g");
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			ASCIIEncoding enc = new ASCIIEncoding();
			string hash = ByteArrayToString(mD5CryptoServiceProvider.ComputeHash(enc.GetBytes(tmpKey)));
			mD5CryptoServiceProvider.Dispose();
			string key = string.Empty;
			for (int i = 0; i < 32; i += 2)
			{
				key += hash[i];
				if (i == 6)
				{
					key += "-";
				}
				if (i == 14)
				{
					key += "-";
				}
				if (i == 22)
				{
					key += "-";
				}
			}
			return key;
		}

		public static string Left(string value, int maxLength)
		{
			if (value.Length >= maxLength)
			{
				return value.Substring(0, maxLength);
			}
			return value;
		}

		public static string FormatTimeSpan(TimeSpan duration)
		{
			if (duration.Days != 0)
			{
				return duration.Days + "d " + duration.Hours + "h " + duration.Minutes + "m " + duration.Seconds + "s";
			}
			if (duration.Hours != 0)
			{
				return duration.Hours + "h " + duration.Minutes + "m " + duration.Seconds + "s";
			}
			if (duration.Minutes != 0)
			{
				return duration.Minutes + "m " + duration.Seconds + "s";
			}
			return duration.Seconds + "s";
		}

		public static string EscapeFileName(string filename)
		{
			return DisallowedChars.Aggregate(filename, (string current, string disallowedChar) => current.Replace(disallowedChar, "_"));
		}

		public static int? ToNullableInt(this string s)
		{
			if (int.TryParse(s, out var i))
			{
				return i;
			}
			return null;
		}
	}
}
