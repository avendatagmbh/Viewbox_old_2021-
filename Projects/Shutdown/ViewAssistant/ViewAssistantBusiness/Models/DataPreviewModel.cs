using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using Utils;
using System.Collections.Generic;

namespace ViewAssistantBusiness.Models
{
    public class DataPreviewModel : NotifyPropertyChangedBase
    {
        public List<String> Columns { get; private set; }
        public List<List<String>> Rows { get; private set; }

        public DataTable View
        {
            get
            {
                var table = new DataTable();
                var typeString = typeof (string);
                foreach(var column in Columns)
                {
                    table.Columns.Add(column, typeString);
                }
                foreach(var row in Rows)
                {
                    table.Rows.Add(row.Select(x => (object) x).ToArray());
                }
                
                return table;
            }
        }

        public DataPreviewModel()
        {
            Columns = new List<string>();
            Rows = new List<List<string>>();
        } 
    }
}
