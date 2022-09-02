using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace MS_Base.Helpers
{

    public class ControllerLogAttribute : ActionFilterAttribute
    {
        private StringBuilder LogStrBuilder = new StringBuilder();
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            LogStrBuilder = new StringBuilder();
            LogStrBuilder.AppendLine();
            
            LogStrBuilder.AppendLine($"==================== {filterContext.RouteData.Values["controller"]}/{filterContext.RouteData.Values["action"]} ====================");
            LogStrBuilder.AppendLine($"Recebido: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")}");

            foreach (KeyValuePair<string, object> args in filterContext.ActionArguments)
            {
                LogStrBuilder.AppendLine($"{args.Key}:");
                LogStrBuilder.AppendLine(JsonConvert.SerializeObject(args.Value, Formatting.Indented));
            }
            //Log("OnActionExecuting", filterContext.RouteData, _strb.ToString());

        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //Verificar exception
            if (LogStrBuilder == null)
            {
                LogStrBuilder = new StringBuilder();
            }
            if (filterContext.Exception != null)
            {
                LogStrBuilder.AppendLine($"Exceção: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")}");
                LogStrBuilder.AppendLine(filterContext.Exception.ToString());

                filterContext.ExceptionHandled = true;
            }
            var objRetorno = (Microsoft.AspNetCore.Mvc.ObjectResult)filterContext.Result;
            if(objRetorno != null && objRetorno.Value != null)
            {
                LogStrBuilder.AppendLine($"Retorno: {JsonConvert.SerializeObject(objRetorno.Value, Formatting.Indented)}");
            }
            else
            {
                LogStrBuilder.AppendLine("Retorno: void or null");
            }
            //Logger.LogPresentation(request.Cliente_ID, request.chrSerial, strL.ToString(), "DescargaController", "PostGetDescarga");
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (LogStrBuilder == null)
            {
                LogStrBuilder = new StringBuilder();
            }
            Log("OnResultExecuting", filterContext.RouteData, LogStrBuilder.ToString());
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            
            Log("OnResultExecuted", filterContext.RouteData, "");
        }


        private void Log(string methodName, RouteData routeData, string strLog)
        {
            var controllerName = routeData.Values["controller"];
            var actionName = routeData.Values["action"];
            Console.WriteLine($"{methodName} controller:{controllerName} action:{actionName}{'\n'}  {strLog}");
        }
    }

}

