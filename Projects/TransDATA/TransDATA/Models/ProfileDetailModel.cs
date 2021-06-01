// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-12
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Base.Localisation;
using Config.Enums;
using Config.Interfaces.DbStructure;
using Utils;

namespace TransDATA.Models {
    public class ProfileDetailModel : NotifyPropertyChangedBase {
        public ProfileDetailModel(IProfile profile) {
            Profile = profile;
        }

        public IProfile Profile { get; set; }

        #region InputConfigTypes
        private static readonly List<NamedEnum> _inputConfigTypes =
            (from object value in Enum.GetValues(typeof (InputConfigTypes))
             select new NamedEnum(
                 value,
                 EnumNames.ResourceManager.GetString("InputConfigTypes_" +
                                                     Enum.GetName(typeof (InputConfigTypes), value))))
                .ToList();

        public static List<NamedEnum> InputConfigTypes { get { return _inputConfigTypes; } }
        #endregion

        #region SelectedInputConfigType
        private NamedEnum _selectedInputConfigType;

        public NamedEnum SelectedInputConfigType {
            get {
                if (_selectedInputConfigType == null) {
                    foreach (
                        var inputConfigType in
                            InputConfigTypes.Where(
                                inputConfigType => (InputConfigTypes) inputConfigType.Value == Profile.InputConfig.Type)
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
            }
        }
        #endregion

        #region MaxThreadCount
        public int MaxThreadCount { get { return Profile.MaxThreadCount; }
            set { Profile.MaxThreadCount = value; }
        }
        #endregion MaxThreadCount

        #region OutputConfigTypes
        private static readonly List<NamedEnum> _outputConfigTypes =
            (from object value in Enum.GetValues(typeof (OutputConfigTypes))
             select new NamedEnum(
                 value,
                 EnumNames.ResourceManager.GetString("OutputConfigTypes_" +
                                                     Enum.GetName(typeof (OutputConfigTypes), value))))
                .ToList();

        public static List<NamedEnum> OutputConfigTypes { get { return _outputConfigTypes; } }
        #endregion

        #region SelectedOutputConfigType
        private NamedEnum _selectedOutputConfigType;

        public NamedEnum SelectedOutputConfigType {
            get {
                if (_selectedOutputConfigType == null) {
                    foreach (
                        var outputConfigTypes in
                            OutputConfigTypes.Where(
                                outputConfigTypes =>
                                (OutputConfigTypes) outputConfigTypes.Value == Profile.OutputConfig.Type)) {
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
    }
}