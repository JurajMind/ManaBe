using Microsoft.ApplicationInsights;using System;using System.Web.Mvc;namespace smartHookah.ErrorHandler{    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AiHandleErrorAttribute : HandleErrorAttribute    {        public override void OnException(ExceptionContext filterContext)        {            if (filterContext != null && filterContext.HttpContext != null && filterContext.Exception != null)            {
                //If customError is Off, then AI HTTPModule will report the exception
                if (filterContext.HttpContext.IsCustomErrorEnabled)                {
                    var ai = new TelemetryClient();                    ai.TrackException(filterContext.Exception);                }
            }            base.OnException(filterContext);        }    }}