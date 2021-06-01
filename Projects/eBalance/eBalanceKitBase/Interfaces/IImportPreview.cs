// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-05-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System.Text;
using Utils;

namespace eBalanceKitBase.Interfaces
{
    public interface IImportPreview
    {
        ObservableCollectionAsync<string> CsvFiles { get; }
        System.Data.DataTable PreviewData { get; }

        char TextDelimiter { get; set; }
        char Separator { get; set; }
        Encoding Encoding { get; set; }

        void Update(char separator, char textDelimiter, Encoding encoding);
        void PreviewFile(string fileName);
        bool Next();
        bool Previous();

        bool IsNextAllowed { get; }
        bool IsPreviousAllowed { get; }
    }
}
