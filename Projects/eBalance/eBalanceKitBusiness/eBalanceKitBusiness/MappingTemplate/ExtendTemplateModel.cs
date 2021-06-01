// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-18
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using PdfGenerator;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;
using eBalanceKitResources.Localisation;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Document = eBalanceKitBusiness.Structures.DbMapping.Document;
using System.Linq;
using IElement = Taxonomy.IElement;

namespace eBalanceKitBusiness.MappingTemplate {
    public class ExtendTemplateModel : TemplateModelBase {
        public ExtendTemplateModel(Document document, MappingTemplateHead template)
            : base(document, template) {

            AssignmentErrors = new ObservableCollection<AssignmentErrorListEntry>();
            AssignmentConflicts = new ObservableCollection<AssignmentConflictListEntry>(); 
            AssignmentErrors.CollectionChanged += AssignmentErrorsOnCollectionChanged;
            AssignmentConflicts.CollectionChanged += AssignmentConflictsOnCollectionChanged;

            foreach (var assignment in Template.Assignments)
                _assignments.Add(new MappingHeaderGui(assignment));
            
            PresentationTrees = MappingTemplatePresentationTrees.Create(Template, Assignments);
        }

        private void AssignmentErrorsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            OnPropertyChanged("HasErrors");
        }

        private void AssignmentConflictsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            OnPropertyChanged("HasConflicts");
        }

        #region Assignments
        private readonly List<MappingHeaderGui> _assignments = new List<MappingHeaderGui>();
        public List<MappingHeaderGui> Assignments { get { return _assignments; } }
        #endregion Assignments

        internal IEnumerable<IPresentationTree> PresentationTrees { get; set; }

        public bool ReplaceAutoComputeEnabledFlag { get; set; }
        public bool ReplaceSendAccountBalanceFlag { get; set; }
        public bool ReplaceIgnoreWarningMessageFlag { get; set; }

        public ObservableCollection<AssignmentErrorListEntry> AssignmentErrors { get; private set; }
        public ObservableCollection<AssignmentConflictListEntry> AssignmentConflicts { get; private set; }

        public bool HasErrors { get { return AssignmentErrors != null && AssignmentErrors.Count > 0 && AssignmentErrors.Any(e => e.Errors.Count > 0); } }
        public bool HasConflicts { get { return AssignmentConflicts != null && AssignmentConflicts.Count > 0 && AssignmentConflicts.Any(c => c.Conflicts.Count > 0); } }

        public void SaveResults(string fileName) {
            var font = new Font(Font.FontFamily.HELVETICA, 8);
            try {
                var traits = new PdfTraits(ResourcesCommon.eBalanceKit, ResourcesCommon.eBalanceKit) { MarginLeft = 20, MarginRight = 20 };
                var pdfDocument = new PdfGenerator.PdfGenerator(traits);
                var headerPhrase = new Phrase(ResourcesCommon.eBalanceKitErrorReport, pdfDocument.Traits.fontH1);

                foreach (var assignmentErrorListEntry in AssignmentErrors) {
                    pdfDocument.AddHeadline(assignmentErrorListEntry.BalanceList.Name);
                    var table = new PdfPTable(1) { HorizontalAlignment = Element.ALIGN_LEFT };
                    table.DefaultCell.Border = PdfPCell.NO_BORDER;
                    table.DefaultCell.BackgroundColor = null;
                    table.AddCell(new PdfPCell { Border = PdfPCell.NO_BORDER });
                    int i = 1;
                    foreach (var assignmentError in assignmentErrorListEntry.Errors) {
                        table.AddCell(new PdfPCell { Border = PdfPCell.NO_BORDER });
                        table.AddCell(new PdfPCell { Border = PdfPCell.TOP_BORDER }); // separator line
                        table.AddCell(new Phrase(i + ". " + assignmentError.Message, font));
                        table.AddCell(new Phrase(assignmentError.AccountLabel, font));
                        table.AddCell(new Phrase(ResourcesCommon.TaxonomyPosition + assignmentError.Element.Label, font));
                        table.AddCell(new Phrase(assignmentError.Element.Id, font));
                        i++;
                    }
                    pdfDocument.CurrentChapter.Add(table);
                }

                pdfDocument.WriteFile(fileName, headerPhrase);
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }            
        }

        /// <summary>
        /// Saves the modifications into mthe template
        /// </summary>
        /// <param name="message">The error message if failed</param>
        /// <returns>True if successful</returns>
        public bool SaveTemplate(out string message) {
            List<IElement> positionsNotFound = null;
            if (ResolveConflicts(out positionsNotFound)) {
                TemplateManager.SaveTemplate(Template);
                message = null;
                return true;
            } else {
                message = string.Empty;
                foreach (IElement element in positionsNotFound) {
                    message += "\r\n" + element.MandatoryLabel;
                }
                message = string.Format(ResourcesCommon.FollowingPositionsNotFound, message);
                return false;
            }
        }

        #region [ Resolve conflicts methods ]

        private bool ResolveConflicts(out List<IElement> positionsNotFound) {
            List<MappingTemplateLine> assignmentsToModiy = Template.Assignments.ToList();
            List<MappingTemplateLine> removableAssignments = new List<MappingTemplateLine>();
            positionsNotFound = new List<IElement>();

            foreach (AssignmentConflictListEntry conflictListEntry in AssignmentConflicts) {
                foreach (AssignmentConflict conflict in conflictListEntry.Conflicts) {
                    // modify the template if mode is replace
                    if (conflict.Mode == TemplateAssignmentConfictResolveMode.Replace) {
                        IElement newPosition = conflict.Element;
                        IElement oldPosition = conflict.OldElement;
                        MappingTemplateLine assignmentToRemove = assignmentsToModiy.FirstOrDefault(a => a.ElementId == oldPosition.Id);
                        if (assignmentToRemove != null) {
                            MappingTemplateLine newAssignment = new MappingTemplateLine() {
                                Template = Template,
                                AccountNumber = assignmentToRemove.AccountNumber,
                                AccountName = assignmentToRemove.AccountName,
                                ElementId = newPosition.Id,
                                CreditElementId = assignmentToRemove.CreditElementId != null ? newPosition.Id : null,
                                DebitElementId = assignmentToRemove.DebitElementId != null ? newPosition.Id : null,
                                IsAccountOfExchange = false
                            };
                            removableAssignments.Add(assignmentToRemove);
                            if (assignmentsToModiy.Remove(assignmentToRemove))
                                assignmentsToModiy.Add(newAssignment);
                        } else
                            positionsNotFound.Add(oldPosition);
                    }
                }
            }

            // if no error, then replace the assignment list
            if (positionsNotFound.Count == 0) { 
                // delete the removed assignments manually because the data access layer is not able to do it
                using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                    try {
                        conn.BeginTransaction();
                        foreach (MappingTemplateLine removableAssignment in removableAssignments) {
                            conn.DbMapping.Delete(removableAssignment);
                        }
                        conn.CommitTransaction();
                    } catch (Exception ex) {
                        throw new Exception(ExceptionMessages.TemplateManagerUpdate + ex.Message);
                    }
                }
                Template.Assignments = new ObservableCollection<MappingTemplateLine>(assignmentsToModiy);
            }

            return positionsNotFound.Count == 0;
        }

        #endregion [ Resolve conflicts methods ]
    }
}
