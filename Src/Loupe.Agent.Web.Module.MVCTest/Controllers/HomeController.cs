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

        public HttpResponseMessage data()
        {

            var resp = new HttpResponseMessage()
            {
                Content = new StringContent("{\"fakeData\":\"123\"}")
            };
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return resp;
        }
    }
}