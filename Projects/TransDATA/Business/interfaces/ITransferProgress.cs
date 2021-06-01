// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using Business.Enums;
using Config.Interfaces.DbStructure;
using DbAccess.Interaction;
using Utils;
using System.Diagnostics;

namespace Business.Interfaces {
    public interface ITransferProgress {

        ObservableCollectionAsync<string> ErrorMessages { get; }

        /// <summary>
        /// Actual step of the export process.
        /// </summary>
        TransferProgressSteps Step { get; set; }

        /// <summary>
        /// Caption of the actiual export precess stepp.
        /// </summary>
        string StepCaption { get; set; }

        /// <summary>
        /// Number of tables, that should be exported.
        /// </summary>
        int EntitiesTotal { get; set; }

        int WorkingTasks { get; set; }

        #region Performance
        string CpuPerformanceLabel { get; }
        string RamPerformanceLabel { get; }
        string HDDPerformanceLabel { get; }
        double CpuPerformance { get; }
        double RamPerformance { get; }
        long HDDPerformance { get; }
        void UpdatePerformace();
        #endregion

        /// <summary>
        /// Number of processed tables. 
        /// </summary>
        int EntitiesFinished { get; set; }

        List<ITransferEntity> EntityFinishedList { get; set; }

        /// <summary>
        /// Detail progress for tables, that are actually processed.
        /// </summary>
        IEnumerable<ITransferTableProgressWrapper> ActualProcessedTables { get; }
        
        /// <summary>
        /// Actual progress in index.xml generation.
        /// </summary>
        int ProgressIndexXmlGenerationValue { get; set; }
        
        /// <summary>
        /// Maximum number of steps in index.xml generation.
        /// </summary>
        int ProgressIndexXmlGenerationMax { get; set; }

        void AddErrorMessage(string message);

        ITransferTableProgress AddProcessedEntity(ITransferEntity entity);

        void RemoveProcessedEntity(ITransferEntity entity);

        void InitHDDObservation(DirectoryInfo di);
    }
}