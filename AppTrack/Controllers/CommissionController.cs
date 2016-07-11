using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;


using System.Collections.Generic;

using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using AppTrack.Helpers;

namespace AppTrack.Controllers
{
    [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
    public class CommissionController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
        private DataHelpers DataHelper = new DataHelpers();

        //
        // INDEX - GET
        //
        [HttpGet]
        public ActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public ActionResult CommissionList(int id = 0, int startPeriodID = 0, int endPeriodID = 0, int searchCommissionID = 0)
        {
            int searchCustID = id;

            if (searchCustID > 0)
            {
                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(searchCustID);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ViewBag.ErrorCode = Constants.fatalErrorCode;
                    ViewBag.ErrorMessage = "Invalid Customer ID provided, unable to retrieve Commission List";
                    return PartialView("_CommissionList");
                }

                ViewBag.DisplayName = checkCustomerResult.C_Info.DisplayName;
            }

            // Initalize the View Model

            var commissionDetailListViewModel = new CommissionDetailListViewModel();

            string searchStatus = " ";

            int periodTypeID = Constants.monthlyPeriodTypeID;

            commissionDetailListViewModel.CommissionDetailList = db.Database.SqlQuery<CommissionDetail>("exec dbo.[LB_GetCommissionDetailByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
             new SqlParameter("@CustID", searchCustID),
             new SqlParameter("@PeriodTypeID", periodTypeID),
             new SqlParameter("@StartPeriodID", startPeriodID),
             new SqlParameter("@EndPeriodID", endPeriodID),
             new SqlParameter("@CommissionID", searchCommissionID),
             new SqlParameter("@Status", searchStatus)
            ).ToList();

            commissionDetailListViewModel.StartPeriodID = startPeriodID;
            commissionDetailListViewModel.EndPeriodID = endPeriodID;
            commissionDetailListViewModel.SearchCustID = searchCustID;
            commissionDetailListViewModel.SearchCommissionID = searchCommissionID;
            commissionDetailListViewModel.SearchStatus = searchStatus;

            commissionDetailListViewModel.SearchCustIDList = DataHelper.GetCustIDList(Constants.memberDirectorCustomerType, false, true);

            commissionDetailListViewModel.SearchPeriodIDList = DataHelper.GetPeriodIDList(Constants.monthlyPeriodTypeID, 24, false, true);

            commissionDetailListViewModel.SearchCommissionIDList = DataHelper.GetManualCommissionIDSelectList(false, true);
             

            commissionDetailListViewModel.SearchStatusList = DataHelper.GetStatusSelectList(Constants.commissionStatusLookupGroupID, false, true);

            return PartialView("_CommissionList", commissionDetailListViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CommissionList([Bind(Include = "StartPeriodID, EndPeriodID, SearchCustID, SearchCommissionID, SearchStatus")] CommissionDetailListViewModel commissionDetailListViewModel)
        {
            commissionDetailListViewModel.CommissionDetailList = db.Database.SqlQuery<CommissionDetail>("exec dbo.[LB_GetCommissionDetailByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
             new SqlParameter("@CustID", commissionDetailListViewModel.SearchCustID),
             new SqlParameter("@PeriodTypeID", Constants.monthlyPeriodTypeID),
             new SqlParameter("@StartPeriodID", commissionDetailListViewModel.StartPeriodID),
             new SqlParameter("@EndPeriodID", commissionDetailListViewModel.EndPeriodID),
             new SqlParameter("@CommissionID", commissionDetailListViewModel.SearchCommissionID),
             new SqlParameter("@Status", commissionDetailListViewModel.SearchStatus)
            ).ToList();

            commissionDetailListViewModel.SearchCustIDList = DataHelper.GetCustIDList(Constants.memberDirectorCustomerType, false, true);

            commissionDetailListViewModel.SearchPeriodIDList = DataHelper.GetPeriodIDList(Constants.monthlyPeriodTypeID, 24, false, true);

            commissionDetailListViewModel.SearchCommissionIDList = DataHelper.GetManualCommissionIDSelectList(false, true);

            commissionDetailListViewModel.SearchStatusList = DataHelper.GetStatusSelectList(Constants.commissionStatusLookupGroupID, false, true);

            return PartialView("_CommissionList", commissionDetailListViewModel);
        }

        [HttpGet]
        public ActionResult VolumeList(int id = 0)
        {
            int CDID = id;

            if (CDID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Commission Detail ID supplied to Volume Detail List";
                return RedirectToAction("Error", "Admin");
            }

            ViewBag.CDID = CDID;

            return View();
        }

        [HttpGet]
        public ActionResult VolumeDetailList(int id = 0)
        {
            int CDID = id;

            if (CDID == 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Customer ID provided, unable to retrieve Commission List";
                return PartialView("_VolumeDetailListWithHeader");
            }

            // Initalize the View Model

            VolumeDetailListViewModel volumeDetailListViewModel = new VolumeDetailListViewModel();

            volumeDetailListViewModel.VolumeDetailList = db.Database.SqlQuery<VolumeDetail>("exec dbo.[LB_GetVolumeDetailByCDID] @CDID",
             new SqlParameter("@CDID", CDID)
            ).ToList();

            return PartialView("_VolumeDetailListWithHeader", volumeDetailListViewModel);
        }

        [HttpGet]
        public ActionResult RebateVolumeDetailList(int id = 0)
        {
            int CHID = id;

            if (CHID == 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Customer ID provided, unable to retrieve Commission List";
                return PartialView("_RebateVolumeDetailListWithHeader");
            }

            // Initalize the View Model

            VolumeDetailListViewModel volumeDetailListViewModel = new VolumeDetailListViewModel();

            volumeDetailListViewModel.VolumeDetailList = db.Database.SqlQuery<VolumeDetail>("exec dbo.[LB_GetVolumeDetailRebateByCHID] @CHID",
             new SqlParameter("@CHID", CHID)
            ).ToList();

            return PartialView("_RebateVolumeDetailListWithHeader", volumeDetailListViewModel);
        }

        [HttpGet]
        public ActionResult RebateCommissionDetailList(int id = 0)
        {
            int CHID = id;

            if (CHID == 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Customer ID provided, unable to retrieve Commission List";
                return PartialView("_CommissionDetailList");
            }

            // Initalize the View Model

            CommissionDetailListViewModel commissionDetailListViewModel = new CommissionDetailListViewModel();

            commissionDetailListViewModel.CommissionDetailList = db.Database.SqlQuery<CommissionDetail>("exec dbo.[LB_GetCommissionDetailByCHID] @CHID",
             new SqlParameter("@CHID", CHID)
            ).ToList();

            return PartialView("_RebateCommissionDetailList", commissionDetailListViewModel);
        }


        [HttpGet]
        public ActionResult CommissionSummary()
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model

            var commissionSummaryViewModel = new CommissionSummaryViewModel();

            int zero = 0;

            commissionSummaryViewModel.StartPeriodID = zero;
            commissionSummaryViewModel.EndPeriodID = zero;

            commissionSummaryViewModel.CommissionSummaryList = db.Database.SqlQuery<CommissionSummary>("exec dbo.[LB_GetCommissionDetailSummary] @PeriodTypeID, @StartPeriodID, @EndPeriodID",
            new SqlParameter("@PeriodTypeID", Constants.monthlyPeriodTypeID),
            new SqlParameter("@StartPeriodID", commissionSummaryViewModel.StartPeriodID),
            new SqlParameter("@EndPeriodID", commissionSummaryViewModel.EndPeriodID)
            ).ToList();

            commissionSummaryViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.monthlyPeriodTypeID, 18);
            commissionSummaryViewModel.CommissionList = DataHelper.GetManualCommissionIDSelectList(false, true);

            return View(commissionSummaryViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CommissionSummary(CommissionSummaryViewModel commissionSummaryViewModel)
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model
            if (ModelState.IsValid)
            {

                commissionSummaryViewModel.CommissionSummaryList = db.Database.SqlQuery<CommissionSummary>("exec dbo.[LB_GetCommissionDetailSummary] @PeriodTypeID, @StartPeriodID, @EndPeriodID",
                new SqlParameter("@PeriodTypeID", Constants.quarterlyPeriodTypeID),
                new SqlParameter("@StartPeriodID", commissionSummaryViewModel.StartPeriodID),
                new SqlParameter("@EndPeriodID", commissionSummaryViewModel.EndPeriodID)
                ).ToList();

            }

            commissionSummaryViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.monthlyPeriodTypeID, 18);
            commissionSummaryViewModel.CommissionList = DataHelper.GetManualCommissionIDSelectList(false, true);

            return View(commissionSummaryViewModel);
        }

        [HttpGet]
        public ActionResult CommissionDetail(int startPeriodID = 0, int endPeriodID = 0, int commissionID = 0)
        {
            ViewBag.StartPeriodID = startPeriodID;
            ViewBag.EndPeriodID = endPeriodID;
            ViewBag.CommissionID = commissionID;

            return View();
        }
    }
}