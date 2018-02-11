using System;

namespace smartHookah.Models
{
    public class Puf
    {
        public Puf(string smokeSessionId,PufType s) : this(smokeSessionId,s,DateTime.Now.ToLocalTime())
        {
          
        }

        public Puf()
        { }


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
        public string SmokeSessionId { get; set; }

        public PufType Type { get; set; }

        public DateTime DateTime { get; set; }

        public long Milis { get; set; }

        public int Presure { get; set; } = 0;
    }

    public enum PufType
    {
        In = 1, Out = 2, Idle = 0
    }
}