using System;
using System.Collections.Generic;
using System.Linq;
using DbAnalyser.Models;

namespace DbAnalyser.Helpers
{
    class ListFiltering
    {
        public static List<ResultBoxView> FilterByWord(List<ResultBoxView> tableList, string filterWord = "")
        {
            if (filterWord.Length > 0)
            {
                if (filterWord[0] == '>')
                {
                    filterWord = filterWord.Remove(0, 1);
                    int cnt;
                    if (Int32.TryParse(filterWord, out cnt))
                    {
                        return tableList.Where(table => table.length > cnt).ToList();
                    }
                }
                else if (filterWord[0] == '<')
                {
                    filterWord = filterWord.Remove(0, 1);
                    int cnt;
                    if (Int32.TryParse(filterWord, out cnt))
                    {
                        return tableList.Where(table => table.length < cnt).ToList();
                    }
                }
                else if (filterWord.ToUpper().Contains("BETWEEN"))
                {
                    string[] limits = filterWord.Split(' ');
                    if (limits.Count() >= 3)
                    {
                        int from;
                        int to;
                        if (Int32.TryParse(limits[1], out from) && Int32.TryParse(limits[2], out to))
                        {
                            return tableList.Where(table => table.length >= from && table.length <= to).ToList();
                        }
                    }
                }
                else
                {
                    return tableList.Where(table => table.colName.Contains(filterWord)).ToList();
                }
            }
            return null;
        }
    }
}
