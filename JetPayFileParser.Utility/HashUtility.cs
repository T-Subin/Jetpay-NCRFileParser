using System;
using System.Security.Cryptography;

namespace JetPayFileParser.Utility
{
    public class HashUtility
    {

        public static string HashIt(string strValue)
        {

            Byte[] dataToHash = new System.Text.UnicodeEncoding().GetBytes(strValue);

            byte[] hashValue = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(dataToHash);

            return BitConverter.ToString(hashValue);

        }


    }
}
