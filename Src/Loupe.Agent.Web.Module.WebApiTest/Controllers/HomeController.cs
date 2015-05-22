using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Mvc;

namespace Loupe.Agent.Web.Module.MVCTest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.SessionId = ControllerContext.HttpContext.Items["LoupeSessionId"].ToString();

            return View();
        }
    }
}