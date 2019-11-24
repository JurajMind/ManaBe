using System.Collections.Generic;

namespace smartHookah.Models.Db
{
    public class Tag
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public virtual ICollection<SmokeSessionMetaData> Used { get; set; }

    }
}