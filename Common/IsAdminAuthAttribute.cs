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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class IsAdminAuthAttribute : Attribute
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            //if (User.Identity.IsAdmin == false)
            //{
            //    // don't continue
            //    context.HttpContext.Response.StatusCode = 403;
            //    return;
            //}
            return;
        }
    }
}
