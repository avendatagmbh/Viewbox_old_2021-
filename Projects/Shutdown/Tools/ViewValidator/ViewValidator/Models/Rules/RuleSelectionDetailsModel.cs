// -----------------------------------------------------------
// Created by Benjamin Held - 30.08.2011 10:27:38
// Copyright AvenDATA 2011
// -----------------------------------------------------------
using System.Windows.Media;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Models.Rules {
    public class RuleSelectionDetailsModel {
        public RuleSelectionDetailsModel(TableMapping tableMapping, int id) {
            this.TableMapping = tableMapping;
            Name = (id + 1) + ". " + TableMapping.TableValidation.Name + " - " + TableMapping.TableView.Name;
            TextColor = Brushes.Black;
            
        }

        public TableMapping TableMapping { get; private set; }
        public string Name { get; set; }
        public Brush TextColor { get; set; }
        public string FilterValidation {
            get { return TableMapping.TableValidation.Filter.FilterString; }
            set { TableMapping.TableValidation.Filter.FilterString = value; }
        }

        public string FilterView {
            get { return TableMapping.TableView.Filter.FilterString; }
            set { TableMapping.TableView.Filter.FilterString = value; }
        }
    }
}
