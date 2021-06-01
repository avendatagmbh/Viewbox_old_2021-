using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ViewValidator.Controls.Datagrid {
    public class UnifiedDataGrid : DataGrid{
        public UnifiedDataGrid() {
            CanUserAddRows = false;
            CanUserDeleteRows = false;
            CanUserReorderColumns = false;
            HeadersVisibility = DataGridHeadersVisibility.Column;
            IsReadOnly = true;
        }
    }
}
