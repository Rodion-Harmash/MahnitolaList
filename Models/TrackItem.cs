namespace MahnitolaList.Models
{
    public class TrackItem
    {
        public string FileName { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }

        public TrackItem(string fileName, int durationSeconds)
        {
            FileName = fileName;
            DurationSeconds = durationSeconds;
        }
    }
}
