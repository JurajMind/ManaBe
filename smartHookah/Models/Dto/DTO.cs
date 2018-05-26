using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Models.Dto
{
    public class DTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int HttpResponseCode { get; set; }
    }
}