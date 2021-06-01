using System;
using System.Collections.Generic;
namespace DbComparisonV2.Business
{
    public interface IComparisonResult
    {
        int[] ColumnCount { get; set; }
        SortedDictionary<string, List<ComparisonResult.ColumnInfoMismatch>> ColumnMismatchSingle { get; set; }
        string[] DbName { get; set; }
        int[] EmptyTablesCount { get; set; }
        List<ComparisonResult.TableInfoMismatch>[] EmptyTablesInOne { get; set; }
        string Hostname1 { get; set; }
        string Hostname2 { get; set; }
        List<ComparisonResult.TableInfoMismatch>[] MissingTables { get; set; }
        SortedDictionary<string, bool> MissingTablesDict { get; set; }
        SortedDictionary<string, List<ComparisonResult.ColumnInfo>> MissingTableToColumns { get; set; }
        int[] TableCount { get; set; }
        ComparisonResult.TableInfoCollection[] Tables { get; set; }
        ComparisonResult.DatabasesCommonInfo Common { get; }
    }
}
