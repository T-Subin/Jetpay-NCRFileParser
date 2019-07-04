using JetPayFileParser.Model;

namespace JetPayFileParser.Utility
{
    public class IniVectorUtility
    {
        public static string GetIniVector()
        {
            string iniVector = "";

            string guid = System.Guid.NewGuid().ToString();

            guid = guid.Replace("-", "");

            iniVector = guid.Substring(0, 4) + guid.Substring(guid.Length - 4, 4);
            return iniVector;
        }
        public static string GetCombinedIni(string ini)
        {
            return string.Concat(ConfigInfo.INIT_VECTORP, ini);
        }
    }
}
