using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Config.Interfaces.DbStructure;
using DbAccess.Structures;

namespace Business.Interfaces {
    internal interface IMetadataAgent {
        void AddTables(IInputAgent importAgent, List<DbTableInfo> tables, IImportDbStructureProgress importProgress);
        void ImportTable(DbTableInfo table, IProfile profile, IImportDbStructureProgress importProgress);
        void Cleanup();
    }
}
