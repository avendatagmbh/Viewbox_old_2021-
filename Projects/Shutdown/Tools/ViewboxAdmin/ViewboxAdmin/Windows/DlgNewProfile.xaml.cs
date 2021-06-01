// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System.Windows;
using ViewboxAdmin.Models.ProfileRelated;
using ViewboxAdmin.ViewModels;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewboxAdmin.Windows
{
    /// <summary>
    /// Interaktionslogik für DlgNewProfile.xaml
    /// </summary>
    public partial class DlgNewProfile : Window
    {

        public DlgNewProfile() {
            InitializeComponent();
        }
        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            Model = (DataContext as CreateNewProfile_ViewModel);
            Model.CloseWindow += Close;
        }

        public CreateNewProfile_ViewModel Model { get; private set; }







    }
}
