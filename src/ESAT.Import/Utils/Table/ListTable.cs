using ESAT.Import.ESATException;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ESAT.Import.Utils
{
    public class ListTable : ListColumn, ITable
    {
        public ListTable(string name, List<string> headers)
        {
            this.Name = name;
            this.Data = new Dictionary<string, List<object>>();
            foreach (var header in headers)
            {
                this.Data[header] = new List<object>();
            }
        }

        private int size = 0;
        public int Size { get => size; }

        public List<string> Headers { get => this.Data.Keys.ToList(); }

        public HashSet<string> HeaderSet { get => this.Data.Keys.ToHashSet(); }

        public string Name { get; set; } = "NewListTable";

        public int AddRecord(Dictionary<string, object> record)
        {
            // Check if record is valid
            if (!this.ValidateHeaders(record.Keys.ToList<string>()))
            {
                throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"Failed to add record:\n{JsonConvert.SerializeObject(record)}");
            }

            // Add record to table
            foreach (var header in this.Headers)
            {
                Data[header].Add(record.ContainsKey(header) ? record[header] : ""); // if header not contained in record, give an empty string
            }

            // Update table size
            UpdateTableSize();

            // Update table size and return
            return Size;
        }

        public int AddRecord(List<object> record)
        {
            // Check if record is valid
            if (!this.ValidateHeaderCount(record.Count))
            {
                throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"Failed to add record:\n{JsonConvert.SerializeObject(record)}");
            }

            // Add record to table
            for (int i = 0; i < Data.Count; i++)
            {
                Data[this.Headers[i]].Add(record[i]);
            }

            // Update table size
            UpdateTableSize();

            // Update table size and return
            return Size;
        }

        public bool DeleteRecord(int recordNum)
        {
            // Check if recordNum is valid
            if (!this.ValidateRecordNum(recordNum))
            {
                throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"Record number {recordNum} is not valid!");
            }

            // Remove record
            foreach (var column in Data.Values)
            {
                column.RemoveAt(recordNum);
            }

            // Update table size
            UpdateTableSize();

            // Return true
            return true;
        }

        public Dictionary<string, object> GetRecord(int recordNum)
        {
            // Check if recordNum is valid
            if (!this.ValidateRecordNum(recordNum))
            {
                throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"Record number {recordNum} is not valid!");
            }

            // Create a record
            Dictionary<string, object> output = new Dictionary<string, object>();

            foreach (var header in this.Headers)
            {
                output[header] = this.Data[header][recordNum];
            }

            // Return
            return output;
        }

        public object GetValue(int recordNum, string headerName)
        {
            // Check if recordNum and headerName is valid
            if (!this.ValidateRecordNum(recordNum))
            {
                throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"Record number {recordNum} is not valid!");
            } else if (!this.ValidateHeader(headerName))
            {
                throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"Header name {headerName} is not valid!");
            }

            // Return value
            return this.Data[headerName][recordNum];            
        }

        public bool SetValue(int recordNum, string headerName, object value)
        {
            // Check if recordNum and headerName is valid
            if (!this.ValidateRecordNum(recordNum))
            {
                throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"Record number {recordNum} is not valid!");
            }
            else if (!this.ValidateHeader(headerName))
            {
                throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"Header name {headerName} is not valid!");
            }

            // Set value
            Data[headerName][recordNum] = value;

            return true;
        }

        public bool ValidateHeaders(List<string> headers)
        {
            HashSet<string> dataHeaders = this.HeaderSet;
            foreach (string header in headers)
            {
                if (!dataHeaders.Contains(header))
                {
                    return false;
                }
            }

            return true;
        }

        public bool ValidateHeaderCount(int headerCnt)
        {
            return this.Data.Count == headerCnt;
        }

        public bool ValidateHeader(string header)
        {
            return this.HeaderSet.Contains(header);
        }

        public bool ValidateRecordNum(int recordNum)
        {
            return recordNum >= 0 && recordNum < this.Size ;
        }

        public string ValidateColumnCount()
        {
            foreach (var header in Data.Keys)
            {
                if (Data[header].Count != this.Size) return header;
            }
            return null;
        }

        private void UpdateTableSize()
        {
            this.size = Data.FirstOrDefault().Value.Count;

            // Validate column size after adding
            string invalidHeader = this.ValidateColumnCount();
            if (invalidHeader != null)
            {

                throw new MainException(new Exception(), MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name,
                        // following is the message send to console
                        $"Column \"{invalidHeader}\" size NOT validation!");
            }
        }
    }
}
