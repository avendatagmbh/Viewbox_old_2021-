using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Utils;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Manager;

namespace eBalanceKit.Windows.Management.Delete.Models
{
    public class DeleteBalanceListModel : NotifyPropertyChangedBase
    {
        public Document Document { get; private set; }
        private IBalanceList _selectedBalanceList;

        public ObservableCollection<IBalanceList> BalanceList { get; set; }

        public IBalanceList SelectedBalanceList
        {
            get { return _selectedBalanceList; }
            set
            {
                _selectedBalanceList = value;
                OnPropertyChanged("SelectedBalanceList");
                OnPropertyChanged("AccountNumberVisibility");
            }
        }

        public string AccountNumberVisibility
        {
            get { return SelectedBalanceList == null ? "Collapsed" : "Visible"; }
        }

        public DeleteBalanceListModel(Document doc)
        {
            BalanceList = new ObservableCollection<IBalanceList>();
            foreach(IBalanceList balanceList in doc.BalanceListsVisible)
            {
                  if(balanceList.IsImported)
                      BalanceList.Add(balanceList);
            }

            Document = doc;
        }

        public void DeleteBalanceList()
        {
            if (SelectedBalanceList != null)
            {
                BalanceListManager.Instance.RemoveBalanceList(SelectedBalanceList);
            }
        }
    }
}
