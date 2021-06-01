using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using System.Windows.Input;
using ViewAssistant.Controls;

namespace ViewAssistantBusiness.Models
{
    public delegate void RenamerSettingsEventHandler(object sender, RenamerSettingsEventArgs e);

    public class RenamerSettingsModel : NotifyPropertyChangedBase
    {
        public event RenamerSettingsEventHandler OnSettingsAccepted;

        private ObservableCollectionAsync<FromToTextDictionaryItem> replaceFromTo;
        public ObservableCollectionAsync<FromToTextDictionaryItem> ReplaceFromTo
        {
            get { return replaceFromTo; }
            set
            {
                replaceFromTo = value;
                OnPropertyChanged("ReplaceFromTo");
            }

        }

        private bool onTableNames;
        public bool OnTableNames
        {
            get { return onTableNames; }
            set
            {
                onTableNames = value;
                OnPropertyChanged("OnTableNames");
            }

        }

        private bool onColumnNames;
        public bool OnColumnNames
        {
            get { return onColumnNames; }
            set
            {
                onColumnNames = value;
                OnPropertyChanged("OnColumnNames");
            }

        }

        public RenamerSettingsModel()
        {
            OnTableNames = true;
            ReplaceFromTo = new ObservableCollectionAsync<FromToTextDictionaryItem>() { new FromToTextDictionaryItem(" ", "_"),
                                                                                        new FromToTextDictionaryItem("ß", "ss"),
                                                                                      };
        }

        public ICommand AcceptCommand { get { return new RelayCommand(Accept); } }

        private void Accept(object sender)
        {
            OnSettingsAccepted(this, new RenamerSettingsEventArgs()
            {
                ReplaceFromTo = this.ReplaceFromTo,
                OnTableNames = this.OnTableNames,
                OnColumnNames = this.OnColumnNames,
            });
        }

        public ICommand DeleteCommand { get { return new RelayCommand(Delete); } }

        private void Delete(object sender)
        {
            if (sender != null && sender is FromToTextDictionaryItem)
            {
                ReplaceFromTo.Remove(sender as FromToTextDictionaryItem);
            }
        }

        public ICommand AddCommand { get { return new RelayCommand(Add); } }

        private void Add(object sender)
        {
            ReplaceFromTo.Add(new FromToTextDictionaryItem(String.Empty, String.Empty));
        }
    }

    public class RenamerSettingsEventArgs : EventArgs
    {
        public ObservableCollectionAsync<FromToTextDictionaryItem> ReplaceFromTo { get; set; }

        public bool OnTableNames { get; set; }

        public bool OnColumnNames { get; set; }
    }


}
