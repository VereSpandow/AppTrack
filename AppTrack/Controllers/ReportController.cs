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
    [AuthorizeAdminRedirect(Roles = Constants.adminRoles)]
    public class ReportController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        [HttpGet]
        public ActionResult MemberAttritionSummary()
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model

            var memberAttritionSummaryViewModel = new MemberAttritionSummaryViewModel();

            C_Periods c_Period = db.Database.SqlQuery<C_Periods>("exec dbo.[LB_GetCurrentPeriod] @PT",
                new SqlParameter("@PT", Constants.monthlyPeriodTypeID)
                ).FirstOrDefault();

            if (c_Period == null)
            {
                memberAttritionSummaryViewModel.StartPeriodID = 0;
                memberAttritionSummaryViewModel.EndPeriodID = 0;                
            }
            else
            {
                memberAttritionSummaryViewModel.StartPeriodID = c_Period.PeriodID;
                memberAttritionSummaryViewModel.EndPeriodID = c_Period.PeriodID;
            }

            memberAttritionSummaryViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.monthlyPeriodTypeID, 36, true, false);
            memberAttritionSummaryViewModel.CustomerTypeList = DataHelper.GetCustomerTypeList(false, true);

            memberAttritionSummaryViewModel.CustomerType = 6;

            memberAttritionSummaryViewModel.MemberAttritionSummaryList = db.Database.SqlQuery<MemberAttritionSummary>("exec dbo.[LB_GetMemberAttritionSummaryMonthly] @CustomerType, @StartPeriodID, @EndPeriodID",
            new SqlParameter("@CustomerType", 6),
            new SqlParameter("@StartPeriodID", memberAttritionSummaryViewModel.StartPeriodID),
            new SqlParameter("@EndPeriodID", memberAttritionSummaryViewModel.EndPeriodID)
            ).ToList();

            return View(memberAttritionSummaryViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberAttritionSummary(MemberAttritionSummaryViewModel memberAttritionSummaryViewModel)
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model
            if (ModelState.IsValid)
            {
                if (memberAttritionSummaryViewModel.StartPeriodID > memberAttritionSummaryViewModel.EndPeriodID)
                {
                    ModelState.AddModelError("", "End Period cannot be before Start Period");
                }
                memberAttritionSummaryViewModel.MemberAttritionSummaryList = db.Database.SqlQuery<MemberAttritionSummary>("exec dbo.[LB_GetMemberAttritionSummaryMonthly] @CustomerType, @StartPeriodID, @EndPeriodID",
                new SqlParameter("@CustomerType", memberAttritionSummaryViewModel.CustomerType),
                new SqlParameter("@StartPeriodID", memberAttritionSummaryViewModel.StartPeriodID),
                new SqlParameter("@EndPeriodID", memberAttritionSummaryViewModel.EndPeriodID)
                ).ToList();
            }

            memberAttritionSummaryViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.monthlyPeriodTypeID, 36, true, false);
            memberAttritionSummaryViewModel.CustomerTypeList = DataHelper.GetCustomerTypeList(false, true);

            return View(memberAttritionSummaryViewModel);
        }

        [HttpGet]
        public ActionResult RebateSummaryByMember()
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model

            var rebateSummaryByMemberViewModel = new RebateSummaryByMemberViewModel();

            int intCustID = 0;
            int zero = 0;


            rebateSummaryByMemberViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.quarterlyPeriodTypeID, 20, true, false);

            rebateSummaryByMemberViewModel.StartPeriodID = zero;
            rebateSummaryByMemberViewModel.EndPeriodID = zero;
            rebateSummaryByMemberViewModel.CustID = " ";

            rebateSummaryByMemberViewModel.RebateSummaryByMemberList = db.Database.SqlQuery<RebateSummaryByMember>("exec dbo.[LB_GetRebateSummaryByMember] @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CustID",
            new SqlParameter("@PeriodTypeID", Constants.quarterlyPeriodTypeID),
            new SqlParameter("@StartPeriodID", rebateSummaryByMemberViewModel.StartPeriodID),
            new SqlParameter("@EndPeriodID", rebateSummaryByMemberViewModel.EndPeriodID),
            new SqlParameter("@CustID", intCustID )
            ).ToList();

            return View(rebateSummaryByMemberViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RebateSummaryByMember(RebateSummaryByMemberViewModel rebateSummaryByMemberViewModel)
        {
            var DataHelper = new DataHelpers();

            int intCustID = 0;

            if (rebateSummaryByMemberViewModel.CustID != "")
            {
                if (!Int32.TryParse(rebateSummaryByMemberViewModel.CustID, out intCustID))
                    intCustID = 0;
            }

            // Initalize the View Model
            if (ModelState.IsValid)
            {

                rebateSummaryByMemberViewModel.RebateSummaryByMemberList = db.Database.SqlQuery<RebateSummaryByMember>("exec dbo.[LB_GetRebateSummaryByMember] @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CustID",
                new SqlParameter("@PeriodTypeID", Constants.quarterlyPeriodTypeID),
                new SqlParameter("@StartPeriodID", rebateSummaryByMemberViewModel.StartPeriodID),
                new SqlParameter("@EndPeriodID", rebateSummaryByMemberViewModel.EndPeriodID),
                new SqlParameter("@CustID", intCustID)
                ).ToList();

            }

            rebateSummaryByMemberViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.quarterlyPeriodTypeID, 20, true, false);

            return View(rebateSummaryByMemberViewModel);
        }

        [HttpGet]
        public ActionResult RebateDetail(int StartPeriodID = 0, int EndPeriodID = 0, int CommissionID = Constants.memberRebateCommissionID, int VendorID = 0, int VolumeID = 0, string CustID = "")
        {
            var DataHelper = new DataHelpers();

            int intCustID = 0;

            if (CustID != "")
            {
                if (!Int32.TryParse(CustID, out intCustID))
                    intCustID = 0;
            }
            // Initalize the View Model


            var rebateDetailViewModel = new RebateDetailViewModel();

            rebateDetailViewModel.StartPeriodID = StartPeriodID;
            rebateDetailViewModel.EndPeriodID = EndPeriodID;
            rebateDetailViewModel.CommissionID = CommissionID;
            rebateDetailViewModel.VendorID = VendorID;
            rebateDetailViewModel.VolumeID = VolumeID;
            rebateDetailViewModel.CustID = CustID;

            if (intCustID == 0 && VendorID == 0)
            {
                ModelState.AddModelError("", "Either Vendor ID or Member ID must be supplied");
            }
            else
            {
                rebateDetailViewModel.RebateDetailList = db.Database.SqlQuery<RebateDetail>("exec dbo.[LB_GetRebateDetail] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @VendorID, @VolumeID",
                new SqlParameter("@CustID", intCustID),
                new SqlParameter("@PeriodTypeID", Constants.quarterlyPeriodTypeID),
                new SqlParameter("@StartPeriodID", rebateDetailViewModel.StartPeriodID),
                new SqlParameter("@EndPeriodID", rebateDetailViewModel.EndPeriodID),
                new SqlParameter("@CommissionID", rebateDetailViewModel.CommissionID),
                new SqlParameter("@VendorID", rebateDetailViewModel.VendorID),
                new SqlParameter("@VolumeID", rebateDetailViewModel.VolumeID)
                ).ToList();
            }

            rebateDetailViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.quarterlyPeriodTypeID, 20);
            rebateDetailViewModel.CommissionList = DataHelper.GetRebateCommissionList();
            rebateDetailViewModel.VendorList = DataHelper.GetRebateVendorList();
            rebateDetailViewModel.VolumeList = DataHelper.GetRebateVolumeList(Constants.memberRebateVolumeType);

            return View(rebateDetailViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RebateDetail(RebateDetailViewModel rebateDetailViewModel)
        {
            var DataHelper = new DataHelpers();

            int intCustID = 0;

            if (rebateDetailViewModel.CustID != "")
            {
                if (!Int32.TryParse(rebateDetailViewModel.CustID, out intCustID))
                    intCustID = 0;
            }

            if (intCustID == 0 && rebateDetailViewModel.VendorID == 0)
            {
                ModelState.AddModelError("", "Either Vendor ID or Member ID must be supplied");
            }

            // Initalize the View Model
            if (ModelState.IsValid)
            {

                rebateDetailViewModel.RebateDetailList = db.Database.SqlQuery<RebateDetail>("exec dbo.[LB_GetRebateDetail] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @VendorID, @VolumeID",
                new SqlParameter("@CustID", intCustID),
                new SqlParameter("@PeriodTypeID", Constants.quarterlyPeriodTypeID),
                new SqlParameter("@StartPeriodID", rebateDetailViewModel.StartPeriodID),
                new SqlParameter("@EndPeriodID", rebateDetailViewModel.EndPeriodID),
                new SqlParameter("@CommissionID", rebateDetailViewModel.CommissionID),
                new SqlParameter("@VendorID", rebateDetailViewModel.VendorID),
                new SqlParameter("@VolumeID", rebateDetailViewModel.VolumeID)
                ).ToList();

            }

            rebateDetailViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.quarterlyPeriodTypeID, 20);
            rebateDetailViewModel.CommissionList = DataHelper.GetRebateCommissionList();
            rebateDetailViewModel.VendorList = DataHelper.GetRebateVendorList();
            rebateDetailViewModel.VolumeList = DataHelper.GetRebateVolumeList(Constants.memberRebateVolumeType);

            return View(rebateDetailViewModel);
        }




    }
}