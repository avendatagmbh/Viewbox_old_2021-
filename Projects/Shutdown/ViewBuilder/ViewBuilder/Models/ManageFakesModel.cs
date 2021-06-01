using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using SystemDb;
using Utils;
using ViewBuilder.Windows;
using ViewBuilderBusiness.Structures.Fakes;

namespace ViewBuilder.Models {
    public class ManageFakesModel {
        #region Constructor
        public ManageFakesModel(SystemDb.SystemDb viewboxDb, string dbName) {
            _viewboxDb = viewboxDb;
            _dbName = dbName;
            FakeTables = new ObservableCollectionAsync<FakeObject>();
            foreach(var fakeTable in viewboxDb.GetFakeTables(dbName))
                FakeTables.Add(new FakeTable(fakeTable){State = State.Remained});
        }
        #endregion Constructor

        #region Properties
        private SystemDb.SystemDb _viewboxDb;
        private string _dbName;
        public ObservableCollectionAsync<FakeObject> FakeTables { get; private set; }
        #endregion Properties

        #region Methods
        public void Finish(DlgManageFakes parent) {
            bool somethingChanged = false;
            List<string> allFakeTables = _viewboxDb.GetFakeTables(_dbName);
            List<FakeTable> deletedTables = (from fakeTable in allFakeTables
                                            where !FakeTables.Any((t) => t.Name.ToLower() == fakeTable.ToLower())
                                            select new FakeTable(fakeTable) {State = State.Deleted}).ToList();

            StringBuilder message = new StringBuilder();
            message.Append("Folgende Basistabellen werden hinzugefügt: " + Environment.NewLine);
            foreach (var table in FakeTables.Where(t => t.State == State.Added && !string.IsNullOrEmpty(t.Name.Trim()))) {
                message.Append(table.Name).Append(Environment.NewLine);
                somethingChanged = true;
            }
            message.Append(Environment.NewLine);
            message.Append("Folgende Basistabellen werden gelöscht: " + Environment.NewLine);
            foreach (var table in deletedTables) {
                message.Append(table.Name).Append(Environment.NewLine);
                somethingChanged = true;
            }

            if(somethingChanged && MessageBox.Show(parent, message.ToString(), "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                foreach (var table in FakeTables.Where(t => t.State == State.Added && !string.IsNullOrEmpty(t.Name.Trim())))
                    _viewboxDb.AddFakeTable(_dbName, table.Name);
                foreach (var table in deletedTables)
                    _viewboxDb.DeleteFakeTable(_dbName, table.Name);
            }
        }
        #endregion Methods
    }
}
