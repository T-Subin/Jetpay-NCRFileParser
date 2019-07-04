using System;
using System.Data;
using System.Data.SqlClient;
using JetPayFileParser.Model;
using JetPayFileParser.Model.Enum;

namespace JetPayFileParser.DataAccess
{
    public class FileImportLogDAL
    {
        #region Parameters
        private const string PARAM_NAME = "@Name";
        private const string PARAM_TYPE = "@Type";
        private const string PARAM_ID = "@Id";
        private const string PARAM_STATUS = "@Status";
        private const string PARAM_ORIGINAL_CREATION_DATE = "@OriginalCreationDate";
        private const string PARAM_PROCESSOR_ID = "@ProcessorId";
        #endregion

        #region SPs
        private const string SP_INSERT_DATA_TO_FILE_IMPORT_LOG = "InsertDataToFileImportLog";
        private const string SP_UPDATE_FILE_IMPORT_LOG_STATUS_TO_SUCCESS = "UpdateFileImportLog";
        private const string SP_IS_FILE_ALREADY_IMPORTED = "IsFileAlreadyImported";
        #endregion

        public long InsertDataToFileImportLog(string name, FileType type, DateTime originalCreationDate)
        {
            SqlParameter[] parms = GetInsertDataToFileImportLogParams();
            parms[0].Value = name;
            parms[1].Value = (int)type;
            parms[2].Value = originalCreationDate;
            parms[3].Value = ConfigInfo.ProcessorId;
            return Convert.ToInt64(SqlHelper.ExecuteScalar(ConfigInfo.DB_CONNECTION_STRING, CommandType.StoredProcedure, SP_INSERT_DATA_TO_FILE_IMPORT_LOG, parms));
        }

        public void UpdateFileImportLog(long fileImportLogId, FileImportLogStatus status)
        {
            SqlParameter[] parm = GetUpdateFileImportLogParam();
            parm[0].Value = fileImportLogId;
            parm[1].Value = (int)status;

            SqlHelper.ExecuteNonQuery(ConfigInfo.DB_CONNECTION_STRING, CommandType.StoredProcedure, SP_UPDATE_FILE_IMPORT_LOG_STATUS_TO_SUCCESS, parm);
        }

        public bool IsFileAlreadyImported(string fileName)
        {
            SqlParameter[] parm = GetIsDraft256FileAlreadyImportedParam();
            parm[0].Value = fileName;
            return Convert.ToBoolean(SqlHelper.ExecuteScalar(ConfigInfo.DB_CONNECTION_STRING, CommandType.StoredProcedure, SP_IS_FILE_ALREADY_IMPORTED, parm));
        }

        private SqlParameter[] GetInsertDataToFileImportLogParams()
        {
            SqlParameter[] parms ={
                                    new SqlParameter(PARAM_NAME,SqlDbType.VarChar),
                                    new SqlParameter(PARAM_TYPE,SqlDbType.Int),
                                    new SqlParameter(PARAM_ORIGINAL_CREATION_DATE,SqlDbType.DateTime),
                                    new SqlParameter(PARAM_PROCESSOR_ID,SqlDbType.Int)
                                  };
            return parms;

        }

        private SqlParameter[] GetUpdateFileImportLogParam()
        {
            SqlParameter[] parm ={
                                    new SqlParameter(PARAM_ID,SqlDbType.BigInt),
                                    new SqlParameter(PARAM_STATUS,SqlDbType.TinyInt)
                                 };
            return parm;
        }

        private SqlParameter[] GetIsDraft256FileAlreadyImportedParam()
        {
            SqlParameter[] parm ={
                                    new SqlParameter(PARAM_NAME,SqlDbType.VarChar)
                                  };
            return parm;
        }
    }
}
