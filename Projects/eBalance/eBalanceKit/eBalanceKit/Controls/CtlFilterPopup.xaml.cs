using System.Windows;
using eBalanceKitBusiness.Structures;
using eBalanceKitBase.Windows;
using eBalanceKitResources.Localisation;
using eBalanceKit.Models;
using eBalanceKitBase.Structures;
using System;

namespace eBalanceKit.Controls {
    /// <summary>
    /// Interaktionslogik für CtlFilterPopup.xaml
    /// </summary>
    public partial class CtlFilterPopup {
        public CtlFilterPopup() { InitializeComponent(); }

        private void BtnFilterOkClick(object sender, RoutedEventArgs e) { FilterPopup.IsOpen = false; }

        #region IsOpen
        public bool IsOpen { get { return FilterPopup.IsOpen; } set { FilterPopup.IsOpen = value; } }
        #endregion IsOpen

        private void BtnFilterCancelClick(object sender, RoutedEventArgs e) { OptionReset(); }

        private void BtnCloseClick(object sender, RoutedEventArgs e) {
            OptionReset(); 
            //FilterPopup.IsOpen = false;
        }

        private void OptionReset() {
            BalanceListFilterOptions options = DataContext as BalanceListFilterOptions;
            if (options != null) {
                if (options.GetAssignmentCount() > 200) 
                {
                    //new DlgProgress(UIHelpers.TryFindParent<Window>(this)) {
                    //    ProgressInfo = {
                    //        IsIndeterminate = true,
                    //        Caption = eBalanceKitResources.Localisation.ResourcesCommon.RefreshList
                    //    }
                    //}.ExecuteModal(options.Reset);

                    DlgProgress progress = new DlgProgress(UIHelpers.TryFindParent<Window>(this)) {
                        ProgressInfo = { IsIndeterminate = true, Caption = ResourcesCommon.RefreshList }
                    };
                    progress.ExecuteModal(() => Dispatcher.Invoke(new Action(delegate
                    {
                        LockableSourceParameter lsp = new LockableSourceParameter(progress.ProgressInfo);
                        using (new MainWindowLocker(
                            UIHelpers.TryFindParent<Window>(this),
                            lsp,
                            (LockableSourceParameter p) => { ((MainWindowModel)GlobalResources.MainWindow.DataContext).LockSource(lsp); },
                            (LockableSourceParameter p) => { ((MainWindowModel)GlobalResources.MainWindow.DataContext).UnLockSource(lsp); },
                            true,
                            true))
                        {
                            options.Reset();
                        }
                    })));
                } 
                else 
                {
                    options.Reset();
                }
            }

            FilterPopup.IsOpen = false;
        }
    }
}
