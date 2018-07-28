using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using smartHookahCommon;

namespace smartHookah.Models
{
    public class Hookah
    {

        public Hookah()
        {
            
        }

        public Hookah(Hookah modelHookah)
        {
            Version = modelHookah.Version;
            Type = modelHookah.Type;
            Setting = new HookahSetting(modelHookah.Setting);
            AutoSessionEndTime = modelHookah.AutoSessionEndTime;
            AutoSleep = modelHookah.AutoSleep;
            UpdateType = modelHookah.UpdateType;

        }
        public int Id { get; set; }

        public string Name { get; set; }

        [DefaultValue(0)]
        public int Version { get; set; }

        public bool Light
        {
            get
            {
                if (this.Type == StandType.SenzorOnly || this.Type == StandType.SenzorOnly_BT)
                {
                    return false;
                }
                return true;
            }
        }

        public bool Bt
        {
            get
            {
                if (this.Type == StandType.SenzorOnly_BT ||
                    this.Type == StandType.Ring32_BT ||
                    this.Type == StandType.Ring60_BT ||
                    this.Type == StandType.Ring8_BT)
                {
                    return true;
                }
                return false;
            }
        }

        public virtual  ICollection<SmokeSession> SmokeSessions { get; set; }

        [Index(IsUnique = true)]
        [MaxLength(128)]
        public string Code { get; set; }
  
        [DefaultValue(0)]
        public StandUsage Usage { get; set; }
        
        public virtual ICollection<Person> Owners { get; set; }

        public int? SettingId { get; set; }
        public virtual HookahSetting Setting { get; set; }

        public StandType Type { get; set; }

        public bool Offline { get; set; }

        public bool AutoSleep { get; set; }

        public int AutoSessionEndTime { get; set; }
 
        public UpdateType UpdateType { get; set; }


        public int? DefaultMetaDataId { get; set; }
        public virtual SmokeSessionMetaData DefaultMetaData { get; set; }

        [NotMapped]
        public string SessionCode
        {
            get { return RedisHelper.GetSmokeSessionId(this.Code); }
        }

        [NotMapped]
        public bool OnlineState { get; set; }


    }
}