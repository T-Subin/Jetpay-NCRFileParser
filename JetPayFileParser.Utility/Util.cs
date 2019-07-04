using System;
using System.Data;
using System.IO;
using Ionic.Zip;
using System.Data.OleDb;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace JetPayFileParser.Utility
{
    public class Util
    {
        static string tempDirectory = AppDomain.CurrentDomain.BaseDirectory + "Temp";
        const string Key = "@$!MplEp@s5w0Rd";

        public static string DecryptFile(string file)
        {
            // If Directory does not exist , then create.
            if ((!Directory.Exists(tempDirectory)))
            {
                Directory.CreateDirectory(tempDirectory);
            }
            Stream stream = null;
            ZipFile zip = null;
            string fileName = "";
            try
            {
                // Extract the file in temp Directory.
                stream = new FileStream(file, FileMode.Open);
                zip = ZipFile.Read(stream);
                fileName = zip[0].FileName;
                zip[0].ExtractWithPassword(tempDirectory, Key);
            }
            finally
            {
                if (zip != null)
                    zip.Dispose();

                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return Path.Combine(tempDirectory, fileName);
        }

        public static string UnZipFile(string file)
        {
            // If Directory does not exist , then create.
            if ((!Directory.Exists(tempDirectory)))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            // Extract the file in temp Directory.
            Stream stream = new FileStream(file, FileMode.Open);
            ZipFile zip = ZipFile.Read(stream);
            string fileName = zip[0].FileName;
            zip[0].Extract(tempDirectory);

            zip.Dispose();
            stream.Close();
            stream.Dispose();

            return Path.Combine(tempDirectory, fileName);
        }


        /// <summary>
        /// Check existence of log directory and log file. If does not exist, then create.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="filePath"></param>
        public static void CheckExistenceOfLogDirectory(string directoryPath, string filePath)
        {
            if ((!Directory.Exists(directoryPath)))
            {

                Directory.CreateDirectory(directoryPath);
            }


            if ((!File.Exists(filePath)))
            {


                FileStream objFileStream = null;
                try
                {

                    objFileStream = File.Create(filePath);

                    objFileStream.Close();


                    objFileStream.Dispose();
                }
                finally
                {

                    if (((objFileStream != null)))
                    {

                        objFileStream.Close();

                        objFileStream.Dispose();
                    }

                }
            }
        }

        public static void ZipEncryptFile(StreamWriter objTextWriter, string filePath, string archivePath)
        {
            var encryptedFile = filePath + ".ENZ";
            ZipFile zip = new ZipFile();
            zip.Encryption = EncryptionAlgorithm.WinZipAes256;
            zip.Password = Key;
            zip.AddFile(filePath, "");

            // Save the encrypted file
            zip.Save(encryptedFile);

            // Delete original file
            File.Delete(filePath);

            // Archive encrypted file to a new location
            var archiveFilePath = Path.Combine(archivePath + ".ENZ");
            File.Move(encryptedFile, archiveFilePath);
            objTextWriter.WriteLine("File Archived - Path: {0} at {1}", archiveFilePath, DateTime.Now.ToString());
        }

        public static DataTable ParseCSV(string filename, string path, string tableName, char delimiter, string MoveFilePathWithName)
        {

            FileStream fs1 = new FileStream(MoveFilePathWithName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamReader sReader = new StreamReader(fs1);

            FileStream MainFS = new FileStream(path + "\\temp_" + filename, FileMode.Append, FileAccess.Write);
            StreamWriter Mainsr = new StreamWriter(MainFS);
            int loopcount = 0;
            while (sReader.Peek() != -1)
            {
                loopcount++;
                string strVal = sReader.ReadLine().Trim();
                if (strVal.Length > 125)//Removing first row
                {
                    if (loopcount > 2)
                    {
                        string result = Regex.Replace(strVal, @",(?=[^""]*""(?:[^""]*""[^""]*"")*[^""]*$)", String.Empty);
                        result = result.Replace("\"", "");
                        result = result.Replace(",", ",'");
                        Mainsr.Write(result);
                    }
                    else
                        Mainsr.Write(strVal);

                    Mainsr.WriteLine();
                }
            }

            sReader.Close();
            fs1.Close();
            Mainsr.Close();
            MainFS.Close();

            File.Delete(path + "\\" + filename);
            File.Move(path + "\\temp_" + filename, path + "\\" + filename);
            
            
            
            //filename = filename.Replace("-", "_");
            DataTable dTable = new DataTable(tableName);
            string sConn = "Provider=Microsoft.ACE.OLEDB.12.0;OLE DB Services=-4;Data Source=" + path + ";Extended Properties=\"text;\"";
            DataSet dsMain = new DataSet();
            OleDbConnection cn = new System.Data.OleDb.OleDbConnection(sConn);
            string TableColName = ConfigurationManager.AppSettings["SelColName"];
            try
            {
                string str = "";
                str = "select " + TableColName + " from " + filename;
                OleDbDataReader dtrd;
                OleDbCommand cmd = new OleDbCommand(str, cn);
                cn.Open();
                OleDbDataAdapter ADcmd = new System.Data.OleDb.OleDbDataAdapter(str, cn);
                ADcmd.Fill(dTable);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Artefacts.Common.Logger.ArtefactsLogger logger = Artefacts.Common.Logger.ArtefactsLogger.getLogger();
                logger.logError(ex);
                logger.CloseLogger();
                throw;
            }
            return dTable;
        }
        /// <summary>
        /// Validata data from the file before the bulk insert.
        /// </summary>
        /// <param name="dtData"></param>
        /// <param name="dtSchema"></param>
        /// <returns></returns>
        public static List<string> ValidateData(DataTable dtData, DataTable dtSchema)
        {
            string result = string.Empty;
            List<string> lstError = new List<string>();       
            try
            {                
                int columnIndex = 0;
                foreach (DataRow schema in dtSchema.Rows)
                {
                    string columnSize = Convert.ToString(schema[10]);

                    string dataColumnName = dtData.Columns[columnIndex].ColumnName;
                    if (dtData.Columns[columnIndex].DataType.Name == "String")
                    {
                        if (!string.IsNullOrEmpty(columnSize) && int.Parse(columnSize) > 0)
                        {
                            var dtRow = dtData.Select("len(" + dataColumnName + ") > " + columnSize);
                            if (dtRow != null && dtRow.Length > 0)
                            {
                                result += "/*** Data Size issue " + dataColumnName + " : "
                                    + Convert.ToString(dtRow[0][dataColumnName]);
                                lstError.Add(result);
                            }
                        }
                    }
                    columnIndex++;
                }
            }
            catch (Exception ex)
            {

            }
            return lstError;
        }

        //public static List<string> ValidateDatatype(string )
        //public static DataTable ParseCSV(string path, string tableName, char delimiter)
        //{
        //    DataTable dTable = new DataTable(tableName);

        //    try
        //    {
        //        string full = Path.GetFullPath(path);

        //        StreamReader sr = new StreamReader(full);
        //        string csvRow = sr.ReadLine();
        //        var rows = csvRow.Split(delimiter);

        //        foreach (string column in rows)
        //        {
        //            // First row of the CSV is used as column header
        //            dTable.Columns.Add(column.Trim().ToUpper());
        //        }

        //        csvRow = sr.ReadLine();
        //        // While row is not empty, add data to data table
        //        while (csvRow != null)
        //        {
        //            rows = csvRow.Split(delimiter);
        //            dTable.Rows.Add(rows);
        //            csvRow = sr.ReadLine();
        //        }

        //        sr.Close();
        //        sr.Dispose();

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        Artefacts.Common.Logger.ArtefactsLogger logger = Artefacts.Common.Logger.ArtefactsLogger.getLogger();
        //        logger.logError(ex);
        //        logger.CloseLogger();
        //        throw;
        //    }

        //    return dTable;
        //}

        public static string ReadFileData(String filePath)
        {
            string filecontent = string.Empty;
            try
            {
                TextReader reader = new StreamReader(filePath);
                filecontent = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Artefacts.Common.Logger.ArtefactsLogger logger = Artefacts.Common.Logger.ArtefactsLogger.getLogger();
                logger.logError(ex);
                logger.CloseLogger();
                filecontent = string.Empty;
            }
            return filecontent;
        }

        /// <summary>
        /// Logs exception and messages
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void LogException(string message = "", Exception ex = null)
        {
            Artefacts.Common.Logger.ArtefactsLogger logger = Artefacts.Common.Logger.ArtefactsLogger.getLogger();

            if (!string.IsNullOrWhiteSpace(message))
                logger.logError(message);
            if (ex != null)
                logger.logError(ex);

            logger.CloseLogger();
        }

        /// <summary>
        /// Validating the row
        /// </summary>
        /// <param name="dtSchema">contains the schema for Main table</param>
        /// <param name="columnMappings">mappings between CSV column names, Raw table column Names and Main table column names</param>
        /// <param name="datarow">current row which is pointing in file</param>
        /// <param name="columnName">contains all the headers present in file</param>
        /// <param name="rowCount">INT : row number which points to</param>
        /// <param name="objTextWriter">StreamWriter : to log the error</param>
        /// <returns></returns>
        public static List<string> ValidateCSVData(DataTable dtSchema, DataTable columnMappings, string[] datarow, string[] columnName, long rowCount, StreamWriter objTextWriter)
        {
            List<string> lstErrors = new List<string>();
            int length = columnName.Length; 
            try
            {

                //contains the matched rows between CSVColumnName and CSV file headers
                var dt = columnMappings.AsEnumerable()
                     .Select(x => new {
                         RowNumber = Array.IndexOf(columnName, x.Field<string>("CSVColumnName")),
                         MainTableColumnName = x.Field<string>("MainTableColumnName"),
                         IsInColumnMappings = Array.IndexOf(columnName, x.Field<string>("CSVColumnName")) >= 0
                     });

                DataTable result = new DataTable();
                DataRow tableRow = null;

                bool IsColumnNameExists = dtSchema.Columns.Contains("COLUMN_NAME");
                bool IsCharacterMaxLengthExists = dtSchema.Columns.Contains("CHARACTER_MAXIMUM_LENGTH");
                bool IsDataTypeExists = dtSchema.Columns.Contains("DATA_TYPE");

                foreach (var row in dt)
                {
                    result = null; tableRow = null;

                    var dtResult = (from schema in dtSchema.AsEnumerable().AsParallel()
                                    where IsColumnNameExists
                                    && schema["COLUMN_NAME"].ToString() == row.MainTableColumnName
                                    && IsCharacterMaxLengthExists
                                    && IsDataTypeExists
                                    select schema);

                    result = dtResult.Any() ? dtResult.CopyToDataTable<DataRow>() : null;
                    
                    if (result != null && result.Rows.Count > 0)
                        tableRow = result.AsEnumerable().First();
                    
                    if (tableRow != null && Convert.ToInt32(row.RowNumber) > 0)
                    {
                        string dataType = ""; int maxLength = 0; int dataLength = 0;
                        dataType = tableRow.GetText("DATA_TYPE");

                        maxLength = tableRow.GetValue<int>("CHARACTER_MAXIMUM_LENGTH").GetValueOrDefault();

                        string data = (!string.IsNullOrEmpty(datarow[row.RowNumber])) ? datarow[row.RowNumber] : "";

                        data = data.Trim();
                        
                        if (!string.IsNullOrEmpty(data))
                        {
                            data = data.Replace("'", "").Replace("\"", ""); dataLength = data.Length;

                            //for checking string type
                            if ((dataType == "varchar" || dataType == "char") && maxLength > 0 &&
                                    dataLength > 0 && dataLength > Convert.ToInt32(maxLength))
                            {
                                lstErrors.Add(LogErrorMessage(datarow, objTextWriter, rowCount, columnName[row.RowNumber]));
                            }

                            //for checking datetime/date type
                            else if (dataType == "datetime" || dataType == "date")
                            {
                                //if (data.All(char.IsDigit))
                                {
                                    //for validating '01-Jan-18' dates
                                    string hyphenSeperatedDate = "";
                                    bool dateTime = false;
                                    string unslashedValue = "";

                                    if (data.Contains('-'))
                                        hyphenSeperatedDate = data;
                                    else
                                        unslashedValue = data;

                                    DateTime date;
                                    
                                    if (dataType == "datetime")
                                    {
                                        if (data.Contains('-'))
                                            dateTime = DateTime.TryParseExact(hyphenSeperatedDate, "dd-MMM-yy HH:mm:ss",
                                                           System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date);

                                        else if (unslashedValue.Length > 8 && unslashedValue.Length > 12)
                                        {
                                            dateTime = DateTime.TryParseExact(unslashedValue, "yyyyMMddHHmmss",
                                                           System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date);
                                        }
                                        else if (unslashedValue.Length > 8)
                                        {
                                            dateTime = DateTime.TryParseExact(unslashedValue, "yyyyMMddHHmm",
                                                           System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date);
                                        }
                                    }

                                    else if (dataType == "date")
                                    {
                                        if (data.Contains('-'))
                                            dateTime = DateTime.TryParseExact(hyphenSeperatedDate, "dd-MMM-yy",
                                                           System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date);

                                        else if (unslashedValue.Length > 8 && unslashedValue.Length < 15)
                                        {
                                            if (unslashedValue.Length == 14)
                                                unslashedValue = unslashedValue.Remove(8, 6);
                                            else if (unslashedValue.Length == 12)
                                                unslashedValue = unslashedValue.Remove(8, 4);
                                            else if (unslashedValue.Length == 10)
                                                unslashedValue = unslashedValue.Remove(8, 2);

                                            dateTime = DateTime.TryParseExact(unslashedValue, "yyyyMMdd",
                                                           System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date);
                                        }
                                        else if (unslashedValue.Length == 8)
                                        {
                                            dateTime = DateTime.TryParseExact(unslashedValue, "yyyyMMdd",
                                                          System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date);
                                        }
                                    }

                                    if (string.IsNullOrEmpty(data))
                                    {
                                    }
                                    else if (!dateTime)
                                    {
                                        lstErrors.Add(LogErrorMessage(datarow, objTextWriter, rowCount, columnName[row.RowNumber]));
                                    }
                                }
                            }

                            //for checking numeric type
                            else if (dataType == "int" || dataType == "bigint" || dataType == "float" ||
                                dataType == "decimal")
                            {
                                if (!Util.IsValid(data, dataType))
                                {
                                    lstErrors.Add(LogErrorMessage(datarow, objTextWriter, rowCount, columnName[row.RowNumber]));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                objTextWriter.WriteLine(ex.Message);
                Util.LogException(string.Empty, ex);
                throw;
            }
            return lstErrors;
        }
        
        private static string LogErrorMessage(string[] errorRow, StreamWriter objTextWriter, long lineNumber, string columnName)
        {
            string errorMessage = "";
            errorMessage = "Error at row : " + lineNumber + " and at column " + (columnName) + ". <br> Row is=" + string.Join(",", errorRow);
            objTextWriter.WriteLine(errorMessage);
            return errorMessage;
        }

        public static bool IsValid(string input, string type)
        {
            if (String.IsNullOrEmpty(input))
                return true;
            else
            {
                switch (type.ToLower())
                {
                    case "int":
                        int i;
                        if (Int32.TryParse(input, out i))
                            return true;
                        else
                            return false;
                    case "float":
                        float f;
                        if (float.TryParse(input, out f))
                            return true;
                        else
                            return false;
                    case "decimal":
                        decimal d;
                        if (decimal.TryParse(input, out d))
                            return true;
                        else
                            return false;
                    case "double":
                        double doub;
                        if (double.TryParse(input, out doub))
                            return true;
                        else
                            return false;
                    case "bigint":
                        long bInt;
                        if (long.TryParse(input, out bInt))
                            return true;
                        else
                            return false;
                    default:
                        return false;
                }
            }
        }
    }
}
