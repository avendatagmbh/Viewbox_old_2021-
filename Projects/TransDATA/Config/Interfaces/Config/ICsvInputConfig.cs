// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-21
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace Config.Interfaces.Config
{
    public interface ICsvInputConfig : IConfig
    {
        string FieldSeperator { get; set; }
        string LineEndSeperator { get; set; }
        string FileEncoding { get; set; }
        string Folder { get; set; }
        string FolderLog { get; set; }
        string OptionallyEnclosedBy { get; set; }
        int IgnoreLines { get; set; }
        bool HeadlineNoHeader { get; set; }
        bool HeadlineInFirstLine { get; set; }
        bool HeadlineInEachFileFirstLine { get; set; }
        bool TableCanHaveMultipleParts { get; set; }
        bool ImportSubDirectories { get; set; }
        bool IsSapCsv { get; set; }
        bool IsBaanCsv { get; set; }
        int BaanCompanyIdLength { get; set; }
        string BaanCompanyIdField { get; set; }

        string GetFieldSeperator();
        string GetLineEndSeperator();

        IEnumerable<string> GetCsvFilesInFolder();
    }
}