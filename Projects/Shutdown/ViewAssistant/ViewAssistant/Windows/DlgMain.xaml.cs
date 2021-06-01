using System.Windows.Forms;
using System.Windows.Input;
using ViewAssistantBusiness;
using ViewAssistantBusiness.Config;
using System.Linq;
using System.Windows.Media.Imaging;
using System;
using AvdWpfControls;
using System.Windows;
using System.ComponentModel;
using ViewAssistantBusiness.Models;
using Base.Localisation;
using Result = System.Windows.Forms.DialogResult;

namespace ViewAssistant.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DlgMain
    {
        public ICommand ClickMeCommand { get; set; }
        
        private MainModel Model
        {
            get { return DataContext as MainModel; }
        }

        private DlgLocalisationTextsSettings locSettingsDialog;

        private DlgLocalisationTextsConfiguration locTextsConfDialog;

        private DlgRenamer renamerDialog;

        private DlgRenamerSettings renamerSettingsDialog;

        public DlgMain()
        {
            InitializeComponent();
            var model = MainModel.Instance;
            DataContext = model;
            model.DataPreviewShowed += DataPreviewShowed;
            model.ConfigureLocalisationTextsClicked += model_ConfigureLocalisationTextsClicked;
            model.LocalisationAllTextsClicked += model_LocalisationAllTextsClicked;
            model.RenamerClicked += model_RenamerClicked;
            model.RenamerSettingsClicked += model_RenamerSettingsClicked;
        }

        #region Localisation events

        void model_LocalisationAllTextsClicked(object sender)
        {
            if (locSettingsDialog != null)
            {
                Model.LocalisationSettings.OnSettingsAccepted -= LocalisationSettings_OnSettingsAccepted;
            }

            locSettingsDialog = new DlgLocalisationTextsSettings();
            Model.LocalisationSettings.OnSettingsAccepted += LocalisationSettings_OnSettingsAccepted;
            locSettingsDialog.DataContext = Model.LocalisationSettings;
            locSettingsDialog.ShowDialog();
        }

        void LocalisationSettings_OnSettingsAccepted(object sender, LocalisationTextsSettingsEventArgs e)
        {
            locSettingsDialog.Close();
        }

        void model_ConfigureLocalisationTextsClicked(object sender, IViewboxLocalisable table)
        {
            locTextsConfDialog = new DlgLocalisationTextsConfiguration();
            var model = new LocalizationTextsConfigurationModel(table, Model);
            locTextsConfDialog.DataContext = model;
            model.SaveFinished += new LocalizationTextsConfigurationEventHandler(LocalizationTextsConfigurationDialog_OnSaveFinished);
            locTextsConfDialog.ShowDialog();
        }

        void LocalizationTextsConfigurationDialog_OnSaveFinished(object sender)
        {
            locTextsConfDialog.Close();
        }

        #endregion

        #region Renamer events

        void model_RenamerClicked(object sender, IRenameable model)
        {
            renamerDialog = new DlgRenamer();
            var renamerModel = new RenamerModel(model, Model);
            renamerDialog.DataContext = renamerModel;
            renamerModel.SaveFinished += renamerModel_SaveFinished;
            renamerDialog.ShowDialog();
        }

        void renamerModel_SaveFinished(object sender)
        {
            renamerDialog.Close();
        }

        void model_RenamerSettingsClicked(object sender)
        {
            if (renamerSettingsDialog != null)
            {
                Model.RenamerSettings.OnSettingsAccepted -= RenamerSettings_OnSettingsAccepted;
            }
            renamerSettingsDialog = new DlgRenamerSettings();
            Model.RenamerSettings.OnSettingsAccepted += RenamerSettings_OnSettingsAccepted;
            renamerSettingsDialog.DataContext = Model.RenamerSettings;
            renamerSettingsDialog.ShowDialog();
        }

        void RenamerSettings_OnSettingsAccepted(object sender, RenamerSettingsEventArgs e)
        {
            renamerSettingsDialog.Close();
        }

        #endregion

        private void DataPreviewShowed(object sender, DataPreviewModel model)
        {
            var dialog = new DlgDataPreview();
            dialog.DataContext = model;
            dialog.ShowDialog();
        }

        private void NewProfileButtonClick(object sender, RoutedEventArgs e)
        {
            FocusDummyButton.Focus();
            var model = new ProfileConfigModel();
            var dialog = new DlgProfile {DataContext = model};
            dialog.assistantControl.Finish +=
                (x, y) =>
                    {
                        ConfigDb.ProfileManager.AddProfile(model);
                        Model.CurrentProfile = model;
                        dialog.Close();
                        LoadProfileExpander();
                    };
            dialog.ShowDialog();
        }

        private void EditProfileButtonClick(object sender, RoutedEventArgs e)
        {
            FocusDummyButton.Focus();
            var model = new ProfileManagementModel();
            var dialog = new DlgProfileCRUD();
            dialog.DataContext = model;
            dialog.Closed += (o, args) => LoadProfileExpander();
            dialog.ShowDialog();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Model.Dispose();
            ConfigDb.Cleanup();
        }

        private void LoadProfileExpander()
        {
            object addNewProfile = profileExpander.Items[0];
            object separator = profileExpander.Items[1];
            profileExpander.Items.Clear();
            profileExpander.Items.Add(addNewProfile);
            profileExpander.Items.Add(separator);

            var bitmap = new BitmapImage(new Uri("pack://application:,,,/ViewAssistant;component/Resources/icon_profile.png"));

            foreach (ProfileConfigModel profile in ConfigDb.ProfileManager.Profiles.OrderBy(p => p.Name))
            {
                var item = new AvdMenuExpanderItem
                               {
                                   ImageSource = bitmap,
                                   ImageHeight = 21,
                                   Caption = profile.Name,
                                   Description = profile.Name,
                                   Tag = profile
                               };
                item.Click += BtnProfileSelected;
                profileExpander.Items.Add(item);
            }
        }

        private void BtnProfileSelected(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as AvdMenuExpanderItem;
            if (menuItem != null)
            {
                ProfileConfigModel profile = menuItem.Tag == null ? null : menuItem.Tag as ProfileConfigModel;
                if (profile != null)
                {
                    Model.CurrentProfile = profile;
                }
            }
        }

        public override void EndInit()
        {
            base.EndInit();
            if(ConfigDb.ProfileManager != null)
            {
                ConfigDb.ProfileManager.ProfilesChanged += LoadProfileExpander;
            }
            LoadProfileExpander();
        }

        private void btnRefreshAll_Click(object sender, RoutedEventArgs e)
        {
            Model.InitConnections();
        }

        private void btnMetaToViewbox_Click(object sender, RoutedEventArgs e)
        {
            Model.MigrateViewboxMetadata(false);
        }
        
        private void btnMetaToViewboxOpt_Click(object sender, RoutedEventArgs e)
        {
            Model.MigrateViewboxMetadata(true);
        }

        private void btnTransferData_Click(object sender, RoutedEventArgs e)
        {
            Model.TransferData();
        }

        private void btnGenerateScripts_Click(object sender, RoutedEventArgs e)
        {
            if (!Model.ShowNotSelectedViewBoxTablesError())
            {
                var dialog = new FolderBrowserDialog();
                var result = dialog.ShowDialog();
                if (result == Result.OK)
                {
                    Model.GenerateScripts(dialog.SelectedPath);
                }
            }
        }

        private void LogDirectoryClick(object sender, RoutedEventArgs e)
        {
            FocusDummyButton.Focus();
            Model.OpenLogDirectory();
        }

        private void BtnInfoClick(object sender, RoutedEventArgs e)
        {
            FocusDummyButton.Focus();
            new DlgInfo().ShowDialog();
        }

        private void BtnProfileSettingsClick(object sender, RoutedEventArgs e)
        {
            if (!Model.ShowNotSelectedError())
            {
                FocusDummyButton.Focus();
                var dialog = new DlgViewProfileSettings();
                dialog.DataContext = Model.CurrentProfile;
                dialog.ShowDialog();
            }
        }

        private void btnRefreshSource_Click(object sender, RoutedEventArgs e)
        {
            Model.LoadSourceData();
        }

        private void btnCancelSource_Click(object sender, RoutedEventArgs e)
        {
            Model.CancelSource();
        }

        private void btnRefreshViewbox_Click(object sender, RoutedEventArgs e)
        {
            Model.LoadViewboxData();
        }

        private void btnCancelViewbox_Click(object sender, RoutedEventArgs e)
        {
            Model.CancelViewbox();
        }
        
        private void btnCancelFinal_Click(object sender, RoutedEventArgs e)
        {
            Model.CancelFinal();
        }

        private void btnRefreshFinal_Click(object sender, RoutedEventArgs e)
        {
            Model.LoadFinalData();
        }


        private void btnAddSpecial_Click(object sender, RoutedEventArgs e)
        {
            if (Model.SelectedViewboxTable != null)
                Model.SelectedViewboxTable.AddSpecial();
        }

        private void BtnRemoveClick(object sender, RoutedEventArgs e)
        {
            if (Model.SelectedViewboxTable != null && Model.SelectedColumn != null && Model.SelectedColumn.IsInViewbox)
                Model.SelectedViewboxTable.RemoveSpecial(Model.SelectedColumn);
        }

        private void BtnAccessMergeFileClick(object sender, RoutedEventArgs e)
        {
            FocusDummyButton.Focus();
            var dialog = new DlgAccessOperations(AccessOperationType.Merge);
            dialog.Content = ResourcesCommon.BtnAccessMergeFile;
            dialog.DataContext = new AccessMergerModel();
            dialog.Icon = new BitmapImage(new Uri("pack://application:,,,/ViewAssistant;component/Resources/icon_merge_files_small.png"));
            dialog.ShowDialog();
        }

        private void BtnAccessCopyFilesClick(object sender, RoutedEventArgs e)
        {
            FocusDummyButton.Focus();
            var dialog = new DlgAccessOperations(AccessOperationType.Copy);
            dialog.Content = ResourcesCommon.BtnAccessCopyFiles;
            dialog.DataContext = new AccessMergerModel();
            dialog.Icon = new BitmapImage(new Uri("pack://application:,,,/ViewAssistant;component/Resources/icon_copy_files_small.png"));
            dialog.ShowDialog();
        }

        private void BtnAccessLinkingFileClick(object sender, RoutedEventArgs e)
        {
            FocusDummyButton.Focus();
            var dialog = new DlgAccessOperations(AccessOperationType.Linking);
            dialog.Content = ResourcesCommon.BtnAccessLinkingFile;
            dialog.DataContext = new AccessMergerModel();
            dialog.Icon = new BitmapImage(new Uri("pack://application:,,,/ViewAssistant;component/Resources/icon_link_to_files_small.png"));
            dialog.ShowDialog();
        }
    }
}
