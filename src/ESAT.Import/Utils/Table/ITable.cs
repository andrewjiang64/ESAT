using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.Utils
{
    interface ITable
    {
        int AddRecord(Dictionary<string, object> record);
        
        Dictionary<string, object> GetRecord(int recordNum);

        bool DeleteRecord(int recordNum);

        object GetValue(int recordNum, string headerName);

        bool SetValue(int recordNum, string headerName, object value);

        bool ValidateHeaders(List<string> headers);

        bool ValidateHeaderCount(int headerCnt);

        bool ValidateHeader(string header);

        bool ValidateRecordNum(int recordNum);

        string ValidateColumnCount();

        int Size { get; }

        List<string> Headers { get; }

        string Name { get; set; }
    }
}
