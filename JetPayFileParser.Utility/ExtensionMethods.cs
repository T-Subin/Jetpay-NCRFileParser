using System;
using System.Data;
using System.Linq;

namespace JetPayFileParser.Utility
{
    public static class ExtensionMethods
    {
        static char[] array1 = { '{', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', '}', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', '{', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '}' };

        // To Integer conversions
        public static int NullSafeStringToInt(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
                return Convert.ToInt32(strData) / 100;
            else
                return 0;
        }
        public static int? NullSafeStringToInteger(this object obdata)
        {
            string strData = obdata.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
                return Convert.ToInt32(strData);
            else
                return null;
        }

        // To Decimal conversions
        public static decimal? NullSafeStringTo2DigitDecimal(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
                return Convert.ToDecimal(strData) / 100;
            else
                return null;
        }
        public static decimal? NullSafeStringToXDigitDecimal(this object obData, int noOfDecimals)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
                return Convert.ToDecimal(strData) / Convert.ToDecimal(Math.Pow(10, noOfDecimals));
            else
                return null;
        }
        public static decimal? NullSafeStringToDecimal(this object obData)
        {
            string strData = obData.ToString();
            if (!string.IsNullOrWhiteSpace(strData))
                return Convert.ToDecimal(strData);
            else
                return null;
        }
        public static Decimal? NullSafeStringNegativeAmountToDecimal(this object obData)
        {
            string completeStringData = obData.ToString();
            string strDataExceptLastCharacter = completeStringData.Substring(0, (completeStringData.Length - 1));
            char lastChar = Char.ToUpper(completeStringData.Last());

            if (!string.IsNullOrWhiteSpace(completeStringData))
            {
                if (array1.Contains(lastChar))
                    return (0 - ((string.Concat(strDataExceptLastCharacter, (Array.IndexOf(array1, lastChar) % 10))).NullSafeStringTo2DigitDecimal()));
                else
                    return completeStringData.NullSafeStringTo2DigitDecimal();
            }
            else
                return null;
        }

        // To Date conversions
        public static DateTime? NullSafeEightDigitStringToDateTime(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
                return new DateTime(Convert.ToInt32(strData.Substring(4, 4)), Convert.ToInt32(strData.Substring(0, 2)), Convert.ToInt32(strData.Substring(2, 2)));
            else
                return null;
        }
        public static DateTime? NullSafeYYYYMMDDStringToDateTime(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
            {
                if (strData.Length == 10)
                    return new DateTime(Convert.ToInt32(strData.Substring(0, 4)), Convert.ToInt32(strData.Substring(5, 2)), Convert.ToInt32(strData.Substring(8, 2)));
                else
                    return new DateTime(Convert.ToInt32(strData.Substring(0, 4)), Convert.ToInt32(strData.Substring(4, 2)), Convert.ToInt32(strData.Substring(6, 2)));
            }
            else
                return null;
        }
        public static DateTime? NullSafeCCYYMMDDStringToDateTime(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
                return new DateTime((Convert.ToInt32(strData.Substring(4, 2)) + 2000), Convert.ToInt32(strData.Substring(0, 2)), Convert.ToInt32(strData.Substring(2, 2)));
            else
                return null;
        }
        public static DateTime? NullSafeUTCDateFromStringToDateTime(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
                return new DateTime(1970, 1, 1).AddSeconds(Convert.ToDouble(strData));
            else
                return null;
        }
        public static DateTime? NullSafeStringToDateTime(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
                return Convert.ToDateTime(strData);
            else
                return null;

        }

        public static DateTime? NullSafeMMDDYYStringToDateTime(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
                return new DateTime((Convert.ToInt32(strData.Substring(4, 2)) + 2000), Convert.ToInt32(strData.Substring(0, 2)), Convert.ToInt32(strData.Substring(2, 2)));
            else
                return null;
        }
        public static DateTime? NullSafeMMDDYYYYStringToDateTime(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
                if (strData.Length == 8)
                {
                    // Format: MMDDYYYY
                    return new DateTime(Convert.ToInt32(strData.Substring(4, 4)), Convert.ToInt32(strData.Substring(0, 2)), Convert.ToInt32(strData.Substring(2, 2)));
                }
                else
                {
                    // Format: MM DD YYYY
                    return new DateTime(Convert.ToInt32(strData.Substring(6, 4)), Convert.ToInt32(strData.Substring(0, 2)), Convert.ToInt32(strData.Substring(3, 2)));
                }
            else
                return null;
        }
        public static DateTime? NullSafeYYYYMMDDHHMMSSMSStringToDateTime(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
                return new DateTime(Convert.ToInt32(strData.Substring(0, 4)), Convert.ToInt32(strData.Substring(5, 2)), Convert.ToInt32(strData.Substring(8, 2)), Convert.ToInt32(strData.Substring(11, 2)), Convert.ToInt32(strData.Substring(14, 2)), Convert.ToInt32(strData.Substring(17, 2)), Convert.ToInt32(strData.Substring(20, 6)));
            else
                return null;
        }
        public static DateTime? NullSafeYYYYMMDDHHMMSSStringToDateTime(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
                if (strData.Length == 14)
                {
                    // Format: YYYYMMDDHHmmss
                    return new DateTime(Convert.ToInt32(strData.Substring(0, 4)), Convert.ToInt32(strData.Substring(4, 2)), Convert.ToInt32(strData.Substring(6, 2)), Convert.ToInt32(strData.Substring(8, 2)), Convert.ToInt32(strData.Substring(10, 2)), Convert.ToInt32(strData.Substring(12, 2)));
                }
                else
                {
                    // Format: YYYY MM DD HH MM SS
                    return new DateTime(Convert.ToInt32(strData.Substring(0, 4)), Convert.ToInt32(strData.Substring(5, 2)), Convert.ToInt32(strData.Substring(8, 2)), Convert.ToInt32(strData.Substring(11, 2)), Convert.ToInt32(strData.Substring(14, 2)), Convert.ToInt32(strData.Substring(17, 2)));
                }

            else
                return null;
        }
        public static DateTime? NullSafeMMDDYYHHMMSSStringToDateTime(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
                return new DateTime(2000 + Convert.ToInt32(strData.Substring(4, 2)), Convert.ToInt32(strData.Substring(0, 2)), Convert.ToInt32(strData.Substring(2, 2)), Convert.ToInt32(strData.Substring(6, 2)), Convert.ToInt32(strData.Substring(8, 2)), Convert.ToInt32(strData.Substring(10, 2)));
            else
                return null;
        }
        public static DateTime? NullSafeMMDDYYYYHHMMSSStringToDateTime(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
            {
                // Format: MMDDYYYYHHmmss
                return new DateTime(Convert.ToInt32(strData.Substring(4, 4)), Convert.ToInt32(strData.Substring(0, 2)), Convert.ToInt32(strData.Substring(2, 2)), Convert.ToInt32(strData.Substring(8, 2)), Convert.ToInt32(strData.Substring(10, 2)), Convert.ToInt32(strData.Substring(12, 2)));
            }
            else
                return null;
        }
        public static DateTime? NullSafeMMDDYYYYHHMMStringToDateTime(this object obData)
        {
            string strData = obData.ToString();

            if (!string.IsNullOrWhiteSpace(strData))
            {
                // Format: MMDDYYYYHHmmss
                return new DateTime(Convert.ToInt32(strData.Substring(4, 4)), Convert.ToInt32(strData.Substring(0, 2)), Convert.ToInt32(strData.Substring(2, 2)), Convert.ToInt32(strData.Substring(8, 2)), Convert.ToInt32(strData.Substring(10, 2)), 0);
            }
            else
                return null;
        }

        // Others
        public static object GetDBNullIfValueIsNull(this object obData)
        {
            if (obData == null)
                return DBNull.Value;
            else
                return obData;
        }
        public static object NullSafeObjectWithoutCharacterOtherThanInteger(this object obData)
        {
            string strData = obData.ToString();
            string output = null;

            if (!string.IsNullOrWhiteSpace(strData))
            {
                output = strData.Replace(",", "").Replace("$", "").Replace("%", "");
                return output;
            }
            else
                return null;
        }


        /// <summary>
        /// To check if column exists in datarow
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static bool IsColumnExists(this DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) ? true : false;
        }

        public static bool IsNumericType(this object obj)
        {
            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsStringType(this object obj)
        {
            switch (Type.GetTypeCode(obj.GetType()))
            {
                case TypeCode.String:
                case TypeCode.Char:
                    return true;
                default:
                    return false;
            }
        }

        public static T? GetValue<T>(this DataRow row, string columnName) where T : struct
        {
            if (row.IsNull(columnName))
                return null;

            return row[columnName] as T?;
        }
        public static string GetText(this DataRow row, string columnName)
        {
            if (row.IsNull(columnName))
                return string.Empty;

            return row[columnName] as string ?? string.Empty;
        }
    }
}
