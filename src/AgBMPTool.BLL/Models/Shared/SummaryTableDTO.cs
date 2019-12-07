using AgBMPTool.BLL.Models.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AgBMPTool.BLL.Models.Shared
{
    public class SummaryTableDTO
    {
        public List<GridColumnsDTO> SummaryTableColumns { get; set; }

        public DataTable SummaryTableData { get; set; }
    }
}
