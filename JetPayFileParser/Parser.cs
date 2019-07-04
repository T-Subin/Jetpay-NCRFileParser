using System;
using System.IO;
using System.Configuration;
using JetPayFileParser.Business;
using JetPayFileParser.Model;
using JetPayFileParser.Model.Enum;
using JetPayFileParser.Utility;
using System.Data;

namespace JetPayFileParser
{
    public class Parser
    {
        static void Main(string[] args)
        {
            StreamWriter objTextWriter = null;
            FileImportLogManager obFileImportLogManager = new FileImportLogManager();

            try
            {
                // Load application settings
                ConfigInfo.AppId = Convert.ToInt32(ConfigurationManager.AppSettings["ApplicationId"]);
                ConfigInfo.ProcessorId = Convert.ToInt32(ConfigurationManager.AppSettings["ProcessorId"]);
                ConfigInfo.CLIENT_ID = Convert.ToInt32(ConfigurationManager.AppSettings["ClientId"]);
                ConfigInfo.INPUT_FOLDER_PATH = ConfigurationManager.AppSettings["InputFolderPath"];
                ConfigInfo.FILE_SUCCESS_ARCHIVE_PATH = ConfigurationManager.AppSettings["SuccessFolderPath"];
                ConfigInfo.FILE_ERROR_ARCHIVE_PATH = ConfigurationManager.AppSettings["ErrorFolderPath"];
                ConfigInfo.FILE_MOVE_FOLDER_PATH = ConfigurationManager.AppSettings["FileMoveFolder"];
                ConfigInfo.PROCESSORAUTH_NAME = ConfigurationManager.AppSettings["ProcessorAuthName"];
                ConfigInfo.CHARGEBACK_NAME = ConfigurationManager.AppSettings["Chargeback"];
                ConfigInfo.SETTLED_NAME = ConfigurationManager.AppSettings["Settle"];
                ConfigInfo.FUNDINGDEP_NAME = ConfigurationManager.AppSettings["FundingDep"];
                ConfigInfo.ACH_NAME = ConfigurationManager.AppSettings["ACH"];
                ConfigInfo.QUALIFICATION = ConfigurationManager.AppSettings["Qualification"];
                ConfigInfo.DEPOSIT_FEE = ConfigurationManager.AppSettings["DepositFee"];
                ConfigInfo.RISK_TRANSACTIONS = ConfigurationManager.AppSettings["RiskTransactions"];
                ConfigInfo.ADJ_RESERVES = ConfigurationManager.AppSettings["AdjReserves"];
                ConfigInfo.MERCHANT = ConfigurationManager.AppSettings["Merchant"];
                ConfigInfo.FUNDING_FULL = ConfigurationManager.AppSettings["FundingFull"];
                ConfigInfo.FUNDING_CATEGORY = ConfigurationManager.AppSettings["FundingCategory"];
                ConfigInfo.FUNDING_DETAIL = ConfigurationManager.AppSettings["FundingDetail"];
                ConfigInfo.SETTLEMENT_DETAIL = ConfigurationManager.AppSettings["SettlementDetail"];
                ConfigInfo.RESERVES = ConfigurationManager.AppSettings["Reserves"];

                ConfigInfo.DB_CONNECTION_STRING = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

                ConfigInfo.ROOT_FOLDER_PATH = ConfigurationManager.AppSettings["RootFolderPath"];
                ConfigInfo.PARSED_FILE_MAIL_TEMPLATE_NAME = ConfigInfo.ROOT_FOLDER_PATH + ConfigurationManager.AppSettings["ParserFailureEmailTemplate"];
                ConfigInfo.PARSER_FAILURE_EMAILID = ConfigurationManager.AppSettings["ParserFailureEmailId"];
                ConfigInfo.PARSER_FAILURE_EMAILID_BCC = ConfigurationManager.AppSettings["ParserFailureEmailIdBCC"];
                ConfigInfo.BULK_COPY_TIMEOUT = ConfigurationManager.AppSettings["BulkCopyTimeout"];
                ConfigInfo.MAIL_FROM_USERNAME = ConfigurationManager.AppSettings["MAIL_FROM_USERNAME"];
                ConfigInfo.MAIL_FROM_PASSWORD = ConfigurationManager.AppSettings["MAIL_FROM_PASSWORD"];

                // Initialize the Run Log
                string runLogFileName = "RunLog_" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".txt";
                string runLogDirectoryPath = AppDomain.CurrentDomain.BaseDirectory + "RunLog";
                string runLogFilePath = runLogDirectoryPath + "\\" + runLogFileName;

                // Create Directory and Log file if they do not exist
                Util.CheckExistenceOfLogDirectory(runLogDirectoryPath, runLogFilePath);
                objTextWriter = new StreamWriter(runLogFilePath, true);
                objTextWriter.AutoFlush = true;

                Console.WriteLine("Parsing started at: {0}", DateTime.Now);
                objTextWriter.WriteLine("-----------Parsing started at: {0}----------", DateTime.Now);
                objTextWriter.WriteLine();
                FileImportLogManager fileImportLogManager = new FileImportLogManager();
                if (Directory.Exists(ConfigInfo.INPUT_FOLDER_PATH))
                {
                    // Processor Auth Files (Realtime)
                    ParseFiles(ConfigInfo.PROCESSORAUTH_NAME, FileType.ACQAuth, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    // Chargeback file
                    ParseFiles(ConfigInfo.CHARGEBACK_NAME, FileType.ChargebackRetrieval, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    // Settle File
                    //ParseFiles(ConfigInfo.SETTLED_NAME, FileType.Draft256, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    // Funding-Deposit File
                    //ParseFiles(ConfigInfo.FUNDINGDEP_NAME, FileType.ACHDetail, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    // ACH Auth File
                    ParseFiles(ConfigInfo.ACH_NAME, FileType.ACHAuth, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    //Qualification report
                    ParseFiles(ConfigInfo.QUALIFICATION, FileType.QUALIFICATION, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    //Deposit Fee report
                    //ParseFiles(ConfigInfo.DEPOSIT_FEE, FileType.DEPOSIT_FEE, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    //Risk History Report
                    ParseFiles(ConfigInfo.RISK_TRANSACTIONS, FileType.RiskTransactions, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    //Adj reserves Report
                    //ParseFiles(ConfigInfo.ADJ_RESERVES, FileType.AdjustmentReserves, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    //Merchant file.
                    ParseFiles(ConfigInfo.MERCHANT, FileType.MerchantFile, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    //Full Funding file.
                    //ParseFiles(ConfigInfo.FUNDING_FULL, FileType.FundingFull, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    //Funding Category file.
                    ParseFiles(ConfigInfo.FUNDING_CATEGORY, FileType.FundingCategory, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    //Funding Detail file.
                    ParseFiles(ConfigInfo.FUNDING_DETAIL, FileType.FundingDetail, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    //Settlement Detail file.
                    ParseFiles(ConfigInfo.SETTLEMENT_DETAIL, FileType.SettlementDetail, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);

                    //Reserves file.
                    ParseFiles(ConfigInfo.RESERVES, FileType.Reserves, fileImportLogManager, ConfigInfo.CLIENT_ID, objTextWriter, ConfigInfo.INPUT_FOLDER_PATH, ConfigInfo.FILE_MOVE_FOLDER_PATH);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Artefacts.Common.Logger.ArtefactsLogger logger = Artefacts.Common.Logger.ArtefactsLogger.getLogger();
                logger.logError(ex);
                logger.CloseLogger();
            }
            Console.WriteLine("Parsing ended at: {0}", DateTime.Now);
            objTextWriter.WriteLine("-----------Parsing ended at: {0}----------", DateTime.Now);

        }

        private static void ParseFiles(string partFileName, FileType fileType, FileImportLogManager fileImportLogManager, int clientId, StreamWriter objTextWriter, string InputPath, string FileMovePath)
        {
            if (!string.IsNullOrWhiteSpace(partFileName))
            {
                // Get list of Partial File Names from comma-separated config value
                string[] partialFileNames = partFileName.Split(',');

                foreach (string partialFileName in partialFileNames)
                {
                    // Get files from directory
                    string[] fileArray = Directory.GetFiles(ConfigInfo.INPUT_FOLDER_PATH, string.Concat('*', partialFileName.Trim(), '*'));

                    objTextWriter.WriteLine("Partial File Name: {0}; Count: {1}", partialFileName.Trim(), fileArray.Length);

                    foreach (string file in fileArray)
                    {
                        string fileName = Path.GetFileName(file).Trim();
                        string orgfile = fileName;
                        string MoveFileName = orgfile.Replace("-", "_");
                        string MoveFilePathWithName = FileMovePath + "\\" + MoveFileName;
                        File.Move(InputPath + "\\" + fileName, MoveFilePathWithName);

                        objTextWriter.WriteLine("-------File: {0} Moved at {1}--------", fileName, DateTime.Now);

                        objTextWriter.WriteLine("-------Processing of file {0} started at {1}--------", fileName, DateTime.Now);

                        // Check if file has been parsed already
                        if (!fileImportLogManager.IsFileAlreadyImported(MoveFileName))
                        {
                            if (fileType == FileType.ACQAuth)
                            {
                                ProcessorAuthManager authManager = new ProcessorAuthManager();
                                authManager.BulkInsertAuth(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            }
                            if (fileType == FileType.ChargebackRetrieval)
                            {
                                ChargebackManager cbManager = new ChargebackManager();
                                cbManager.BulkInsertChargeback(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            }
                            //if (fileType == FileType.Draft256)
                            //{
                            //    SettledManager settleManager = new SettledManager();
                            //    settleManager.BulkInsertSettle(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            //}
                            //if (fileType == FileType.ACHDetail)
                            //{
                            //    FundingDepManager funDepManager = new FundingDepManager();
                            //    funDepManager.BulkInsertFundingDep(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            //}
                            if (fileType == FileType.ACHAuth)
                            {
                                ACHAuthManager authManager = new ACHAuthManager();
                                authManager.BulkInsertAuth(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            }
                            if (fileType == FileType.QUALIFICATION)
                            {
                                QualificationManager qualificationManager = new QualificationManager();
                                qualificationManager.BulkInsertAuth(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            }
                            //if (fileType == FileType.DEPOSIT_FEE)
                            //{
                            //    DepositFeeManager feeManager = new DepositFeeManager();
                            //    feeManager.BulkInsertAuth(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            //}
                            if (fileType == FileType.RiskTransactions)
                            {
                                RiskHistoryManager riskHistoryManager = new RiskHistoryManager();
                                riskHistoryManager.BulkInsertAuth(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            }
                            //if (fileType == FileType.AdjustmentReserves)
                            //{
                            //    AdjustmentReservesManager adjustmentReservesManager = new AdjustmentReservesManager();
                            //    adjustmentReservesManager.BulkInsertAuth(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            //}
                            if (fileType == FileType.MerchantFile)
                            {
                                MerchantFileManager merchantFileManager = new MerchantFileManager();
                                merchantFileManager.BulkInsertAuth(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            }
                            //if (fileType == FileType.FundingFull)
                            //{
                            //    FullFundingManager fullFundingManager = new FullFundingManager();
                            //    fullFundingManager.BulkInsertAuth(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            //}
                            if (fileType == FileType.FundingCategory)
                            {
                                FundingCategoryManager fundingCategoryManager = new FundingCategoryManager();
                                fundingCategoryManager.BulkInsertAuth(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            }
                            if (fileType == FileType.FundingDetail)
                            {
                                FundingDetailManager fundingDetailManager = new FundingDetailManager();
                                fundingDetailManager.BulkInsertAuth(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            }
                            if (fileType == FileType.SettlementDetail)
                            {
                                SettlementDetailManager settlementDetailManager = new SettlementDetailManager();
                                settlementDetailManager.BulkInsertAuth(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            }
                            if (fileType == FileType.Reserves)
                            {
                                ReservesManager reservesManager = new ReservesManager();
                                reservesManager.BulkInsertAuth(clientId, MoveFileName, objTextWriter, InputPath, MoveFilePathWithName, FileMovePath);
                            }
                        }
                        else
                        {
                            objTextWriter.WriteLine("File {0} is already imported", MoveFileName);

                            // Encrypt and Archive
                            Util.ZipEncryptFile(objTextWriter, MoveFilePathWithName,
                                ConfigInfo.FILE_SUCCESS_ARCHIVE_PATH + "Archive_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + MoveFileName);
                        }

                        objTextWriter.WriteLine("-------Processing of file ended at {0}--------", DateTime.Now);
                        objTextWriter.WriteLine();
                    }
                }
            }
        }
        private static void SendEmail(DataTable dtParseInfo)
        {
            string _Subject = "Files parsed";
            string _MailTo = ConfigInfo.Success_Mail_To;
            string _MailBcc = string.Empty;

            //string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"EmailTemplates\ParsedFileMailTemplate.html");
            string path = @"C:\Artefacts GIT\JetPay\ao\Parsers\JetPayFileParser\JetPayFileParser\EmailTemplates\ParsedFileMailTemplate.html";
            string _Body = Util.ReadFileData(path);
            _Body = _Body.Replace("ParseInfo", EmailUtility.ConvertDataTableToHTML(dtParseInfo));
            EmailUtility.SendMail(_Subject, _Body, _MailTo, _MailBcc, null);
        }
    }
}
