using JetPayFileParser.DataAccess;
using JetPayFileParser.Model;
using JetPayFileParser.Model.Attribute;
using JetPayFileParser.Model.Enum;
using JetPayFileParser.Utility;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JetPayFileParser.Business
{
    class BulkDataInsertManager
    {
        public static DataTable ConvertListToDataTable<T>(List<T> myDataObjectList, string tableName)
        {
            DataTable dt = new DataTable(tableName);
            // Create the datatable from the object.  This allows changes
            // to be made in the data object and flow to this controller.
            PropertyInfo[] props = typeof(T).GetProperties();
            int columnCount = props.Length;
            for (Int32 i = 0; i < columnCount; i++)
            {
                DataColumnAttribute col = GetColumnNameFromProperty<T>(props[i].Name);
                if (col != null)
                    dt.Columns.Add(col.ColumnName, col.ColumnType);
            }

            // Convert the list to a datatable.
            foreach (T rec in myDataObjectList)
            {
                DataRow row = dt.NewRow();
                for (int x = 0; x < columnCount; x++)
                {

                    if (typeof(T).GetProperty(props[x].Name).GetValue(rec, null) == null)
                        row[x] = DBNull.Value;
                    else
                        row[x] = typeof(T).GetProperty(props[x].Name).GetValue(rec, null);
                }
                dt.Rows.Add(row);
            }

            return dt;
        }

        /// <summary>
        /// Bulk Insert datatable into database table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dt"></param>
        public static void BulkInsert(string tableName, System.Data.DataTable dt, bool destinationTableHasIdentity = false)
        {

            // Bulk Insert the datatable to the database.
            using (SqlConnection connection = new SqlConnection(ConfigInfo.DB_CONNECTION_STRING))
            {
                // make sure to enable triggers
                // more on triggers in next post
                SqlBulkCopy bulkCopy = new SqlBulkCopy(connection);

                // Set the destination table name
                bulkCopy.DestinationTableName = tableName;
                connection.Open();

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (destinationTableHasIdentity == true)
                    {
                        // If the first column of the destination table is an identity column
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, i + 1));
                    }
                    else
                    {
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, i));
                    }
                }

                // write the data in the dataTable
                bulkCopy.BatchSize = ConfigInfo.BATCH_SIZE;
                try
                {
                    bulkCopy.WriteToServer(dt);
                }
                catch (Exception e)
                {
                    throw new Exception("Error in " + tableName + " and error is " + e.Message);
                }
                finally
                {
                    connection.Close();
                    bulkCopy.Close();
                }
            }
        }

        public static void BulkInsertWithColumnMapping(string tableName, System.Data.DataTable dt, ICollection<KeyValuePair<string, string>> columnMappings)
        {
            using (SqlConnection connection = new SqlConnection(ConfigInfo.DB_CONNECTION_STRING))
            {
                SqlBulkCopy bulkCopy = new SqlBulkCopy(connection);
                // Set the destination table name
                bulkCopy.DestinationTableName = tableName;
                connection.Open();

                // Map by column names
                //foreach (var columnMapping in columnMappings)
                //{
                //    SqlBulkCopyColumnMapping mapping = new SqlBulkCopyColumnMapping(columnMapping.Key, columnMapping.Value);
                //    bulkCopy.ColumnMappings.Add(mapping);
                //}

                // write the data in the dataTable

                if (!string.IsNullOrEmpty(ConfigInfo.BULK_COPY_TIMEOUT) && Convert.ToInt32(ConfigInfo.BULK_COPY_TIMEOUT) > 0)
                    bulkCopy.BulkCopyTimeout = Convert.ToInt32(ConfigInfo.BULK_COPY_TIMEOUT);

                bulkCopy.BatchSize = ConfigInfo.BATCH_SIZE;
                bulkCopy.BulkCopyTimeout = 600;
                try
                {
                    bulkCopy.WriteToServer(dt);
                }
                catch (Exception e)
                {
                    throw new Exception("Error in " + tableName + " and error is " + e.Message);
                }
                finally
                {
                    connection.Close();
                    bulkCopy.Close();
                }
            }
        }
        public static void BulkInsertWithColumnMapping(string tableName, System.Data.DataTable dt, ICollection<KeyValuePair<string, string>> columnMappings,bool IsColumnMapping)
        {
            using (SqlConnection connection = new SqlConnection(ConfigInfo.DB_CONNECTION_STRING))
            {
                SqlBulkCopy bulkCopy = new SqlBulkCopy(connection,SqlBulkCopyOptions.KeepIdentity,null);
                // Set the destination table name
                bulkCopy.DestinationTableName = tableName;
                connection.Open();
                if (IsColumnMapping)
                {
                    if (tableName == "DepositFee_JetPay")
                    {
                        bulkCopy = BulkCopyMappingForDepositFee(bulkCopy);
                    }
                    else
                    {
                        bulkCopy = BulkCopyMapping(bulkCopy);
                    }
                }
                // write the data in the dataTable
                if (!string.IsNullOrEmpty(ConfigInfo.BULK_COPY_TIMEOUT) && Convert.ToInt32(ConfigInfo.BULK_COPY_TIMEOUT) > 0)
                    bulkCopy.BulkCopyTimeout = Convert.ToInt32(ConfigInfo.BULK_COPY_TIMEOUT);

                bulkCopy.BatchSize = ConfigInfo.BATCH_SIZE;
                bulkCopy.BulkCopyTimeout = 600;
                try
                {
                    bulkCopy.WriteToServer(dt);
                }
                catch (Exception e)
                {
                    throw new Exception("Error in " + tableName + " and error is " + e.Message);
                }
                finally
                {
                    connection.Close();
                    bulkCopy.Close();
                }
            }
        }
        private static SqlBulkCopy BulkCopyMapping(SqlBulkCopy bulkCopy)
        {
            bulkCopy.ColumnMappings.Add(0, 1);
            bulkCopy.ColumnMappings.Add(1, 2);
            bulkCopy.ColumnMappings.Add(2, 3);
            bulkCopy.ColumnMappings.Add(3, 4);
            bulkCopy.ColumnMappings.Add(4, 5);
            bulkCopy.ColumnMappings.Add(5, 6);
            bulkCopy.ColumnMappings.Add(6, 7);
            bulkCopy.ColumnMappings.Add(7, 8);
            bulkCopy.ColumnMappings.Add(8, 9);
            bulkCopy.ColumnMappings.Add(9, 10);
            bulkCopy.ColumnMappings.Add(10, 11);
            bulkCopy.ColumnMappings.Add(11, 12);
            bulkCopy.ColumnMappings.Add(12, 13);
            bulkCopy.ColumnMappings.Add(13, 14);
            bulkCopy.ColumnMappings.Add(14, 15);
            bulkCopy.ColumnMappings.Add(15, 16);
            bulkCopy.ColumnMappings.Add(16, 17);
            bulkCopy.ColumnMappings.Add(17, 18);
            
            return bulkCopy;
        }
        private static SqlBulkCopy BulkCopyMappingForDepositFee(SqlBulkCopy bulkCopy)
        {
            bulkCopy.ColumnMappings.Add(0, 1);
            bulkCopy.ColumnMappings.Add(1, 2);
            bulkCopy.ColumnMappings.Add(2, 3);
            bulkCopy.ColumnMappings.Add(3, 4);
            bulkCopy.ColumnMappings.Add(4, 19);
            bulkCopy.ColumnMappings.Add(5, 5);
            bulkCopy.ColumnMappings.Add(6, 6);
            bulkCopy.ColumnMappings.Add(7, 7);
            bulkCopy.ColumnMappings.Add(8, 8);
            bulkCopy.ColumnMappings.Add(9, 9);
            bulkCopy.ColumnMappings.Add(10, 10);
            bulkCopy.ColumnMappings.Add(11, 11);
            bulkCopy.ColumnMappings.Add(12, 12);
            bulkCopy.ColumnMappings.Add(13, 13);
            bulkCopy.ColumnMappings.Add(14, 14);
            bulkCopy.ColumnMappings.Add(15, 15);
            bulkCopy.ColumnMappings.Add(16, 16);
            bulkCopy.ColumnMappings.Add(17, 17);
            bulkCopy.ColumnMappings.Add(18, 18);
            return bulkCopy;
        }
        private static DataColumnAttribute GetColumnNameFromProperty<T>(String propertyName)
        {
            DataColumnAttribute propAttr = null;
            object[] arrColumnAttributes = null;

            foreach (PropertyInfo pi in typeof(T).GetProperties())
            {
                if (pi.Name == propertyName)
                {
                    arrColumnAttributes = pi.GetCustomAttributes(typeof(DataColumnAttribute), true);
                    if (arrColumnAttributes.Length > 0)
                    {
                        propAttr = (DataColumnAttribute)arrColumnAttributes[0];
                        break;
                    }
                }
            }

            return propAttr;
        }

        /// <summary>
        /// Convert Delimitied file into datatable
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="tableName"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static DataTable ConvertDelimitedFileToDataTable(string filePath, string tableName, string delimiter, List<string> lstColumns)
        {
            DataTable dt = new DataTable(tableName);
            StreamReader s = null;
            try
            {
                //Open the file in a stream reader.
                s = new StreamReader(filePath);

                //Set the Column Headers                   
                foreach (string col in lstColumns)
                {
                    dt.Columns.Add(col);
                }

                //Read the data in the file.        
                string AllData = s.ReadToEnd();

                //Split off each row at the Carriage Return/Line Feed
                //Default line ending in most windows exports.  
                //You may have to edit this to match your particular file.
                //This will work for Excel, Access, etc. default exports.
                string[] rows = AllData.Split("\r\n".ToCharArray());

                //Now add each row to the DataSet        

                foreach (string r in rows)
                {
                    //Checkig empty row 
                    if (!String.IsNullOrEmpty(r))
                    {
                        //Split the row at the delimiter.
                        string[] items = r.Split(delimiter.ToCharArray());

                        string[] items1 = items.Take(lstColumns.Count).ToArray();

                        ////To add two more columns as creation date and File Import Log Id
                        //var items2 = new List<string>();
                        ////added clientId
                        //items2.Add(clientId.ToString());
                        //items2.AddRange(items1.ToList());
                        //items2.Add(Convert.ToString(DateTime.Now));
                        //items2.Add(Convert.ToString(FileImportLogId));

                        //Add the item
                        dt.Rows.Add(items1);
                    }
                }
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                    s.Dispose();
                }
            }
            return dt;
        }


        /// <summary>
        /// Convert Delimitied file into datatable
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="tableName"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static DataTable ConvertDelimitedFileToDataTableIgnoreHeaderTrailer(string filePath, string tableName, string delimiter, List<string> lstColumns)
        {
            DataTable dt = new DataTable(tableName);
            StreamReader s = null;
            try
            {
                //Open the file in a stream reader.
                s = new StreamReader(filePath);

                //Set the Column Headers                   
                foreach (string col in lstColumns)
                {
                    dt.Columns.Add(col);
                }

                //Read the data in the file.        
                string AllData = s.ReadToEnd();

                //Split off each row at the Carriage Return/Line Feed
                //Default line ending in most windows exports.  
                //You may have to edit this to match your particular file.
                //This will work for Excel, Access, etc. default exports.
                string[] rows = AllData.Split("\r\n".ToCharArray());

                //Now add each row to the DataSet        

                foreach (string r in rows)
                {
                    //Checkig empty row 
                    if (!String.IsNullOrEmpty(r) && !(r.StartsWith("HEADER") || r.StartsWith("TRAILER")))
                    {
                        //Split the row at the delimiter.
                        string[] items = r.Split(delimiter.ToCharArray());

                        string[] items1 = items.Take(lstColumns.Count).ToArray();

                        ////To add two more columns as creation date and File Import Log Id
                        //var items2 = new List<string>();
                        ////added clientId
                        //items2.Add(clientId.ToString());
                        //items2.AddRange(items1.ToList());
                        //items2.Add(Convert.ToString(DateTime.Now));
                        //items2.Add(Convert.ToString(FileImportLogId));

                        //Add the item
                        dt.Rows.Add(items1);
                    }
                }
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                    s.Dispose();
                }
            }
            return dt;
        }

        public static string[] SplitCSV(string input)
        {
            if(input.IndexOf(',') == 0)
            {
                input = " " + input;
            }
            Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);
            List<string> list = new List<string>();
            string curr = null;
            foreach (Match match in csvSplit.Matches(input))
            {
                curr = match.Value;
                if (0 == curr.Length)
                {
                    list.Add("");
                }
                list.Add(curr.TrimStart(',').Trim());
            }

            return list.ToArray();
        }
        public static string[] SplitCSVWithComma(string input)
        {           
            var dataLst = input.Split(',');
            //List<string> list = new List<string>();
            //string columnData = string.Empty;

            //foreach (var data in datalst)
            //{
            //    columnData = Regex.Match(data, "(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)").Value;
            //    list.Add(columnData);
            //}
            return dataLst;
        }
        
        public static FetchDataInfo FetchDataFromCSV(string filepath, string tableName, string delimiter, StreamWriter objTextWriter, 
            DataTable dtSchema, DataTable dtColumnMappings, bool ignoreFirstLine)
        {
            FetchDataInfo objFetchDataInfo = new FetchDataInfo();
            //List<string> lstString = new List<string>();
            //var lstErrorData = new List<string>();
            DataTable dt = new DataTable(tableName);
            //int count = 1;
            //CommonDAL commonDAL = new CommonDAL();

            try
            {
                using (TextFieldParser parser = new TextFieldParser(filepath))
                {                    
                    parser.TrimWhiteSpace = true;
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(delimiter);
                    parser.HasFieldsEnclosedInQuotes = false;

                    //to ignore the first line in file
                    if (ignoreFirstLine)
                    {
                        ignoreFirstLine = false;
                        string[] ignoreRow = parser.ReadFields();

                    }
                    //read column names
                    string[] CommaSplittedColumns = parser.ReadFields();

                    string[] colFields = null;

                    if (string.Compare(delimiter, AppConstants.DOUBLE_QUOTED_COMMA_DELIMITER) == 0)
                    {
                        colFields = CommaSplittedColumns[0].Split(',');
                    }
                    else if (string.Compare(delimiter, AppConstants.COMMA_DELIMITER) == 0)
                    {
                        colFields = CommaSplittedColumns;
                    }

                    string[] colFieldsFiltered = null;
                    
                    if (string.IsNullOrWhiteSpace(colFields.Last().ToString()))
                        colFieldsFiltered = colFields.Take(colFields.Length - 1).ToArray();
                    else
                        colFieldsFiltered = colFields;

                    foreach (string column in colFieldsFiltered)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        dt.Columns.Add(datecolumn);
                    }

                    while (!parser.EndOfData)
                    {
                        try
                        {
                            string[] parts = SplitCSV(parser.ReadLine());
                            dt.Rows.Add(parts);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            objTextWriter.WriteLine(ex.Message);
                            Util.LogException(string.Empty, ex);
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                objTextWriter.WriteLine(ex.Message);
                Artefacts.Common.Logger.ArtefactsLogger logger = Artefacts.Common.Logger.ArtefactsLogger.getLogger();
                logger.logError(ex);
                logger.CloseLogger();
                throw;
            }

            objFetchDataInfo.dataTable = dt;
            //objFetchDataInfo.lstIgnoreRow = lstString;
            //objFetchDataInfo.lstErrorMessage = lstErrorData;
            
            return objFetchDataInfo;
        }
        public static FetchDataInfo FetchDataFromCSVWithValidation(string filepath, string tableName, string delimiter, StreamWriter objTextWriter,
            DataTable dtSchema, DataTable dtColumnMappings, bool ignoreFirstLine)
        {
            FetchDataInfo objFetchDataInfo = new FetchDataInfo();
            List<string> lstString = new List<string>();
            var lstErrorData = new List<string>();
            DataTable dt = new DataTable(tableName);
            int count = 1;
            CommonDAL commonDAL = new CommonDAL();

            try
            {

                using (TextFieldParser parser = new TextFieldParser(filepath))
                {

                    parser.TrimWhiteSpace = true;
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(delimiter);
                    parser.HasFieldsEnclosedInQuotes = false;

                    //to ignore the first line in file
                    if (ignoreFirstLine)
                    {
                        ignoreFirstLine = false;
                        string[] ignoreRow = parser.ReadFields();

                    }
                    //read column names
                    string[] CommaSplittedColumns = parser.ReadFields();

                    string[] colFields = null;

                    if (string.Compare(delimiter, AppConstants.DOUBLE_QUOTED_COMMA_DELIMITER) == 0)
                    {
                        colFields = CommaSplittedColumns[0].Split(',');
                    }
                    else if (string.Compare(delimiter, AppConstants.COMMA_DELIMITER) == 0)
                    {
                        colFields = CommaSplittedColumns;
                    }

                    string[] colFieldsFiltered = null;

                    if (string.IsNullOrWhiteSpace(colFields.Last().ToString()))
                        colFieldsFiltered = colFields.Take(colFields.Length - 1).ToArray();
                    else
                        colFieldsFiltered = colFields;

                    foreach (string column in colFieldsFiltered)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        dt.Columns.Add(datecolumn);
                    }

                    while (!parser.EndOfData)
                    {
                        try
                        {
                            count++;

                            string[] parts = SplitCSV(parser.ReadLine());

                            // To avoid empty last columns
                            if (parts.Length > colFieldsFiltered.Length || parts.Length < colFieldsFiltered.Length)
                            {
                                var result = string.Join(",", parts);
                                string message = "Invalid row length at row number: " + count + ". Row is=" + result;
                                lstErrorData.Add(message);
                                lstString.Add(result);
                                objTextWriter.WriteLine(message);
                            }
                            else if (parts.Length == colFieldsFiltered.Length)
                            {
                                List<string> errors = new List<string>();
                                errors = Util.ValidateCSVData(dtSchema, dtColumnMappings, parts, colFieldsFiltered, count, objTextWriter);
                                if (errors.Count > 0 && errors[0] != "")
                                {
                                    lstErrorData.Add(errors[0]);
                                }
                                else
                                {
                                    //objTextWriter.WriteLine("Coming in else at row {0}", count);
                                    dt.Rows.Add(parts);
                                }
                            }
                            else
                            {
                                if (string.Compare(delimiter, AppConstants.DOUBLE_QUOTED_COMMA_DELIMITER) == 0)
                                {
                                    string FirstElementOfArray = parts[0];
                                    string LastElementOfArray = parts[parts.Length - 1];

                                    //objTextWriter.WriteLine("Coming in IF at row {0}", count);

                                    // Removing first element's starting double quote
                                    if (string.Compare(FirstElementOfArray.Substring(0, 1), "\"") == 0)
                                        parts[0] = FirstElementOfArray.Substring(1, parts[0].Length - 1);
                                    // Removing last element's last double quote character
                                    if (string.Compare(LastElementOfArray.Substring(LastElementOfArray.Length - 1, 1), "\"") == 0)
                                        parts[parts.Length - 1] = LastElementOfArray.Substring(0, LastElementOfArray.Length - 1);
                                }
                                else
                                {
                                    objTextWriter.WriteLine("Coming in else at row {0}", count);
                                }
                                dt.Rows.Add(parts);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            objTextWriter.WriteLine(ex.Message);
                            Util.LogException(string.Empty, ex);
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                objTextWriter.WriteLine(ex.Message);
                Artefacts.Common.Logger.ArtefactsLogger logger = Artefacts.Common.Logger.ArtefactsLogger.getLogger();
                logger.logError(ex);
                logger.CloseLogger();
                throw;
            }

            objFetchDataInfo.dataTable = dt;
            objFetchDataInfo.lstIgnoreRow = lstString;
            objFetchDataInfo.lstErrorMessage = lstErrorData;

            return objFetchDataInfo;
        }


    }
}
