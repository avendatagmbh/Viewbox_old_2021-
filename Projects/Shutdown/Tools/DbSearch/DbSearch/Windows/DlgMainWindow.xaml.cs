// -----------------------------------------------------------
// Created by Benjamin Held - 07.09.2011 13:43:51
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Threading;
using DbSearch.Models;
using DbSearchBase;
using DbSearchLogic.Manager;
using DbSearchLogic.ProcessQueue;
using DbSearchLogic.SearchCore;
using DbSearchLogic.SearchCore.Threading;
using DbSearchLogic.Structures.TableRelated;
using DbSearchLogic.SearchCore.KeySearch;
using System.Collections.Concurrent;
using Microsoft.Win32;
using Utils;
using log4net;
using ErrorEventArgs = DbSearchLogic.SearchCore.Events.ErrorEventArgs;
using Key = DbSearchLogic.SearchCore.KeySearch.Key;
using AV.Log;

namespace DbSearch.Windows {
    /// <summary>
    /// Interaktionslogik für DlgMainWindow.xaml
    /// </summary>
    public partial class DlgMainWindow : Window {

        internal ILog _log = LogHelper.GetLogger();

        public DlgMainWindow() {
            try {
                InitializeComponent();

                Model = new MainWindowModel(this) {DummyButtonControl = dummyButtonControl};
                DataContext = Model;
                Model.NoKeysFound += ModelOnNoKeysFound;
                Model.CandidateKeysFound += ModelOnCandidateKeysFound;
                ThreadManager.ErrorHandler += ThreadManager_Error;
            }
            catch(Exception ex) {
                _log.Log(LogLevelEnum.Error, ex.Message);
                //throw;
                MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThreadManager_Error(object sender, ErrorEventArgs e) {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background,
                              new Action(
                                  () => {
                                      _log.Log(LogLevelEnum.Error, e.Message);
                                      MessageBox.Show(this, e.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                                  }
                                )
                )
            ;
        }

        public MainWindowModel Model { get; set; }

        private void btnExit_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (ThreadManager.RunningThreads.Count > 0) {
                if (MessageBox.Show(this, "Es laufen noch Threads, wollen Sie wirklich beenden?", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No) {
                    e.Cancel = true;
                    return;
                }
            }

            if (Model.SelectedProfile != null) {
                ApplicationManager.ApplicationConfig.LastProfile = Model.SelectedProfile.Name;
                if (Model.SelectedQuery != null)
                    ApplicationManager.ApplicationConfig.LastQueryTable = Model.SelectedQuery.Name;
            }

            ApplicationManager.Save();
        }

        private void btnSearchAll_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            Model.SearchAllQueries();
        }

        private void btnImportQueries_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            Model.ImportQueries();
        }

        private void cboSelectedQuery_DropDownClosed(object sender, EventArgs e) {
            Model.SelectedQuery = (Query) cboSelectedQuery.SelectedItem;
        }

        private void btnSearchQuery_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            Model.StartQuerySearch();
        }

        private void btnSaveQuery_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            Model.SaveQuery();
            MessageBox.Show(this, "Abfrage gespeichert", "", MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void btnFindOptimalRows_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            if (MessageBox.Show("Bitte beachten Sie, dass alle bisherigen Zeilen und Spalten dabei verloren gehen. Trotzdem fortfahren?", "", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                Model.FindOptimalRows();
                MessageBox.Show(this, "Optimale Zeilen wurden ausgewählt", "", MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            DlgInfo dlgInfo = new DlgInfo();
            dlgInfo.ShowDialog();
        }

        #region DragDrop
        private void Window_DragOver(object sender, DragEventArgs e) {
            //if (this.DragDropPopup.IsOpen) {
            //    Size popupSize = new Size(DragDropPopup.ActualWidth, DragDropPopup.ActualHeight);
            //    DragDropPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);
            //    if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            //        e.Effects = DragDropEffects.Copy;
            //}

        }

        private void Window_Drop(object sender, DragEventArgs e) {
            //Model.RuleListModel.DragDropData.Dragging = false;
            ////If a rule is dropped somewhere in the window where it should not be dropped, then the user most certainly wanted to delete the rule
            //if (Model.RuleListModel.DragDropData.DragSource != null) {
            //    Model.RuleListModel.DragDropData.DragSource.DeleteRule();
            //}
        }
        #endregion DragDrop

        private void btnUpdateDatabase_Click(object sender, RoutedEventArgs e) {
            try {
                var profile = Model.SelectedProfile;
                Model.SelectedProfile.UpdateDatabase();
                Model.SelectedProfile = null;
                Model.SelectedProfile = profile;
            } catch (Exception ex) {
                MessageBox.Show(this,
                                "Es gab einen Fehler beim Update der Datenbank: " + Environment.NewLine + ex.Message, "",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBox.Show(this,"Update war erfolgreich", "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnDistinct_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            SelectThreadNumberModel model = new SelectThreadNumberModel();
            DlgSelectThreadNumber dialog = new DlgSelectThreadNumber(){DataContext = model, Owner = this};
            var result = dialog.ShowDialog();
            if(result != null && result.Value)
                Model.DistinctDb(model.ThreadCount);
        }

        private void btnAddEmptyQuery_Click(object sender, RoutedEventArgs e) {
            dummyButtonControl.Focus();
            Model.AddEmptyQuery();
        }

        /// <summary>
        /// The keysearching
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event</param>
        private void BtnFindKeys_OnClick(object sender, RoutedEventArgs e) {
            CancellationTokenSource cancelationSource = new CancellationTokenSource();
            Action<object> action = (object obj) => HandleError(() => {
                PrimaryKeySearchStatEntry pkStatEntry = new PrimaryKeySearchStatEntry(2, 0, cancelationSource, Model.KeySearchManagerInstance.CurrentProfile.DisplayString);
                pkStatEntry.KeySearchTask = new Task<KeySearchResult>(() => { using (pkStatEntry) return Model.FindKeys(RecreateTables, pkStatEntry); });
                Model.KeySearchManagerInstance.AddKeySearchStatEnrty<PrimaryKeySearchStatEntry>(pkStatEntry);
            });
            Task task = new Task(action, string.Empty);//, cancelationSource);
            //Task task = new Task(action, cancelationSource);
            task.Start();

            //Task observerTask = task.ContinueWith((t) => {
            //    foreach (Exception exception in t.Exception.Flatten().InnerExceptions)
            //         Console.WriteLine("Observed exception: " + exception.Message);
            //}, TaskContinuationOptions.OnlyOnFaulted);

            dummyButtonControl.Focus();
        }

        public bool RecreateTables() {
            AskDelegate handler = Ask;
            List<bool> result = new List<bool>();
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate() { handler(result); }) );
            return result.FirstOrDefault();
        }

        private delegate void AskDelegate(List<bool> result);
        private void Ask(List<bool> retVal) {
            MessageBoxResult result = MessageBox.Show(this, "The key search has already run on this database!\r\nWould you really like to delete all keys and run a new search?", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes) {
                Model.ReCreateTables();
                Model.ReCreateTempTables();
                retVal.Add(true);
                return;
            }
            retVal.Add(false);
        }

        private void HandleError(Action action) {
            try {
                action();
            } catch (DbException ex) {
                Dispatcher.Invoke(
                  DispatcherPriority.Normal,
                  new Action( delegate() { MessageBox.Show(this, "Error: " + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error); }
                ));
            }
            catch (Exception) {
                throw;
            }
        }

        private void ExportKeys() {
            Dispatcher.Invoke(
                 DispatcherPriority.Normal,
                 new Action(delegate() {
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.FileName = string.Format(Model.SelectedProfile.DbConfigView.DbName + "_PrimaryKeys");
                    dlg.FileOk += SaveKeysToFile;
                    dlg.Filter = "csv files (*.csv)|*.csv";
                    dlg.Tag = false;
                    dlg.ShowDialog();
                 }
               ));
        }

        private void SaveKeysToFile(object sender, CancelEventArgs e) {
            string fileName = ((SaveFileDialog)sender).FileName;
            if (Model.ExportKeys(fileName))
                MessageBox.Show(this, "The keys were saved successfully.", "", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(this, "Saving the keys failed!", "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// The foreign keysearching
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event</param>
        private void BtnFindForeignKeys_OnClick(object sender, RoutedEventArgs e){
            try {
                this.Cursor = Cursors.Wait;

                if (Model.IsPrimaryKeySearchRunning()) {
                    MsgPrimaryKeySearchIsRunning();
                    return;
                }

                bool isKeysExists = false;
                HandleError(() => isKeysExists = Model.KeysExists());
                dummyButtonControl.Focus();
                if (!isKeysExists) {
                    MsgNoKeysFound();
                    return;
                }
                if (Model.ExistTable<KeyCandidate>()) {
                    MsgCandidateKeysFound();
                    return;
                }

                TableListModel model = new TableListModel(Model.GetAllTables());
                //Show the table selector dialog to the user
                DlgSelectTable dialog = new DlgSelectTable() { DataContext = model, Owner = this };
                this.Cursor = Cursors.Arrow;
                bool? result = dialog.ShowDialog();
                if (result.HasValue && result.Value) {
                    //HandleError(() => Model.StartForeignKeySerchForTable(keys, model.SelectedTable, 2));
                    Action<object> action = (object obj) => {
                        HandleError(() => {
                            _log.Log(LogLevelEnum.Info, "Loading keys...", true);
                            foreach (Table selectedTable in model.SelectedTables) {
                                Model.InitiateForeignKeySearch(selectedTable);
                            }
                        });
                    };
                    System.Threading.Tasks.Task task = new System.Threading.Tasks.Task(action, string.Empty);
                    task.Start();
                } else
                    _log.Log(LogLevelEnum.Info, "Foreign key search cancelled!", true);
            }
            finally {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void ExportForeignKeys(KeyValuePair<int, string> tableName) {
            Dispatcher.Invoke(
                 DispatcherPriority.Normal,
                 new Action(delegate() {
                        SaveFileDialog dlg = new SaveFileDialog();
                        dlg.FileName = string.Format(Model.SelectedProfile.DbConfigView.DbName + "_" + tableName.Value + "_ForeignKeys");
                        dlg.FileOk += SaveForeignKeysToFile;
                        dlg.Filter = "csv files (*.csv)|*.csv";
                        dlg.Tag = false;
                        dlg.ShowDialog();
                    }
               ));
        }
        
        private void SaveForeignKeysToFile(object sender, CancelEventArgs e) {
            string fileName = ((SaveFileDialog)sender).FileName;
            if (Model.ExportForeignKeys(fileName))
                MessageBox.Show(this, "The foreign keys were saved successfully.", "", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(this, "Saving the foreign keys failed!", "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void MsgPrimaryKeySearchIsRunning() {
            string msg = "Foreign key search cannot be performed. Primary key search is running!";
            // DEVNOTE: deadlock in Log|Mamager?
            //LogManager.Status(msg);
            MessageBox.Show(this, msg, "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MsgNoKeysFound() {
            string msg = "Foreign key search cannot be performed. Please run the key search first, there are no keys found!";
            _log.Log(LogLevelEnum.Info, msg, true);
            MessageBox.Show(this, msg, "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MsgCandidateKeysFound() {
            string msg = "Foreign key search cannot be performed. The key search is not finished yet, if interrupted please run the key search again!";
            _log.Log(LogLevelEnum.Info, msg, true);
            MessageBox.Show(this, msg, "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ModelOnNoKeysFound(object sender, EventArgs eventArgs) {
            string msg = "Automatic foreign key search cannot be performed. Please run the key search first, there are no keys found!";
            _log.Log(LogLevelEnum.Info, msg, true);
            MessageBox.Show(this, msg, "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ModelOnCandidateKeysFound(object sender, EventArgs eventArgs) {
            string msg = "Automatic foreign key search cannot be performed. The key search is not finished yet, if interrupted please run the key search again!";
            _log.Log(LogLevelEnum.Info, msg, true);
            MessageBox.Show(this, msg, "", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Main window hotkeys handling
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Parameter</param>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            //if (e.Key.ToString().ToLower() == "escape" && Model.CancellationTokenSource != null) {
            //    //Set the cancellation token and save the results
            //    if (MessageBox.Show(this, "Do you really want to stop key search?", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            //        Model.CancellationTokenSource.Cancel();
            //}
        }

    }
}