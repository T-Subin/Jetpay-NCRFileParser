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
    public class ChargebackManager
    {

        
        ChargebackDAL chargeDal = new ChargebackDAL();

        public bool BulkInsertChargeback(int clientId, string filePath, StreamWriter objTextWriter, string InputPath, string MoveFilePathWithName, string FileMovePath)
        {
            FileImportLogManager fileImportLogManager = new FileImportLogManager();
            CommonDAL commonDAL = new CommonDAL();
            long fileImportLogId = 0;
            try
            {
                string fileName = Path.GetFileName(MoveFilePathWithName);
                FileInfo fileInfo = new FileInfo(MoveFilePathWithName);
                fileImportLogId = fileImportLogManager.InsertDataToFileImportLog(fileName, FileType.ChargebackRetrieval, fileInfo.LastWriteTime);

                //{
                try
                {
                    //DataTable dt = Util.ParseCSV(fileName, FileMovePath, AppConstants.Table_ProcessorAuth_JatPay, Convert.ToChar(","), MoveFilePathWithName);
                    DataTable dtSchema = commonDAL.GetTableSchema(AppConstants.Table_ChargebackRetrieval);
                    DataTable dtColumnMappings = commonDAL.GetColumnMappingsByFileTypeId((int)FileType.ChargebackRetrieval);

                    FetchDataInfo objFetchDataInfo = BulkDataInsertManager.FetchDataFromCSV(MoveFilePathWithName, AppConstants.Table_Chargeback_JetPay,
                        Convert.ToString(","), objTextWriter, dtSchema, dtColumnMappings, true);

                    DataTable dt = objFetchDataInfo.dataTable;

                    if (objFetchDataInfo != null && objFetchDataInfo.lstErrorMessage != null && objFetchDataInfo.lstErrorMessage.Count > 0)
                    {
                        commonDAL.AddErrorLog(fileImportLogId, objFetchDataInfo.lstErrorMessage, null);
                        EmailUtility.SendMailOnParserFailure(objFetchDataInfo.lstErrorMessage, null, Path.GetFileName(MoveFilePathWithName));
                    }

                    dt.Columns.Add("CreationDate", typeof(DateTime));
                    dt.Columns.Add("ClientId", typeof(long));
                    dt.Columns.Add("FileImportLogId", typeof(long));
                    dt.Columns.Add("ProcessorId", typeof(long));
                    dt.Columns.Add("IsDataMoved", typeof(bool));

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Entity ID"])))
                            {
                                if (string.IsNullOrEmpty(Convert.ToString(dr["Chargeback Amount"])))
                                {
                                    dr["Chargeback Amount"] = "0";
                                }
                                dr["CreationDate"] = DateTime.Now;
                                dr["ClientId"] = ConfigInfo.CLIENT_ID;
                                dr["FileImportLogId"] = fileImportLogId;
                                dr["ProcessorId"] = ConfigInfo.ProcessorId;
                                dr["IsDataMoved"] = false;
                            }
                        }
                    }

                    objTextWriter.WriteLine("New data table created with audit columns");

                    dt.AcceptChanges();
                    objTextWriter.WriteLine("File converted to data table");

                    if (dt.Rows.Count > 0)
                    {
                        using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(1, 15, 0)))
                        {
                            BulkDataInsertManager.BulkInsertWithColumnMapping(AppConstants.Table_Chargeback_JetPay, dt, GetColumnMappings());
                            objTextWriter.WriteLine("Bulk insertion complete");

                            chargeDal.MoveDataToTransactionTables();

                            objTextWriter.WriteLine("Data moved to transaction table");

                            // Commit the transaction
                            transactionScope.Complete();

                            objTextWriter.WriteLine("Transaction committed");
                        }

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
                //}

                

            }
            catch (Exception ex)
            { // Code that runs when an unhandled error occurs
                Console.WriteLine(ex.Message);
                objTextWriter.WriteLine(ex.Message);

                Artefacts.Common.Logger.ArtefactsLogger logger = Artefacts.Common.Logger.ArtefactsLogger.getLogger();
                logger.logError(ex);
                logger.CloseLogger();

                //send mail on failing of parser.
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
            columnMappings.Add(new KeyValuePair<string, string>("Entity ID", "Entity_Id"));
            columnMappings.Add(new KeyValuePair<string, string>("Chargeback Date", "Chargeback_Date"));
            columnMappings.Add(new KeyValuePair<string, string>("TID", "Tid"));
            columnMappings.Add(new KeyValuePair<string, string>("Chargeback Type", "Chargeback_Type"));
            columnMappings.Add(new KeyValuePair<string, string>("Card Type", "Card_Type"));
            columnMappings.Add(new KeyValuePair<string, string>("Card Number", "Card_Number"));
            columnMappings.Add(new KeyValuePair<string, string>("Chargeback Amount", "Chargeback_Amount"));
            columnMappings.Add(new KeyValuePair<string, string>("Chargeback Currency", "Chargeback_Currency"));
            columnMappings.Add(new KeyValuePair<string, string>("Chargeback Reason", "Chargeback_Reason"));
            columnMappings.Add(new KeyValuePair<string, string>("Arn", "Arn"));
            columnMappings.Add(new KeyValuePair<string, string>("Chargeback Transaction ID", "Chargeback_TransactionId"));
            columnMappings.Add(new KeyValuePair<string, string>("Original Amt", "OriginalAmt"));
            columnMappings.Add(new KeyValuePair<string, string>("Original Approval Date", "Original_Approval_Date"));
            columnMappings.Add(new KeyValuePair<string, string>("Settle Date", "Settle_Date"));
            columnMappings.Add(new KeyValuePair<string, string>("Original Approval Nbr", "Original_Approval_Nbr"));
            columnMappings.Add(new KeyValuePair<string, string>("Retrieval Request Date", "Retrieval_Request_Date"));           
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
