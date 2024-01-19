using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SMSS.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SMSS.Controllers
{
    public abstract class BaseController : Controller
    {
        public void Notify(string message, string title = "SMSS Message Alert", NotificationType notificationType = NotificationType.success)
        {
            var msg = new
            {
                message = message,
                title = title,
                icon = notificationType.ToString(),
                type = notificationType.ToString(),
                provider = GetProvider()
            };
            TempData["Message"] = JsonConvert.SerializeObject(msg);
        }

        public void AlertMessage(string message, NotificationType notificationType)
        {
            var msg = "swal.fire('" + notificationType.ToString() + "', '" + message + "', '" + notificationType + "')" + "";
            TempData["notification"] = msg;
        }

        private string GetProvider()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();

            var value = configuration["NotificationProvider"];

            return value;
        }

        //public static async Task<string> RenderViewToStringAsync(
        //    string viewName, 
        //    object model,
        //    ControllerContext controllerContext,
        //    bool isPartial = false
        //    )
        //{

        //    var actionContext = controllerContext as ActionContext;
        //    var serviceProvider = controllerContext.HttpContext.RequestServices;
        //    var razorViewEngine = serviceProvider.GetService(typeof(IRazorViewEngine)) as IRazorViewEngine;
        //    var tempDataProvider = serviceProvider.GetService(typeof(ITempDataProvider)) as ITempDataProvider;

        //    using (var sw = new StringWriter())
        //    {
        //        var viewResult = razorViewEngine.FindView(actionContext, viewName, !isPartial);

        //        if (viewResult?.View == null)
        //            throw new ArgumentException($"{viewName} does not match any available view");

        //        var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        //        { Model = model };

        //        var viewContext = new ViewContext(
        //            actionContext, 
        //            viewResult.View, 
        //            viewDictionary, 
        //            new TempDataDictionary(actionContext.HttpContext, tempDataProvider), 
        //            sw, 
        //            new HtmlHelperOptions());

        //        await viewResult.View.RenderAsync(viewContext);
        //        return sw.ToString();
        //    }
        //}

        

    }
}
