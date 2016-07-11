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
    public class MemberBudgetReportController : BaseController
    {
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public MemberBudgetReportController()
        {
        }
        public MemberBudgetReportController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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

        // GET: MemberBudgetReport
        [HttpGet]
        public ActionResult Index(string submit = "submit")
        {
            int startPeriodID = 0;
            int endPeriodID = 0;
            var DataHelper = new DataHelpers();

            MemberBudgetReportViewModel model = new MemberBudgetReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                model.MemberBudgetReportListXTab = db.Database.SqlQuery<MemberBudgetReportXTab>("exec dbo.[LB_UpdateMemberBudgetCountsDailyXTab] @StartPeriodID, @EndPeriodID",
                new SqlParameter("@StartPeriodID", startPeriodID),
                new SqlParameter("@EndPeriodID", endPeriodID)
               ).ToList();
            }

            if (submit == "Download")
            {

                if (model.MemberBudgetReportListXTab.Count > 0)
                {
                    int i = 0;
                    string attachment = "attachment; filename=" + AdminID.ToString() + "-MemberBudgetReport.csv";
                    HttpContext.Response.AddHeader("content-disposition", attachment);
                    var sw = new StreamWriter(new MemoryStream());
                    //sw.WriteLine("\"Member ID\",\"Status\",\"Member Type\",\"Period1\",\"Period2\",\"Period3\",\"Period4\",\"Period5\",\"Period6\",\"Period7\",\"Period8\",\"Period9\",\"Period10\",\"Period11\",\"Period12\",\"Period13\"");
                    foreach (var line in model.MemberBudgetReportListXTab)
                    {
                        i = i + 1;
                        if (i == 1)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\"",
                            "Title",
                            "DataGroupID",
                            "Seqno",
                            "Flag1",
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
                        sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\"",
                        line.Title,
                        line.DataGroupID,
                        line.Seqno,
                        line.Flag1,
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

    }
}


