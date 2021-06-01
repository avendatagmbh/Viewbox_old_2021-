// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-04-17
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Export;

namespace eBalanceKitBusiness.Options {
    public interface IOptions {
        /// <summary>
        /// Hide or show only information for the current selected legal form.
        /// </summary>
        bool ShowSelectedLegalForm { get; set; }

        /// <summary>
        /// Hide or show the marked warnings after validation.
        /// </summary>
        bool HideChosenWarnings { get; set; }
        
        /// <summary>
        /// Hide or show warnings after validation.
        /// </summary>
        bool HideAllWarnings { get; set; }
        
        /// <summary>
        /// Hide or show only information for the current selected TypeOperatingResult (GKV/UKV)
        /// </summary>
        bool ShowTypeOperatingResult { get; set; }
        
        /// <summary>
        /// Hide or show only information for the current selected taxonomy.
        /// </summary>
        bool ShowSelectedTaxonomy { get; set; }

        /// <summary>
        /// Hide or show only mandatory information.
        /// </summary>
        bool ShowOnlyMandatoryPostions { get; set; }

        /// <summary>
        /// Enable or disable audit mode. With enabled audit mode futher options to add tax audit based value corrections are provided.
        /// </summary>
        bool AuditModeEnabled { get; set; }

        /// <summary>
        /// Information if the page "Would you like to insert additional company information?" in the ManagementAssistant will be shown.
        /// </summary>
        bool ManagementAssistantAskAgain { get; set; }

        #region Methods
        /// <summary>
        /// Save the current configured options in the database.
        /// </summary>
        /// <returns>Was there a change?</returns>
        bool SaveConfiguration();

        void GetPdfExportOptionsFromDatabase(IExportConfig config);

        void SetPdfExportOptionsValue(IExportConfig config);

        /// <summary>
        /// Resets the options by reseting the XmlDocument.InnerXml.
        /// </summary>
        void Reset(); 
        #endregion
    }
}