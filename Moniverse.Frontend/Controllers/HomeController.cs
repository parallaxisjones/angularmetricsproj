using PlayverseMetrics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace PlayverseMetrics.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
#if PROD
    string env = "prod";
#elif STAGING
    string env = "staging";
#else
            string env = "local";
#endif
            SystemsModel systemsModel = new SystemsModel();
            ViewBag.WindowsServiceStatus = systemsModel.CheckWindowsServiceStatus();
            ViewBag.Environment = env;
            return View();
        }
    }
}