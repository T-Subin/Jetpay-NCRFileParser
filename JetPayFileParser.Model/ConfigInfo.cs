using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetPayFileParser.Model
{
    public class ConfigInfo
    {
        public static string MAIN_DB_CONNECTION_STRING { get; set; }
        public static string DB_CONNECTION_STRING;
        public static int BATCH_SIZE = 5000;
        public static string PASS_PHRASE = "_@13prfdd";
        public static string SALT_VALUE = "_98*Vafdd";
        public static string INIT_VECTORP = "0e5F3gfdd";
        public static string SMTP_SERVER = "smtp.gmail.com";
        public static int SMTP_PORT = 587;
        public static bool USE_SECURE_SMTP = true;
        public static string MAIL_FROM_ADDRESS = "no-reply@artefactsys.com";
        //public static string MAIL_FROM_USERNAME = "no-reply@artefactsys.com";
        //public static string MAIL_FROM_PASSWORD = "Qirt4i7AP2u2";
        public static string PARSED_FILE_MAIL_TEMPLATE_NAME = "ParsedFileMailTemplate.html";
        public static string Success_Mail_To = "ajith.m@artefactsys.com";

        public static string MAIL_FROM_USERNAME { get; set; }
        public static string MAIL_FROM_PASSWORD { get; set; }

        public static int AppId { get; set; }
        public static int ProcessorId { get; set; }

        public static int CLIENT_ID { get; set; }
        public static string INPUT_FOLDER_PATH { get; set; }
        public static string FILE_SUCCESS_ARCHIVE_PATH { get; set; }
        public static string FILE_ERROR_ARCHIVE_PATH { get; set; }
        public static string FILE_MOVE_FOLDER_PATH { get; set; }

        public static string PARTIAL_EMAF_NAME { get; set; }
        public static string PARTIAL_DEPOSIT_NAME { get; set; }
        public static string PROCESSORAUTH_NAME { get; set; }
        public static string CHARGEBACK_NAME { get ;set ;}
        public static string SETTLED_NAME { get; set; }
        public static string FUNDINGDEP_NAME { get; set; }
        public static string ACH_NAME { get; set; }
        public static string QUALIFICATION { get; set; }
        public static string DEPOSIT_FEE { get; set; }
        public static string PARSER_FAILURE_EMAIL_TEMPLATE { get; set; }
        public static string PARSER_FAILURE_EMAILID { get; set; }
        public static string PARSER_FAILURE_EMAILID_BCC { get; set; }
        public static string ROOT_FOLDER_PATH { get; set; }
        public static string RISK_TRANSACTIONS { get; set; }
        public static string ADJ_RESERVES { get; set; }
        public static string MERCHANT { get; set; }
        public static string FUNDING_FULL { get; set; }
        public static string FUNDING_CATEGORY { get; set; }
        public static string FUNDING_DETAIL { get; set; }
        public static string SETTLEMENT_DETAIL { get; set; }
        public static string RESERVES { get; set; }
        public static string BULK_COPY_TIMEOUT { get; set; }
        //public static string MAIL_FROM_USER { get; set; }
        //public static string MAIL_FROM_PWD { get; set; }

        //public static string PARTIAL_ACQ_AUTH_NAME { get; set; }
        //public static string PARTIAL_DRAFT_256_NAME { get; set; }
        //public static string PARTIAL_CHARGE_BACK_NAME { get; set; }
        //public static string PARTIAL_REJECT_DOWNLOAD_NAME { get; set; }
        //public static string PATIAL_RESERVE_FUNDING_SETTINGS_NAME { get; set; }
        //public static string PARTIAL_ACH_310_NAME { get; set; }
        //public static string PARTIAL_MERCHANT_DETAIL_NAME { get; set; }
        //public static string PARTIAL_TSYS_EXPRESS_MERCHANT_DETAIL_NAME { get; set; }
        //public static string PARTIAL_WORKED_CHARGE_BACK_NAME { get; set; }
        //public static string PARTIAL_RESIDUAL_MANAGER_NAME { get; set; }
        //public static string PARTIAL_RESERVE_FUNDING_ACTIVITY_NAME { get; set; }
        //public static string PARTIAL_ACH_RETURN_NAME { get; set; }
        //public static string PARTIAL_STATEMENT_NAME { get; set; }
        //public static string PARTIAL_ADF_AUTH_NAME { get; set; }
        //public static string ACH310_ARCHIVE_PATH { get; set; }
        //public static string WORKED_CHARGEBACK_ARCHIVE_PATH { get; set; }
        //public static string PARTIAL_ACH_DETAIL_NAME { get; set; }
        //public static string MERCHANT_DETAIL_ARCHIVE_PATH { get; set; }
        //public static string RESERVE_FUNDING_SETTINGS_ARCHIVE_PATH { get; set; }
        //public static string RESERVE_FUNDING_ACTIVITY_ARCHIVE_PATH { get; set; }
        //public static string AUTH_MID_PREFIX { get; set; }
        //public static string PARTIAL_CALPIAN_STATEMENT_NAME { get; set; }
        //public static string FILE_MONTH { get; set; }

        public static void ResetClientConfigSettings()
        {
            ConfigInfo.DB_CONNECTION_STRING = string.Empty;
            ConfigInfo.FILE_ERROR_ARCHIVE_PATH = string.Empty;
            ConfigInfo.FILE_SUCCESS_ARCHIVE_PATH = string.Empty;
            ConfigInfo.INPUT_FOLDER_PATH = string.Empty;

            //ConfigInfo.PARTIAL_ACQ_AUTH_NAME = string.Empty;
            //ConfigInfo.PARTIAL_DRAFT_256_NAME = string.Empty;
            //ConfigInfo.PARTIAL_CHARGE_BACK_NAME = string.Empty;
            //ConfigInfo.PARTIAL_REJECT_DOWNLOAD_NAME = string.Empty;
            //ConfigInfo.PATIAL_RESERVE_FUNDING_SETTINGS_NAME = string.Empty;
            //ConfigInfo.PARTIAL_ACH_310_NAME = string.Empty;
            //ConfigInfo.PARTIAL_MERCHANT_DETAIL_NAME = string.Empty;
            //ConfigInfo.PARTIAL_WORKED_CHARGE_BACK_NAME = string.Empty;
            //ConfigInfo.PARTIAL_RESIDUAL_MANAGER_NAME = string.Empty;
            //ConfigInfo.PARTIAL_RESERVE_FUNDING_ACTIVITY_NAME = string.Empty;
            //ConfigInfo.PARTIAL_ACH_RETURN_NAME = string.Empty;
            //ConfigInfo.PARTIAL_STATEMENT_NAME = string.Empty;
            //ConfigInfo.ACH310_ARCHIVE_PATH = string.Empty;
            //ConfigInfo.WORKED_CHARGEBACK_ARCHIVE_PATH = string.Empty;
            //ConfigInfo.PARTIAL_ACH_DETAIL_NAME = string.Empty;
            //ConfigInfo.MERCHANT_DETAIL_ARCHIVE_PATH = string.Empty;
            //ConfigInfo.RESERVE_FUNDING_SETTINGS_ARCHIVE_PATH = string.Empty;
            //ConfigInfo.RESERVE_FUNDING_ACTIVITY_ARCHIVE_PATH = string.Empty;
            //ConfigInfo.AUTH_MID_PREFIX = string.Empty;
            //ConfigInfo.PARTIAL_CALPIAN_STATEMENT_NAME = string.Empty;
        }
    }
}
