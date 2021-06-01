// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-11-16
// copyright 2011 AvenDATA GmbH
// refactored and unit tested by Attila Papp 2012-09-19
// --------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Markup.Localizer;
using Utils;
using ViewboxAdmin_ViewModel;
using ViewboxAdmin_ViewModel.Structures.Config;
using ErrorEventArgs = System.IO.ErrorEventArgs;
using Part = SystemDb.SystemDb.Part;


namespace ViewboxAdmin.Models.ProfileRelated {
    public class ProfileModel : NotifyBase, IProfileModel {

        #region Constructor
        public ProfileModel(IProfile profile, IProgressCalculator progresscalculator, IProfilePartLoadingEnumHelper partloadingenumhelper) {
            this.Profile = profile;
            this.LoadingProgress = progresscalculator;
            this.ProfilePartLoadingEnumHelper = partloadingenumhelper;
            LoadingCompletedPartDictionary = new Dictionary<Part,string>();
            InitDictionary();
        }

        
        
        #endregion

        #region Events
        public event EventHandler<ErrorEventArgs> FinishedProfileLoadingEvent;
        private void OnFinishedProfileLoadingEvent(ErrorEventArgs e) {
            EventHandler<ErrorEventArgs> handler = FinishedProfileLoadingEvent;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<ErrorEventArgs> Error;
        private void OnError(ErrorEventArgs e)
        {
            EventHandler<ErrorEventArgs> handler = Error;
            if (handler != null) handler(this, e);
        }
        #endregion Events

        #region Properties

        public IProfilePartLoadingEnumHelper ProfilePartLoadingEnumHelper { get; private set; }
        public IProfile Profile { get; private set; }

        public Dictionary<Part, string> LoadingCompletedPartDictionary { get; private set; }
        
        private IProgressCalculator _loadingProgress;
        public IProgressCalculator LoadingProgress {
            get { return _loadingProgress; }
            set {
                if (_loadingProgress != value) {
                    _loadingProgress = value;
                    OnPropertyChanged("LoadingProgress");
                }
            }
        }
        
        private object _lock = new object();
        #endregion

        #region public methods

        public void LoadData() {
            lock (_lock)
            {
                if (Profile.Loaded || Profile.IsLoading)
                {
                    OnFinishedProfileLoadingEvent(new ErrorEventArgs(null));
                    return;
                }
                Profile.SystemDb.PartLoadingCompleted += SystemDb_PartLoadingCompleted;
                Profile.SystemDb.LoadingFinished += () => OnFinishedProfileLoadingEvent(new ErrorEventArgs(null));

                DoAsyncLoading();
            }
        }

        #endregion

        #region private methods

        private void DoAsyncLoading() {
            var task = Task.Factory.StartNew(() => Profile.Load());
            try {
                task.Wait();
            } catch (AggregateException e) {
                OnFinishedProfileLoadingEvent(new ErrorEventArgs(e.InnerException));
            }
        }

        private void SystemDb_PartLoadingCompleted(SystemDb.SystemDb sender, Part part) {
            LoadingProgress.StepDone();
            if (ProfilePartLoadingEnumHelper.IsNotLast(part)) {
                Part nextPart = ProfilePartLoadingEnumHelper.GetNextPartEnum(part);
                try {
                    if (LoadingCompletedPartDictionary.ContainsKey(nextPart))
                    {LoadingProgress.Description = LoadingCompletedPartDictionary[nextPart];}
                } catch (Exception ex) {
                    
                    OnError(new ErrorEventArgs(new ApplicationException(string.Format("({0}) {1}",nextPart.ToString(), ex.Message), ex)));
                    //throw new ArgumentException(string.Format("Part does not exists: {0}", nextPart));
                }
            }
        }

        private void InitDictionary() {
            LoadingCompletedPartDictionary.Add(Part.Languages, "Sprachen");
            LoadingCompletedPartDictionary.Add(Part.Users, "Benutzer");
            LoadingCompletedPartDictionary.Add(Part.Optimizations, "Optimierungen");
            LoadingCompletedPartDictionary.Add(Part.Categories, "Kategorien");
            LoadingCompletedPartDictionary.Add(Part.Properties, "Eigenschaften");
            LoadingCompletedPartDictionary.Add(Part.Schemes, "Schemata");
            LoadingCompletedPartDictionary.Add(Part.TableObjects, "Tabellenobjekte");
            LoadingCompletedPartDictionary.Add(Part.IssueExtensions, "Report Erweiterungen");
            LoadingCompletedPartDictionary.Add(Part.TableSchemes, "Tabellenschemata");
            LoadingCompletedPartDictionary.Add(Part.OrderAreas, "Order Areas");
            LoadingCompletedPartDictionary.Add(Part.Columns, "Spalten");
            LoadingCompletedPartDictionary.Add(Part.Parameters, "Parameters");
            LoadingCompletedPartDictionary.Add(Part.Relations, "Relationen");
            LoadingCompletedPartDictionary.Add(Part.About, "About");
            LoadingCompletedPartDictionary.Add(Part.Notes, "Notizen");
            LoadingCompletedPartDictionary.Add(Part.Indexes, "Indexes");
        }

        #endregion Methods
        
    }
}
