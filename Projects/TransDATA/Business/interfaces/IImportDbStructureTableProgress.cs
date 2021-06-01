// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since:  2011-01-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

namespace Business.Interfaces {
    public interface IImportDbStructureTableProgress {
        string TableName { get; }
        string Filter { get; }
        long DatasetsTotal { get; }
        long DataSetsProcessed { get; }
    }
}
