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
    public class SalesPOPMembersONLYReportController : BaseController
    {

        [HttpGet]
        public ActionResult Index()
        {
            int intCustID = 0;
            int searchVendorID = 0;
            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> vendorList = DataHelper.GetVendorsWithRebatesList(true, true);

            if (!Int32.TryParse(vendorList.ElementAt(1).Value, out searchVendorID))
                searchVendorID = 0;

            SalesPoPReportViewModel model = new SalesPoPReportViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                model.SalesPoPReportList = db.Database.SqlQuery<SalesPoPReport>("exec dbo.[LB_GetMemberONLYVendorSalesTrendsV2] @MemberID, @VendorID",
                new SqlParameter("@MemberID", intCustID),
                new SqlParameter("@VendorID", searchVendorID)
               ).ToList();
            }

            model.SearchPeriodList = DataHelper.GetPeriodIDList(Constants.quarterlyPeriodTypeID, 12, true, true);
            model.SearchVendorList = vendorList;
            model.searchVendorID = searchVendorID;

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(string searchCustID = "", int searchVendorID = 0, int searchPeriodID = 0, string submit = "submit")
        {
            int intCustID = 0;

            if (searchCustID != "")
            {
                if (!Int32.TryParse(searchCustID, out intCustID))
                    intCustID = 0;
            }

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> vendorList = DataHelper.GetVendorsWithRebatesList(true, true);

            if ((intCustID == 0) && (searchVendorID == 0))
            {
                if (!Int32.TryParse(vendorList.ElementAt(1).Value, out searchVendorID))
                    searchVendorID = 0;
            }

            SalesPoPReportViewModel model = new SalesPoPReportViewModel();

            using (var db = new DevProvidentIDOCEntities())
            {

                model.SalesPoPReportList = db.Database.SqlQuery<SalesPoPReport>("exec dbo.[LB_GetMemberONLYVendorSalesTrendsV2] @MemberID, @VendorID, @MaxPeriodID",
                new SqlParameter("@MemberID", intCustID),
                new SqlParameter("@VendorID", searchVendorID),
                new SqlParameter("@MaxPeriodID", searchPeriodID)
               ).ToList();

            }

            if (submit == "Download")
            {

                if (model.SalesPoPReportList.Count > 0)
                {
                    string attachment = "attachment; filename=" + AdminID.ToString() + "-MemberSalesReport.csv";

                    HttpContext.Response.AddHeader("content-disposition", attachment);

                    var sw = new StreamWriter(new MemoryStream());

                    sw.WriteLine("\"Member ID\",\"Member Name\",\"Vendor ID\",\"Vendor Name\",\"Period 1\",\"Sales Period 1\",\"Period 2\",\"Sales Period 2\",\"Period 3\",\"Sales Period 3\",\"Period 4\",\"Sales Period 4\",\"Period 5\",\"Sales Period 5\"");
                    foreach (var line in model.SalesPoPReportList)
                    {
                        sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\"",
                            line.CustID,
                            line.VendorID,
                            line.MemberName,
                            line.VendorName,
                            line.PeriodLabelOne,
                            line.SalesPeriodOne,
                            line.PeriodLabelTwo,
                            line.SalesPeriodTwo,
                            line.PeriodLabelThree,
                            line.SalesPeriodThree,
                            line.PeriodLabelFour,
                            line.SalesPeriodFour,
                            line.PeriodLabelFive,
                            line.SalesPeriodFive
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
            model.SearchVendorList = vendorList;
            model.searchVendorID = searchVendorID;
            return View(model);
        }

        [HttpGet]
        public JsonResult LineChart(int searchCustID = 0, int searchVendorID = 0)
        {
            SalesPoPReport model = new SalesPoPReport();
            using (var db = new DevProvidentIDOCEntities())
            {
                model = db.Database.SqlQuery<SalesPoPReport>("exec dbo.[LB_GetMemberONLYVendorSalesTotal] @MemberID, @VendorID",
                new SqlParameter("@MemberID", searchCustID),
                new SqlParameter("@VendorID", searchVendorID)
               ).FirstOrDefault();

                if (model != null)
                {

                    List<SalesTrendData> thisData = new List<SalesTrendData>();
                    thisData.Add(new SalesTrendData(model.PeriodLabelFive, model.SalesPeriodFive));
                    thisData.Add(new SalesTrendData(model.PeriodLabelFour, model.SalesPeriodFour));
                    thisData.Add(new SalesTrendData(model.PeriodLabelThree, model.SalesPeriodThree));
                    thisData.Add(new SalesTrendData(model.PeriodLabelTwo, model.SalesPeriodTwo));
                    thisData.Add(new SalesTrendData(model.PeriodLabelOne, model.SalesPeriodOne));

                    return Json(thisData, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    List<SalesTrendData> thisData = new List<SalesTrendData>();
                    thisData.Add(new SalesTrendData("5", 0));
                    thisData.Add(new SalesTrendData("4", 0));
                    thisData.Add(new SalesTrendData("3", 0));
                    thisData.Add(new SalesTrendData("2", 0));
                    thisData.Add(new SalesTrendData("1", 0));

                    return Json(thisData, JsonRequestBehavior.AllowGet);
                }
            }
        }
    }
}

