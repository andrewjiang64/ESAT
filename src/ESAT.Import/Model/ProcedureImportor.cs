using AgBMTool.DBL.Interface;
using ESAT.Import.Utils;
using ESAT.Import.Utils.TextFile;
using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.Model
{
    public class ProcedureImportor
    {
        public void ImportProcedureFromTextFiles(List<string> textFiles)
        {
            // Create EsatDbWriter
            var edw = new EsatDbWriter();

            // Import from text file
            foreach (string file in textFiles)
            {
                edw.ExecuteNonQuery(TextReader.GetText(file));
            }
        }
    }
}
