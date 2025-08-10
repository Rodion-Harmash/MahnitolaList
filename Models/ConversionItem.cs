namespace MahnitolaList.Models
{
    public class ConversionItem
    {
        public string InputPath { get; set; } = string.Empty;
        public string InputName { get; set; } = string.Empty;

        public string OutputName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public int Progress { get; set; }
        public bool IsIndeterminate { get; set; }
        public string? Error { get; set; }
    }
}
