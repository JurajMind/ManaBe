using System.Collections.Generic;

namespace smartHookah.Models
{
    public class Tag
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public virtual ICollection<SmokeSessionMetaData> Used { get; set; }
        
    }
}