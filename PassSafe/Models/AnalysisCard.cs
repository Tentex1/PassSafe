namespace PassSafe.Models
{
    /// <summary>
    /// Represents a summary card in the Password Analyzer screen (e.g., Strong, Weak, Risky totals).
    /// </summary>
    public class AnalysisCard
    {
        public string Title { get; set; }

        public int Count { get; set; }

        public string SideColor { get; set; }

        public string Description { get; set; }

        public string IconKey { get; set; }
    }
}