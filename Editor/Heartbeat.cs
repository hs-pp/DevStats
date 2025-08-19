namespace DevStats.Editor
{
    public struct Heartbeat
    {
        public string File;
        public decimal Timestamp;
        public bool IsWrite; // Basically "IsSaved"
        public string Category;
    }
}