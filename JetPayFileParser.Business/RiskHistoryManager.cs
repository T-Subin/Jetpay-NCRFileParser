using JetPayFileParser.DataAccess;
using JetPayFileParser.Model;
using JetPayFileParser.Model.Enum;
using JetPayFileParser.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace JetPayFileParser.Business
{
    public class RiskHistoryManager
    {
        RiskHistoryDAL riskTransactionsDAL = new RiskHistoryDAL();
        CommonDAL commonDAL = new CommonDAL();

        public bool BulkInsertAuth(int clientId, string filePath, StreamWriter objTextWriter, string InputPath, string MoveFilePathWithName, string FileMovePath)
        {
            FileImportLogManager fileImportLogManager = new FileImportLogManager();
            long fileImportLogId = 0;
            try
            {
                string fileName = Path.GetFileName(MoveFilePathWithName);
                FileInfo fileInfo = new FileInfo(MoveFilePathWithName);
                fileImportLogId = fileImportLogManager.InsertDataToFileImportLog(fileName, FileType.RiskTransactions, fileInfo.LastWriteTime);
                //using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(1, 15, 0)))
                {
                    try
                    {
                        //DataTable dt = Util.ParseCSV(fileName, FileMovePath, AppConstants.Table_RiskTransactions_JetPay, Convert.ToChar(","), MoveFilePathWithName);

                        DataTable dtSchema = commonDAL.GetTableSchema(AppConstants.Table_RiskTransactions);
                        DataTable dtColumnMappings = commonDAL.GetColumnMappingsByFileTypeId((int)FileType.RiskTransactions);

                        FetchDataInfo objFetchDataInfo = BulkDataInsertManager.FetchDataFromCSV(MoveFilePathWithName, AppConstants.Table_RiskTransactions_JetPay,
                            Convert.ToString(","), objTextWriter, dtSchema, dtColumnMappings, true);

                        DataTable dt = objFetchDataInfo.dataTable;

                        if (objFetchDataInfo != null && objFetchDataInfo.lstErrorMessage != null && objFetchDataInfo.lstErrorMessage.Count > 0)
                        {
                            commonDAL.AddErrorLog(fileImportLogId, objFetchDataInfo.lstErrorMessage, null);
                            EmailUtility.SendMailOnParserFailure(objFetchDataInfo.lstErrorMessage, null, Path.GetFileName(MoveFilePathWithName));
                        }

                        //DataTable dtSchema = commonDAL.GetTableSchema(AppConstants.Table_Qualification_JetPay);
                        //Util.ValidateData(dt,dtSchema);

                        dt.Columns.Add("CreationDate", typeof(DateTime));
                        dt.Columns.Add("ClientId", typeof(long));
                        dt.Columns.Add("FileImportLogId", typeof(long));
                        dt.Columns.Add("ProcessorId", typeof(long));
                        dt.Columns.Add("IsDataMoved", typeof(bool));

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
                                dr["IsDataMoved"] = false;
                            }
                        }
                        dt.AcceptChanges();

                        objTextWriter.WriteLine("New data table created with audit columns");

                        if (dt.Rows.Count > 0)
                        {
                            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(1, 15, 0)))
                            {
                                BulkDataInsertManager.BulkInsertWithColumnMapping(AppConstants.Table_RiskTransactions_JetPay, dt, GetColumnMappings(), false);
                                objTextWriter.WriteLine("Bulk insertion complete");

                                riskTransactionsDAL.MoveDataToRiskTransactions();

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
                }

                

            }
            catch (Exception ex)
            { // Code that runs when an unhandled error occurs
                Console.WriteLine(ex.Message);
                objTextWriter.WriteLine(ex.Message);

                Artefacts.Common.Logger.ArtefactsLogger logger = Artefacts.Common.Logger.ArtefactsLogger.getLogger();
                logger.logError(ex);
                logger.CloseLogger();

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
            columnMappings.Add(new KeyValuePair<string, string>("ENTITY_ID", "EntityId"));
            columnMappings.Add(new KeyValuePair<string, string>("TID", "TID"));
            columnMappings.Add(new KeyValuePair<string, string>("CARD_TYPE", "CardType"));
            columnMappings.Add(new KeyValuePair<string, string>("TRANSACTION_ID", "TransactionId"));
            columnMappings.Add(new KeyValuePair<string, string>("REQUEST_TYPE", "RequestType"));
            columnMappings.Add(new KeyValuePair<string, string>("SETTLE_DATE", "SettleDate"));
            columnMappings.Add(new KeyValuePair<string, string>("CARDNUM", "CardNumber"));
            columnMappings.Add(new KeyValuePair<string, string>("OTHER_DATA3", "Other Data3"));
            columnMappings.Add(new KeyValuePair<string, string>("OTHER_DATA4", "Other Data4"));
            columnMappings.Add(new KeyValuePair<string, string>("ARN", "ARN"));
            columnMappings.Add(new KeyValuePair<string, string>("TRANSACTION_TYPE", "TransactionType"));
            columnMappings.Add(new KeyValuePair<string, string>("AMOUNT", "Amount"));
            columnMappings.Add(new KeyValuePair<string, string>("AUTH_CODE", "AuthCode"));
            columnMappings.Add(new KeyValuePair<string, string>("AUTHDATETIME", "AuthDatetime"));

            // Audit Columns
            columnMappings.Add(new KeyValuePair<string, string>("CreationDate", "CreationDate"));
            columnMappings.Add(new KeyValuePair<string, string>("ClientId", "ClientId"));
            columnMappings.Add(new KeyValuePair<string, string>("FileImportLogId", "FileImportLogId"));
            columnMappings.Add(new KeyValuePair<string, string>("ProcessorId", "ProcessorId"));
            columnMappings.Add(new KeyValuePair<string, string>("IsDataMoved", "IsDataMoved"));
            return columnMappings;
        }
    }
}
