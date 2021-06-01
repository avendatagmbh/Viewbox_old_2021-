using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Utils;
using ViewBuilder.Structures;
using ViewBuilder.Windows;
using ViewBuilderBusiness.SapBillSchemaImport;
using ViewBuilderBusiness.Structures.Config;
using ViewBuilderBusiness.Manager;

namespace ViewBuilder.Models
{
    public class DlgSapBillSchemaImportViewModel : NotifyPropertyChangedBase
    {
        public bool IncludeSubDirectories = false;

        public DlgSapBillSchemaImportViewModel()
        {
            _files = new ObservableCollection<SapBillSchemaImportFile>();
            _filePath = "";
        }

        private string _filePath;

        public string FilePath
        {
            get
            {
                return
                    _filePath;
            }
            set
            {
                _filePath = value;
                RefreshFiles();

                Settings.CurrentProfileConfig.ScriptSource.BilanzDirectory = value;
                ProfileManager.Save(Settings.CurrentProfileConfig);

                OnPropertyChanged("FilePath");
            }
        }

        private void RefreshFiles()
        {
            Files.Clear();

            try
            {
                _groupCheckedChange = true;
                try
                {
                    string[] files = System.IO.Directory.GetFiles(_filePath, "*.txt");
                    if (files != null && files.Any())
                    {
                        foreach (string f in files)
                        {
                            var file = new SapBillSchemaImportFile() {FilePath = f};
                            file.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(file_PropertyChanged);
                            Files.Add(file);
                        }
                    }
                }
                finally
                {
                    _groupCheckedChange = false;
                    OnPropertyChanged("AllChecked");
                }
            }
            catch (Exception ex)
            {
            }
        }

        void file_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Selected")
            {
                if (!_groupCheckedChange)
                    OnPropertyChanged("AllChecked");
            }
        }

        private bool _groupCheckedChange = false;

        public bool? AllChecked
        {
            get
            {
                return Files.All(w => w.Selected) ? true : Files.Any(w => w.Selected) ? (bool?)null : false;
            }
            set
            {
                if (value.HasValue)
                {
                    _groupCheckedChange = true;
                    try
                    {
                        foreach (var file in Files)
                        {
                            file.Selected = value.Value;
                        }
                    }
                    finally
                    {
                        _groupCheckedChange = false;
                        OnPropertyChanged("AllChecked");
                    }
                }
                
            }
        }

        private ObservableCollection<SapBillSchemaImportFile> _files;
        public ObservableCollection<SapBillSchemaImportFile> Files
        {
            get { return _files; }
            set { _files = value; }
        }

    }
}
