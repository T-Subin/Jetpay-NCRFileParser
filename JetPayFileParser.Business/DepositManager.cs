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
    public class DepositManager
    {
        //DepositDAL depositDAL = new DepositDAL();

        //public bool BulkInsertDeposits(int clientId, string filePath, StreamWriter objTextWriter)
        //{
        //    FileImportLogManager fileImportLogManager = new FileImportLogManager();

        //    long fileImportLogId = 0;

        //    try
        //    {
        //        string fileName = Path.GetFileName(filePath);

        //        // Insert FileImportLog
        //        FileInfo fileInfo = new FileInfo(filePath);
        //        fileImportLogId = fileImportLogManager.InsertDataToFileImportLog(fileName, FileType.ACHDetail, fileInfo.LastWriteTime);

        //        using (TransactionScope transactionScope = new TransactionScope())
        //        {
        //            try
        //            {
        //                DataTable dt = Util.ParseCSV(filePath, AppConstants.Table_Deposit_Max2, Convert.ToChar(","));

        //                dt.Columns.Add("CreationDate", typeof(DateTime));
        //                dt.Columns.Add("ClientId", typeof(long));
        //                dt.Columns.Add("FileImportLogId", typeof(long));
        //                dt.Columns.Add("ProcessorId", typeof(long));
        //                dt.Columns.Add("IsDataMoved", typeof(bool));

        //                objTextWriter.WriteLine("File converted to data table");

        //                DataTable dtNew = new DataTable(AppConstants.Table_Deposit_Max2);
        //                foreach (DataColumn dtColumn in dt.Columns)
        //                {
        //                    dtNew.Columns.Add(dtColumn.ColumnName);
        //                }

        //                int counter = 0;
        //                foreach (DataRow dtOldRow in dt.Rows)
        //                {
        //                    if (dtOldRow["MID"].ToString() != string.Empty)
        //                    {
        //                        dtNew.ImportRow(dtOldRow);

        //                        DataRow dtRow = dtNew.Rows[counter];

        //                        // If the amounts are blank, assign zeroes
        //                        if (dtRow["TRAN FEE"].ToString() == string.Empty)
        //                        {
        //                            dtRow["TRAN FEE"] = "0";
        //                        }

        //                        if (dtRow["RESERVE"].ToString() == string.Empty)
        //                        {
        //                            dtRow["RESERVE"] = "0";
        //                        }

        //                        if (dtRow["FUNDING AMOUNT"].ToString() == string.Empty)
        //                        {
        //                            dtRow["FUNDING AMOUNT"] = "0";
        //                        }

        //                        if (dtRow["TRANSACTION DATE/TIME"].ToString() != string.Empty)
        //                        {
        //                            dtRow["TRANSACTION DATE/TIME"] = DateTime.ParseExact(dtRow["TRANSACTION DATE/TIME"].ToString(), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        //                        }

        //                        dtRow["CreationDate"] = DateTime.Now;
        //                        dtRow["ClientId"] = ConfigInfo.CLIENT_ID;
        //                        dtRow["FileImportLogId"] = fileImportLogId;
        //                        dtRow["ProcessorId"] = ConfigInfo.ProcessorId;
        //                        dtRow["IsDataMoved"] = false;
        //                    }

        //                    counter = counter + 1;
        //                }

        //                objTextWriter.WriteLine("New data table created with audit columns");

        //                BulkDataInsertManager.BulkInsertWithColumnMapping(AppConstants.Table_Deposit_Max2, dtNew, GetColumnMappings());

        //                objTextWriter.WriteLine("Bulk insertion complete");

        //                depositDAL.MoveDataToTransactionTables();

        //                objTextWriter.WriteLine("Data moved to transaction table");

        //                // Commit the transaction
        //                transactionScope.Complete();

        //                objTextWriter.WriteLine("Transaction committed");
        //            }
        //            catch (Exception ex)
        //            {
        //                // Code that runs when an unhandled error occurs
        //                string msg = "Error in file " + fileName + " Error: " + ex.Message;
        //                Console.WriteLine(msg);
        //                objTextWriter.WriteLine(msg);

        //                Artefacts.Common.Logger.ArtefactsLogger logger = Artefacts.Common.Logger.ArtefactsLogger.getLogger();
        //                logger.logError("Error in file " + fileName);
        //                logger.logError(ex);
        //                logger.CloseLogger();
        //                throw;
        //            }
        //        }

        //        fileImportLogManager.UpdateFileImportLog(fileImportLogId, FileImportLogStatus.Success);

        //        if (File.Exists(filePath))
        //        {
        //            // Encrypt and Archive
        //            Util.ZipEncryptFile(objTextWriter, filePath,
        //                ConfigInfo.FILE_SUCCESS_ARCHIVE_PATH + "Archive_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + fileName);
        //        }
        //        else
        //        {
        //            objTextWriter.WriteLine("File does not exist at {0}", filePath);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Code that runs when an unhandled error occurs
        //        Console.WriteLine(ex.Message);
        //        objTextWriter.WriteLine(ex.Message);

        //        Artefacts.Common.Logger.ArtefactsLogger logger = Artefacts.Common.Logger.ArtefactsLogger.getLogger();
        //        logger.logError(ex);
        //        logger.CloseLogger();

        //        fileImportLogManager.UpdateFileImportLog(fileImportLogId, FileImportLogStatus.Fail);

        //        // Archive input file
        //        if (File.Exists(filePath))
        //        {
        //            // Encrypt and Archive
        //            Util.ZipEncryptFile(objTextWriter, filePath,
        //                ConfigInfo.FILE_ERROR_ARCHIVE_PATH + "Archive_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(filePath));
        //        }
        //        else
        //        {
        //            objTextWriter.WriteLine("File does not exist at {0}", filePath);
        //        }

        //        return false;
        //    }

        //    return true;
        //}

        private ICollection<KeyValuePair<string, string>> GetColumnMappings()
        {
            // The source column names are in upper case as the data table is created with upper case column headers
            ICollection<KeyValuePair<string, string>> columnMappings = new List<KeyValuePair<string, string>>();
            columnMappings.Add(new KeyValuePair<string, string>("MID", "MID"));
            columnMappings.Add(new KeyValuePair<string, string>("MERCHANT", "DBAName"));
            columnMappings.Add(new KeyValuePair<string, string>("BATCH #", "BatchNumber"));
            columnMappings.Add(new KeyValuePair<string, string>("DATE", "DepositDate"));
            columnMappings.Add(new KeyValuePair<string, string>("SETTLED AMOUNT", "SettleAmount"));
            columnMappings.Add(new KeyValuePair<string, string>("TRAN FEE", "TranFees"));
            columnMappings.Add(new KeyValuePair<string, string>("RESERVE", "ReserveAmount"));
            columnMappings.Add(new KeyValuePair<string, string>("FUNDING AMOUNT", "FundingAmount"));
            columnMappings.Add(new KeyValuePair<string, string>("CARD ACCOUNT NUMBER", "AccountNumber"));
            columnMappings.Add(new KeyValuePair<string, string>("AUTH AMOUNT", "AuthAmount"));
            columnMappings.Add(new KeyValuePair<string, string>("AUTH CODE", "AuthCode"));
            columnMappings.Add(new KeyValuePair<string, string>("TERMINAL NUMBER", "TerminalNumber"));
            columnMappings.Add(new KeyValuePair<string, string>("STATUS", "DepositStatus"));
            columnMappings.Add(new KeyValuePair<string, string>("TRANSACTION DATE/TIME", "TransactionDate"));

            //// Additional Columns that are present in Source only
            //// TODO: Following two date columns to be mapped once the date format is finalized
            ////columnMappings.Add(new KeyValuePair<string, string>("PROCESS/BUSINESS DATE", "ProcessDate"));
            ////columnMappings.Add(new KeyValuePair<string, string>("TRANSACTION DATE/TIME", "TransactionDate"));
            //columnMappings.Add(new KeyValuePair<string, string>("CARD TYPE", "CardType"));
            //columnMappings.Add(new KeyValuePair<string, string>("AVS RESPONSE CODE", "AVSResponseCode"));
            //columnMappings.Add(new KeyValuePair<string, string>("STORE NUMBER", "StoreNumber"));
            //columnMappings.Add(new KeyValuePair<string, string>("MERCHANT NUMBER", "MerchantNumber"));
            //columnMappings.Add(new KeyValuePair<string, string>("MERCHANT NAME", "MerchantName"));
            //columnMappings.Add(new KeyValuePair<string, string>("ENTRY MODE", "EntryMode"));
            //columnMappings.Add(new KeyValuePair<string, string>("REFERENCE NUMBER", "ReferenceNumber"));
            //columnMappings.Add(new KeyValuePair<string, string>("TRANSACTION CODE", "TransactionCode"));
            //columnMappings.Add(new KeyValuePair<string, string>("CHAIN CODE", "ChainCode"));
            //columnMappings.Add(new KeyValuePair<string, string>("BATCH NUMBER", "BatchNumber2"));
            //columnMappings.Add(new KeyValuePair<string, string>("CARD EMV", "CardEMV"));
            //columnMappings.Add(new KeyValuePair<string, string>("CVV2 PRESENCE INDICATOR", "CVV2PresenceIndicator"));
            //columnMappings.Add(new KeyValuePair<string, string>("CVV2 RESPONSE CODE", "CVV2ResponseCode"));
            //columnMappings.Add(new KeyValuePair<string, string>("EMV TRANSACTION", "EMVTransaction"));
            //columnMappings.Add(new KeyValuePair<string, string>("MCC CODE", "MCCCode"));
            //columnMappings.Add(new KeyValuePair<string, string>("NUM REAUTH ATMPT", "NumReAuthAttempt"));
            //columnMappings.Add(new KeyValuePair<string, string>("OFFLINE EMV", "OfflineEMV"));
            //columnMappings.Add(new KeyValuePair<string, string>("REGISTER NUMBER", "RegisterNumber"));
            //columnMappings.Add(new KeyValuePair<string, string>("SEQUENCE NUMBER", "SequenceNumber"));
            //columnMappings.Add(new KeyValuePair<string, string>("TOKEN", "Token"));
            //columnMappings.Add(new KeyValuePair<string, string>("TOKEN ID", "TokenID"));
            //columnMappings.Add(new KeyValuePair<string, string>("TRAN ID", "TranID"));
            //columnMappings.Add(new KeyValuePair<string, string>("PIN-LESS", "PinLess"));
            //columnMappings.Add(new KeyValuePair<string, string>("RESPONSE/DENIAL CODE", "ResponseDenialCode"));
            //columnMappings.Add(new KeyValuePair<string, string>("TRANSACTION TYPE", "TransactionType"));
            //columnMappings.Add(new KeyValuePair<string, string>("CUSTOMER FIELD 1", "CustomerField1"));
            //columnMappings.Add(new KeyValuePair<string, string>("CUSTOMER FIELD 2", "CustomerField2"));
            //// TODO: Amount column to be mapped once the null values are handled
            ////columnMappings.Add(new KeyValuePair<string, string>("RESERVEBASE", "ReserveBaseAmount"));
            ////columnMappings.Add(new KeyValuePair<string, string>("INTERCHANGE", "InterchangeAmount"));
            ////columnMappings.Add(new KeyValuePair<string, string>("SURCHARGE", "SurchargeAmount"));

            // Audit Columns
            columnMappings.Add(new KeyValuePair<string, string>("FILEIMPORTLOGID", "FileImportLogId"));
            columnMappings.Add(new KeyValuePair<string, string>("CREATIONDATE", "CreationDate"));
            columnMappings.Add(new KeyValuePair<string, string>("PROCESSORID", "ProcessorId"));
            columnMappings.Add(new KeyValuePair<string, string>("CLIENTID", "ClientId"));
            columnMappings.Add(new KeyValuePair<string, string>("ISDATAMOVED", "IsDataMoved"));

            return columnMappings;
        }
    }
}
