using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> logger;

        public ErrorController(ILogger<ErrorController> _logger)
        {
            logger = _logger;
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Sorry, the resource you requested could not found";
                    /* showing status code and query path to user
                     * ViewBag.Path = statusCodeResult.OriginalPath;
                     ViewBag.QS = statusCodeResult.OriginalQueryString;*/
                    logger.LogWarning($"404 error {statusCodeResult.OriginalPath}" + $"Query string {statusCodeResult.OriginalQueryString}");
                    break;
            }
                
            return View("NotFound");
        }

        [Route("Error")]
        [AllowAnonymous]
        public IActionResult HttpExceptionHandler()
        {
            var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            ViewBag.exceptionPath = exceptionDetails.Path;
            ViewBag.exceptionMessge = exceptionDetails.Error.Message;
            ViewBag.exceptionStackTrace = exceptionDetails.Error.StackTrace;
            logger.LogError($"The path {exceptionDetails.Path} is not valid" + $"Message {exceptionDetails.Error}");
            return View("Error");
        }
    }
}
