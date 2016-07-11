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
    public class MemberSalesReportController : BaseController
    {

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public MemberSalesReportController()
        {
        }
        public MemberSalesReportController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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
        // GET: 
        [HttpGet]
        public ActionResult Index(int epid = 0, int spid = 0, int vid  = 0, int isGroup = 1)
        {
            var DataHelper = new DataHelpers();

            MemberSalesReportViewModel model = new MemberSalesReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                if ((epid == 0) && (spid == 0))
                {

                    C_Periods periodDefault = db.Database.SqlQuery<C_Periods>("exec dbo.[LB_GetVendorSalePeriodDefaults]").First();
                    epid = periodDefault.PeriodID;
                    spid = epid - 1;
                }

                model.MemberSalesReportList = db.Database.SqlQuery<MemberSalesReport>("exec dbo.[LB_GetMemberTopSalesCounts] @EndPeriodID, @StartPeriodID, @VendorID",
                new SqlParameter("@EndPeriodID", epid),
                new SqlParameter("@StartPeriodID", spid),
                new SqlParameter("@VendorID", vid)
               ).ToList();
            }

            model.epid = epid;
            model.spid = spid;
            model.vid = vid;

            model.SearchPeriodList = DataHelper.GetPeriodIDList(Constants.quarterlyPeriodTypeID, 12, true, false);
            model.SearchVendorList = DataHelper.GetVendorsWithRebatesList(true, true);

            return View(model);
        }

        // Post: 
        [HttpPost]
        public ActionResult Index(int epid = 0, int spid = 0, int vid = 0, int isGroup = 1, string submit = "submit")
        {
            var DataHelper = new DataHelpers();

            MemberSalesReportViewModel model = new MemberSalesReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                if ((epid == 0) && (spid == 0))
                {

                    C_Periods periodDefault = db.Database.SqlQuery<C_Periods>("exec dbo.[LB_GetVendorSalePeriodDefaults]").First();
                    epid = periodDefault.PeriodID;
                    spid = epid - 1;
                }

                model.MemberSalesReportList = db.Database.SqlQuery<MemberSalesReport>("exec dbo.[LB_GetMemberTopSalesCounts] @EndPeriodID, @StartPeriodID, @VendorID",
                new SqlParameter("@EndPeriodID", epid),
                new SqlParameter("@StartPeriodID", spid),
                new SqlParameter("@VendorID", vid)
               ).ToList();
            }

            model.epid = epid;
            model.spid = spid;
            model.vid = vid;

            if (submit == "Download")
            {

                if (model.MemberSalesReportList.Count > 0)
                {
                    string attachment = "attachment; filename=" + AdminID.ToString() + "-MemberSalesReport.csv";

                    HttpContext.Response.AddHeader("content-disposition", attachment);

                    var sw = new StreamWriter(new MemoryStream());

                    if (User.IsInRole("Executive"))
                    {
                        sw.WriteLine("\"Member ID\",\"Member Name\",\"Vendor ID\",\"Vendor Name\",\"Period Name\",\"Total Sales\",\"Member Rebates\",\"Corporate Rebates\"");
                        foreach (var line in model.MemberSalesReportList)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\"",
                                line.CustID,
                                line.MemberName,
                                line.VendorID,
                                line.VendorName,
                                line.PeriodName,
                                line.Sales,
                                line.MemberRebateAmount,
                                line.CorporateRebateAmount
                                ));
                        }
                    }
                    else
                    {
                        sw.WriteLine("\"Member ID\",\"Member Name\",\"Vendor ID\",\"Vendor Name\",\"Period Name\",\"Total Sales\",\"Member Rebates\"");
                        foreach (var line in model.MemberSalesReportList)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"",
                                line.CustID,
                                line.MemberName,
                                line.VendorID,
                                line.VendorName,
                                line.PeriodName,
                                line.Sales,
                                line.MemberRebateAmount
                                ));
                        }

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
            model.SearchVendorList = DataHelper.GetVendorsWithRebatesList(true, true);

            return View(model);
        }

        // GET: MemberCounts
        [HttpGet]
        public ActionResult Delta(int epid = 0, int spid = 0, int vid = 0, int mid = 0, string submit = "submit")
        {
            var DataHelper = new DataHelpers();

            if ((mid == 0) && (vid == 0))
            {
                vid = 275877;
            }

            MemberSalesDeltaReportViewModel model = new MemberSalesDeltaReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                if ((epid == 0) && (spid == 0))
                    {

                        C_Periods periodDefault = db.Database.SqlQuery<C_Periods>("exec dbo.[LB_GetVendorSalePeriodDefaults]").First();
                        epid = periodDefault.PeriodID;
                        spid = epid-1;
                    }

                model.MemberSalesDeltaReportList = db.Database.SqlQuery<MemberSalesDeltaReport>("exec dbo.[LB_GetMemberSalesDeltaV2] @EndPeriodID, @StartPeriodID, @VendorID, @MemberID",
                new SqlParameter("@EndPeriodID", epid),
                new SqlParameter("@StartPeriodID", spid),
                new SqlParameter("@VendorID", vid),
                new SqlParameter("@MemberID", mid)
               ).ToList();
            }

            model.epid = epid;
            model.spid = spid;
            model.vid = vid;
            model.searchCustID = mid;

            if (submit == "Download")
            {

                if (model.MemberSalesDeltaReportList.Count > 0)
                {
                    string attachment = "attachment; filename=" + AdminID.ToString() + "-MemberChangeInSalesReport.csv";

                    HttpContext.Response.AddHeader("content-disposition", attachment);

                    var sw = new StreamWriter(new MemoryStream());

                    if (User.IsInRole("Executive"))
                    {

                        sw.WriteLine("\"Member ID\",\"Member Name\",\"Vendor ID\",\"Vendor Name\",\"Period ID (1)\",\"Period Name (1)\",\"Total Sales (1)\",\"Member Rebates (1)\",\"Corporate Rebates (1)\",\"Period ID (2)\",\"Period Name (2)\",\"Total Sales (2)\",\"Member Rebates (2)\",\"Corporate Rebates (2)\",\"Change-Sales\",\"Percent Change-Sales\",\"Change-M-Rebate\",\"Percent Change-M-Rebates\",\"Change-Corp-Rebates\",\"Percent Change Corp-Rebates\"");
                        foreach (var line in model.MemberSalesDeltaReportList)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\"",
                                line.CustID,
                                line.MemberName,
                                line.VendorID,
                                line.VendorName,
                                line.PeriodID1,
                                line.PeriodName1,
                                line.Sales1,
                                line.MemberRebateAmount1,
                                line.CorporateRebateAmount1,
                                line.PeriodID2,
                                line.PeriodName2,
                                line.Sales2,
                                line.MemberRebateAmount2,
                                line.CorporateRebateAmount2,
                                line.SalesDelta,
                                line.SalesDeltaPercent,
                                line.MemberRebateAmountDelta,
                                line.MemberRebateAmountPercent,
                                line.CorporateRebateAmountDelta,
                                line.CorporateRebateAmountPercent
                                ));
                        }
                    }
                    else
                    {                       
                        sw.WriteLine("\"Member ID\",\"Member Name\",\"Vendor ID\",\"Vendor Name\",\"Period ID (1)\",\"Period Name (1)\",\"Total Sales (1)\",\"Member Rebates (1)\",\"Period ID (2)\",\"Period Name (2)\",\"Total Sales (2)\",\"Member Rebates (2)\",\"Change-Sales\",\"Percent Change-Sales\",\"Change-M-Rebate\",\"Percent Change-M-Rebates\"");
                        foreach (var line in model.MemberSalesDeltaReportList)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\"",
                                line.CustID,
                                line.MemberName,
                                line.VendorID,
                                line.VendorName,
                                line.PeriodID1,
                                line.PeriodName1,
                                line.Sales1,
                                line.MemberRebateAmount1,
                                line.PeriodID2,
                                line.PeriodName2,
                                line.Sales2,
                                line.MemberRebateAmount2,
                                line.SalesDelta,
                                line.SalesDeltaPercent,
                                line.MemberRebateAmountDelta,
                                line.MemberRebateAmountPercent
                                ));
                        }

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
            model.SearchVendorList = DataHelper.GetVendorsWithRebatesList(true, false);

            return View(model);
        }

        // GET: MemberCounts
        [HttpGet]
        public ActionResult NoSales(int vid = 0, int pid = 0, string submit = "submit")
        {
          if ((pid == 0) && (vid == 0))
            {
                vid = 275877;
            }
            var DataHelper = new DataHelpers();
            MemberNoSalesReportViewModel model = new MemberNoSalesReportViewModel();
            if ((vid > 0) || (pid > 0))
            {   
                using (var db = new DevProvidentIDOCEntities())
                {
                    if (pid == 1)
                    {
                        C_Periods lastPeriod = db.Database.SqlQuery<C_Periods>("exec dbo.[LB_GetLastClosedPeriodID] @PeriodTypeID",
                        new SqlParameter("@PeriodTypeID", @Constants.quarterlyPeriodTypeID)
                        ).FirstOrDefault();
                        if (lastPeriod == null)
                        {
                            pid = 0;
                        }
                        else
                        {
                            pid = lastPeriod.PeriodID;
                        }

                    }
                    model.pid = pid;
                    model.MemberNoSalesReportList = db.Database.SqlQuery<MemberNoSalesReport>("exec dbo.[LB_GetMemberNOSales] @VendorID,@PeriodID",
                    new SqlParameter("@VendorID", vid),
                    new SqlParameter("@PeriodID", pid)
                   ).ToList();
                }

                if (submit == "Download")
                {

                    if (model.MemberNoSalesReportList.Count > 0)
                    {
                        string attachment = "attachment; filename=" + AdminID.ToString() + "-MemberNoSalesReport.csv";

                        HttpContext.Response.AddHeader("content-disposition", attachment);

                        var sw = new StreamWriter(new MemoryStream());

                        sw.WriteLine("\"Member ID\",\"Member Name\",\"Vendor ID\",\"Vendor Name\",\"Started On\",\"Active Through\",\"Period Name\"");
                        foreach (var line in model.MemberNoSalesReportList)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\"",
                                line.CustID,
                                line.MemberName,
                                line.VendorID,
                                line.VendorName,
                                line.StartDate,
                                line.EndDate,
                                line.PeriodName
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
            }
            else
            {
                vid = 0;
                pid = 0;
            }
            model.vid = vid;
            model.pid = pid;
            model.SearchPeriodList = DataHelper.GetPeriodIDList(Constants.quarterlyPeriodTypeID, 12, true, false);
            model.SearchVendorList = DataHelper.GetVendorsWithRebatesList(true, false);

            return View(model);
        }

        // GET: MemberCounts
        [HttpGet]
        public ActionResult UnexpectedSales(int spid = 0, int vid = 0, string submit = "submit")
        {
            var DataHelper = new DataHelpers();

            MemberUnexpectedSalesReportViewModel model = new MemberUnexpectedSalesReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                model.MemberUnexpectedSalesReportList = db.Database.SqlQuery<MemberUnexpectedSalesReport>("exec dbo.[LB_GetMemberUnexpectedSales] @PeriodID, @VendorID",
                new SqlParameter("@PeriodID", spid),
                new SqlParameter("@VendorID", vid)
               ).ToList();
            }

            if (submit == "Download")
            {

                if (model.MemberUnexpectedSalesReportList.Count > 0)
                {
                    string attachment = "attachment; filename=" + AdminID.ToString() + "-MemberUnexpectedSalesReport.csv";

                    HttpContext.Response.AddHeader("content-disposition", attachment);

                    var sw = new StreamWriter(new MemoryStream());

                    sw.WriteLine("\"Member ID\",\"Member Name\",\"Vendor ID\",\"Vendor Name\",\"Period Name\"");
                    foreach (var line in model.MemberUnexpectedSalesReportList)
                    {
                        sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"",
                            line.CustID,
                            line.MemberName,
                            line.VendorID,
                            line.VendorName,
                            line.PeriodName
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
            model.spid = spid;
            model.vid = vid;
            model.SearchPeriodList = DataHelper.GetPeriodIDList(Constants.quarterlyPeriodTypeID, 12, true, true);
            model.SearchVendorList = DataHelper.GetVendorsWithRebatesList(true, true);

            return View(model);
        }

    }
}