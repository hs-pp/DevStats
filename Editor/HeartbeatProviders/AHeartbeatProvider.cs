using System;

namespace DevStats.Editor
{
    public abstract class AHeartbeatProvider
    {
        public abstract void Initialize();
        public abstract void Deinitialize();
        public Action<Heartbeat> TriggerHeartbeat;

        protected void SendHeartbeat(string file, bool isSaveAction, string category = null)
        {
            TriggerHeartbeat?.Invoke(new Heartbeat()
            {
                File = file,
                Timestamp = (decimal)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000,
                IsWrite = isSaveAction,
                Category = category,
            });
        }
    }
}