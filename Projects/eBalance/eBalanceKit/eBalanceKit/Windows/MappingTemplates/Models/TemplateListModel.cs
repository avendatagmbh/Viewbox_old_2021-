// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-11-22
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using Utils;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates.Models {
    internal class TemplateListModel : NotifyPropertyChangedBase {
        
        public TemplateListModel(DlgTemplatesModel parent) {
            Templates.CollectionChanged += Templates_CollectionChanged;
            Parent = parent;
            ShowUpdateMessage = false;
        }

        public DlgTemplatesModel Parent { get; set; }

        void Templates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged("TemplatesCount");
        }
        
        #region SelectedTemplate
        private MappingTemplateHead _selectedTemplate;

        public MappingTemplateHead SelectedTemplate {
            get { return _selectedTemplate; }
            set {
                if (_selectedTemplate != value) {
                    _selectedTemplate = value;
                    OnPropertyChanged("SelectedTemplate");
                }
            }
        }
        #endregion

        #region Templates
        private readonly ObservableCollectionAsync<MappingTemplateHead> _templates =
            new ObservableCollectionAsync<MappingTemplateHead>();

        public ObservableCollectionAsync<MappingTemplateHead> Templates { get { return _templates; } }
        #endregion Templates 

        public bool ShowUpdateMessage { get; set; }
    }
}