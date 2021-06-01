// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since:  2012-01-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
namespace Business.Interfaces {
    public interface IImporterDbStructure {
        IImportDbStructureProgress ImportProgress { get; }
        event EventHandler Finished;
        void Start();
        void Cancel();
    }
}
