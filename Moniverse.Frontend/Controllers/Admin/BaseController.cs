using System;
using System.Text;
using System.Web.Mvc;

namespace PlayverseMetrics.Controllers
{
    [AllowCrossSiteJson]
    [Authorize]
    public class BaseController : Controller
    {

        public JsonResult JsonResult(object data)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = "application/json",
                ContentEncoding = Encoding.UTF8,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = Int32.MaxValue
            };
        }
    }
}