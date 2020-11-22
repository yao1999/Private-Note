using Microsoft.AspNetCore.Mvc.Filters;
using Private_Note.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Buffers;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Private_Note.Data;
using Private_Note.Models;

namespace Private_Note.Common
{
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class IsAdminAuthAttribute : ActionFilterAttribute
    {
        public bool IsAdmin { get; set; }
        //public override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    bool isAdmin = IsAdmin;
        //    //if (string.IsNullOrEmpty(name))
        //    //    name = filterContext.Controller.GetType().Name;
                 

        //    filterContext.Controller.ViewData["IsAdminAuth"] = IsAdmin;
        //    base.OnActionExecuting(filterContext);
        //}
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Controller controller = filterContext.Controller as Controller;

            if (controller != null)
            {
                //getting a service
                //IMyService myService = controller.HttpContext.RequestServices.GetService(typeof(IMyService)) as IMyService;
                var isAdmin = controller.Equals(filterContext);
                //injecting values in the ViewData
                controller.ViewData["IsAdminAuth"] = isAdmin;
                base.OnActionExecuting(filterContext);
            }
        }
    }
}
