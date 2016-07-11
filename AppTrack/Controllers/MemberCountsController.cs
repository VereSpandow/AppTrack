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
    public class MemberCountsController : BaseController
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public MemberCountsController()
        {
        }
        public MemberCountsController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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

        // GET: MemberCounts
        [HttpGet]
        public ActionResult Index(string submit = "submit")
        {
            string countType = "Type";
            string statusType = "All";
            var DataHelper = new DataHelpers();

            MemberCountReportViewModel model = new MemberCountReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                model.MemberCountReportListXTab = db.Database.SqlQuery<MemberCountReportXTab>("exec dbo.[LB_GetMemberCounts] @CountType, @StatusType, @isXTab",
                new SqlParameter("@CountType", countType),
                new SqlParameter("@StatusType", statusType),
                new SqlParameter("@isXTab", 1)
               ).ToList();
            }

            if (submit == "Download")
            {

                if (model.MemberCountReportListXTab.Count > 0)
                {
                    int i = 0;
                    string attachment = "attachment; filename=" + AdminID.ToString() + "-MemberRetentionByTypeReport.csv";
                    HttpContext.Response.AddHeader("content-disposition", attachment);
                    var sw = new StreamWriter(new MemoryStream());
                    //sw.WriteLine("\"Member ID\",\"Status\",\"Member Type\",\"Period1\",\"Period2\",\"Period3\",\"Period4\",\"Period5\",\"Period6\",\"Period7\",\"Period8\",\"Period9\",\"Period10\",\"Period11\",\"Period12\",\"Period13\"");
                    foreach (var line in model.MemberCountReportListXTab)
                    {
                        i = i+1;
                        if (i == 1){
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\"",
                            "Status",
                            "Member Type",
                            line.PeriodLabel1,
                            line.PeriodLabel2,
                            line.PeriodLabel3,
                            line.PeriodLabel4,
                            line.PeriodLabel5,
                            line.PeriodLabel6,
                            line.PeriodLabel7,
                            line.PeriodLabel8,
                            line.PeriodLabel9,
                            line.PeriodLabel10,
                            line.PeriodLabel11,
                            line.PeriodLabel12,
                            line.PeriodLabel13
                            ));
                        }
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\"",
                            line.Status,
                            line.StoreName,
                            line.PeriodData1,
                            line.PeriodData2,
                            line.PeriodData3,
                            line.PeriodData4,
                            line.PeriodData5,
                            line.PeriodData6,
                            line.PeriodData7,
                            line.PeriodData8,
                            line.PeriodData9,
                            line.PeriodData10,
                            line.PeriodData11,
                            line.PeriodData12,
                            line.PeriodData13
                            ));
                    }
                    sw.Flush();
                    sw.BaseStream.Seek(0, SeekOrigin.Begin);

                    //return new FileStreamResult(sw.BaseStream, "text/csv");
                    return File(sw.BaseStream, "text/csv");
                }
                else
                {
                    ModelState.AddModelError("", "Cannot download an empty list");
                }
            }


            model.SearchPeriodList = DataHelper.GetPeriodIDList(Constants.quarterlyPeriodTypeID, 12, true, true);

            return View(model);
        }

        [HttpGet]
        public ActionResult ByIMD( int ssid = 0, string submit = "submit" )
        {
            string countType = "Type";
            string statusType = "All";
            var DataHelper = new DataHelpers();

            MemberCountIMDReportViewModel model = new MemberCountIMDReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                model.MemberCountIMDReportListXTab = db.Database.SqlQuery<MemberCountIMDReportXTab>("exec dbo.[LB_GetMemberIMDCounts] @CountType, @StatusType, @isXTab, @SecSponsorID",
                new SqlParameter("@CountType", countType),
                new SqlParameter("@StatusType", statusType),
                new SqlParameter("@isXTab", 1),
                new SqlParameter("@SecSponsorID", ssid)
               ).ToList();
            }

            if (submit == "Download")
            {

                if (model.MemberCountIMDReportListXTab.Count > 0)
                {
                    int i = 0;
                    string attachment = "attachment; filename=" + AdminID.ToString() + "-MemberRetentionByIMDReport.csv";
                    HttpContext.Response.AddHeader("content-disposition", attachment);
                    var sw = new StreamWriter(new MemoryStream());
                    foreach (var line in model.MemberCountIMDReportListXTab)
                    {
                        i = i + 1;
                        if (i == 1)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\"",
                            "Status",
                            "IMDID",
                            "IMD Name",
                            line.PeriodLabel1,
                            line.PeriodLabel2,
                            line.PeriodLabel3,
                            line.PeriodLabel4,
                            line.PeriodLabel5,
                            line.PeriodLabel6,
                            line.PeriodLabel7,
                            line.PeriodLabel8,
                            line.PeriodLabel9,
                            line.PeriodLabel10,
                            line.PeriodLabel11,
                            line.PeriodLabel12,
                            line.PeriodLabel13
                            ));
                        }
                        sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\"",
                        line.Status,
                        line.SecSponsorID,
                        line.DisplayName,
                        line.PeriodData1,
                        line.PeriodData2,
                        line.PeriodData3,
                        line.PeriodData4,
                        line.PeriodData5,
                        line.PeriodData6,
                        line.PeriodData7,
                        line.PeriodData8,
                        line.PeriodData9,
                        line.PeriodData10,
                        line.PeriodData11,
                        line.PeriodData12,
                        line.PeriodData13
                        ));
                    }
                    sw.Flush();
                    sw.BaseStream.Seek(0, SeekOrigin.Begin);

                    //return new FileStreamResult(sw.BaseStream, "text/csv");
                    return File(sw.BaseStream, "text/csv");
                }
                else
                {
                    ModelState.AddModelError("", "Cannot download an empty list");
                }
            }

            model.SearchPeriodList = DataHelper.GetPeriodIDList(Constants.quarterlyPeriodTypeID, 12, true, true);
            model.SearchIMDList = DataHelper.GetMemberDirectorSelectList(false, true);

            return View(model);
        }

        [HttpGet]
        public ActionResult BySalesRep(int sid = 0, string submit = "submit")
        {
            string countType = "Type";
            string statusType = "All";
            var DataHelper = new DataHelpers();

            MemberCountSalesRepReportViewModel model = new MemberCountSalesRepReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                model.MemberCountSalesRepReportListXTab = db.Database.SqlQuery<MemberCountSalesRepReportXTab>("exec dbo.[LB_GetMemberSalesRepCounts] @CountType, @StatusType, @isXTab,@SponsorID",
                new SqlParameter("@CountType", countType),
                new SqlParameter("@StatusType", statusType),
                new SqlParameter("@isXTab", 1),
                new SqlParameter("@SponsorID", sid)
               ).ToList();
            }

            if (submit == "Download")
            {

                if (model.MemberCountSalesRepReportListXTab.Count > 0)
                {
                    int i = 0;
                    string attachment = "attachment; filename=" + AdminID.ToString() + "-MemberRetentionBySalesRepReport.csv";
                    HttpContext.Response.AddHeader("content-disposition", attachment);
                    var sw = new StreamWriter(new MemoryStream());
                    foreach (var line in model.MemberCountSalesRepReportListXTab)
                    {
                        i = i + 1;
                        if (i == 1)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\"",
                            "Status",
                            "SalesRepID",
                            "Sales Rep Name",
                            line.PeriodLabel1,
                            line.PeriodLabel2,
                            line.PeriodLabel3,
                            line.PeriodLabel4,
                            line.PeriodLabel5,
                            line.PeriodLabel6,
                            line.PeriodLabel7,
                            line.PeriodLabel8,
                            line.PeriodLabel9,
                            line.PeriodLabel10,
                            line.PeriodLabel11,
                            line.PeriodLabel12,
                            line.PeriodLabel13
                            ));
                        }
                        sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\"",
                        line.Status,
                        line.SponsorID,
                        line.DisplayName,
                        line.PeriodData1,
                        line.PeriodData2,
                        line.PeriodData3,
                        line.PeriodData4,
                        line.PeriodData5,
                        line.PeriodData6,
                        line.PeriodData7,
                        line.PeriodData8,
                        line.PeriodData9,
                        line.PeriodData10,
                        line.PeriodData11,
                        line.PeriodData12,
                        line.PeriodData13
                        ));
                    }
                    sw.Flush();
                    sw.BaseStream.Seek(0, SeekOrigin.Begin);

                    //return new FileStreamResult(sw.BaseStream, "text/csv");
                    return File(sw.BaseStream, "text/csv");
                }
                else
                {
                    ModelState.AddModelError("", "Cannot download an empty list");
                }
            }


            model.SearchPeriodList = DataHelper.GetPeriodIDList(Constants.quarterlyPeriodTypeID, 12, true, true);
            model.SearchSalesRepList = DataHelper.GetCustIDList(3,false, true);


            return View(model);
        }

        [HttpGet]
        public JsonResult StackedBarChart(string ct = "Type", string st = "All")
        {
            string countType = ct;
            string statusType = st;
            int i = 0;
            int zero = 0;
            MemberCountReportViewModel model = new MemberCountReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                model.MemberCountReportList = db.Database.SqlQuery<MemberCountReport>("exec dbo.[LB_GetMemberCounts] @CountType, @StatusType, @isXTab",
                new SqlParameter("@CountType", countType),
                new SqlParameter("@StatusType", statusType),
                new SqlParameter("@isXTab", zero)
               ).ToList();
            }

            List<StackBarData> thisChartData = new List<StackBarData>();
            foreach (var listitem  in model.MemberCountReportList)
            {
                if (listitem.StoreID != 0)
                {
                    i = i++;
                    thisChartData.Add(new StackBarData(i, listitem.PeriodName, listitem.StoreName, listitem.MemberCount));
                }
            }
            return Json(thisChartData, JsonRequestBehavior.AllowGet);
            

            if (1 == 2)
            {
                List<StackBarData> thisData = new List<StackBarData>();
                    thisData.Add(new StackBarData(1, "Dec", "AppTrack", 1000));
                    thisData.Add(new StackBarData(2, "Jan", "AppTrack", 900));
                    thisData.Add(new StackBarData(3, "Feb", "AppTrack", 1100));
                    thisData.Add(new StackBarData(4, "Mar", "AppTrack", 1200));
                    thisData.Add(new StackBarData(5, "Apr", "AppTrack", 1300));
                    thisData.Add(new StackBarData(1, "Dec", "PRIMA", 400));
                    thisData.Add(new StackBarData(2, "Jan", "PRIMA", 450));
                    thisData.Add(new StackBarData(3, "Feb", "PRIMA", 650));
                    thisData.Add(new StackBarData(4, "Mar", "PRIMA", 700));
                    thisData.Add(new StackBarData(5, "Apr", "PRIMA", 750));
                    return Json(thisData, JsonRequestBehavior.AllowGet);
            }
        }
 
    }
}


