using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using DbAccess;
using Utils.Commands;
using eBalanceKitBase.Interfaces;
using eBalanceKitBase.Windows;
using eBalanceKitResources.Documents;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Structures.DbMapping {
    // todo if new release is out : 1. In the DatabaseManagement in the upgrader create a new row for upgrade_information.
    //                              2. In the ResourceMappingForUpdate add new row with the ResourceName key.
    [DbTable("upgrade_information", Description = "Stores the informations about upgrades", ForceInnoDb = true)]
    public class UpgradeInformation {

        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public int Id { get; set; }

        [DbColumn("upgrade_available_from", AllowDbNull = false)]
        public DateTime UpgradeAvailableFrom { get; set; }

        [DbColumn("version_string", AllowDbNull = false)]
        public string VersionString { get; set; }

        /// <summary>
        /// for comparison of different versions.
        /// </summary>
        [DbColumn("ordinal", AllowDbNull = false)]
        public decimal Ordinal { get; set; }

        /// <summary>
        /// The file name of the update log. This string is used (after replacing the dot with underscore) in
        ///  the ResourceMappingForUpdate.resx as key.
        /// </summary>
        [DbColumn("resource_name")]
        public string ResourceName { get; set; }

        /// <summary>
        /// the save as function of update log.
        /// </summary>
        private void dlg_FileOk(object sender, CancelEventArgs e) {
            var fileDialog = (SaveFileDialog) sender;
            // tag is set to the filePath in CreateUpgradeRow()
            string filePath = fileDialog.Tag.ToString();
            if (File.Exists(filePath)) {
                Exception ex;
                if (!Utils.FileHelper.IsFileBeingUsed(filePath, out ex)) {
                    // copy with overwrite
                    File.Copy(filePath, fileDialog.FileName, true);
                }
            } else {
                MessageBox.Show(ResourcesCommon.MissingUpgradeReadme, ResourcesCommon.Error);
            }
        }

        /// <summary>
        /// creates an upgrade row for the update. The user's UpgradeDataContext will hold this information.
        /// </summary>
        /// <returns>a row that is shown in the welcome dialog.</returns>
        public UpgradeRow CreateUpgradeRow() {
            // if the update doesn't contain resource name than it's skipped.
            if (!string.IsNullOrEmpty(ResourceName)) {
                // relative path to the file. ResourceName looks like "file name.pdf"
                // TODO : the relative path is wrong in release. Change it to the right path.
                string filePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "..", "..", "..", "eBalanceKitResources",
                                               "Documents", ResourceName);
                if (File.Exists(filePath)) {
                    Exception ex;
                    if (!Utils.FileHelper.IsFileBeingUsed(filePath, out ex)) {
                        UpgradeRow ret = new UpgradeRow {
                            // the save and open command is depending on the file name, so the effective command is built here.
                            SaveCommand = new DelegateCommand(o => true, o => {
                                SaveFileDialog saveFileDialog = new SaveFileDialog {
                                    // filePath is passed through the Tag.
                                    Tag = filePath,
                                    Filter =
                                        "pdf " + ResourcesCommon.Files + " (*.pdf)|*.pdf",
                                    // can be overwritten
                                    OverwritePrompt = true
                                };
                                saveFileDialog.FileOk += dlg_FileOk;
                                saveFileDialog.ShowDialog();
                            }),
                            // open the file
                            OpenCommand =
                                new DelegateCommand(o => true, o => Process.Start(filePath)),
                            // the text of the row is depending on the file name. 
                            // The resource string of the same key (replaced the dot) from the ResourceMappingForUpdate is shown.
                            Value = ResourceMappingForUpdate.ResourceManager.GetString(ResourceName.Replace('.', '_'))
                        };
                        return ret;
                    }
                } else {
                    MessageBox.Show(ResourcesCommon.MissingUpgradeReadme, ResourcesCommon.Error);
                }
            }
            return null;
        }
    }
}
