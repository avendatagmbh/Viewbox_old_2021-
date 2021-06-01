using System;
using System.Collections.Generic;
namespace EasyDocExtraction
{
    public interface IEasyMetadataExtractor
    {
        string GetDatabaseInfo();
        IEnumerable<string> GetMetadataBodies();
        IEnumerable<string> GetMetadataHeaders();
    }
}
