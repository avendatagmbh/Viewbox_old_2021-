using System;
using DbAccess;
using DbAccess.Structures;

namespace ViewboxBusiness.ProfileDb
{
	public class DbUpgrader
	{
		private string _upgradedToVersion;

		public DbUpgrader()
		{
			_upgradedToVersion = string.Empty;
		}

		public string UpgradeDb(DbConfig dbConfig)
		{
			using (DatabaseBase database = ConnectionManager.CreateConnection(dbConfig))
			{
				database.Open();
				string currentVersion = (string)database.ExecuteScalar("SELECT value FROM info WHERE `key` = 'Version' LIMIT 1");
				string originalVersion = currentVersion;
				while (VersionInfo.VersionToDouble(VersionInfo.Instance.CurrentDbVersion) > VersionInfo.VersionToDouble(currentVersion))
				{
					currentVersion = VersionInfo.Instance.GetNextDbVersion(currentVersion);
					UpgradeToNextVersion(database, currentVersion);
				}
				if (VersionInfo.VersionToDouble(currentVersion) > VersionInfo.VersionToDouble(originalVersion))
				{
					UpdateVersion(database);
				}
			}
			return _upgradedToVersion;
		}

		private void UpdateVersion(DatabaseBase connection)
		{
			string insertString = $"UPDATE info SET value = '{_upgradedToVersion}' WHERE `key` = 'Version';";
			connection.ExecuteNonQuery(insertString);
		}

		private bool FieldExist(DatabaseBase database, string tableName, string fieldName)
		{
			string command = $"SELECT count(*)\r\n                FROM \r\n                 information_schema.COLUMNS\r\n                WHERE\r\n                 TABLE_SCHEMA = '{database.DbConfig.DbName}'\r\n                AND\r\n                 TABLE_NAME = '{tableName}'\r\n                AND\r\n                 COLUMN_NAME = '{fieldName}'";
			return (long)database.ExecuteScalar(command) == 1;
		}

		private void UpgradeToNextVersion(DatabaseBase connection, string currentVersion)
		{
			switch (currentVersion)
			{
			case "1.0.2":
				UpgradeTo102(connection);
				break;
			case "1.0.3":
				UpgradeTo103(connection);
				break;
			case "1.0.4":
				UpgradeTo104(connection);
				break;
			case "1.0.5":
				UpgradeTo105(connection);
				break;
			default:
				throw new Exception("Invalid version: " + currentVersion);
			}
		}

		private void UpgradeTo102(DatabaseBase database)
		{
			if (!FieldExist(database, "viewscripts", "fileName"))
			{
				database.ExecuteNonQuery("ALTER TABLE viewscripts ADD fileName VARCHAR(2048);");
			}
			_upgradedToVersion = "1.0.2";
		}

		private void UpgradeTo103(DatabaseBase database)
		{
			if (!FieldExist(database, "views", "fileName"))
			{
				database.ExecuteNonQuery("ALTER TABLE views ADD fileName VARCHAR(2048) AFTER id;");
			}
			_upgradedToVersion = "1.0.3";
		}

		private void UpgradeTo104(DatabaseBase database)
		{
			if (!FieldExist(database, "views", "state"))
			{
				database.ExecuteNonQuery("ALTER TABLE views ADD state INT;");
			}
			if (!FieldExist(database, "views", "error"))
			{
				database.ExecuteNonQuery("ALTER TABLE views ADD error LONGTEXT;");
			}
			if (!FieldExist(database, "views", "warnings"))
			{
				database.ExecuteNonQuery("ALTER TABLE views ADD warnings LONGTEXT;");
			}
			_upgradedToVersion = "1.0.4";
		}

		private void UpgradeTo105(DatabaseBase database)
		{
			if (!FieldExist(database, "viewscripts", "parsingState"))
			{
				database.ExecuteNonQuery("ALTER TABLE viewscripts ADD parsingState BIT;");
			}
			if (!FieldExist(database, "viewscripts", "parsingError"))
			{
				database.ExecuteNonQuery("ALTER TABLE viewscripts ADD parsingError VARCHAR(4096);");
			}
			_upgradedToVersion = "1.0.5";
		}
	}
}
