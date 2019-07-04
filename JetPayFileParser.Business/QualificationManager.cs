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
    public class QualificationManager
    {
        QualificationDAL qualificationDal = new QualificationDAL();
        CommonDAL commonDAL = new CommonDAL();

        public bool BulkInsertAuth(int clientId, string filePath, StreamWriter objTextWriter, string InputPath, string MoveFilePathWithName, string FileMovePath)
        {
            FileImportLogManager fileImportLogManager = new FileImportLogManager();
            long fileImportLogId = 0;
            try
            {
                string fileName = Path.GetFileName(MoveFilePathWithName);
                FileInfo fileInfo = new FileInfo(MoveFilePathWithName);
                fileImportLogId = fileImportLogManager.InsertDataToFileImportLog(fileName, FileType.QUALIFICATION, fileInfo.LastWriteTime);

                //{
                try
                {
                    //DataTable dt = Util.ParseCSV(fileName, FileMovePath, AppConstants.Table_Qualification_JetPay, Convert.ToChar(","), MoveFilePathWithName);

                    DataTable dtSchema = commonDAL.GetTableSchema(AppConstants.Table_QualificationData);
                    DataTable dtColumnMappings = commonDAL.GetColumnMappingsByFileTypeId((int)FileType.QUALIFICATION);

                    FetchDataInfo objFetchDataInfo = BulkDataInsertManager.FetchDataFromCSV(MoveFilePathWithName, AppConstants.Table_Qualification_JetPay,
                        Convert.ToString(","), objTextWriter, dtSchema, dtColumnMappings, true);

                    DataTable dt = objFetchDataInfo.dataTable;

                    if (objFetchDataInfo != null && objFetchDataInfo.lstErrorMessage != null && objFetchDataInfo.lstErrorMessage.Count > 0)
                    {
                        commonDAL.AddErrorLog(fileImportLogId, objFetchDataInfo.lstErrorMessage, null);
                        EmailUtility.SendMailOnParserFailure(objFetchDataInfo.lstErrorMessage, null, Path.GetFileName(MoveFilePathWithName));
                    }

                    //Removing the unwanted column.
                    if (dt.Columns.Contains("NoName"))
                        dt.Columns.Remove("NoName");

                    //DataTable dtSchema = commonDAL.GetTableSchema(AppConstants.Table_Qualification_JetPay);
                    //Util.ValidateData(dt,dtSchema);

                    dt.Columns.Add("CreationDate", typeof(DateTime));
                    dt.Columns.Add("ClientId", typeof(long));
                    dt.Columns.Add("FileImportLogId", typeof(long));
                    dt.Columns.Add("ProcessorId", typeof(long));
                    dt.Columns.Add("IsDataMoved", typeof(bool));

                    objTextWriter.WriteLine("File converted to data table");

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dr["Merchant ID"])))
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
                    }
                    dt.AcceptChanges();

                    objTextWriter.WriteLine("New data table created with audit columns");

                    if (dt.Rows.Count>0)
                    {
                        //using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Required, new System.TimeSpan(1, 15, 0)))
                        //{
                            BulkDataInsertManager.BulkInsertWithColumnMapping(AppConstants.Table_Qualification_JetPay, dt, GetColumnMappings(), false);
                            objTextWriter.WriteLine("Bulk insertion complete");

                            qualificationDal.MoveDataToQualificationData();

                            objTextWriter.WriteLine("Data moved to transaction table");

                            // Commit the transaction
                            //transactionScope.Complete();

                            objTextWriter.WriteLine("Transaction committed");
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
            columnMappings.Add(new KeyValuePair<string, string>("Merchant ID", "MID"));
            columnMappings.Add(new KeyValuePair<string, string>("Merchant Name", "MerchantName"));
            columnMappings.Add(new KeyValuePair<string, string>("Qualification", "Qualification"));
            columnMappings.Add(new KeyValuePair<string, string>("Card Number", "CardNumber"));
            columnMappings.Add(new KeyValuePair<string, string>("Amount", "Amount"));
            columnMappings.Add(new KeyValuePair<string, string>("Interchange", "Interchange"));
            columnMappings.Add(new KeyValuePair<string, string>("Auth Date", "AuthDate"));
            columnMappings.Add(new KeyValuePair<string, string>("Settle Date", "Settle Date"));
            columnMappings.Add(new KeyValuePair<string, string>("ARN", "ARN"));
            columnMappings.Add(new KeyValuePair<string, string>("MAS Code", "MAS Code"));
            columnMappings.Add(new KeyValuePair<string, string>("MAS Code Downgrade", "MASCodeDowngrade"));
            columnMappings.Add(new KeyValuePair<string, string>("POS Ch Auth Method", "POSChAuthMethod"));
            columnMappings.Add(new KeyValuePair<string, string>("POS Ch Present", "POSChPresent"));
            columnMappings.Add(new KeyValuePair<string, string>("POS Crd Present", "POSCrdPresent"));
            columnMappings.Add(new KeyValuePair<string, string>("Program Desc", "ProgramDesc"));
            columnMappings.Add(new KeyValuePair<string, string>("Transaction Id", "TransactionId"));
            columnMappings.Add(new KeyValuePair<string, string>("Other Data4", "Other Data4"));
            columnMappings.Add(new KeyValuePair<string, string>("Tier", "Tier"));

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
