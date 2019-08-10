using System;
using System.Collections.Generic;

namespace smartHookahCommon.Exceptions
{
    public class ManaException : Exception
    {

        public ManaException()
        {

        }

        public ManaException(string code,string message) : base(message)
        {
            this.Code = code;
        }

        public ManaException(string code) : base(code)
        {
            this.Code = code;
        }

        public ManaException(string code, string message,Exception innerException) : base(message,innerException)
        {
            this.Code = code;
        }

        public ManaException(string code, string message, Dictionary<string,string> parameters) : base(message)
        {
            this.Code = code;
            parameters = Parameters;
        }


        public string Code { get; set; }

        public Dictionary<string, string> Parameters;

    }
}
