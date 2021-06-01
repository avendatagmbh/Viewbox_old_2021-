using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ViewboxAdmin.Controls;
using ViewboxAdmin.Structures;
using ViewboxAdmin.Windows;

namespace ViewboxAdmin.ViewModels
{
    public class NavigationEntriesDataContext : INavigationEntriesDataContext {
        public NavigationEntriesDataContext(INavigationTree navtree) {
            this.NavigationTree = navtree;
            this.NavigationTreeEntries = new Dictionary<string, INavigationTreeEntry>();
            InitializeComponent();
        }

        private void InitializeComponent() {
            INavigationTreeEntry generalEntry = NavigationTree.AddEntry("Settings", null);
            generalEntry.IsExpanded = true;
            NavigationTreeEntries.Add("Verwaltung", generalEntry);

            INavigationTreeEntry testEntry = NavigationTree.AddEntry("Profile", new AttilaCtlListView(), generalEntry);
            NavigationTreeEntries.Add("Profile", testEntry);

            INavigationTreeEntry viewboxEntry = NavigationTree.AddEntry("Viewbox", null);
            NavigationTreeEntries.Add("ViewBox",viewboxEntry);

            INavigationTreeEntry deletesystemEntry = NavigationTree.AddEntry("Delete System", new SystemDeleteView(), viewboxEntry);
            NavigationTreeEntries.Add("DeleteSystem",deletesystemEntry);

            INavigationTreeEntry editEntry = NavigationTree.AddEntry("Edit Text", new EditTextView(), viewboxEntry);
            NavigationTreeEntries.Add("EditText", editEntry);

            INavigationTreeEntry mergeEntry = NavigationTree.AddEntry("Merge", new MergerView(), viewboxEntry);
            NavigationTreeEntries.Add("Merge", mergeEntry);

            INavigationTreeEntry optimizationEntry = NavigationTree.AddEntry("Optimizations", new OptimizationView(), viewboxEntry);
            NavigationTreeEntries.Add("Optimizations", optimizationEntry);

            INavigationTreeEntry languageEntry = NavigationTree.AddEntry("Languages", null, viewboxEntry);
            NavigationTreeEntries.Add("Languages", languageEntry);

            INavigationTreeEntry parametersEntry = NavigationTree.AddEntry("Parameters", new Parameter_View(), viewboxEntry);
            NavigationTreeEntries.Add("Parameters", parametersEntry);

            INavigationTreeEntry visibilityEntry = NavigationTree.AddEntry("Users", new Users_View(), viewboxEntry);
            NavigationTreeEntries.Add("Users",visibilityEntry);

            INavigationTreeEntry rightsEntry = NavigationTree.AddEntry("Roles", new RoleEditor(), viewboxEntry);
            NavigationTreeEntries.Add("Roles", rightsEntry);

        }

        public INavigationTree NavigationTree { get; private set; }

        public Dictionary<string, INavigationTreeEntry> NavigationTreeEntries { get; set; }

        public object this[string NavTreeItem] { set { NavigationTreeEntries[NavTreeItem].Content.DataContext = value; } }

    }
}
