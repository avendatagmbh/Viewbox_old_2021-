// -----------------------------------------------------------
// Created by Benjamin Held - 30.08.2011 10:27:22
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using DbAccess.Structures;
using ViewValidator.Models.Profile;
using ViewValidatorLogic.Structures.InitialSetup;
using System.ComponentModel;

namespace ViewValidator.Models.Rules {
    public class ExtendedDataPreviewModel :DataPreviewModel{
        //#region Events
        //public event PropertyChangedEventHandler PropertyChanged;
        //private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        //#endregion

        #region Properties

        public RuleAssignmentModel RuleAssignmentModel { get; private set; }
        #endregion

        #region Constructor
        public ExtendedDataPreviewModel(RuleAssignmentModel ruleAssignmentModel){
            RuleAssignmentModel = ruleAssignmentModel;
        }

        public ExtendedDataPreviewModel(TableMapping tableMapping, RuleAssignmentModel ruleAssignmentModel)
        : base(tableMapping) {
            RuleAssignmentModel = ruleAssignmentModel;
        }
        #endregion

        #region Methods
        public override void FillData(bool addLimit){
            base.FillData(addLimit);
            OnPropertyChanged("DataValidation");
            OnPropertyChanged("DataView");
        }
        #endregion
    }
}
