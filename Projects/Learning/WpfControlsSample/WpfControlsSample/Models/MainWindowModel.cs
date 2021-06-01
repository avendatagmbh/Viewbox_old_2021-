using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using DbSearch.Structures.Navigation;
using WpfControlsSample.Controls;

namespace WpfControlsSample.Models {
    public class MainWindowModel {
        #region Constructor
        public MainWindowModel(Window owner) {
            NavigationTree = new NavigationTree(owner);
            NavigationTree.AddEntry("AssistantControl", new CtlAssistantControlDemo());
            CtlButtonsDemo ctlButtonsDemo = new CtlButtonsDemo(){DataContext = new ButtonsDemoModel(owner)};
            NavigationTree.AddEntry("Buttons", ctlButtonsDemo);
            NavigationTree.AddEntry("Transformations", new CtlTransformationDemo());
            NavigationTree.AddEntry("Animations", new CtlAnimationDemo());
            NavigationTree.AddEntry("Lists", new CtlListDemo(){DataContext=new ListDemoModel()});
        }
        #endregion Constructor

        #region Properties
        public NavigationTree NavigationTree { get; private set; }
        #endregion Properties

        #region Methods
        #endregion Methods
    }
}
