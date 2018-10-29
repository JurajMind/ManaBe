using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Models.Db
{
    public class HookahPersonSetting
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Defaut { get; set; }

        public int PersonId { get; set; }
        public virtual Person Person { get; set; }

        public int SettingId { get; set; }

        public virtual HookahSetting Setting { get; set; }
    }
}