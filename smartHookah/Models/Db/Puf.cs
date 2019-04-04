using System;
using Newtonsoft.Json;

namespace smartHookah.Models.Db
{
    public class Puf
    {
        public Puf(string smokeSessionId,PufType s) : this(smokeSessionId,s,DateTime.Now.ToLocalTime())
        {
          
        }

        public Puf()
        { }


        public Puf(DbPuf s)
        {
            DateTime = s.DateTime;
            Milis = s.Milis;
            Presure = s.Presure;
            Type = s.Type;
        }

        public Puf(string smokeSessionId, PufType s, DateTime pufTime)
        {
            DateTime = pufTime;
            Type = (PufType)s;
            SmokeSessionId = smokeSessionId;
        }

        public Puf(string smokeSessionId, PufType s, DateTime pufTime,long milis, int presure = 0) :this(smokeSessionId,s,pufTime)
        {
            Milis = milis;
            Presure = presure;
        }
        [JsonProperty(PropertyName = "SId")]
        public string SmokeSessionId { get; set; }

        [JsonProperty(PropertyName = "T")]
        public PufType Type { get; set; }

        [JsonProperty(PropertyName = "D")]
        public DateTime DateTime { get; set; }

        [JsonProperty(PropertyName = "M")]
        public long Milis { get; set; }

        [JsonProperty(PropertyName = "P")]
        public int Presure { get; set; } = 0;
    }

    public enum PufType
    {
        In = 1, Out = 2, Idle = 0
    }
}