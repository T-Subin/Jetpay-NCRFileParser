
namespace JetPayFileParser.Model.Type
{
    public static class RecordType
    {
        // FILE HEADER RECORD
        public const string CreditFileHeader = "010";
        public const string DebitFileHeader = "020";
        public const string EBTFileHeader = "030";
        public const string GiftCardFileHeader = "040";
        public const string POSFileHeader = "050";
        public const string WICFileHeader = "060";

        // FILE TRAILER RECORD
        public const string CreditFileTrailer = "910";
        public const string DebitFileTrailer = "920";
        public const string EBTFileTrailer = "930";
        public const string GiftCardFileTrailer = "940";
        public const string POSFileTrailer = "950";
        public const string WICFileTrailer = "960";

        // MERCHANT ACCOUNT NUMBER (MID) /BATCH HEADER RECORDS
        public const string MerchantBatchHeader = "070";
        public const string MerchantBatchHeader2 = "071";
        public const string MerchantBatchHeader3 = "072";

        // MERCHANT ACCOUNT NUMBER (MID) /BATCH TRAILER RECORD
        public const string MerchantBatchTrailer = "970";

        // CREDIT AUTHORIZATION DETAIL TRANSACTION REPORTING RECORDS
        public const string CreditAuth = "100";
        public const string CreditAuth2 = "101";

        // DEBIT AUTHORIZATION DETAIL TRANSACTION REPORTING RECORDS
        public const string DebitAuth = "120";
        public const string DebitAuth2 = "121";

        // EBTT AUTHORIZATION DETAIL TRANSACTION REPORTING RECORDS
        public const string EBTAuth = "130";
        public const string EBTAuth2 = "131";

        // GIFT CARD AUTHORIZATION DETAIL TRANSACTION REPORTING RECORDS
        public const string GiftCardAuth = "140";
        public const string GiftCardAuth2 = "141";

        // POS CHECK SERVICES AUTHORIZATION DETAIL TRANSACTION REPORTING RECORDS
        public const string POSCheckAuth = "150";
        public const string POSCheckAuth2 = "151";

        // WIC AUTHORIZATION DETAIL TRANSACTION REPORTING RECORDS
        public const string WICAuth = "160";
        public const string WICAuth2 = "161";

        // CREDIT RECONCILIATION DETAIL TRANSACTION REPORTING RECORDS
        public const string CreditSettle = "300";
        public const string CreditSettle2 = "301";
        public const string CreditSettle3 = "302";
        public const string CreditSettle4 = "303";
        public const string CreditSettle7 = "306";

        // DEBIT RECONCILIATION DETAIL TRANSACTION REPORTING RECORDS
        public const string DebitSettle = "320";
        public const string DebitSettle2 = "321";

    }
}
