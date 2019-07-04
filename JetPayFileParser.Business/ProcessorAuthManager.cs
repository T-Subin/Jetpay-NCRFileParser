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
using System.Data.SqlClient;

namespace JetPayFileParser.Business
{
    public class ProcessorAuthManager
    {
        ProcessorAuthDAL authdal = new ProcessorAuthDAL();

        public bool BulkInsertAuth(int clientId, string filePath, StreamWriter objTextWriter, string InputPath, string MoveFilePathWithName, string FileMovePath)
        {
            FileImportLogManager fileImportLogManager = new FileImportLogManager();
            CommonDAL commonDAL = new CommonDAL();
            long fileImportLogId = 0;
            try
            {
                string fileName = Path.GetFileName(MoveFilePathWithName);
                FileInfo fileInfo = new FileInfo(MoveFilePathWithName);
                fileImportLogId = fileImportLogManager.InsertDataToFileImportLog(fileName, FileType.ACQAuth, fileInfo.LastWriteTime);

                // Create the TransactionScope to execute the commands

                //{
                try
                {
                    //DataTable dt = Util.ParseCSV(fileName, FileMovePath, AppConstants.Table_ProcessorAuth_JatPay, Convert.ToChar(","), MoveFilePathWithName);

                    DataTable dtSchema = commonDAL.GetTableSchema(AppConstants.Table_ACQAuthFile);
                    DataTable dtColumnMappings = commonDAL.GetColumnMappingsByFileTypeId((int)FileType.ACQAuth);

                    FetchDataInfo objFetchDataInfo = BulkDataInsertManager.FetchDataFromCSV(MoveFilePathWithName, AppConstants.Table_ProcessorAuth_JatPay,
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

                                if (dr.IsColumnExists("Authdatetime"))
                                    dr["Authdatetime"] = dr["Authdatetime"].ToString().Replace("'", "").NullSafeYYYYMMDDHHMMSSStringToDateTime();
                                if (dr.IsColumnExists("Shipdatetime"))
                                    dr["Shipdatetime"] = dr["Shipdatetime"].ToString().Replace("'", "").NullSafeYYYYMMDDHHMMSSStringToDateTime();
                            }
                        }
                    }
                    objTextWriter.WriteLine("New data table created with audit columns");

                    dt.AcceptChanges();
                    objTextWriter.WriteLine("File converted to data table");

                    if (dt.Rows.Count > 0)
                    {
                        //try
                        //{
                            //using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(1, 15, 0)))
                            //{
                                objTextWriter.WriteLine("Bulk insertion starting...: {0}----------", DateTime.Now);
                                BulkDataInsertManager.BulkInsertWithColumnMapping(AppConstants.Table_ProcessorAuth_JatPay, dt, GetColumnMappings());
                                objTextWriter.WriteLine("Bulk insertion completed : {0}----------", DateTime.Now);

                                objTextWriter.WriteLine("Data movement to transaction table starting : {0}----------", DateTime.Now);
                                authdal.MoveDataToTransactionTables(fileImportLogId);
                                objTextWriter.WriteLine("Data moved to transaction table completed : {0}----------", DateTime.Now);

                                // Commit the transaction
                                //transactionScope.Complete();

                                objTextWriter.WriteLine("Transaction committed");
                            //}
                        //}
                        //catch (TransactionAbortedException ex)
                        //{
                        //    // we can get here even if scope.Complete() was called.
                        //    // log TransactionAborted exception if necessary

                        //    string msg = "Error in Transaction : " + fileName + " - Error: " + ex.Message;
                        //    Console.WriteLine(msg);
                        //    objTextWriter.WriteLine(msg);
                        //}


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
            {
                // Code that runs when an unhandled error occurs
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
            columnMappings.Add(new KeyValuePair<string, string>("MID", "MID"));
            columnMappings.Add(new KeyValuePair<string, string>("Entity ID", "Entity ID"));
            columnMappings.Add(new KeyValuePair<string, string>("TID", "TID"));
            columnMappings.Add(new KeyValuePair<string, string>("Card Type", "Card Type"));
            columnMappings.Add(new KeyValuePair<string, string>("Action Code", "Action Code"));
            columnMappings.Add(new KeyValuePair<string, string>("Transaction ID", "Transaction ID"));
            columnMappings.Add(new KeyValuePair<string, string>("Request Type", "Request Type"));
            columnMappings.Add(new KeyValuePair<string, string>("Cardnum", "Cardnum"));
            columnMappings.Add(new KeyValuePair<string, string>("Exp Date", "Exp Date"));
            columnMappings.Add(new KeyValuePair<string, string>("Other Data4", "Other Data4"));
            columnMappings.Add(new KeyValuePair<string, string>("Transaction Type", "Transaction Type"));
            columnMappings.Add(new KeyValuePair<string, string>("Amount", "Amount"));
            columnMappings.Add(new KeyValuePair<string, string>("Auth Code", "Auth Code"));
            columnMappings.Add(new KeyValuePair<string, string>("CVV", "CVV"));
            columnMappings.Add(new KeyValuePair<string, string>("AVS Response", "AVS Response"));
            columnMappings.Add(new KeyValuePair<string, string>("Authdatetime", "Authdatetime"));
            columnMappings.Add(new KeyValuePair<string, string>("Shipdatetime", "Shipdatetime"));
            columnMappings.Add(new KeyValuePair<string, string>("Ticket No", "Ticket No"));
            columnMappings.Add(new KeyValuePair<string, string>("UD1", "UD1"));
            columnMappings.Add(new KeyValuePair<string, string>("UD2", "UD2"));
            columnMappings.Add(new KeyValuePair<string, string>("UD3", "UD3"));
            columnMappings.Add(new KeyValuePair<string, string>("Authorization Authority", "Authorization Authority"));
            columnMappings.Add(new KeyValuePair<string, string>("Network ID", "Network ID"));
            columnMappings.Add(new KeyValuePair<string, string>("POS Entry", "POS Entry"));
            columnMappings.Add(new KeyValuePair<string, string>("Ordernumber", "Ordernumber"));
            columnMappings.Add(new KeyValuePair<string, string>("Cardholdername", "Cardholdername"));
            columnMappings.Add(new KeyValuePair<string, string>("Address", "Address"));
            columnMappings.Add(new KeyValuePair<string, string>("City", "City"));
            columnMappings.Add(new KeyValuePair<string, string>("State", "State"));
            columnMappings.Add(new KeyValuePair<string, string>("Zip", "Zip"));
            columnMappings.Add(new KeyValuePair<string, string>("Country", "Country"));
            columnMappings.Add(new KeyValuePair<string, string>("Phone", "Phone"));
            columnMappings.Add(new KeyValuePair<string, string>("Email", "Email"));
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
