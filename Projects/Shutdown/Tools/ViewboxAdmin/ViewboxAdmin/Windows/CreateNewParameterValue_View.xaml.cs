using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ViewboxAdmin.Structures;
using ViewboxAdmin.ViewModels;

namespace ViewboxAdmin.Windows
{
    /// <summary>
    /// Interaction logic for CollectionEdit_Window.xaml
    /// </summary>
    public partial class CreateNewParameterValue_View : Window
    {
        public CreateNewParameterValue_View() {
            InitializeComponent();
        }

        private void CollectionControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            (DataContext as CreateParameterValue_ViewModel).ParameterValuaCreationFinished += new EventHandler<EventArgs>(CollectionEdit_Window_CloseWindow);
        }

        void CollectionEdit_Window_CloseWindow(object sender, EventArgs e) {
            this.Close();
        }

        

        

        

    }
}
