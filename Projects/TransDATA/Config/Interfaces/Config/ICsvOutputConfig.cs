// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-01-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Config.Interfaces.Config {
    public interface ICsvOutputConfig : IConfig {
        bool CompressAfterExport { get; set; }
        bool DoAnalyzation { get; set; }
        bool DoFileSplit { get; set; }
        string FieldSeperator { get; set; }
        string LineEndSeperator { get; set; }
        string FileEncoding { get; set; }
        int FileSplitSize { get; set; }
        string Folder { get; set; }
    }
}