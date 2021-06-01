using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using AV.Log;
using ProjectDb.Tables;
using ViewBuilderBusiness.EventArgs;
using ViewBuilderBusiness.Exceptions;
using ViewBuilderBusiness.Structures;
using ViewBuilderBusiness.Structures.Config;
using log4net;
using ErrorEventArgs = Utils.ErrorEventArgs;

namespace ViewBuilderBusiness
{
    /// <summary>
    /// </summary>
    public class ScriptLoader : INotifyPropertyChanged
    {
        private readonly ILog log = LogHelper.GetLogger();

        #region events

        /// <summary>
        ///   Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///   Occurs when an error occured.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        ///   Occurs when [script error].
        /// </summary>
        public event EventHandler ScriptError;

        /// <summary>
        ///   Occurs when [multiple view error].
        /// </summary>
        public event EventHandler MultipleViewError;

        #endregion events

        #region eventTrigger

        /// <summary>
        ///   Called when a property changed.
        /// </summary>
        /// <param name="property"> The property. </param>
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        ///   Called when an error occured.
        /// </summary>
        private void OnError(string message)
        {
            if (Error != null) Error(this, new ErrorEventArgs(message));
        }

        /// <summary>
        ///   Called when [script error].
        /// </summary>
        private void OnScriptError(string scriptfile, string message)
        {
            if (ScriptError != null) ScriptError(this, new ScriptParseErrorArgs(scriptfile, message));
        }

        /// <summary>
        ///   Called when [multiple view error].
        /// </summary>
        private void OnMultipleViewError(string view, List<string> scriptfiles)
        {
            if (MultipleViewError != null) MultipleViewError(this, new MultipleViewErrorArgs(view, scriptfiles));
        }

        #endregion eventTrigger

        #region fields

        /// <summary>
        ///   See property CurrentFile.
        /// </summary>
        private string _currentFile;

        /// <summary>
        ///   See property progress.
        /// </summary>
        private double _progress;

        /// <summary>
        ///   See property state.
        /// </summary>
        private string _state;

        #endregion fields

        #region properties

        /// <summary>
        ///   Gets or sets the progress.
        /// </summary>
        /// <value> The progress. </value>
        public double Progress
        {
            get { return _progress; }
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged("Progress");
                    OnPropertyChanged("ProgressInt");
                }
            }
        }

        /// <summary>
        ///   Gets the progress int.
        /// </summary>
        /// <value> The progress int. </value>
        public int ProgressInt
        {
            get { return (int) (100*Progress); }
        }

        /// <summary>
        ///   Gets or sets the state.
        /// </summary>
        /// <value> The state. </value>
        public string State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the current file.
        /// </summary>
        /// <value> The current file. </value>
        public string CurrentFile
        {
            get { return _currentFile; }
            set
            {
                if (_currentFile != value)
                {
                    _currentFile = value;
                    OnPropertyChanged("CurrentFile");
                }
            }
        }

        #endregion properties

        #region methods

        /// <summary>
        ///   Loads the specified directory.
        /// </summary>
        /// <param name="directory"> The directory. </param>
        /// <param name="includeSubdirectories"> if set to <c>true</c> [include subdirectories]. </param>
        /// <returns> </returns>
        public List<ScriptFile> Load(string directory, bool includeSubdirectories, ProfileConfig Profile)
        {
            Progress = 0;

            List<ScriptFile> scripts = new List<ScriptFile>();
            try
            {
                State = "Initialisiere Scriptliste...";
                FileInfo[] files;
                try
                {
                    if (includeSubdirectories)
                    {
                        files = new DirectoryInfo(directory).GetFiles("*.sql", SearchOption.AllDirectories);
                    }
                    else
                    {
                        files = new DirectoryInfo(directory).GetFiles("*.sql", SearchOption.TopDirectoryOnly);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorWithCheck(ex.Message, ex);
                    throw;
                }
                State = "Parse Viewscripte...";
                int curFileIndex = 0;
                foreach (FileInfo fi in files)
                {
                    try
                    {
                        CurrentFile = fi.FullName;
                        scripts.Add(new ScriptFile(fi, Profile));
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (InvalidScriptException ex)
                    {
                        OnScriptError(fi.FullName, ex.Message);
                    }
                    catch (Exception ex)
                    {
                        OnError(ex.Message);
                    }
                    curFileIndex++;
                    Progress = curFileIndex/(double) files.Count();
                }
                State = "Prüfe auf doppelte Einträge...";
                Dictionary<string, List<string>> viewDict =
                    new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                foreach (ScriptFile script in scripts)
                {
                    foreach (Viewscript view in script.Views)
                    {
                        if (!viewDict.ContainsKey(view.Name))
                        {
                            viewDict[view.Name] = new List<string>();
                        }
                        viewDict[view.Name].Add(view.FileInfo.FullName);
                    }
                }
                foreach (string viewname in viewDict.Keys)
                {
                    if (viewDict[viewname].Count > 1)
                    {
                        OnMultipleViewError(viewname, viewDict[viewname]);
                    }
                }
                State = "Vorgang abgeschlossen...";
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
            return scripts;
        }

        #endregion methods

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/
    }
}