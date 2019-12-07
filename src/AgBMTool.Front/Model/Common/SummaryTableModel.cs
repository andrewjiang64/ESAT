using AgBMPTool.BLL.Models.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AgBMTool.Front.Model.Common
{
    public class SummaryTableModel
    {
        public List<GridColumnsDTO> SummaryTableColumns { get; set; }

        public DataTable SummaryTableData { get; set; }
    }
}
