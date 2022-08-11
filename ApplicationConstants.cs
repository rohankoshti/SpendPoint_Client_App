namespace SpendPoint
{
    public static class ApplicationConstants
    {
        public static GoogleSheetStatus GoogleSheetUploadStatus { get; set; }
        public const string googleSheetNulName = "Counts & Orders NUL.csv";
    }

    public enum GoogleSheetStatus
    {
        FailedCompletely = 0,
        WrittenToLocaalNULFile = 1,
        WrittenToCloud = 2
    }
}
