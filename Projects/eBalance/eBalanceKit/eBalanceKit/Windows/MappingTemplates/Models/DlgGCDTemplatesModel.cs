﻿// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-03-24
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Windows;
using Taxonomy.Enums;
using Utils;
using Utils.Commands;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.MappingTemplate;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;
using eBalanceKitResources.Localisation;
using Document = eBalanceKitBusiness.Structures.DbMapping.Document;
using MessageBox = System.Windows.MessageBox;
using System.IO;




namespace eBalanceKit.Windows.MappingTemplates.Models
{
    internal class DlgGCDTemplatesModel : NotifyPropertyChangedBase
    {
        #region constructor
        internal DlgGCDTemplatesModel(
            Document document, Window owner)
        {
            Templates = new GCDTemplateListModel(this);
            Document = document;
            Owner = owner;

            Templates = new GCDTemplateListModel(this);
            Templates.PropertyChanged += Templates_PropertyChanged;

            ObsoleteTemplates = new GCDTemplateListModel(this) { ShowUpdateMessage = true };
            ObsoleteTemplates.PropertyChanged += ObsoleteTemplates_PropertyChanged;

            InitTemplateList();

            AddCommand = new DelegateCommand((obj) => true, delegate { AddTemplate(); });
            EditCommand = new DelegateCommand((obj) => true, delegate { EditTemplate(); });
            DeleteCommand = new DelegateCommand((obj) => true, delegate { DeleteTemplate(); });
            ExtendCommand = new DelegateCommand((obj) => true, delegate { ExtendTemplate(); });
            ApplyCommand = new DelegateCommand((obj) => true, delegate { ApplyTemplate(); });
            ExportCommand = new DelegateCommand((obj) => true, delegate { ExportTemplate(); });
            ImportCommand = new DelegateCommand((obj) => true, delegate { ImportTemplate(); });
            CopyCommand = new DelegateCommand((obj) => true, delegate { CopyTemplate(); });
        }

        void Templates_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedTemplate")
            {
                UpdateMenuProperties();
            }
        }

        private void UpdateMenuProperties()
        {
            OnPropertyChanged("IsCreateTemplateEditAllowed");
            OnPropertyChanged("IsDeleteTemplateAllowed");
            OnPropertyChanged("IsEditTemplateAllowed");
            OnPropertyChanged("IsExportTemplateAllowed");
            OnPropertyChanged("IsCopyTemplateAllowed");
            OnPropertyChanged("IsApplyTemplateAllowed");
            OnPropertyChanged("IsExtendTemplateAllowed");
        }

        void ObsoleteTemplates_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TemplatesCount")
            {
                OnPropertyChanged("HasObsoleteTemplates");
            }
        }

        #endregion

        #region properties

        #region Commands
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand EditCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand ExtendCommand { get; set; }
        public DelegateCommand ApplyCommand { get; set; }
        public DelegateCommand ExportCommand { get; set; }
        public DelegateCommand ImportCommand { get; set; }
        public DelegateCommand CopyCommand { get; set; }
        #endregion Commands

        public GCDTemplateListModel Templates { get; private set; }
        public GCDTemplateListModel ObsoleteTemplates { get; private set; }


        public bool IsCreateTemplateEditAllowed { get { return Document != null && Document.BalanceListsImported.Count > 0; } }
        public bool IsDeleteTemplateAllowed { get { return SelectedTabIndex == 0 && Templates.SelectedTemplate != null; } }
        public bool IsEditTemplateAllowed { get { return SelectedTabIndex == 0 && Templates.SelectedTemplate != null; } }
        public bool IsExportTemplateAllowed { get { return SelectedTabIndex == 0 && Templates.SelectedTemplate != null; } }
        public bool IsCopyTemplateAllowed { get { return SelectedTabIndex == 0 && Templates.SelectedTemplate != null; } }

        public bool IsApplyTemplateAllowed
        {
            get
            {
                return SelectedTabIndex == 0 && Templates.SelectedTemplate != null && Document != null &&
                       (Document.MainTaxonomyInfo.Name == Templates.SelectedTemplate.TaxonomyInfo.Name ||
                       (Document.MainTaxonomyInfo.Type == TaxonomyType.GAAP && Templates.SelectedTemplate.TaxonomyInfo.Type == TaxonomyType.OtherBusinessClass) ||
                       (Document.MainTaxonomyInfo.Type == TaxonomyType.OtherBusinessClass && Templates.SelectedTemplate.TaxonomyInfo.Type == TaxonomyType.GAAP));
            }
        }

        public bool IsExtendTemplateAllowed
        {
            get
            {
                return SelectedTabIndex == 0 && Templates.SelectedTemplate != null && Document != null &&
                       Document.MainTaxonomyInfo.Name == Templates.SelectedTemplate.TaxonomyInfo.Name;
            }
        }

        public bool HasObsoleteTemplates { get { return ObsoleteTemplates.Templates.Count > 0; } }

        public Document Document { get; set; }
        public Window Owner { get; set; }

        #region SelectedTabIndex
        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    UpdateMenuProperties();
                    OnPropertyChanged("SelectedTabIndex");
                }
            }
        }
        #endregion SelectedTabIndex
        #endregion properties

        #region methods
        internal void EditTemplate()
        {
            /*
            if (Templates.SelectedTemplate == null) return;
            var template = Templates.SelectedTemplate;

            var dlgEdit = new DlgEditTemplate(template) { Owner = Owner };
            var result = dlgEdit.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var dlg = new DlgTemplateDetails(template) { Owner = Owner };

                var result2 = dlg.ShowDialog();
                if (result2.HasValue && result2.Value) TemplateManager.SaveTemplate(template);
                else TemplateManager.ResetTemplate(template);
            }
            else
            {
                TemplateManager.ResetTemplate(template);
            }

            InitTemplateList();
             */
        }

        internal void DeleteTemplate()
        {
            if (Templates.SelectedTemplate == null)
                return;

            if (MessageBox.Show(
                    string.Format(ResourcesCommon.DeleteTemplateRequest, Templates.SelectedTemplate.Name),
                    ResourcesCommon.DeleteTemplateRequestTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No) == MessageBoxResult.Yes)
            {

                TemplateManager.DeleteTemplate(Templates.SelectedTemplate);
                InitTemplateList();
            }
        }

        internal void ApplyTemplate()
        {
            if (Document == null || Templates.SelectedTemplate == null)
                return;

            try
            {
                var model = new ApplyGCDTemplateModel(Document, Templates.SelectedTemplate);
                //bool? result = new DlgApplyTemplates(Templates.SelectedTemplate) {Owner = Owner, DataContext = model}.ShowDialog();

                var progress = new DlgProgress(Owner);
                progress.ExecuteModal(TemplateManager.ApplyTemplateGcd, new object[] {progress.ProgressInfo, model});

                if (model.AssignmentErrors.Count > 0) {
                    //new DlgApplyTemplateErrors { Owner = Owner, DataContext = model }.ShowDialog();
                }
                else {
                    MessageBox.Show(Owner, ResourcesCommon.TemplateIsApplied);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        internal void AddTemplate()
        {
            if (Document == null) return;

            try
            {
                MappingTemplateHeadGCD template = TemplateManager.CreateTemplateGcd(Document);

                var model = new CreateTemplateModel(Document, template);
                var result1 = new DlgCreateGCDTemplate { Owner = Owner, DataContext = model }.ShowDialog();
                if (!result1.HasValue || !result1.Value) return;

                TemplateManager.InitTemplateGcd(model);

                /*
                var dlg = new DlgTemplateDetails(template) { Owner = Owner };
                var result2 = dlg.ShowDialog();
                if (result2.HasValue && result2.Value)
                {
                    // save new template
                    TemplateManager.AddTemplateGcd(template);
                    InitTemplateList();
                }
                 */
                TemplateManager.AddTemplateGcd(template);
                InitTemplateList();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Copies the selected template.
        /// It's possible to change its name and comments.
        /// </summary>
        internal void CopyTemplate()
        {
            /*
            // Obtain the selected template.
            if (Templates.SelectedTemplate == null) return;
            var templateToCopy = Templates.SelectedTemplate;
            var tempTemplate = TemplateManager.CreateTemplate();

            // Edit the template name for avoiding the creation of template with same name
            tempTemplate.Name = templateToCopy.Name + " (Kopiert)";

            // Ask the user for the datas
            bool? result = new DlgEditTemplate(tempTemplate) { Owner = Owner }.ShowDialog();
            if (result.HasValue && result.Value)
            {
                // for backing up the template datas
                String templateName = templateToCopy.Name;
                String accountStructure = templateToCopy.AccountStructure;
                String comment = templateToCopy.Comment;

                // screwing up the template datas
                templateToCopy.Name = tempTemplate.Name;
                templateToCopy.AccountStructure = tempTemplate.AccountStructure;
                templateToCopy.Comment = tempTemplate.Comment;

                String tempFile = Path.Combine(Path.GetTempPath(),
                                               Path.ChangeExtension(Guid.NewGuid().ToString(), ".TMP"));

                TemplateManager.ExportTemplate(templateToCopy, tempFile);

                var template = TemplateManager.CreateTemplate();
                TemplateManager.ImportTemplate(template, tempFile);

                // restoring the template datas.
                templateToCopy.Name = templateName;
                templateToCopy.AccountStructure = accountStructure;
                templateToCopy.Comment = comment;

                // Record in the database.
                TemplateManager.AddTemplate(template);

                // Reload list
                InitTemplateList();
             
            }
             */
        }

        internal void ImportTemplate()
        {
            bool? result = new DlgImportTemplate { Owner = Owner }.ShowDialog();
            if (result.HasValue && result.Value)
                InitTemplateList();
        }

        internal void ExtendTemplate()
        {
            /*
            if (Document == null || Templates.SelectedTemplate == null)
                return;

            try
            {
                var model = new ExtendTemplateModel(Document, Templates.SelectedTemplate);
                bool? result = new DlgExtendTemplate { Owner = Owner, DataContext = model }.ShowDialog();
                if (!result.HasValue || !result.Value) return;

                TemplateManager.ExtendTemplates(model);

                result = new DlgExtendTemplateResult { Owner = Owner, DataContext = model }.ShowDialog();
                if (!result.HasValue || !result.Value)
                {
                    TemplateManager.ResetTemplate(Templates.SelectedTemplate);
                    InitTemplateList();
                }
                else
                {
                    //Save has already been done in the model
                    //TemplateManager.SaveTemplate(Templates.SelectedTemplate);
                    //Message has already been showed in the DlgExtendTemplateResult after save is done
                    //MessageBox.Show(
                    //    "Die Vorlage wurden erfolgreich erweitert.",
                    //    "Vorlage erweitern",
                    //    MessageBoxButton.OK,
                    //    MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
             */
        }

        internal void ExportTemplate()
        {
            if (Templates.SelectedTemplate != null)
            {
                //new DlgExportTemplate(Templates.SelectedTemplate) { Owner = Owner }.ShowDialog();
            }
        }

        internal void UpdateTemplate(MappingTemplateHeadGCD template)
        {
            //var updateTemplateResultModel = TaxonomyManager.UpgradeTemplate(template);
            //new DlgShowTaxonomyUpdate { Owner = Owner, DataContext = updateTemplateResultModel }.ShowDialog();

            InitTemplateList();
            SelectedTabIndex = 0;
            Templates.SelectedTemplate = template;
        }


        #region InitTemplateList

        private void InitTemplateList()
        {
            Templates.Templates.Clear();
            foreach (var template in
                (from template in TemplateManager.TemplatesGcd
                 orderby template.Name, template.TaxonomyInfo.Version descending, template.Name, template.CreationDate
                 select template))

                Templates.Templates.Add(template);

            UpdateMenuProperties();
        }
        #endregion InitTemplateList

        #endregion methods
    }
}
