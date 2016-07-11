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

using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using AppTrack.Helpers;

namespace AppTrack.Controllers
{
    //[AuthorizeAdminRedirect(Roles = Constants.adminRoles)]
    public class MemberActivityController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        [HttpGet]
        public ActionResult ActivityList(int ID = 0)
        {
            DateTime scheduledStartDateTime = new DateTime(1900,1,1);
            DateTime scheduledEndDateTime = new DateTime(1900,1,1);
            DateTime completedStartDateTime = new DateTime(1900,1,1);
            DateTime completedEndDateTime = new DateTime(1900,1,1);

            int zero = 0;
            int searchCustID = ID;
            int searchOwnerID = zero;
            int searchVendorID = zero;

            int searchCategoryID = zero;
            string searchActivityStatus = "";

            // Debug.WriteLine("Get-" + searchPhrase + "|" + searchStatus + "|" + startDateTime + "|" + endDateTime);
            var MemberActivityList = db.Database.SqlQuery<MemberActivity>("exec dbo.[LB_GetMemberActivity] @CustID, @OwnerID, @VendorID, @CategoryID, @ScheduledStartDate, @ScheduledEndDate, @CompletedStartDate, @CompletedEndDate, @Status",
            new SqlParameter("@CustID", searchCustID),
            new SqlParameter("@OwnerID", searchOwnerID),
            new SqlParameter("@VendorID", searchVendorID),
            new SqlParameter("@CategoryID", searchCategoryID),
            new SqlParameter("@ScheduledStartDate", scheduledStartDateTime),
            new SqlParameter("@ScheduledEndDate", scheduledEndDateTime),
            new SqlParameter("@CompletedStartDate", completedStartDateTime),
            new SqlParameter("@CompletedEndDate", completedEndDateTime),
            new SqlParameter("@Status", searchActivityStatus)
            ).ToList();

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> searchActivityStatusList = DataHelper.GetStatusSelectList(Constants.activityStatusLookupGroupID, false, true);
            IEnumerable<System.Web.Mvc.SelectListItem> searchCategoryList = DataHelper.GetCategorySelectList(Constants.activityCategoryGroupID, true, false);

            var model = new MemberActivityListViewModel
            {
                SearchCustID = searchCustID,
                SearchOwnerID = searchOwnerID,
                SearchVendorID = searchVendorID,
                SearchCategoryID = searchCategoryID,
                SearchActivityStatus = searchActivityStatus, 
                MemberActivityList = MemberActivityList,
                SearchActivityStatusList = searchActivityStatusList,
                SearchCategoryList = searchCategoryList,
            };

            return PartialView("_MemberActivityList", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActivityList([Bind(Include = "SearchCustID, SearchOwnerID, SearchVendorID, SearchCategoryID, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchActivityStatus")] MemberActivityListViewModel MemberActivityListViewModel)
        {
            ModelState.Remove("Activity");

            int searchCustID = MemberActivityListViewModel.SearchCustID;
            int searchOwnerID = MemberActivityListViewModel.SearchOwnerID;
            int searchVendorID = MemberActivityListViewModel.SearchVendorID;
            int searchCategoryID = MemberActivityListViewModel.SearchCategoryID;

            DateTime scheduledStartDateTime = MemberActivityListViewModel.SearchScheduledStartDate ?? new DateTime(1900,1,1);
            DateTime scheduledEndDateTime = MemberActivityListViewModel.SearchScheduledEndDate ?? new DateTime(1900,1,1);
            DateTime completedStartDateTime = MemberActivityListViewModel.SearchCompletedStartDate ?? new DateTime(1900, 1, 1);
            DateTime completedEndDateTime = MemberActivityListViewModel.SearchCompletedEndDate ?? new DateTime(1900, 1, 1);

            if (scheduledStartDateTime > scheduledEndDateTime)
            {
                scheduledStartDateTime = DateTime.Now.AddDays(-365);
                scheduledEndDateTime = DateTime.Now.AddDays(1);
            }

            if (completedStartDateTime > completedEndDateTime)
            {
                completedStartDateTime = DateTime.Now.AddDays(-365);
                completedEndDateTime = DateTime.Now.AddDays(1);
            }

            string searchActivityStatus = MemberActivityListViewModel.SearchActivityStatus ?? "";

            var MemberActivityList = db.Database.SqlQuery<MemberActivity>("exec dbo.[LB_GetMemberActivity] @CustID, @OwnerID, @VendorID, @CategoryID, @ScheduledStartDate, @ScheduledEndDate, @CompletedStartDate, @CompletedEndDate, @Status",
            new SqlParameter("@CustID", searchCustID),
            new SqlParameter("@OwnerID", searchOwnerID),
            new SqlParameter("@VendorID", searchVendorID),
            new SqlParameter("@CategoryID", searchCategoryID),
            new SqlParameter("@ScheduledStartDate", scheduledStartDateTime),
            new SqlParameter("@ScheduledEndDate", scheduledEndDateTime),
            new SqlParameter("@CompletedStartDate", completedStartDateTime),
            new SqlParameter("@CompletedEndDate", completedEndDateTime),
            new SqlParameter("@Status", searchActivityStatus)
            ).ToList();

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> searchActivityStatusList = DataHelper.GetStatusSelectList(Constants.activityStatusLookupGroupID, false, true);
            IEnumerable<System.Web.Mvc.SelectListItem> searchCategoryList = DataHelper.GetCategorySelectList(Constants.activityCategoryGroupID, true, false);

            MemberActivityListViewModel.MemberActivityList = MemberActivityList;
            MemberActivityListViewModel.SearchActivityStatusList = searchActivityStatusList;
            MemberActivityListViewModel.SearchCategoryList = searchCategoryList;

            ViewBag.ShowSearchForm = "block";

            return PartialView("_MemberActivityList", MemberActivityListViewModel);
        }



        [HttpGet]
        public ActionResult AddActivityInit()
        {
            MemberActivityUpdateViewModel MemberActivityUpdateViewModel = new MemberActivityUpdateViewModel();

            ModelState.Remove("Title");

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Unable to initialize Activity add Form");
            }

            var DataHelper = new DataHelpers();

            MemberActivityUpdateViewModel.EditCategoryList = DataHelper.GetCategorySelectList(Constants.activityCategoryGroupID, true, false);
            MemberActivityUpdateViewModel.EditActivityTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            MemberActivityUpdateViewModel.EditActivityVendorList = DataHelper.GetRebateVendorList(false, false);
            MemberActivityUpdateViewModel.EditActivityStatusList = DataHelper.GetStatusSelectList(Constants.activityStatusLookupGroupID, true, false);

            ViewBag.ShowAddform = true;

            return PartialView("_MemberActivityAdd", MemberActivityUpdateViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddActivityInit([Bind(Include = "SearchCustID, SearchOwnerID, SearchVendorID, SearchCategoryID, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchActivityStatus")] MemberActivityUpdateViewModel MemberActivityUpdateViewModel)
        {
            ModelState.Remove("Title");

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Unable to initialize Activity add Form");
            }

            var DataHelper = new DataHelpers();

            MemberActivityUpdateViewModel.EditCategoryList = DataHelper.GetCategorySelectList(Constants.activityCategoryGroupID, true, false);
            MemberActivityUpdateViewModel.EditActivityTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            MemberActivityUpdateViewModel.EditActivityVendorList = DataHelper.GetRebateVendorList(false, false);
            MemberActivityUpdateViewModel.EditActivityStatusList = DataHelper.GetStatusSelectList(Constants.activityStatusLookupGroupID, true, false);

            ViewBag.ShowAddform = true;

            return PartialView("_MemberActivityAdd", MemberActivityUpdateViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddActivity([Bind(Include = "OwnerID, VendorID, ActivityType, CategoryId, Title, ShortDescription, Description, ScheduledDate, SearchCustID, SearchOwnerID, SearchVendorID, SearchCategoryID, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchActivityStatus")] MemberActivityUpdateViewModel MemberActivityUpdateViewModel)
        {
            ViewBag.Success = "N";

            if (ModelState.IsValid)
            {
                int thisCustID = MemberActivityUpdateViewModel.SearchCustID;
                int thisVendorID = MemberActivityUpdateViewModel.VendorID ?? 0;
                string thisActivityType = MemberActivityUpdateViewModel.ActivityType;
                int thisActivityCategoryID = MemberActivityUpdateViewModel.CategoryID ?? 0;
                string thisActivityTitle = MemberActivityUpdateViewModel.Title;
                string thisActivityShortDescription = MemberActivityUpdateViewModel.ShortDescription;
                string thisActivityDescription = MemberActivityUpdateViewModel.Description;
                Nullable<System.DateTime> thisActivityScheduledDate = MemberActivityUpdateViewModel.ScheduledDate;

                if (thisCustID == 0)
                {
                    ModelState.AddModelError("", "Unable to identify the Member and/or Admin user.");
                }
                else
                {
                    try
                    {
                        ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                        ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                        db.LB_InsertMemberActivity(thisCustID, AdminID, thisVendorID, AdminID, thisActivityType, thisActivityCategoryID, thisActivityTitle, thisActivityShortDescription, thisActivityDescription, thisActivityScheduledDate, AdminID, returnID, returnMessage);

                        var scalarID = (int)returnID.Value;
                        var errorMessage = (string)returnMessage.Value ?? "";

                        if (scalarID == -1)
                        {
                            ModelState.AddModelError("", errorMessage);
                        }
                        else
                        {
                            ViewBag.Success = "Y";
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Error encountered trying to update Activity");
                    }
                }
            }

            var DataHelper = new DataHelpers();

            MemberActivityUpdateViewModel.EditCategoryList = DataHelper.GetCategorySelectList(Constants.activityCategoryGroupID, true, false);
            MemberActivityUpdateViewModel.EditActivityTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            MemberActivityUpdateViewModel.EditActivityVendorList = DataHelper.GetRebateVendorList(false, false);
            MemberActivityUpdateViewModel.EditActivityStatusList = DataHelper.GetStatusSelectList(Constants.activityStatusLookupGroupID, true, false);

            return PartialView("_MemberActivityAdd", MemberActivityUpdateViewModel);
        }



        [HttpGet]
        public ActionResult EditActivityInit(int ActivityID = 0)
        {

            var MemberActivityUpdateViewModel = new MemberActivityUpdateViewModel();

            var MemberActivitySingle = db.Database.SqlQuery<C_ActivityTracking>("exec dbo.[LB_GetMemberActivityByID] @ActivityID",
            new SqlParameter("@ActivityID", ActivityID)
            ).FirstOrDefault();

            if (MemberActivitySingle != null)
            {
                MemberActivityUpdateViewModel.CustID = MemberActivitySingle.CustID;
                MemberActivityUpdateViewModel.OwnerID = MemberActivitySingle.OwnerID;
                MemberActivityUpdateViewModel.VendorID = MemberActivitySingle.VendorID;
                MemberActivityUpdateViewModel.ActivityType = MemberActivitySingle.ActivityType;
                MemberActivityUpdateViewModel.CategoryID = MemberActivitySingle.CategoryID;
                MemberActivityUpdateViewModel.Title = MemberActivitySingle.Title;
                MemberActivityUpdateViewModel.ShortDescription = MemberActivitySingle.ShortDescription;
                MemberActivityUpdateViewModel.Description = MemberActivitySingle.Description;
                MemberActivityUpdateViewModel.ScheduledDate = MemberActivitySingle.ScheduledDate;
                MemberActivityUpdateViewModel.CompletionDate = MemberActivitySingle.CompletionDate;
                MemberActivityUpdateViewModel.Status = MemberActivitySingle.Status;
            }
            else
            {
                ModelState.AddModelError("", "Activity could not be found");
            }

            var DataHelper = new DataHelpers();

            MemberActivityUpdateViewModel.EditCategoryList = DataHelper.GetCategorySelectList(Constants.activityCategoryGroupID, true, false);
            MemberActivityUpdateViewModel.EditActivityTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            MemberActivityUpdateViewModel.EditActivityVendorList = DataHelper.GetRebateVendorList(false, false);
            MemberActivityUpdateViewModel.EditActivityStatusList = DataHelper.GetStatusSelectList(Constants.activityStatusLookupGroupID, true, false);

            ViewBag.ShowEditform = true;

            return PartialView("_MemberActivityEdit", MemberActivityUpdateViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditActivityInit([Bind(Include = "ActivityID, SearchCustID, SearchOwnerID, SearchVendorID, SearchCategoryID, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchActivityStatus")] MemberActivityUpdateViewModel MemberActivityUpdateViewModel)
        {
            ModelState.Remove("Title");

            var MemberActivitySingle = db.Database.SqlQuery<C_ActivityTracking>("exec dbo.[LB_GetMemberActivityByID] @ActivityID",
            new SqlParameter("@ActivityID", MemberActivityUpdateViewModel.ActivityID)
            ).FirstOrDefault();

            if (MemberActivitySingle != null)
            {
                MemberActivityUpdateViewModel.CustID = MemberActivitySingle.CustID;
                MemberActivityUpdateViewModel.OwnerID = MemberActivitySingle.OwnerID;
                MemberActivityUpdateViewModel.VendorID = MemberActivitySingle.VendorID;
                MemberActivityUpdateViewModel.ActivityType = MemberActivitySingle.ActivityType;
                MemberActivityUpdateViewModel.CategoryID = MemberActivitySingle.CategoryID;
                MemberActivityUpdateViewModel.Title = MemberActivitySingle.Title;
                MemberActivityUpdateViewModel.ShortDescription = MemberActivitySingle.ShortDescription;
                MemberActivityUpdateViewModel.Description = MemberActivitySingle.Description;
                MemberActivityUpdateViewModel.Outcome = MemberActivitySingle.Outcome;
                MemberActivityUpdateViewModel.ScheduledDate = MemberActivitySingle.ScheduledDate;
                MemberActivityUpdateViewModel.CompletionDate = MemberActivitySingle.CompletionDate;
                MemberActivityUpdateViewModel.Status = MemberActivitySingle.Status;
            }
            else
            {
                ModelState.AddModelError("", "Activity could not be found");
            }

            var DataHelper = new DataHelpers();

            MemberActivityUpdateViewModel.EditCategoryList = DataHelper.GetCategorySelectList(Constants.activityCategoryGroupID, true, false);
            MemberActivityUpdateViewModel.EditActivityTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            MemberActivityUpdateViewModel.EditActivityVendorList = DataHelper.GetRebateVendorList(false, false);
            if (MemberActivityUpdateViewModel.CategoryID == Constants.activityPrimaOnBoardingCategoryID || MemberActivityUpdateViewModel.CategoryID == Constants.activityAppTrackOnBoardingCategoryID)
            {
                MemberActivityUpdateViewModel.EditActivityStatusList = DataHelper.GetStatusSelectList(Constants.activityOnBoardingStatusLookupGroupID, true, false);
            }
            else
            {
                MemberActivityUpdateViewModel.EditActivityStatusList = DataHelper.GetStatusSelectList(Constants.activityStatusLookupGroupID, true, false);
            }

            ViewBag.ShowEditform = true;

            return PartialView("_MemberActivityEdit", MemberActivityUpdateViewModel);
        }

        
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditActivity([Bind(Include = "ActivityID, CustID, OwnerID, VendorID, ActivityType, CategoryId, Title, ShortDescription, Description, Outcome, ScheduledDate, CompletionDate, Status, SearchCustID, SearchOwnerID, SearchVendorID, SearchCategoryID, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchActivityStatus")] MemberActivityUpdateViewModel MemberActivityUpdateViewModel)
        {
            ViewBag.Success = "N";

            if (ModelState.IsValid)
            {
                int thisActivityID = MemberActivityUpdateViewModel.ActivityID;
                int thisCustID = MemberActivityUpdateViewModel.CustID ?? 0;
                int thisOwnerID = MemberActivityUpdateViewModel.OwnerID ?? 0;
                int thisVendorID = MemberActivityUpdateViewModel.VendorID ?? 0;
                string thisActivityType = MemberActivityUpdateViewModel.ActivityType;
                int thisActivityCategoryID = MemberActivityUpdateViewModel.CategoryID ?? 0;
                string thisActivityTitle = MemberActivityUpdateViewModel.Title;
                string thisActivityShortDescription = MemberActivityUpdateViewModel.ShortDescription;
                string thisActivityDescription = MemberActivityUpdateViewModel.Description;
                string thisActivityOutcome = MemberActivityUpdateViewModel.Outcome;
                Nullable<System.DateTime> thisActivityScheduledDate = MemberActivityUpdateViewModel.ScheduledDate;
                Nullable<System.DateTime> thisActivityCompletionDate = MemberActivityUpdateViewModel.CompletionDate;
                string thisActivityStatus = MemberActivityUpdateViewModel.Status;

                if (thisActivityStatus.ToUpper() == "COMPLETE" && thisActivityCompletionDate == null)
                {
                    ModelState.AddModelError("", "Completion date must be set if status is Completed");
                }
                else
                {
                    //                try
                    //                {
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateMemberActivity(thisActivityID, thisCustID, thisOwnerID, thisVendorID, AdminID, thisActivityType, thisActivityCategoryID, thisActivityTitle, thisActivityShortDescription, thisActivityDescription, thisActivityOutcome, thisActivityScheduledDate, thisActivityCompletionDate, thisActivityStatus, returnID, returnMessage);

                    var scalarID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        ViewBag.Success = "Y";
                    }
                    //                }
                    //                catch
                    //                {
                    //                    ModelState.AddModelError("", "Error encountered trying to update Activity");
                    //                }
                }
            }

            var DataHelper = new DataHelpers();

            MemberActivityUpdateViewModel.EditCategoryList = DataHelper.GetCategorySelectList(Constants.activityCategoryGroupID, true, false);
            MemberActivityUpdateViewModel.EditActivityTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            MemberActivityUpdateViewModel.EditActivityVendorList = DataHelper.GetRebateVendorList(true, false);
            MemberActivityUpdateViewModel.EditActivityStatusList = DataHelper.GetStatusSelectList(Constants.activityStatusLookupGroupID, true, false);

            return PartialView("_MemberActivityEdit", MemberActivityUpdateViewModel);
        }



        [HttpGet]
        public ActionResult ActivityReports()
        {
            return View("MemberActivityReports");
        }


        [HttpGet]
        public ActionResult ActivitySummary(int ID = 0)
        {
            DateTime scheduledStartDateTime = new DateTime(1900, 1, 1);
            DateTime scheduledEndDateTime = new DateTime(1900, 1, 1);
            DateTime completedStartDateTime = new DateTime(1900, 1, 1);
            DateTime completedEndDateTime = new DateTime(1900, 1, 1);

            int zero = 0;
            int searchCategoryID = zero;
            string searchActivityStatus = "";

            // Debug.WriteLine("Get-" + searchPhrase + "|" + searchStatus + "|" + startDateTime + "|" + endDateTime);
            var memberActivitySummaryList = db.Database.SqlQuery<MemberActivitySummary>("exec dbo.[LB_GetMemberActivitySummary] @CategoryID, @ScheduledStartDate, @ScheduledEndDate, @CompletedStartDate, @CompletedEndDate, @Status",
            new SqlParameter("@CategoryID", searchCategoryID),
            new SqlParameter("@ScheduledStartDate", scheduledStartDateTime),
            new SqlParameter("@ScheduledEndDate", scheduledEndDateTime),
            new SqlParameter("@CompletedStartDate", completedStartDateTime),
            new SqlParameter("@CompletedEndDate", completedEndDateTime),
            new SqlParameter("@Status", searchActivityStatus)
            ).ToList();

            var DataHelper = new DataHelpers();


            var model = new MemberActivitySummaryViewModel
            {
                SearchCategoryID = searchCategoryID,
                SearchActivityStatus = searchActivityStatus,
                MemberActivitySummaryList = memberActivitySummaryList,
            };

            model.SearchActivityStatusList = DataHelper.GetStatusSelectList(Constants.activityOnBoardingStatusLookupGroupID, false, true);
            model.EditCategoryList = DataHelper.GetCategorySelectList(Constants.activityCategoryGroupID, false, true);

            return PartialView("_MemberActivitySummary", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActivitySummary([Bind(Include = "SearchCategoryID, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchActivityStatus")] MemberActivitySummaryViewModel MemberActivitySummaryViewModel)
        {

            int searchCategoryID = MemberActivitySummaryViewModel.SearchCategoryID;

            DateTime scheduledStartDateTime = MemberActivitySummaryViewModel.SearchScheduledStartDate ?? new DateTime(1900, 1, 1);
            DateTime scheduledEndDateTime = MemberActivitySummaryViewModel.SearchScheduledEndDate ?? new DateTime(1900, 1, 1);
            DateTime completedStartDateTime = MemberActivitySummaryViewModel.SearchCompletedStartDate ?? new DateTime(1900, 1, 1);
            DateTime completedEndDateTime = MemberActivitySummaryViewModel.SearchCompletedEndDate ?? new DateTime(1900, 1, 1);

            if (scheduledStartDateTime > scheduledEndDateTime)
            {
                scheduledStartDateTime = DateTime.Now.AddDays(-365);
                scheduledEndDateTime = DateTime.Now.AddDays(1);
            }

            if (completedStartDateTime > completedEndDateTime)
            {
                completedStartDateTime = DateTime.Now.AddDays(-365);
                completedEndDateTime = DateTime.Now.AddDays(1);
            }

            string searchActivityStatus = MemberActivitySummaryViewModel.SearchActivityStatus ?? "";

            var memberActivitySummaryList = db.Database.SqlQuery<MemberActivitySummary>("exec dbo.[LB_GetMemberActivitySummary] @CategoryID, @ScheduledStartDate, @ScheduledEndDate, @CompletedStartDate, @CompletedEndDate, @Status",
            new SqlParameter("@CategoryID", searchCategoryID),
            new SqlParameter("@ScheduledStartDate", scheduledStartDateTime),
            new SqlParameter("@ScheduledEndDate", scheduledEndDateTime),
            new SqlParameter("@CompletedStartDate", completedStartDateTime),
            new SqlParameter("@CompletedEndDate", completedEndDateTime),
            new SqlParameter("@Status", searchActivityStatus)
            ).ToList();

            var DataHelper = new DataHelpers();

            MemberActivitySummaryViewModel.MemberActivitySummaryList = memberActivitySummaryList;

            MemberActivitySummaryViewModel.SearchActivityStatusList = DataHelper.GetStatusSelectList(Constants.activityStatusLookupGroupID, false, true);
            MemberActivitySummaryViewModel.EditCategoryList = DataHelper.GetCategorySelectList(Constants.activityCategoryGroupID, false, true);

            ViewBag.ShowSearchForm = "block";

            return PartialView("_MemberActivitySummary", MemberActivitySummaryViewModel);
        }

        [HttpGet]
        public ActionResult ActivityDetail(int ID = 0)
        {
            DateTime startDateTime = new DateTime(1900, 1, 1);
            DateTime endDateTime = new DateTime(1900, 1, 1);
            DateTime scheduledStartDateTime = new DateTime(1900, 1, 1);
            DateTime scheduledEndDateTime = new DateTime(1900, 1, 1);
            DateTime completedStartDateTime = new DateTime(1900, 1, 1);
            DateTime completedEndDateTime = new DateTime(1900, 1, 1);

            int zero = 0;
            int searchCustID = ID;
            int searchOwnerID = zero;
            int searchVendorID = zero;
            int searchCategoryID = zero;
            int searchStoreID = zero;

            string searchActivityStatus = "";

            // Debug.WriteLine("Get-" + searchPhrase + "|" + searchStatus + "|" + startDateTime + "|" + endDateTime);
            var MemberActivityList = db.Database.SqlQuery<MemberActivity>("exec dbo.[LB_GetMemberActivityDetail] @CustID, @OwnerID, @VendorID, @CategoryID, @StoreID, @StartDate, @EndDate, @ScheduledStartDate, @ScheduledEndDate, @CompletedStartDate, @CompletedEndDate, @Status",
            new SqlParameter("@CustID", searchCustID),
            new SqlParameter("@OwnerID", searchOwnerID),
            new SqlParameter("@VendorID", searchVendorID),
            new SqlParameter("@CategoryID", searchCategoryID),
            new SqlParameter("@StoreID", searchStoreID),
            new SqlParameter("@StartDate", scheduledStartDateTime),
            new SqlParameter("@EndDate", scheduledEndDateTime),
            new SqlParameter("@ScheduledStartDate", scheduledStartDateTime),
            new SqlParameter("@ScheduledEndDate", scheduledEndDateTime),
            new SqlParameter("@CompletedStartDate", completedStartDateTime),
            new SqlParameter("@CompletedEndDate", completedEndDateTime),
            new SqlParameter("@Status", searchActivityStatus)
            ).ToList();

            var DataHelper = new DataHelpers();

            var MemberActivityListViewModel = new MemberActivityListViewModel
            {
                SearchCustID = searchCustID,
                SearchOwnerID = searchOwnerID,
                SearchVendorID = searchVendorID,
                SearchCategoryID = searchCategoryID,
                SearchActivityStatus = searchActivityStatus,
                MemberActivityList = MemberActivityList,
                ScheduledDateOption = "",
                CompletedDateOption = "",
            };

            MemberActivityListViewModel.SearchCategoryList = DataHelper.GetCategorySelectList(Constants.activityCategoryGroupID, false, true);
            MemberActivityListViewModel.SearchActivityTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, false, true);
            MemberActivityListViewModel.SearchActivityVendorList = DataHelper.GetRebateVendorList(false, true);
            MemberActivityListViewModel.SearchActivityStatusList = DataHelper.GetStatusSelectList(Constants.activityStatusLookupGroupID, false, true);
            MemberActivityListViewModel.SearchAssignedToList = DataHelper.GetAdminSelectList(true);
            MemberActivityListViewModel.SearchStoreList = DataHelper.GetStoreList(true);

            return PartialView("_MemberActivityDetail", MemberActivityListViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActivityDetail([Bind(Include = "SearchCustID, SearchOwnerID, SearchVendorID, SearchCategoryID, SearchStoreID, SearchStartDate, SearchEndDate, ScheduledDateOption, SearchScheduledStartDate, SearchScheduledEndDate, CompletedDateOption, SearchCompletedStartDate, SearchCompletedEndDate, SearchActivityStatus")] MemberActivityListViewModel MemberActivityListViewModel)
        {
            ModelState.Remove("Activity");

            int searchCustID = MemberActivityListViewModel.SearchCustID;
            int searchOwnerID = MemberActivityListViewModel.SearchOwnerID;
            int searchVendorID = MemberActivityListViewModel.SearchVendorID;
            int searchCategoryID = MemberActivityListViewModel.SearchCategoryID;
            int searchStoreID = MemberActivityListViewModel.SearchStoreID;

            DateTime startDateTime = MemberActivityListViewModel.SearchStartDate ?? new DateTime(1900, 1, 1);
            DateTime endDateTime = MemberActivityListViewModel.SearchEndDate ?? new DateTime(1900, 1, 1);
            DateTime scheduledStartDateTime = MemberActivityListViewModel.SearchScheduledStartDate ?? new DateTime(1900, 1, 1);
            DateTime scheduledEndDateTime = MemberActivityListViewModel.SearchScheduledEndDate ?? new DateTime(1900, 1, 1);
            DateTime completedStartDateTime = MemberActivityListViewModel.SearchCompletedStartDate ?? new DateTime(1900, 1, 1);
            DateTime completedEndDateTime = MemberActivityListViewModel.SearchCompletedEndDate ?? new DateTime(1900, 1, 1);

            if (startDateTime > endDateTime)
            {
                startDateTime = DateTime.Now.AddDays(-365);
                endDateTime = DateTime.Now.AddDays(1);
            }

            if (scheduledStartDateTime > scheduledEndDateTime)
            {
                scheduledStartDateTime = DateTime.Now.AddDays(-365);
                scheduledEndDateTime = DateTime.Now.AddDays(1);
            }

            if (completedStartDateTime > completedEndDateTime)
            {
                completedStartDateTime = DateTime.Now.AddDays(-365);
                completedEndDateTime = DateTime.Now.AddDays(1);
            }

            string searchActivityStatus = MemberActivityListViewModel.SearchActivityStatus ?? "";

            var MemberActivityList = db.Database.SqlQuery<MemberActivity>("exec dbo.[LB_GetMemberActivityDetail] @CustID, @OwnerID, @VendorID, @CategoryID, @StoreID, @StartDate, @EndDate, @ScheduledStartDate, @ScheduledEndDate, @CompletedStartDate, @CompletedEndDate, @Status",
            new SqlParameter("@CustID", searchCustID),
            new SqlParameter("@OwnerID", searchOwnerID),
            new SqlParameter("@VendorID", searchVendorID),
            new SqlParameter("@CategoryID", searchCategoryID),
            new SqlParameter("@StoreID", searchStoreID),
            new SqlParameter("@StartDate", scheduledStartDateTime),
            new SqlParameter("@EndDate", scheduledEndDateTime),
            new SqlParameter("@ScheduledStartDate", scheduledStartDateTime),
            new SqlParameter("@ScheduledEndDate", scheduledEndDateTime),
            new SqlParameter("@CompletedStartDate", completedStartDateTime),
            new SqlParameter("@CompletedEndDate", completedEndDateTime),
            new SqlParameter("@Status", searchActivityStatus)
            ).ToList();

            var DataHelper = new DataHelpers();

            MemberActivityListViewModel.MemberActivityList = MemberActivityList;

            MemberActivityListViewModel.SearchCategoryList = DataHelper.GetCategorySelectList(Constants.activityCategoryGroupID, false, true);
            MemberActivityListViewModel.SearchActivityTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, false, true);
            MemberActivityListViewModel.SearchActivityVendorList = DataHelper.GetRebateVendorList(false, true);
            MemberActivityListViewModel.SearchActivityStatusList = DataHelper.GetStatusSelectList(Constants.activityStatusLookupGroupID, false, true);
            MemberActivityListViewModel.SearchAssignedToList = DataHelper.GetAdminSelectList(true);
            MemberActivityListViewModel.SearchStoreList = DataHelper.GetStoreList(true);

            ViewBag.ShowSearchForm = "block";

            DateTime nullDateTime = new DateTime(1900, 1, 1);
            DateTime anyDateTime = new DateTime(2900, 1, 1);

            if (scheduledStartDateTime == nullDateTime || scheduledStartDateTime == anyDateTime)
            {
                MemberActivityListViewModel.SearchScheduledStartDate = null;
                MemberActivityListViewModel.SearchScheduledEndDate = null;
            }

            if (completedStartDateTime == nullDateTime || completedStartDateTime == anyDateTime)
            {
                MemberActivityListViewModel.SearchCompletedStartDate = null;
                MemberActivityListViewModel.SearchCompletedEndDate = null;
            }

            if (ModelState.IsValid)
            {
                // clear the values in the model state so the view will use the values in the model to populate the form
                // needed whenever we override the input by assigning new values to the model, such as the null dates
                ModelState.Clear();
            }

            return PartialView("_MemberActivityDetail", MemberActivityListViewModel);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}
