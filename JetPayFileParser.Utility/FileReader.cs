using System.Collections.Generic;
using System.IO;

namespace JetPayFileParser.Utility
{
    public class FileReader
    {
        public static List<string> GetFileRecords(string filePath)
        {
            List<string> lstFileContents = new List<string>();

            using (StreamReader objStreamReader = new StreamReader(filePath, System.Text.Encoding.Default))
            {
                string fileRecord = objStreamReader.ReadLine();

                while ((objStreamReader.Peek() != -1 || fileRecord != null))
                {
                    if (fileRecord.Length > 2 && fileRecord != null)
                    {
                        lstFileContents.Add(fileRecord);
                    }
                    fileRecord = objStreamReader.ReadLine();

                }
            }
            return lstFileContents;
        }
    }
}
