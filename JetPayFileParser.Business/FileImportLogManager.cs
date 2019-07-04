using System;
using JetPayFileParser.DataAccess;
using JetPayFileParser.Model.Enum;

namespace JetPayFileParser.Business
{
    public class FileImportLogManager
    {
        FileImportLogDAL obFileImportLogDAL = new FileImportLogDAL();
        /// <summary>
        /// Saves file related information to file import log table
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="Id"></param>
        public long InsertDataToFileImportLog(string name, FileType type, DateTime originalCreationDate)
        {
            return obFileImportLogDAL.InsertDataToFileImportLog(name, type, originalCreationDate);
        }
        public void UpdateFileImportLog(long fileImportLogId, FileImportLogStatus status)
        {
            obFileImportLogDAL.UpdateFileImportLog(fileImportLogId, status);
        }
        public bool IsFileAlreadyImported(string fileName)
        {
            return obFileImportLogDAL.IsFileAlreadyImported(fileName);
        }
    }
}
