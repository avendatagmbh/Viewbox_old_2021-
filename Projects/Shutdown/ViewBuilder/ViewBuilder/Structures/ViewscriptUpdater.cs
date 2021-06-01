using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using SystemDb;
using ProjectDb.Enums;
using ProjectDb.Tables;
using Utils;
using ViewBuilder.Models;
using ViewBuilder.Windows;
using ViewBuilderBusiness;
using ViewBuilderBusiness.EventArgs;
using ViewBuilderBusiness.Exceptions;
using ViewBuilderBusiness.Structures;
using ViewBuilderBusiness.Structures.Config;
using ViewBuilderCommon;
using System.Diagnostics;
using log4net;
using AV.Log;

namespace ViewBuilder.Structures {
    class ViewscriptUpdater {

        #region Constructor

        public ViewscriptUpdater(Window parent, ProfileConfig profile) {
            _parentWindow = parent;
            Profile = profile;
        }
        #endregion Constructor

        private ILog Log = LogHelper.GetLogger();

        #region Properties
        private Window _parentWindow;
        private ProfileConfig Profile { get; set; }
        private PopupProgressBar _popupProgressBar;
        private DlgScriptErrorsModel _dlgScriptErrorsModel = new DlgScriptErrorsModel();

        public bool HadError { get; set; }
        #endregion Properties

        #region Methods
        public void UpdateViewscripts(bool updateOnlyChecked) {
            ProgressCalculator progress = new ProgressCalculator();
            progress.Title = "Lese Viewskripte";
            if(!updateOnlyChecked)
                progress.DoWork += UpdateViewscriptsThread;
            else
                progress.DoWork += UpdateSelectedViewscripts;
            progress.RunWorkerCompleted += UpdateViewScriptsCompleted;
            _popupProgressBar = new PopupProgressBar() { Owner = _parentWindow };
            _popupProgressBar.DataContext = progress;
            progress.RunWorkerAsync();
            _popupProgressBar.ShowDialog();
            
        }

        void UpdateSelectedViewscripts(object sender, DoWorkEventArgs e) {
            ProgressCalculator progress = sender as ProgressCalculator;
            var checkedViewscripts = Profile.Viewscripts.Where(v => v.IsChecked).ToList();
            progress.SetWorkSteps(checkedViewscripts.Count, false);
            string errors = "";
            Profile.GetTableNames(true);
            foreach (var viewscript in checkedViewscripts) {
                progress.Description = "Bearbeite Viewskript " + viewscript.Name;
                try {
                    ScriptFile file = new ScriptFile(viewscript.FileInfo, Profile);
                    var newViewscripts = file.Views.Where(v => v.Name.ToLower() == viewscript.Name.ToLower()).ToList();
                    if (newViewscripts.Count == 1) {

                        var newViewscript = newViewscripts.First();
                        var tobjs = GetTableObjects();
                        UpdateViewscript(newViewscript, viewscript, tobjs);
                    }
                } catch (Exception ex) {
                    errors += Environment.NewLine + "Fehler in Viewskript " + viewscript.Name + ":" + Environment.NewLine +
                              ex.Message;
                }
                progress.StepDone();
            }
            if (!string.IsNullOrEmpty(errors)) {
                throw new InvalidScriptException("Folgende Viewskripte weisen Fehler auf:" + errors);

            }
        }

        void UpdateViewScriptsCompleted(object sender, RunWorkerCompletedEventArgs e) {
            _popupProgressBar.Close();
            if (e.Error != null) {
                HadError = true;
                MessageBox.Show(_parentWindow, e.Error.Message,"Fehler beim Updaten der Viewskripte",MessageBoxButton.OK,MessageBoxImage.Error);
            }
            if ((_dlgScriptErrorsModel.MultipleViewError.Count > 0) || (_dlgScriptErrorsModel.ScriptParseErrors.Count > 0)) {
                new DlgScriptErrors {
                    DataContext = _dlgScriptErrorsModel,
                    Owner = _parentWindow
                }.ShowDialog();
            }
        }


        private void UpdateViewscriptsThread(object sender, DoWorkEventArgs e) {
            ProgressCalculator progress = sender as ProgressCalculator;
            ScriptLoader scriptLoader = new ScriptLoader();
            scriptLoader.Error += scriptLoader_Error;
            scriptLoader.MultipleViewError += scriptLoader_MultipleViewError;
            scriptLoader.ScriptError += scriptLoader_ScriptError;
            List<ScriptFile> scripts = scriptLoader.Load(this.Profile.ScriptSource.Directory,
                                                         this.Profile.ScriptSource.IncludeSubdirectories, Profile);
            progress.SetWorkSteps(scripts.Count, false);
            Profile.GetTableNames(true);
            // create list of all avaliable viewscripts
            Dictionary<string, Viewscript> viewscripts = new Dictionary<string, Viewscript>();
            foreach (ScriptFile script in scripts) {
                foreach (Viewscript viewscript in script.Views) {
                    if (!viewscripts.ContainsKey(viewscript.Name)) {
                        viewscripts.Add(viewscript.Name, viewscript);
                    }
                }
            }

            // create list of missing viewscripts
            List<string> missingScripts = new List<string>();
            foreach (Viewscript viewscript in Profile.Viewscripts) {
                if (!viewscripts.ContainsKey(viewscript.Name)) {
                    missingScripts.Add(viewscript.Name);
                }
            }

            // remove views with missing scripts from viewscript database
            foreach (string missing in missingScripts) {
                this.Profile.RemoveViewscript(missing);
            }

            var tobjs = GetTableObjects();

            using (var connection = Profile.ProjectDb.ConnectionManager.GetConnection())
            {

                foreach (Viewscript viewscript in viewscripts.Values)
                {
                    progress.Description = "Bearbeite Viewskript " + viewscript.Name;
                    Viewscript currentViewscript = viewscript;
                    if (Profile.IsViewscriptExisting(viewscript))
                    {
                        // update existing view
                        currentViewscript = Profile.GetViewscript(viewscript.Name);
                        UpdateViewscript(viewscript, currentViewscript, tobjs);
                    }
                    else
                    {
                        // add new view
                        if (Profile.ViewboxDb.Tables.Count != 0)
                        {
                            var tableNames = Profile.ViewboxDb.GetTableNamesAndComplexityNew(
                                viewscript.Script, tobjs);
                            viewscript.SetTables(Profile.GetCorrectTableNames(tableNames));
                            viewscript.Hash = viewscript.GetHash(viewscript.Script);
                            Profile.AddViewscript(viewscript, connection);
                        }
                    }
                    if (currentViewscript.State == ViewscriptStates.Ready &&
                        Profile.ViewboxDb.Objects.Any(
                            (obj) => obj.TableName.ToLower() == currentViewscript.Name.ToLower() &&
                                     obj.Database == Profile.DbConfig.DbName))
                    {
                        currentViewscript.State = ViewscriptStates.Completed;
                    }
                    progress.StepDone();
                }
            }
        }

        private Dictionary<string, long> GetTableObjects() {
            var tobjs = (from o in Profile.ViewboxDb.Objects
                         where o.Type == TableType.Table && o.Database.ToLower() == Profile.DbConfig.DbName.ToLower()
                         select new Tuple<string, long>(o.TableName, o.RowCount*o.ColumnCount)).Union(
                             from o in Profile.ViewboxDb.Objects
                             where
                                 o.Type == TableType.Table && o.Database.ToLower() == Profile.DbConfig.DbName.ToLower()
                             select new Tuple<string, long>("_" + o.TableName, o.RowCount*o.ColumnCount)).ToDictionary(
                                 t => t.Item1, t => t.Item2);
            return tobjs;
        }

        private void UpdateViewscript(Viewscript newViewscript, Viewscript viewscriptToUpdate, Dictionary<string, long> tobjs) {
            viewscriptToUpdate.FileInfo = newViewscript.FileInfo;
            viewscriptToUpdate.Script = newViewscript.Script;
            viewscriptToUpdate.ViewInfo = newViewscript.ViewInfo;

            //if (viewscriptToUpdate.GetHash(viewscriptToUpdate.Script) != viewscriptToUpdate.Hash || viewscriptToUpdate.Indizes != newViewscript.Indizes || viewscriptToUpdate.Tables != newViewscript.Tables) {
            if (viewscriptToUpdate.GetHash(viewscriptToUpdate.Script) != viewscriptToUpdate.Hash) {
                //currentViewscript.HasDictionary = viewscript.HasDictionary;
                viewscriptToUpdate.Warnings = newViewscript.Warnings;
                viewscriptToUpdate.ErrorSql = newViewscript.ErrorSql;
                viewscriptToUpdate.ErrorSqlPrefix = newViewscript.ErrorSqlPrefix;
                viewscriptToUpdate.ErrorSqlSuffix = newViewscript.ErrorSqlSuffix;


                var tableNames = Profile.ViewboxDb.GetTableNamesAndComplexityNew(newViewscript.Script, tobjs);
                Dictionary<string, long> correctTableNames = Profile.GetCorrectTableNames(tableNames);
                var currentTableNames = viewscriptToUpdate.GetTables();
                if (currentTableNames.Count == 0) {
                    viewscriptToUpdate.State = ViewscriptStates.Warning;
                    viewscriptToUpdate.AddWarning(
                        "Keine der im Skript vorkommenden Tabellen ist im System vorhanden.");
                }
                if (!currentTableNames.All(t => correctTableNames.ContainsKey(t.Key)) ||
                    !correctTableNames.All(t => currentTableNames.ContainsKey(t.Key))) {
                    viewscriptToUpdate.SetTables(correctTableNames);
                }

                viewscriptToUpdate.Hash = viewscriptToUpdate.GetHash(viewscriptToUpdate.Script);
                viewscriptToUpdate.Indizes = newViewscript.Indizes;

                viewscriptToUpdate.SaveOrUpdate();
            }
        }

        void scriptLoader_MultipleViewError(object sender, EventArgs e) {
            if (e is MultipleViewErrorArgs) 
                _dlgScriptErrorsModel.MultipleViewError.Add(e);
        }

        /// <summary>
        /// Handles the ScriptError event of the scriptLoader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ViewBuilderBusiness.EventArgs.MessageEventArgs"/> instance containing the event data.</param>
        void scriptLoader_ScriptError(object sender, EventArgs e) {
            if (e is ScriptParseErrorArgs) 
                _dlgScriptErrorsModel.ScriptParseErrors.Add(e);
        }

        /// <summary>
        /// Handles the Error event of the scriptLoader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ViewBuilderBusiness.EventArgs.MessageEventArgs"/> instance containing the event data.</param>
        void scriptLoader_Error(object sender, Utils.ErrorEventArgs e) {
            _parentWindow.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate() {
                MessageBox.Show(_parentWindow,
                    e.Message,
                    "Allgemeiner Fehler beim Skript laden",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

            }));
        }
        #endregion Methods
    }
}
