// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-05-25
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using eBalanceKitBusiness.AuditCorrections;
using eBalanceKitBusiness.Structures.DbMapping;
using PdfGenerator;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace eBalanceKitBusiness.Structures {
    /// <summary>
    /// Class that contains a list of <see cref="ProblemCategory"/> called <see cref="Categories"/> and is able to create a PDF representation of the Categories and problems.
    /// </summary>
    public class ProblemSummary : INotifyPropertyChanged {
        
        public ProblemSummary() {
            CategoriesDict = new Dictionary<string, ProblemCategory>();
        }


        public ProblemSummary(IEnumerable<ProblemCategory> categories, string topic) : this(topic) {
            foreach (var problemCategory in categories) {
                CategoriesDict.Add(problemCategory.Name, problemCategory);
            }
        }

        public ProblemSummary(string topic) :this() { Topic = topic; }


        /// <summary>
        /// List of <see cref="ProblemCategory"/>.
        /// </summary>
        public List<ProblemCategory> Categories { get { return CategoriesDict.Values.ToList(); } }

        private Dictionary<string, ProblemCategory> _categories;
        private Dictionary<string, ProblemCategory> CategoriesDict { get { return _categories; } set { _categories = value; OnPropertyChanged("Categories");  } }
        
        /// <summary>
        /// Topic of the problems
        /// </summary>
        public string Topic { get; set; }


        #region PDF Creation stuff

        private PdfGenerator.PdfGenerator _pdfGenerator;

        private void GeneratePdfContent() {
            
            foreach (var problemCategory in Categories) {
                problemCategory.GeneratePdf(_pdfGenerator);
            }

        }

        /// <summary>
        /// Creating a Pdf and saving it at the given destination.
        /// </summary>
        /// <param name="path">Destination file path + name. (eg. C:\User\sev\Documents\problem.pdf)</param>
        /// <returns></returns>
        public bool GeneratePdf(string path) {
            PdfTraits traits = new PdfTraits("eBilanz-Kit", "eBilanz-Kit");
            traits.MarginLeft = 20;
            traits.MarginRight = 20;
            _pdfGenerator = new PdfGenerator.PdfGenerator(traits);

            _pdfGenerator.AddHeadline(Topic ?? "ProblemSummary");
            GeneratePdfContent();
            _pdfGenerator.WriteFile(path, Topic ?? "ProblemSummary");
            return true;
        }
        #endregion

        /// <summary>
        /// Add a <see cref="ProblemCategory"/> to this <see cref="ProblemSummary"/>
        /// </summary>
        /// <param name="category">The new Category</param>
        public void AddCategory(ProblemCategory category) {
            if (category == null) {
                return;
            }
            if (CategoriesDict.ContainsKey(category.Name) && CategoriesDict[category.Name] == category) {
                return;
            }
            if (CategoriesDict.ContainsKey(category.Name) && CategoriesDict[category.Name] != category) {
#if DEBUG
                throw new System.Exception();
#else
                return;
#endif
            }
            CategoriesDict.Add(category.Name, category);
        }

        /// <summary>
        /// Add a problem to problemEntry.Category.
        /// </summary>
        /// <param name="problemEntry">The new entry.</param>
        public void AddProblem(ProblemEntry problemEntry) {
            if (CategoriesDict.ContainsValue(problemEntry.Category)) {
                CategoriesDict[problemEntry.Category.Name].AddProblem(problemEntry);
            } else {
                CategoriesDict.Add(problemEntry.Category.Name, problemEntry.Category);
                CategoriesDict[problemEntry.Category.Name].AddProblem(problemEntry);
            }
        }

        #region Implementation of INotifyPropertyChanged

        private PropertyChangedEventHandler _propertyChanged;
        public event PropertyChangedEventHandler PropertyChanged {
            add { this._propertyChanged += value; }
            remove { this._propertyChanged -= value; }
        }

        protected virtual void OnPropertyChanged(string propertyName) {
            if (_propertyChanged != null) _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class ProblemCategory {
        public List<ProblemEntry> Entries { get; set; }
        public string Name { get; set; }

        public ProblemCategory() { Entries = new List<ProblemEntry>(); }
        public ProblemCategory(string name) : this() { Name = name; }
        
        public ProblemCategory(string name, ProblemEntry entry) :this(name) {
            Entries.Add(entry);
        }

        public ProblemCategory(string name, IEnumerable<ProblemEntry> entries) :this(name) {
            Entries.AddRange(entries);
        }

        
        /// <summary>
        /// Add a <see cref="ProblemEntry"/> to this <see cref="ProblemCategory"/>
        /// </summary>
        /// <param name="problemEntry">The new problem.</param>
        public void AddProblem(ProblemEntry problemEntry) {
            Entries.Add(problemEntry);
        }
        /// <summary>
        /// Add a list of <see cref="ProblemEntry"/> to this <see cref="ProblemCategory"/>
        /// </summary>
        /// <param name="problemEntry">The list of new problems.</param>
        public void AddProblem(IEnumerable<ProblemEntry> problemEntry) {
            Entries.AddRange(problemEntry);
        }

        /// <summary>
        /// Adding a subheadline for this category (Name) and a new SubSubHeadline for each <see cref="ProblemEntry"/> with different problementry.HeadLine.
        /// Add the problementry.Descritpion as Content.
        /// </summary>
        public void GeneratePdf(PdfGenerator.PdfGenerator pdfGenerator) {
            pdfGenerator.AddSubHeadline(this.Name);
            foreach (var problemEntry in Entries) {
                if (pdfGenerator.CurrentSubSection == null || !pdfGenerator.CurrentSubSection.GetBookmarkTitle().Content.EndsWith(problemEntry.HeadLine)) {
                    pdfGenerator.AddSubSubHeadline(problemEntry.HeadLine);
                }
                PdfPTable t = GetNewPdfPTable();
                t.AddCell(problemEntry.Description);
                pdfGenerator.CurrentSection.Add(t);
            }
        }

        /// <summary>
        /// Method to get a default <see cref="PdfPTable"/>
        /// </summary>
        private PdfPTable GetNewPdfPTable() {
            PdfPTable table = new PdfPTable(1) { HorizontalAlignment = Element.ALIGN_LEFT };
            table.DefaultCell.Border = PdfPCell.NO_BORDER;
            table.DefaultCell.BackgroundColor = null;
            table.AddCell(new PdfPCell { Border = PdfPCell.NO_BORDER });

            return table;
        }

    }

    /// <summary>
    /// General class that contains <see cref="HeadLine"/> and <see cref="Description"/>.
    /// Visualized in CtlProblemOverview
    /// </summary>
    public class ProblemEntry {

        public ProblemEntry(string headline) { HeadLine = headline; }
        
        public ProblemEntry(string headline, string detail) {
            HeadLine = headline;
            Description = detail;
        }

        public string HeadLine { get; set; }

        public string Description { get; set; }

        public ProblemCategory Category { get; set; }
    }
}