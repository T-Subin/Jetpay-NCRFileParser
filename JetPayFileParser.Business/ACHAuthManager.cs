using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Transactions;
using JetPayFileParser.DataAccess;
using JetPayFileParser.Model;
using JetPayFileParser.Model.Enum;
using JetPayFileParser.Model.Type;
using JetPayFileParser.Utility;

namespace JetPayFileParser.Business
{
    public class ACHAuthManager
    {
        ACHAuthDAL authdal = new ACHAuthDAL();
        CommonDAL commonDAL = new CommonDAL();
        public bool BulkInsertAuth(int clientId, string filePath, StreamWriter objTextWriter, string InputPath, string MoveFilePathWithName, string FileMovePath)
        {
            FileImportLogManager fileImportLogManager = new FileImportLogManager();
            CommonDAL commonDAL = new CommonDAL();
            long fileImportLogId = 0;
            try
            {
                string fileName = Path.GetFileName(MoveFilePathWithName);
                FileInfo fileInfo = new FileInfo(MoveFilePathWithName);
                fileImportLogId = fileImportLogManager.InsertDataToFileImportLog(fileName, FileType.ACHAuth, fileInfo.LastWriteTime);
                                
                try
                {
                    //DataTable dt = Util.ParseCSV(fileName, FileMovePath, AppConstants.Table_ProcessorAuth_JatPay, Convert.ToChar(","), MoveFilePathWithName);

                    DataTable dtSchema = commonDAL.GetTableSchema(AppConstants.Table_ACHAuth);
                    DataTable dtColumnMappings = commonDAL.GetColumnMappingsByFileTypeId((int)FileType.ACHAuth);

                    FetchDataInfo objFetchDataInfo = BulkDataInsertManager.FetchDataFromCSV(MoveFilePathWithName, AppConstants.Table_ACHAuth_JetPay,
                        Convert.ToString(","), objTextWriter, dtSchema, dtColumnMappings, true);

                    DataTable dt = objFetchDataInfo.dataTable;

                    if (objFetchDataInfo != null && objFetchDataInfo.lstErrorMessage != null && objFetchDataInfo.lstErrorMessage.Count > 0)
                    {
                        commonDAL.AddErrorLog(fileImportLogId, objFetchDataInfo.lstErrorMessage, null);
                        EmailUtility.SendMailOnParserFailure(objFetchDataInfo.lstErrorMessage, null, Path.GetFileName(MoveFilePathWithName));
                    }

                    //DataTable dtSchema = commonDAL.GetTableSchema(AppConstants.Table_ProcessorAuth_JatPay);
                    //Util.ValidateData(dt, dtSchema);

                    dt.Columns.Add("CreationDate", typeof(DateTime));
                    dt.Columns.Add("ClientId", typeof(long));
                    dt.Columns.Add("FileImportLogId", typeof(long));
                    dt.Columns.Add("ProcessorId", typeof(long));
                    
                    objTextWriter.WriteLine("File converted to data table");

                    foreach (DataRow dr in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(dr["MID"])))
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dr["Amount"])))
                            {
                                dr["Amount"] = "0";
                            }
                            dr["CreationDate"] = DateTime.Now;
                            dr["ClientId"] = ConfigInfo.CLIENT_ID;
                            dr["FileImportLogId"] = fileImportLogId;
                            dr["ProcessorId"] = ConfigInfo.ProcessorId;
                        }
                    }
                    dt.AcceptChanges();
                    objTextWriter.WriteLine("New data table created with audit columns");

                    if (dt.Rows.Count > 0)
                    {
                        objTextWriter.WriteLine("Bulk insertion Started : {0}----------", DateTime.Now);
                        BulkDataInsertManager.BulkInsertWithColumnMapping(AppConstants.Table_ACHAuth_JetPay, dt, GetColumnMappings(), false);
                        objTextWriter.WriteLine("Bulk insertion complete : {0}----------", DateTime.Now);

                        objTextWriter.WriteLine("Data movement to Main table started : {0}----------", DateTime.Now);
                        authdal.MoveDataToACHMainTable(fileImportLogId);
                        objTextWriter.WriteLine("Data movement to Main table Completed : {0}----------", DateTime.Now);

                        fileImportLogManager.UpdateFileImportLog(fileImportLogId, FileImportLogStatus.Success);
                        if (File.Exists(MoveFilePathWithName))
                        {
                            // Encrypt and Archive
                            Util.ZipEncryptFile(objTextWriter, MoveFilePathWithName,
                                ConfigInfo.FILE_SUCCESS_ARCHIVE_PATH + "Archive_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + fileName);
                        }
                        else
                        {
                            objTextWriter.WriteLine("File does not exist at {0}", MoveFilePathWithName);
                        }
                    }
                    else if (objFetchDataInfo.lstErrorMessage.Count > 0)
                    {
                        fileImportLogManager.UpdateFileImportLog(fileImportLogId, FileImportLogStatus.Fail);

                        // Archive input file
                        if (File.Exists(MoveFilePathWithName))
                        {
                            // Encrypt and Archive
                            Util.ZipEncryptFile(objTextWriter, MoveFilePathWithName,
                                ConfigInfo.FILE_ERROR_ARCHIVE_PATH + "Archive_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(MoveFilePathWithName));
                        }
                        else
                        {
                            objTextWriter.WriteLine("File does not exist at {0}", MoveFilePathWithName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Code that runs when an unhandled error occurs
                    string msg = "Error in file " + fileName + " Error: " + ex.Message;
                    Console.WriteLine(msg);
                    objTextWriter.WriteLine(msg);

                    Artefacts.Common.Logger.ArtefactsLogger logger = Artefacts.Common.Logger.ArtefactsLogger.getLogger();
                    logger.logError("Error in file " + fileName);
                    logger.logError(ex);
                    logger.CloseLogger();
                    throw;
                }
            }
            catch (Exception ex)
            { // Code that runs when an unhandled error occurs
                Console.WriteLine(ex.Message);
                objTextWriter.WriteLine(ex.Message);

                Artefacts.Common.Logger.ArtefactsLogger logger = Artefacts.Common.Logger.ArtefactsLogger.getLogger();
                logger.logError(ex);
                logger.CloseLogger();
                try
                {
                    commonDAL.AddErrorLog(fileImportLogId, new List<string> { ex.Message }, null);
                    EmailUtility.SendMailOnParserFailure(null, ex, Path.GetFileName(MoveFilePathWithName));
                }
                catch (Exception exception)
                {
                    Artefacts.Common.Logger.ArtefactsLogger log = Artefacts.Common.Logger.ArtefactsLogger.getLogger();
                    log.logError(exception);
                    log.CloseLogger();
                }
                fileImportLogManager.UpdateFileImportLog(fileImportLogId, FileImportLogStatus.Fail);

                // Archive input file
                if (File.Exists(MoveFilePathWithName))
                {
                    // Encrypt and Archive
                    Util.ZipEncryptFile(objTextWriter, MoveFilePathWithName,
                        ConfigInfo.FILE_ERROR_ARCHIVE_PATH + "Archive_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(MoveFilePathWithName));
                }
                else
                {
                    objTextWriter.WriteLine("File does not exist at {0}", MoveFilePathWithName);
                }

                return false;
            }
            return true;
        }

        private ICollection<KeyValuePair<string, string>> GetColumnMappings()
        {
            // The source column names are in upper case as the data table is created with upper case column headers
            ICollection<KeyValuePair<string, string>> columnMappings = new List<KeyValuePair<string, string>>();
            columnMappings.Add(new KeyValuePair<string, string>("MID", "MID"));
            columnMappings.Add(new KeyValuePair<string, string>("TID", "TID"));
            columnMappings.Add(new KeyValuePair<string, string>("Request Type", "RequestType"));
            columnMappings.Add(new KeyValuePair<string, string>("Txn_ID", "TransactionID"));
            columnMappings.Add(new KeyValuePair<string, string>("Account", "Account"));
            columnMappings.Add(new KeyValuePair<string, string>("Name", "Cardholdername"));
            columnMappings.Add(new KeyValuePair<string, string>("Status", "Status"));
            columnMappings.Add(new KeyValuePair<string, string>("Amount", "Amount"));
            columnMappings.Add(new KeyValuePair<string, string>("Authdatetime", "Authdatetime"));
            columnMappings.Add(new KeyValuePair<string, string>("Phase1", "Phase1"));
            columnMappings.Add(new KeyValuePair<string, string>("Phase2", "Phase2"));
            columnMappings.Add(new KeyValuePair<string, string>("Completion Date", "CompletionDate"));
            columnMappings.Add(new KeyValuePair<string, string>("Unique ID", "UniqueID"));
            columnMappings.Add(new KeyValuePair<string, string>("Ordernumber", "Ordernumber"));
            columnMappings.Add(new KeyValuePair<string, string>("Address", "Address"));
            columnMappings.Add(new KeyValuePair<string, string>("City", "City"));
            columnMappings.Add(new KeyValuePair<string, string>("State", "State"));
            columnMappings.Add(new KeyValuePair<string, string>("Zip", "Zip"));
            columnMappings.Add(new KeyValuePair<string, string>("Phone", "Phone"));
            columnMappings.Add(new KeyValuePair<string, string>("Email", "Email"));
            columnMappings.Add(new KeyValuePair<string, string>("UD1", "Ud1"));
            columnMappings.Add(new KeyValuePair<string, string>("UD2", "Ud2"));
            columnMappings.Add(new KeyValuePair<string, string>("UD3", "Ud3"));

            // Audit Columns
            columnMappings.Add(new KeyValuePair<string, string>("CREATIONDATE", "CreationDate"));
            columnMappings.Add(new KeyValuePair<string, string>("CLIENTID", "ClientId"));
            columnMappings.Add(new KeyValuePair<string, string>("FILEIMPORTLOGID", "FileImportLogId"));
            columnMappings.Add(new KeyValuePair<string, string>("PROCESSORID", "ProcessorId"));
            columnMappings.Add(new KeyValuePair<string, string>("ISDATAMOVED", "IsDataMoved"));
            return columnMappings;
        }

    }
}
