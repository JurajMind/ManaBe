using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace smartHookah.Models
{
    public class Update
    {
       public int Id { get; set; }
       public int Version { get; set; }

       public DateTime ReleseDate { get; set; }

       public string ReleseNote { get; set; }

       public UpdateType Type { get; set; }

       public string Path { get; set; }


    }


    public enum UpdateType
    {
        Beta  = 1,
        Alfa = 2,
        Stable = 0,
        Devel = 3,
    }

    public enum StandType
    {
        Ring32 = 0,
        Ring60 = 1,
        SenzorOnly = 2,
        Ring8 = 3,
        Ring32_BT = 4,
        Ring60_BT = 5,
        SenzorOnly_BT = 6,
        Ring8_BT = 7,
        HydrogenArgon = 8,
        Emulator = 9
    }


    public class UpdateLog
    {
        public int Id { get; set; }

        public int HookahId { get; set; }
        public virtual Hookah Hookah { get; set; }

        public int OldVersion { get; set; }
        
        public int NewVersionId { get; set; }
        public virtual Update NewVersion { get; set; }

        public DateTime UpdateTime { get; set; }
       
    }
}