// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-05-31
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Utils.Commands;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Models {
    public class ExternCallModel {
        public ExternCallModel() {
            ExternPrograms = new List<ProgramInfo>();

            // config
            {
                var config = new ProgramInfo("Config.exe") {
                    Name = ResourcesExternalTools.ToolNameConfig,
                    Description = ResourcesExternalTools.ToolDescriptionConfig,
                    ImgLink = "/eBalanceKitResources;component/Resources/toolConfig.ico"
                };
                ExternPrograms.Add(config);
            }
            
            // database management
            {
                var database = new ProgramInfo("DatabaseManagement.exe") {
                    Name = ResourcesExternalTools.ToolNameDatabaseManagement,
                    Description = ResourcesExternalTools.ToolDescriptionDatabaseManagement,
                    ImgLink = "/eBalanceKitResources;component/Resources/toolDatabase.ico"
                };
                ExternPrograms.Add(database);
            }
            
            // eBalanceKitManagement
            {
                var logViewer = new ProgramInfo("eBalanceKitManagement.exe") {
                    Name = ResourcesExternalTools.ToolNameLogviewer,
                    Description = ResourcesExternalTools.ToolDescriptionLogviewer,
                    ImgLink = "/eBalanceKitResources;component/Resources/toolLogViewer.ico"
                };
                ExternPrograms.Add(logViewer);
            }
        }

        public List<ProgramInfo> ExternPrograms { get; set; }

        #region Nested type: ProgramInfo
        public class ProgramInfo {
            private readonly string _exeName;
            private readonly ICommand _openCommand;

            public ProgramInfo(string exeName) {
                _exeName = exeName;
                _openCommand = new DelegateCommand(
                    obj => ToolExists,
                    obj => {
                        if (!ToolExists)
                            return;

                        Process.Start(Path,
                                      "mode=ebk " +
                                      UserManager.Instance.CurrentUser.Id + "=" +
                                      UserManager.Instance.CurrentUser.PasswordHash);
                    });
            }

            public string Name { get; set; }
            public string Description { get; set; }
            public string ImgLink { get; set; }
            public bool IsSelected { get; set; }
            public ICommand OpenCommand { get { return _openCommand; } }
            public string Path { get { return Environment.CurrentDirectory + @"\" + _exeName; } }
            public bool ToolExists { get { return File.Exists(Path); } }
        }
        #endregion
    }
}