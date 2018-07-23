using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Models.Dto
{
    public class SmokeSessionDTO
    {
    }

    public class ValidationDTO : DTO
    {
        public String Id { get; set; }
        public SessionState Flag { get; set; }
    }

    public enum SessionState{ Live, Completed }
}