using PlayverseMetrics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace PlayverseMetrics.Controllers
{
    
    public class CostTrackerController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View("Summary");
        }
    }
}