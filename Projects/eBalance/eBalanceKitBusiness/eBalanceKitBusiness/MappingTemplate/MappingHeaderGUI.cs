// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-05
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKitBusiness.MappingTemplate {
    public class MappingHeaderGui : MappingGuiBase {
        public MappingHeaderGui(MappingTemplateLine mapping) {
            Mapping = mapping;
            Mapping.PropertyChanged += MappingPropertyChanged;
            InitTemplateLines();
        }

        #region properties

        public override bool IsSelected {
            get { return base.IsSelected; }
            set {
                base.IsSelected = value;
                if (_templateLines.Count == 1) _templateLines.First().IsSelected = true;
            }
        }

        internal MappingTemplateLine Mapping { get; private set; }

        #region TemplateLines
        private readonly ObservableCollection<MappingLineGui> _templateLines =
            new ObservableCollection<MappingLineGui>();

        public IEnumerable<MappingLineGui> TemplateLines { get { return _templateLines; } }
        #endregion TemplateLines

        public override string AccountNumber { get { return Mapping.AccountNumber; } set { Mapping.AccountNumber = value; } }
        public override string AccountName { get { return Mapping.AccountName; } set { Mapping.AccountName = value; } }
        public bool IsAccountOfExchange { get { return Mapping.IsAccountOfExchange; } set { Mapping.IsAccountOfExchange = value; } }
        public string AccountLabel { get { return AccountNumber + " - " + AccountName; } }

        #region IsAssigned
        public bool IsAssigned {
            get {
                return IsAccountOfExchange
                           ? Mapping.CreditElementId != null && Mapping.DebitElementId != null
                           : Mapping.ElementId != null;
            }
        }
        #endregion

        #region MissingAssignmentWarning
        public string MissingAssignmentWarning {
            get {
                if (IsAccountOfExchange) {
                    string msg = string.Empty;
                    if (Mapping.CreditElementId == null) msg += "Für dieses Konto wurde noch keine Zuordnung für Haben-Beträge vorgenommen.";
                    if (Mapping.DebitElementId == null) {
                        if (msg.Length > 0) msg += Environment.NewLine;
                        msg += "Für dieses Konto wurde noch keine Zuordnung für Soll-Beträge vorgenommen.";
                    }
                    return msg;
                }

                return Mapping.ElementId == null ? "Für dieses Konto wurde noch keine Zuordnung vorgenommen." : string.Empty;
            }
        }
        #endregion

        #endregion properties

        private void MappingPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {

                case "CreditElementId":
                case "DebitElementId":
                    if (IsAccountOfExchange) {
                        OnPropertyChanged("IsAssigned");
                        OnPropertyChanged("MissingAssignmentWarning");
                    }
                    break;

                case "ElementId":
                    if (!IsAccountOfExchange) {
                        OnPropertyChanged("IsAssigned");
                        OnPropertyChanged("MissingAssignmentWarning");
                    }
                    break;

                case "IsAccountOfExchange": {
                    List<IPresentationTreeNode> nodes =
                        _templateLines.SelectMany(line => line.Parents).Cast<IPresentationTreeNode>().Distinct().ToList();

                    foreach (var line in _templateLines) line.RemoveFromParents();
                
                    InitTemplateLines();

                    foreach (var line in _templateLines) {
                        var elementId = line.ElementId;
                        foreach (var node in nodes.Where(node => elementId != null && node.Element.Id == elementId)) {
                            node.AddChildren(line);
                        }
                    }

                    OnPropertyChanged("IsAssigned");
                    OnPropertyChanged("MissingAssignmentWarning");
                }
                    break;
            }
        }

        private void InitTemplateLines() {
            _templateLines.Clear();
            if (IsAccountOfExchange) {
                _templateLines.Add(new MappingLineGui(this) { Type = MappingTemplateTypes.Debitor });
                _templateLines.Add(new MappingLineGui(this) { Type = MappingTemplateTypes.Creditor });
            } else {
                _templateLines.Add(new MappingLineGui(this) { Type = MappingTemplateTypes.None });
            }
        }
        
    }
}