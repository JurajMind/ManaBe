using System;

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

        public string Code { get; set; }

    }
}
