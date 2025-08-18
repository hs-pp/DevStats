namespace DevStats.Editor
{
    public struct Heartbeat
    {
        public string File;
        public float Timestamp;
        public bool IsWrite; // Basically "IsSaved"
        public string Category;

        public override string ToString()
        {
            return @$"[Heartbeat] {File}
Timestamp: {Timestamp}
IsWrite: {IsWrite}
Category: {Category}";
        }
    }
}