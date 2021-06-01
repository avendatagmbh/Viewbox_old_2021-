using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using DataAnalyze.ComplexConfigurations;
using DataAnalyze.Model;
using Microsoft.Win32;
using PdfGenerator;
using Utils;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Controls.BalanceList {
    /// <summary>
    /// Interaction logic for BalListImpAssistViewErrors.xaml
    /// </summary>
    public partial class BalListImpAssistViewErrors : BalListImpAssistPageBase {
        /// <summary>
        /// Initializes a new instance of the <see cref="BalListImpAssistViewErrors" /> class.
        /// </summary>
        public BalListImpAssistViewErrors() { InitializeComponent(); }

        /// <summary>
        /// Handles the Click event of the btn_ExportToCsv control.
        /// It handles the exporting the problems to CSV format.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btn_ExportToCsv_Click(object sender, RoutedEventArgs e) {
            try {
                String SaveTo = ShowFileDialog(".csv", ResourcesCommon.CsvDescription + " (.csv)|*.csv");

                if (SaveTo != null) {
                    using (var writer = new CsvWriter(SaveTo, Encoding.Default)) {

                        // Writes the header line
                        writer.WriteCsvData(
                            ResourcesValidation.TableHeaderID,
                            ResourcesValidation.TableHeaderRowNumber,
                            ResourcesValidation.TableHeaderDate,
                            ResourcesValidation.TableHeaderSeverity,
                            ResourcesValidation.TableHeaderProblem,
                            ResourcesValidation.TableHeaderRowDump);

                        // Writes the data.
                        int i = 1;
                        foreach (Anomaly an in BalanceListImportValidator.SkippableAnomalies) {
                            writer.WriteCsvData(
                                i.ToString() // ID
                                , (an.RowNumber + 1).ToString() // Row no.
                                , an.CreationDate.ToString() // Date
                                , an.Severity.ToString() // Severity
                                , an.NiceProblem // Problem
                                , an.Dump());
                            i++;
                        }
                    }

                    // Open directory that contains the exported document.
                    Process.Start(new FileInfo(SaveTo).DirectoryName);
                }
            } catch (Exception) {
                MessageBox.Show(
                    ResourcesCommon.CannotSaveFileMessage,
                    ResourcesCommon.CannotSaveFileTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the btn_ExportToPdf control.
        /// It handles the exporting the problems to PDf format.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btn_ExportToPdf_Click(object sender, RoutedEventArgs e) {
            try {
                String SaveTo = ShowFileDialog(".pdf", ResourcesCommon.PdfDescription + " (.pdf)|*.pdf");

                if (SaveTo != null) {
                    // Initialize an instance for creating pdf.
                    var traits = new PdfTraits(
                        ResourcesCommon.eBalanceKit,
                        ResourcesCommon.eBalanceKit);
                    var pdf = new PdfGenerator.PdfGenerator(traits);

                    // Set up headline.
                    pdf.AddHeadline(ResourcesCommon.ImportErrors);

                    // Add errors table.
                    pdf.AddTable(BalanceListImportValidator.SkippableAnomaliesTable, true, 10);

                    // Export to file.
                    pdf.WriteFile(SaveTo, ResourcesCommon.ImportErrors);

                    // Open directory that contains the exported document.
                    Process.Start(new FileInfo(SaveTo).DirectoryName);
                }
            } catch (Exception) {
                MessageBox.Show(
                    ResourcesCommon.CannotSaveFileMessage,
                    ResourcesCommon.CannotSaveFileTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Shows the file dialog.
        /// </summary>
        /// <param name="defaultExtension">The default extension.</param>
        /// <param name="defaultFilter">The default filter.</param>
        /// <returns>The file path to export, or null if canceled.</returns>
        private String ShowFileDialog(String defaultExtension, String defaultFilter) {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.DefaultExt = defaultExtension;
            dlg.Filter = defaultFilter;
            dlg.FileName = "Import Errors - " + DateTime.Today.Year + "-" + DateTime.Today.Month
                           + "-" + DateTime.Today.Day + defaultExtension;

            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }
    }
}