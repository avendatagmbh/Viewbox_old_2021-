using System;
namespace DBComparisonBusiness.Business
{
    public interface IViewComparisonResult
    {
        string DBHostName { get; set; }
        string DBName { get; set; }
        ComparisonResult.TableInfoCollection DBTableInfos { get; set; }
        System.Collections.Generic.Dictionary<System.Collections.Generic.List<string>, System.Collections.Generic.List<string>> GetMissingColumns(ViewScriptInfo script);
        System.Collections.Generic.List<string> GetMissingTables(ViewScriptInfo script);
        System.Collections.Generic.List<ViewScriptInfo> ViewScriptInfos { get; set; }
    }
}
