// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-05
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using Utils.Commands;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Import;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Model {
    public class AskingBalanceListTestlistModel : AskingModelBase{
        public AskingBalanceListTestlistModel(Window owner) : base(owner) {

            CmdYes = new DelegateCommand(o => true, o => GenerateTestBalanceList(owner));
            //CmdNo = new DelegateCommand(o => true, o => /*GoForward()*/);
        }

        private void GenerateTestBalanceList(Window window) {
            //Result = new eBalanceKitBusiness.Import.BalanceListImporter(null, window)
            //BalanceListImportConfig importConfig = new BalanceListImportConfig();
            //importConfig.ImportType = BalanceListImportType.SignedBalance;
            //var i = new BalanceListImporter(null, window);
            //i.Import()
            var ctl = new eBalanceKitBusiness.Structures.ExampleCSV();

            var suSaAssistant = new BalanceList.BalListImportAssistant(null, window);

            var balListImportAssistantModel = suSaAssistant.DataContext as eBalanceKit.Models.Assistants.BalListImportAssistantModel;
            if (balListImportAssistantModel != null) {
                balListImportAssistantModel.Importer.Config.BalanceListName = ctl.ImportConfig.BalanceListName;
                balListImportAssistantModel.Importer.Config.CsvFileName = ctl.CsvFilePath;

                balListImportAssistantModel.Importer.Config.Comment= ctl.ImportConfig.Comment;
                balListImportAssistantModel.Importer.Config.Encoding = ctl.ImportConfig.Encoding;
                balListImportAssistantModel.Importer.Config.FirstLineIsHeadline = ctl.ImportConfig.FirstLineIsHeadline;
                balListImportAssistantModel.Importer.Config.ImportType = ctl.ImportConfig.ImportType;
                balListImportAssistantModel.Importer.Config.Index = ctl.ImportConfig.Index;
                balListImportAssistantModel.Importer.Config.Seperator = ctl.ImportConfig.Seperator;
                balListImportAssistantModel.Importer.Config.TaxonomyColumnExists = ctl.ImportConfig.TaxonomyColumnExists;
                balListImportAssistantModel.Importer.Config.TextDelimiter = ctl.ImportConfig.TextDelimiter;
                
                //balListImportAssistantModel.Importer.Config.ColumnDict = ctl.ImportConfig.ColumnDict;

                suSaAssistant.NavigateNext();
            }

            ShowWindow(suSaAssistant);

            //ctl.ImportConfig.CsvFileName = file;

            //var importer = new BalanceListImporter(null, window) {Config = ctl.ImportConfig};

            //Result = importer;
            //OnPropertyChanged("Result");
        }

        public override ObjectTypes ObjectType { get { return ObjectTypes.Other; } set { } }

        public DelegateCommand CmdYes { get; set; }

        public DelegateCommand CmdNo { get; set; }

        public string Question { get { return ResourcesManamgement.QuestionBalanceListTestlist; } }
    }
}