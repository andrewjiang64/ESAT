using System.IO;
using System.Text;

namespace ESAT.Import.Utils.TextFile
{
    public class TextReader
    {
        public static string[] GetLines(string textFile)
        {
            if (File.Exists(textFile))
            {
                // Read a text file line by line.
                return File.ReadAllLines(textFile);
            }

            return new string[] { };
        }

        public static string GetText(string textFile)
        {
            if (File.Exists(textFile))
            {
                // Read a text file line by line.
                return File.ReadAllText(textFile);
            }

            return "";
        }

    }
}
