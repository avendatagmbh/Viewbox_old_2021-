// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-05-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Utils;

namespace eBalanceKitBase.Interfaces {
    public interface IAssistedImport {
        char TextDelimiter { get; set; }
        char Separator { get; set; }
        Encoding Encoding { get; set; }

        ICommand AddFileCommand { get; }
        ICommand DeleteFileCommand { get; }
        ObservableCollectionAsync<string> CsvFiles { get; }
        string SelectedFile { get; set; }

        bool HasSelectedFile { get; }
        bool HasFiles { get; }
        bool IsStepPreviewEnabled { get; }
        bool IsStepErrorsEnabled { get; }

        bool IgnoreErrors { get; set; }
        bool CanImport { get; }
        DataTable PreviewData { get; }

        bool ValidateAndSetDialogPage(int currentPage);
        int CurrentPage { get; }

        IImportPreview Preview { get; }
        void Import(Window owner);
        void PreviewImport(Window owner);


    }
}
