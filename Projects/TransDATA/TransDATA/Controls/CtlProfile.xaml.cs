// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-28
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using Config.Interfaces.DbStructure;
using TransDATA.Models;
using TransDATA.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlProfile.xaml
    /// </summary>
    public partial class CtlProfile : UserControl {
        public CtlProfile() {
            InitializeComponent();
            
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            //((ProfileDetailModel)DataContext).Profile.InputConfig.Save();
            //((ProfileDetailModel)DataContext).Profile.OutputConfig.Save();
        }
    }
}