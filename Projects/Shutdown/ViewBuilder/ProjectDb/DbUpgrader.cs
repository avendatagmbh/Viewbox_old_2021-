using System;
using DbAccess;
using DbAccess.Structures;

namespace ProjectDb
{
    public class DbUpgrader
    {
        private string _upgradedToVersion;

        public DbUpgrader()
        {
            _upgradedToVersion = String.Empty;
        }

        /// <summary>
        ///   Upgrades the database to the latest version
        /// </summary>
        /// <param name="dbConfig"> database configuration </param>
        /// <returns> </returns>
        public string UpgradeDb(DbConfig dbConfig)
        {
            using (IDatabase database = ConnectionManager.CreateConnection(dbConfig))
            {
                database.Open();
                string currentVersion =
                    (string) database.ExecuteScalar("SELECT value FROM info WHERE `key` = 'Version' LIMIT 1");
                string originalVersion = currentVersion;
                while (VersionInfo.VersionToDouble(VersionInfo.Instance.CurrentDbVersion) >
                       VersionInfo.VersionToDouble(currentVersion))
                {
                    currentVersion = VersionInfo.Instance.GetNextDbVersion(currentVersion);
                    UpgradeToNextVersion(database, currentVersion);
                }
                if (VersionInfo.VersionToDouble(currentVersion) > VersionInfo.VersionToDouble(originalVersion))
                    UpdateVersion(database);
            }
            return _upgradedToVersion;
        }

        /// <summary>
        ///   Updates Version in table Info
        /// </summary>
        /// <param name="connection"> </param>
        private void UpdateVersion(IDatabase connection)
        {
            string insertString = string.Format("UPDATE info SET value = '{0}' WHERE `key` = 'Version';",
                                                _upgradedToVersion);
            connection.ExecuteNonQuery(insertString);
        }

        /// <summary>
        ///   Checks whether the given field exists in the database
        /// </summary>
        /// <param name="database"> database </param>
        /// <param name="tableName"> table name </param>
        /// <param name="fieldName"> field name </param>
        /// <returns> true if the field exists </returns>
        private bool FieldExist(IDatabase database, string tableName, string fieldName)
        {
            string command = string.Format(
                @"SELECT count(*)
                FROM 
                 information_schema.COLUMNS
                WHERE
                 TABLE_SCHEMA = '{0}'
                AND
                 TABLE_NAME = '{1}'
                AND
                 COLUMN_NAME = '{2}'",
                database.DbConfig.DbName, tableName, fieldName);

            return (Int64) database.ExecuteScalar(command) == 1 ? true : false;
        }

        #region Version Upgrade

        /// <summary>
        ///   Gets the next version from the current one and calls the corresponding update method
        /// </summary>
        /// <param name="connection"> open connection </param>
        /// <param name="currentVersion"> the current version from which the next one gets estimated </param>
        private void UpgradeToNextVersion(IDatabase connection, string currentVersion)
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

        private void UpgradeTo102(IDatabase database)
        {
            if (!FieldExist(database, "viewscripts", "fileName"))
                database.ExecuteNonQuery("ALTER TABLE viewscripts ADD fileName VARCHAR(2048);");
            _upgradedToVersion = "1.0.2";
        }

        private void UpgradeTo103(IDatabase database)
        {
            if (!FieldExist(database, "views", "fileName"))
                database.ExecuteNonQuery("ALTER TABLE views ADD fileName VARCHAR(2048) AFTER id;");
            _upgradedToVersion = "1.0.3";
        }

        private void UpgradeTo104(IDatabase database)
        {
            if (!FieldExist(database, "views", "state"))
                database.ExecuteNonQuery("ALTER TABLE views ADD state INT;");
            if (!FieldExist(database, "views", "error"))
                database.ExecuteNonQuery("ALTER TABLE views ADD error LONGTEXT;");
            if (!FieldExist(database, "views", "warnings"))
                database.ExecuteNonQuery("ALTER TABLE views ADD warnings LONGTEXT;");
            _upgradedToVersion = "1.0.4";
        }

        private void UpgradeTo105(IDatabase database)
        {
            if (!FieldExist(database, "viewscripts", "parsingState"))
                database.ExecuteNonQuery("ALTER TABLE viewscripts ADD parsingState BIT;");
            if (!FieldExist(database, "viewscripts", "parsingError"))
                database.ExecuteNonQuery("ALTER TABLE viewscripts ADD parsingError VARCHAR(4096);");
            _upgradedToVersion = "1.0.5";
        }

        #endregion
    }
}