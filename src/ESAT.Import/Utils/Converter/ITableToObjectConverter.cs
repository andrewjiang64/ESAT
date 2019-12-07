using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.Utils
{
    interface ITableToObjectConverter
    {
        List<object> TableToObjects(ListTable table);

        List<object> RecordToObjects(Dictionary<string, object> record);
    }
}
