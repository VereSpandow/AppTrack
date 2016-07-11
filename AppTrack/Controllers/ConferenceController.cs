using AppTrack.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppTrack.Controllers
{

    [AuthorizeAdminRedirect(Roles = Constants.adminRoles)]
    public class ConferenceController : BaseController
    {
        // GET: Conference
        public ActionResult Index()
        {
            return View();
        }
    }
}