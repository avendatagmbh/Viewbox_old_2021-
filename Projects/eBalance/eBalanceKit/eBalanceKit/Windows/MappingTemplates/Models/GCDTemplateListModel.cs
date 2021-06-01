using Utils;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates.Models
{
    internal class GCDTemplateListModel : NotifyPropertyChangedBase
    {
        public GCDTemplateListModel(DlgGCDTemplatesModel parent)
        {
            Templates.CollectionChanged += Templates_CollectionChanged;
            Parent = parent;
            ShowUpdateMessage = false;
        }

        public DlgGCDTemplatesModel Parent { get; set; }

        void Templates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("TemplatesCount");
        }

        #region SelectedTemplate
        private MappingTemplateHeadGCD _selectedTemplate;

        public MappingTemplateHeadGCD SelectedTemplate
        {
            get { return _selectedTemplate; }
            set
            {
                if (_selectedTemplate != value)
                {
                    _selectedTemplate = value;
                    OnPropertyChanged("SelectedTemplate");
                }
            }
        }
        #endregion

        #region Templates
        private readonly ObservableCollectionAsync<MappingTemplateHeadGCD> _templates =
            new ObservableCollectionAsync<MappingTemplateHeadGCD>();

        public ObservableCollectionAsync<MappingTemplateHeadGCD> Templates { get { return _templates; } }
        #endregion Templates

        public bool ShowUpdateMessage { get; set; }
    }
}
