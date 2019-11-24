using System.ComponentModel.DataAnnotations.Schema;

namespace smartHookah.Models.Db
{
    using System.ComponentModel.DataAnnotations;

    public class DevicePreset
    {
        public string Name { get; set; }

        public int? PersonId { get; set; }

        public virtual Person Person { get; set; }

        [Key]
        [ForeignKey("DeviceSetting")]
        public int Id { get; set; }

        public virtual DeviceSetting DeviceSetting { get; set; }
    }
}