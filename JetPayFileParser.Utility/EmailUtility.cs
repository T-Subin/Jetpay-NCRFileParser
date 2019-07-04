
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Artefacts.Common.Logger;
using System.IO.Compression;
using System.Data;
using JetPayFileParser.Model;
using System.Configuration;

namespace JetPayFileParser.Utility
{
    public class EmailUtility
    {
        // GlobalConfigReader reader = new GlobalConfigReader();


        public static void SendMail(string _Subject, string _Body, string _MailTo, string _MailBcc, string attachmentFilePath = null)
        {
            System.Net.Mail.Attachment attachment = null;

            // GlobalConfigReader.LoadSettingFromXML();
            try
            {
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.From = new MailAddress(ConfigInfo.MAIL_FROM_USERNAME);
                if (!string.IsNullOrEmpty(_MailBcc.Trim()))
                    mail.Bcc.Add(_MailBcc);
                mail.Subject = _Subject;
                mail.IsBodyHtml = true;
                mail.Body = _Body; // _Body.Replace("REPLACE_LOGO_PATH", GlobalConfigReader._LogoPath);// ro replace the logo image here


                if (attachmentFilePath != null)
                {
                    attachment = new System.Net.Mail.Attachment(attachmentFilePath);
                    mail.Attachments.Add(attachment);
                }
                
                SmtpClient smtp = new SmtpClient(ConfigInfo.SMTP_SERVER) { UseDefaultCredentials = false };
                int smtpPort = Convert.ToInt32(ConfigInfo.SMTP_PORT);
                smtp.Timeout = 6000000;

                if (!string.IsNullOrEmpty(ConfigInfo.MAIL_FROM_PASSWORD))
                    smtp.Credentials = new System.Net.NetworkCredential(ConfigInfo.MAIL_FROM_USERNAME, ConfigInfo.MAIL_FROM_PASSWORD);

                smtp.EnableSsl = ConfigInfo.USE_SECURE_SMTP;
                smtp.Port = smtpPort;
                if (!String.IsNullOrEmpty(_MailTo.Trim()))
                {
                    mail.To.Add(_MailTo);
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                ArtefactsLogger logger = ArtefactsLogger.getLogger();
                logger.logError(ex);
                throw;
            }
            finally
            {
                if (attachment != null)
                    attachment.Dispose();
            }
        }
        public static string ConvertDataTableToHTML(DataTable dt)
        {
            string html = "<table style='font-family: arial, sans-serifborder-collapse: collapse;width: 100%;'>";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<th style='border: 1px solid #dddddd;text-align: left;padding: 8px;' >"
                    + dt.Columns[i].ColumnName + "</th>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i % 2 == 0)
                {
                    html += "<tr style='background-color: #dddddd'>";
                }
                else
                {
                    html += "<tr>";
                }
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    html += "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>" 
                        + dt.Rows[i][j].ToString() + "</td>";
                }
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }

        public static void SendMailOnParserFailure(List<string> lstErrors, Exception exception, string fileName)
        {
            string subject = "Error on Parsing";

            string emailTemplatePath = "";
            string mailBody = "";
            if (System.IO.File.Exists(ConfigInfo.PARSED_FILE_MAIL_TEMPLATE_NAME))
            {
                emailTemplatePath = ConfigInfo.PARSED_FILE_MAIL_TEMPLATE_NAME != null ? ConfigInfo.PARSED_FILE_MAIL_TEMPLATE_NAME : string.Empty;
                mailBody = Util.ReadFileData(emailTemplatePath);
            }
            if (mailBody != string.Empty)
            {
                if (lstErrors != null && lstErrors.Count > 0)
                {
                    mailBody = mailBody.Replace("@!Exception@!", string.Join("<br>", lstErrors));
                }
                else
                    mailBody = mailBody.Replace("@!Exception@!", exception.Message);

                mailBody = mailBody.Replace("@!FileName@!", fileName);
            }

            string mailTo = ConfigInfo.PARSER_FAILURE_EMAILID != null ? ConfigInfo.PARSER_FAILURE_EMAILID : string.Empty;
            string mailBCC = ConfigInfo.PARSER_FAILURE_EMAILID_BCC != null ? ConfigInfo.PARSER_FAILURE_EMAILID_BCC : string.Empty;

            try
            {
               //SendMail(subject, mailBody, mailTo, mailBCC, null);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
