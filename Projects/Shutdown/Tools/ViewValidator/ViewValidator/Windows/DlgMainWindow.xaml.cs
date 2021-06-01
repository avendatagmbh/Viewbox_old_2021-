using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using ViewValidator.Models;
using ViewValidatorLogic.Manager;
using ViewValidatorLogic.Structures.Misc;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using ViewValidator.Models.Rules;

namespace ViewValidator.Windows {

    /// <summary>
    /// Interaktionslogik für DlgMainWindow.xaml
    /// </summary>
    public partial class DlgMainWindow : Window {
        /// <summary>
        /// Initializes a new instance of the <see cref="DlgMainWindow"/> class.
        /// </summary>
        public DlgMainWindow() {
            InitializeComponent();
            Application.Current.MainWindow = this;
            //viewValidator = new ViewValidatorLogic.Logic.ViewValidator(profile.ValidationSetup);
            try {
                ApplicationManager.Load();
                ProfileManager.Init();
                Model = new MainWindowModel(ProfileManager.GetProfile(ApplicationManager.ApplicationConfig.LastProfile), 
                    ApplicationManager.ApplicationConfig.LastTableMapping, this);

                Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_model_PropertyChanged);
                this.DataContext = Model;

                if (Model.SelectedProfile != null) {
                    Model.SelectedProfile.Error += new EventHandler<MessageEventArgs>(Profile_Error);
                }

                _infoGridRowHeight = new GridLength(160, GridUnitType.Pixel);
                _infoGridRowMinHeight = 194;


            } catch (Exception ex) {
                MessageBox.Show(this, "Es ist ein Fehler aufgetreten: " + Environment.NewLine + ex.Message);
                this.Close();
                return;
            }
        }


        //private static ConnectionStateToSignalStateConverter _connectionStateToSignalStateConverter = new ConnectionStateToSignalStateConverter();
        private delegate void voidDelegate();

        /*****************************************************************************************************/


        #region fields

        public MainWindowModel Model{get;private set;}
        #endregion fields

        /*****************************************************************************************************/

        #region eventHandler

        /// <summary>
        /// Handles the Error event of the Profile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ViewValidator.EventsArgs.MessageEventArgs"/> instance containing the event data.</param>
        void Profile_Error(object sender, MessageEventArgs e) {
            MessageBox.Show(e.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Handles the PropertyChanged event of the _model control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Profile":
                    if (Model.SelectedProfile != null)
                        Model.SelectedProfile.Error += new EventHandler<MessageEventArgs>(Profile_Error);
                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnQuit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnQuit_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        /// <summary>
        /// Handles the Closing event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            try {
                ApplicationManager.ApplicationConfig.LastProfile = Model.SelectedProfile == null ? "" : Model.SelectedProfile.Name;
                ApplicationManager.ApplicationConfig.LastTableMapping = Model.SelectedTableMapping == null ? "" : Model.SelectedTableMapping.UniqueName;
                if(Model.SelectedProfile != null)
                    ProfileManager.Save(Model.SelectedProfile);
                ApplicationManager.Save();
            } catch (Exception ex) {
                MessageBox.Show(
                    "Fehler beim Speichern der Programmkonfiguration: " + ex.Message,
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnStartValidation_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            Model.Validate();
        }

        #region info expander
        private GridLength _infoGridRowHeight;
        private double _infoGridRowMinHeight = 194;

        private void Expander_Expanded(object sender, RoutedEventArgs e) {
            SettingsGridRow.Height = _infoGridRowHeight;
            SettingsGridRow.MinHeight = _infoGridRowMinHeight;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e) {
            _infoGridRowHeight = SettingsGridRow.Height;
            SettingsGridRow.Height = GridLength.Auto;
            SettingsGridRow.MinHeight = 0;
        }

        #endregion info expander


        #endregion eventHandler

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Finds the visual parent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        static T FindVisualParent<T>(UIElement element) where T : UIElement {
            UIElement parent = element;
            while (parent != null) {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null) {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        /// <summary>
        /// Handles the Closed event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Window_Closed(object sender, EventArgs e) {
            //_model.Dispose();
        }

        #endregion methods

        private void ValidateCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void ValidateCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
            Model.Validate();
            e.Handled = true;
        }

        private void btnAddView_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            Model.AddNewView();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void btnRuleWindow_Click(object sender, RoutedEventArgs e){
            dummyButtonControl.Focus();
            Model.ShowRuleAssignmentWindow();
        }

        private void btnExportExcel_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            Model.ExportToExcel();
        }

        private void btnStartValidationInMemory_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            Model.ValidateInMemory();
        }
    }
}
