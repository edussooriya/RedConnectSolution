using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Filters;

namespace RedConnect.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var error = context.HttpContext.Request.Query["error"].ToString();

            if (!string.IsNullOrEmpty(error))
            {
                TempData["Error"] = error;
            }

            base.OnActionExecuting(context);
        }
        
    }
}
