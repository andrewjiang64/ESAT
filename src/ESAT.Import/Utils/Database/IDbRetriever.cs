using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.Utils
{
    interface IDbRetriever
    {
        bool ContainsTable(string schemaName, string tableName);

        bool ContainsColumn(string schemaName, string tableName, string columnName);

        List<string> GetTableNames();

        List<string> GetColumnNames(string schemaName, string tableName);

        string GetColumnName(string schemaName, string tableName, int columnId);

        Dictionary<string, string> GetColumnNameTypes(string schemaName, string tableName);

        string GetColumnType(string schemaName, string tableName, int columnId);

        string GetColumnType(string schemaName, string tableName, string columnName);
    }
}
