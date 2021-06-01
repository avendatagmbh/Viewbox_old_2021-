// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-14
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Business.Interfaces {
    public interface IImportDbStructureTableProgressWrapper {
        IImportDbStructureTableProgress Progress { get; set; }
    }
}