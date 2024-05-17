using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerifyV2Quickstart.Filters;
using VerifyV2Quickstart.Models;

namespace VerifyV2Quickstart.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        [ServiceFilter(typeof(VerifyFilter))]
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}