// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since:  2012-01-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Config.Interfaces.DbStructure;
using Utils;
using Business.Enums;

namespace Business.Interfaces {
    public interface IImportDbStructureProgress {

        ObservableCollectionAsync<string> ErrorMessages { get; }

        /// <summary>
        /// Actual step of the export process.
        /// </summary>
        ImportDbStructureSteps Step { get; set; }

        /// <summary>
        /// Caption of the actiual export precess stepp.
        /// </summary>
        string StepCaption { get; }

        /// <summary>
        /// Number of tables, that should be exported.
        /// </summary>
        int TablesTotal { get; set; }

        /// <summary>
        /// Number of processed tables. 
        /// </summary>
        int TablesFinished { get; }

        int WorkingTasks { get; set; }

        /// <summary>
        /// Detail progress for tables, that are actually processed.
        /// </summary>
        IEnumerable<IImportDbStructureTableProgressWrapper> ActualProcessedTables { get; }

        IImportDbStructureTableProgress AddProcessedTable(ITable table);

        void RemoveProcessedTable(ITable table);

        void AddErrorMessage(string message);
    }
}
