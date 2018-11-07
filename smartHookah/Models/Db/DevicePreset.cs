using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Models.Db
{
    using System.ComponentModel.DataAnnotations;

    public class DevicePreset
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int? PersonId { get; set; }

        public virtual Person Person { get; set; }

        public int DeviceSettingId { get; set; }

        public virtual DeviceSetting DeviceSetting { get; set; }
    }
}