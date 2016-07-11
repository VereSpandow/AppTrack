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
    public class DashboardController : BaseController
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public JsonResult LineChart1(int searchCustID = 0, int searchVendorID = 0)
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
        [HttpGet]
        public JsonResult LineChart2(int searchCustID = 0, int searchVendorID = 0)
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

        [HttpGet]
        public JsonResult LineChart3(int searchCustID = 0, int searchVendorID = 0)
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

        [HttpGet]
        public JsonResult LineChart4(int searchCustID = 0, int searchVendorID = 0)
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