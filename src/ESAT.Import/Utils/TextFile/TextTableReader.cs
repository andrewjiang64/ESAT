using System.IO;
using System.Linq;

namespace ESAT.Import.Utils.TextFile
{
    public class TextTableReader : TextReader
    {
        public string[][] GetTokens(string textFile, string seperator)
        {
            string[] lines = GetLines(textFile);

            string[][] output = new string[lines.Length][];

            for (int i = 0; i < lines.Length; i++)
            {
                output[i] = lines[i].Split(seperator);
            }

            return output;
        }

        public ListTable CsvToTable(string csvFile)
        {
            string[][] tokens = new TextTableReader().GetTokens(csvFile, ",");

            ListTable outTable = new ListTable(Path.GetFileName(csvFile), tokens[0].ToList());

            for (int i = 1; i < tokens.Length; i++)
            {
                outTable.AddRecord(tokens[i].ToList<object>());
            }

            return outTable;
        }
    }
}
