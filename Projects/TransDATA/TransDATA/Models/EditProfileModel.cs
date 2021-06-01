using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Base.Localisation;
using Config;
using Config.Enums;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Config.Manager;
using TransDATA.Controls;
using TransDATA.Controls.Config;
using TransDATA.Structures;
using Utils;

namespace TransDATA.Models {
    internal class EditProfileModel : NotifyPropertyChangedBase{
        #region Constructor
        internal EditProfileModel(IProfile profile, Window owner) {
            Owner = owner;
            Profile = profile;
            if (Profile == null) {
                Profile = ConfigDb.ProfileManager.CreateProfile();
                //Prevent DbUpdate as this profile is new and will perhaps not be saved
                Profile.DoDbUpdate = false;
                IsNewProfile = true;
            }
            //Init navigation
            NavigationTree = new NavigationTree(owner);
            _commonNavEntry = NavigationTree.AddEntry(ResourcesCommon.Common, new CtlProfileDetailsWrapper(new CtlProfileGeneral(), ResourcesCommon.Common));
            _commonNavEntry.IsSelected = true;
            _inputNavEntry = NavigationTree.AddEntry(ResourcesCommon.InputConfig, new CtlProfileDetailsWrapper(new CtlInputConfigWithSelector(),ResourcesCommon.InputConfig));
            _outputNavEntry = NavigationTree.AddEntry(ResourcesCommon.OutputConfig, new CtlProfileDetailsWrapper(new CtlOutputConfigWithSelector(),ResourcesCommon.OutputConfig));
        }
        #endregion Constructor

        #region Properties
        //public ICommand SaveCommand {get;private set;}
        private Window Owner {get; set; }

        private IProfile _profile;
        public IProfile Profile {
            get { return _profile; }
            set {
                _profile = value;
                OnPropertyChanged("Profile");
            }
        }

        public bool IsNewProfile { get; set; }
        public bool Saved {get; private set; }
        #region InputConfigTypes
        private static readonly List<NamedEnum> _inputConfigTypes =
            (from object value in Enum.GetValues(typeof(InputConfigTypes))
             select new NamedEnum(
                 value,
                 EnumNames.ResourceManager.GetString("InputConfigTypes_" +
                                                     Enum.GetName(typeof(InputConfigTypes), value))))
                .ToList();

        public static List<NamedEnum> InputConfigTypes {
            get {
                return _inputConfigTypes;
            }
        }
        #endregion

        #region SelectedInputConfigType
        private NamedEnum _selectedInputConfigType;

        public NamedEnum SelectedInputConfigType {
            get {
                if (_selectedInputConfigType == null) {
                    foreach (
                        var inputConfigType in
                            InputConfigTypes.Where(
                                inputConfigType => (InputConfigTypes)inputConfigType.Value == Profile.InputConfig.Type)
                        ) {
                        _selectedInputConfigType = inputConfigType;
                        break;
                    }
                }
                return _selectedInputConfigType;
            }
            set {
                _selectedInputConfigType = value;
                Profile.InputConfig.Type = (InputConfigTypes)value.Value;
                //switch(Profile.InputConfig.Type) {
                //    case Config.Enums.InputConfigTypes.Database:
                //        InputConfigModel = new SelectDatabaseInputModel(Profile.InputConfig as IDatabaseInputConfig);
                //        break;
                //    case Config.Enums.InputConfigTypes.Csv:
                //        InputConfigModel = new SelectCsvInputModel(Profile.InputConfig as ICsvInputConfig);
                //        break;
                //    default:
                //        throw new ArgumentOutOfRangeException();
                //}
            }
        }
        #endregion

        #region MaxThreadCount
        public int MaxThreadCount {
            get {
                return Profile.MaxThreadCount;
            }
            set {
                Profile.MaxThreadCount = value;
            }
        }
        #endregion MaxThreadCount

        #region OutputConfigTypes
        private static readonly List<NamedEnum> _outputConfigTypes =
            (from object value in Enum.GetValues(typeof(OutputConfigTypes))
             select new NamedEnum(
                 value,
                 EnumNames.ResourceManager.GetString("OutputConfigTypes_" +
                                                     Enum.GetName(typeof(OutputConfigTypes), value))))
                .ToList();

        public static List<NamedEnum> OutputConfigTypes {
            get {
                return _outputConfigTypes;
            }
        }
        #endregion

        #region SelectedOutputConfigType
        private NamedEnum _selectedOutputConfigType;
        private readonly NavigationTreeEntry _commonNavEntry;
        private readonly NavigationTreeEntry _inputNavEntry;
        private readonly NavigationTreeEntry _outputNavEntry;

        public NamedEnum SelectedOutputConfigType {
            get {
                if (_selectedOutputConfigType == null) {
                    foreach (
                        var outputConfigTypes in
                            OutputConfigTypes.Where(
                                outputConfigTypes =>
                                (OutputConfigTypes)outputConfigTypes.Value == Profile.OutputConfig.Type)) {
                        _selectedOutputConfigType = outputConfigTypes;
                        break;
                    }
                }
                return _selectedOutputConfigType;
            }
            set {
                _selectedOutputConfigType = value;
                Profile.OutputConfig.Type = (OutputConfigTypes)value.Value;
            }
        }
        #endregion

        //public  void Hack() {
        //    OnPropertyChanged("Type");
        //    OnPropertyChanged("Config");
        //}

        public NavigationTree NavigationTree { get; set; }
        #endregion Properties

        #region Methods
        private bool ValidateInput() {
            StringBuilder errors = new StringBuilder();
            if (string.IsNullOrEmpty(Profile.Name)) {
                errors.Append("Bitte geben Sie einen Profil Namen ein.").Append(Environment.NewLine);
                _commonNavEntry.ValidationError = true;
            }
            string error;
            if (!Profile.InputConfig.Validate(out error)) {
                errors.Append(error).Append(Environment.NewLine);
                _inputNavEntry.ValidationError = true;
            }

            if (!Profile.OutputConfig.Config.Validate(out error)) {
                errors.Append(error).Append(Environment.NewLine);
                _outputNavEntry.ValidationError = true;
            }

            if (errors.Length != 0)
                return MessageBox.Show(Owner,
                                "Die eingetragenen Daten weisen folgende Fehler auf:" + Environment.NewLine +
                                errors.ToString() + Environment.NewLine + "Möchten Sie dennoch speichern?", "", MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) == MessageBoxResult.Yes;
            return true;
        }

        public void Save() {
            if (!ValidateInput())
                return;
            //Do some checking if everything is alright
            Saved = true;
            Profile.DoDbUpdate = true;
            Profile.Save();
            Owner.Close();
        }

        public void Cancel() {
            Owner.Close();
        }
        #endregion Methods
    }
}
