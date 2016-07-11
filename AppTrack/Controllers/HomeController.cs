using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using AppTrack.Helpers;

namespace AppTrack.Controllers
{

    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index","Site");

        }
        

        public ActionResult Error()
        {
            return View();
        }
    }
}