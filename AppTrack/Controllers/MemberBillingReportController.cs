using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

using System.Security.Claims;
using System.Collections.Generic;
using System.Web;
using System.Web.WebPages;
using System.Web.UI.DataVisualization.Charting;
using System.Text;
using System.IO;
using System.Drawing;

using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using AppTrack.Helpers;
using System.Text.RegularExpressions;
using System.Data;
using System.Configuration;

namespace AppTrack.Controllers
{
    [AuthorizeAdminRedirect(Roles = "Executive")]
    public class MemberBillingReportController : BaseController
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public MemberBillingReportController()
        {
        }
        public MemberBillingReportController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        // GET: MemberBillingReport
        [HttpGet]
        public ActionResult Index()
        {
            int startPeriodID = 0;
            int endPeriodID = 0;
            var DataHelper = new DataHelpers();

            MemberBillingReportViewModel model = new MemberBillingReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                model.MemberBillingReportList = db.Database.SqlQuery<MemberBillingReport>("exec dbo.[LB_GetMemberBillingTotals] @EndPeriodID, @StartPeriodID",
                new SqlParameter("@EndPeriodID", endPeriodID),
                new SqlParameter("@StartPeriodID", startPeriodID)
               ).ToList();
            }

            model.SearchPeriodList = DataHelper.GetPeriodIDList(Constants.quarterlyPeriodTypeID, 12, true, true);

            return View(model);
        }

        [HttpGet]
        public JsonResult StackedBarChartSales(int spid = 0 , int epid = 0)
        {
            int startPeriodID = spid;
            int endPeriodID = epid;
            int i = 0;
            MemberBillingReportViewModel model = new MemberBillingReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                model.MemberBillingReportList = db.Database.SqlQuery<MemberBillingReport>("exec dbo.[LB_GetMemberBillingTotals] @EndPeriodID, @StartPeriodID",
                new SqlParameter("@EndPeriodID", endPeriodID),
                new SqlParameter("@StartPeriodID", startPeriodID)
               ).ToList();
            }

            List<StackedBarSalesData> thisChartData = new List<StackedBarSalesData>();
            foreach (var listitem in model.MemberBillingReportList)
            {
                i++;
                thisChartData.Add(new StackedBarSalesData(i, listitem.PeriodName, "Totals", listitem.SalesTotal, listitem.PaidTotal, listitem.BalanceDue));
            }
            return Json(thisChartData, JsonRequestBehavior.AllowGet);


            if (1 == 2)
            {
                List<StackedBarSalesData> thisData = new List<StackedBarSalesData>();
                thisData.Add(new StackedBarSalesData(1, "Dec", "AppTrack", 1000, 500, 500));
                thisData.Add(new StackedBarSalesData(2, "Jan", "AppTrack", 900, 700, 200));
                thisData.Add(new StackedBarSalesData(3, "Feb", "AppTrack", 1100, 800, 300));
                thisData.Add(new StackedBarSalesData(4, "Mar", "AppTrack", 1200, 1100, 100));
                thisData.Add(new StackedBarSalesData(5, "Apr", "AppTrack", 1300, 1200, 100));
                return Json(thisData, JsonRequestBehavior.AllowGet);
            }
        }


        // GET: MemberBillingReport
        [HttpGet]
        public ActionResult ByType()
        {
            string countType = "Type";
            string statusType = "All";
            var DataHelper = new DataHelpers();

            MemberBillingReportViewModel model = new MemberBillingReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                model.MemberBillingReportList = db.Database.SqlQuery<MemberBillingReport>("exec dbo.[LB_GetMemberBillingTotalsByType] @EndPeriodID, @StartPeriodID",
                new SqlParameter("@EndPeriodID", 0),
                new SqlParameter("@StartPeriodID", 0)
               ).ToList();
            }

            model.SearchPeriodList = DataHelper.GetPeriodIDList(Constants.quarterlyPeriodTypeID, 12, true, true);

            return View(model);
        }



        // GET: MemberBillingReport
        [HttpGet]
        public ActionResult ByProduct()
        {
            string countType = "Type";
            string statusType = "All";
            var DataHelper = new DataHelpers();

            MemberBillingReportViewModel model = new MemberBillingReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                model.MemberBillingReportList = db.Database.SqlQuery<MemberBillingReport>("exec dbo.[LB_GetMemberBillingTotalsByProduct] @EndPeriodID, @StartPeriodID",
                new SqlParameter("@EndPeriodID", 0),
                new SqlParameter("@StartPeriodID", 0)
               ).ToList();
            }

            model.SearchPeriodList = DataHelper.GetPeriodIDList(Constants.quarterlyPeriodTypeID, 12, true, true);

            return View(model);
        }







    }
}


