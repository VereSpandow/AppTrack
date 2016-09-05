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
    [AuthorizeAdminRedirect(Roles = Constants.adminRoles + ",Member")]
    public class CustomerNotesController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        [HttpGet]
        public ActionResult TaskList()
        {
            DateTime startDateTime = new DateTime(1900, 1, 1);
            DateTime endDateTime = new DateTime(1900, 1, 1);
            DateTime scheduledStartDateTime = new DateTime(1900, 1, 1);
            DateTime scheduledEndDateTime = new DateTime(1900, 1, 1);
            DateTime completedStartDateTime = new DateTime(1900, 1, 1);
            DateTime completedEndDateTime = new DateTime(1900, 1, 1);

            int zero = 0;
            int searchActivityID = zero;
            int searchCustID = zero;
            int searchVendorID = zero;

            if (User.IsInRole("Member"))
            {
                searchCustID = CustID;
            }

            string searchPhrase = "";
            string searchNoteStatus = "New";
            string searchNoteType = "Task";

/*
            var CustomerNoteList = db.Database.SqlQuery<CustomerNote>("exec dbo.[LB_GetNotesV2] @CustID, @ActivityID, @AssignedToID, @StartDate, @EndDate, @ScheduledStartDate, @ScheduledEndDate, @CompletedStartDate, @CompletedEndDate, @SearchPhrase, @Status, @NoteType, @CommType, @CommDirection, @AdminID",
            new SqlParameter("@CustID", searchCustID),
            new SqlParameter("@ActivityID", searchActivityID),
            new SqlParameter("@AssignedToID", searchAssignedToID),
            new SqlParameter("@StartDate", startDateTime),
            new SqlParameter("@EndDate", endDateTime),
            new SqlParameter("@ScheduledStartDate", scheduledStartDateTime),
            new SqlParameter("@ScheduledEndDate", scheduledEndDateTime),
            new SqlParameter("@CompletedStartDate", completedStartDateTime),
            new SqlParameter("@CompletedEndDate", completedEndDateTime),
            new SqlParameter("@SearchPhrase", searchPhrase),
            new SqlParameter("@Status", searchNoteStatus),
            new SqlParameter("@NoteType", searchNoteType),
            new SqlParameter("@CommType", searchCommType),
            new SqlParameter("@CommDirection", searchCommDirection),
            new SqlParameter("@AdminID", zero)
            ).ToList();
*/

            var CustomerNoteList = db.Database.SqlQuery<CustomerNote>("exec dbo.[LB_GetNotesV3] @CustID, @ActivityID, @VendorID,  @SearchPhrase, @Status, @NoteType, @StartDate, @EndDate, @ScheduledStartDate, @ScheduledEndDate, @CompletedStartDate, @CompletedEndDate",
            new SqlParameter("@CustID", searchCustID),
            new SqlParameter("@ActivityID", searchActivityID),
            new SqlParameter("@VendorID", searchVendorID),
            new SqlParameter("@SearchPhrase", searchPhrase),
            new SqlParameter("@Status", searchNoteStatus),
            new SqlParameter("@NoteType", searchNoteType),
            new SqlParameter("@StartDate", startDateTime),
            new SqlParameter("@EndDate", endDateTime),
            new SqlParameter("@ScheduledStartDate", scheduledStartDateTime),
            new SqlParameter("@ScheduledEndDate", scheduledEndDateTime),
            new SqlParameter("@CompletedStartDate", completedStartDateTime),
            new SqlParameter("@CompletedEndDate", completedEndDateTime)
            ).ToList();

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> searchNoteTypeList = DataHelper.GetStatusSelectList(Constants.noteTypeLookupGroupID, true, true);
            IEnumerable<System.Web.Mvc.SelectListItem> searchNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, true);

            var model = new CustomerNoteListViewModel
            {
                CustomerNoteList = CustomerNoteList,
                SearchCustID = searchCustID,
                SearchActivityID = searchActivityID,
                SearchVendorID = searchVendorID,
                SearchNoteType = searchNoteType,
                SearchPhrase = searchPhrase,
                SearchNoteStatus = searchNoteStatus,
                SearchNoteTypeList = searchNoteTypeList,
                SearchNoteStatusList = searchNoteStatusList,
            };

            return View("TaskList", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaskList([Bind(Include = "SearchCustID, SearchActivityID, SearchVendorID, SearchStartDate, SearchEndDate, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchNoteType, SearchPhrase, SearchNoteStatus")] CustomerNoteListViewModel CustomerNoteListViewModel)
        {

            int zero = 0;

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Unable to initialize Note add Form");
            }
            else
            {
                int searchCustID = CustomerNoteListViewModel.SearchCustID;
                int searchActivityID = CustomerNoteListViewModel.SearchActivityID;
                int searchVendorID = CustomerNoteListViewModel.SearchVendorID;

                DateTime startDateTime = CustomerNoteListViewModel.SearchStartDate ?? new DateTime(1900, 1, 1);
                DateTime endDateTime = CustomerNoteListViewModel.SearchEndDate ?? new DateTime(1900, 1, 1);
                DateTime scheduledStartDateTime = CustomerNoteListViewModel.SearchScheduledStartDate ?? new DateTime(1900, 1, 1);
                DateTime scheduledEndDateTime = CustomerNoteListViewModel.SearchScheduledEndDate ?? new DateTime(1900, 1, 1);
                DateTime completedStartDateTime = CustomerNoteListViewModel.SearchCompletedStartDate ?? new DateTime(1900, 1, 1);
                DateTime completedEndDateTime = CustomerNoteListViewModel.SearchCompletedEndDate ?? new DateTime(1900, 1, 1);

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

                string searchNoteType = CustomerNoteListViewModel.SearchNoteType ?? "";
                string searchPhrase = CustomerNoteListViewModel.SearchPhrase ?? "";
                string searchNoteStatus = CustomerNoteListViewModel.SearchNoteStatus ?? "";

                var CustomerNoteList = db.Database.SqlQuery<CustomerNote>("exec dbo.[LB_GetNotesV3] @CustID, @ActivityID, @VendorID, @SearchPhrase, @Status, @NoteType, @StartDate, @EndDate, @ScheduledStartDate, @ScheduledEndDate, @CompletedStartDate, @CompletedEndDate",
                new SqlParameter("@CustID", searchCustID),
                new SqlParameter("@ActivityID", searchActivityID),
                new SqlParameter("@VendorID", searchVendorID),
                new SqlParameter("@SearchPhrase", searchPhrase),
                new SqlParameter("@Status", searchNoteStatus),
                new SqlParameter("@NoteType", searchNoteType),
                new SqlParameter("@StartDate", startDateTime),
                new SqlParameter("@EndDate", endDateTime),
                new SqlParameter("@ScheduledStartDate", scheduledStartDateTime),
                new SqlParameter("@ScheduledEndDate", scheduledEndDateTime),
                new SqlParameter("@CompletedStartDate", completedStartDateTime),
                new SqlParameter("@CompletedEndDate", completedEndDateTime)
                ).ToList();

                CustomerNoteListViewModel.CustomerNoteList = CustomerNoteList;

                DateTime nullDateTime = new DateTime(1900, 1, 1);
                DateTime anyDateTime = new DateTime(2900, 1, 1);

                if (startDateTime == nullDateTime || startDateTime == anyDateTime)
                {
                    CustomerNoteListViewModel.SearchStartDate = null;
                    CustomerNoteListViewModel.SearchEndDate = null;
                }
                if (scheduledStartDateTime == nullDateTime || scheduledStartDateTime == anyDateTime)
                {
                    CustomerNoteListViewModel.SearchScheduledStartDate = null;
                    CustomerNoteListViewModel.SearchScheduledEndDate = null;
                }
                if (completedStartDateTime == nullDateTime || completedStartDateTime == anyDateTime)
                {
                    CustomerNoteListViewModel.SearchCompletedStartDate = null;
                    CustomerNoteListViewModel.SearchCompletedEndDate = null;
                }
            }
            if (ModelState.IsValid)
            {
                // clear the values in the model state so the view will use the values in the model to populate the form
                // needed whenever we override the input by assigning new values to the model, such as the null dates
                ModelState.Clear();
            }

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> searchNoteTypeList = DataHelper.GetStatusSelectList(Constants.noteTypeLookupGroupID, true, true);
            IEnumerable<System.Web.Mvc.SelectListItem> searchNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, true);

            CustomerNoteListViewModel.SearchNoteTypeList = searchNoteTypeList;
            CustomerNoteListViewModel.SearchNoteStatusList = searchNoteStatusList;

            return View("TaskList", CustomerNoteListViewModel);
        }


        [HttpGet]
        public ActionResult NotesList(int ID = 0, int ActivityID = -1)
        {
            DateTime startDateTime = new DateTime(1900, 1, 1);
            DateTime endDateTime = new DateTime(1900, 1, 1);
            DateTime scheduledStartDateTime = new DateTime(1900, 1, 1);
            DateTime scheduledEndDateTime = new DateTime(1900, 1, 1);
            DateTime completedStartDateTime = new DateTime(1900, 1, 1);
            DateTime completedEndDateTime = new DateTime(1900, 1, 1);

            int zero = 0;
            int searchActivityID = ActivityID;
            int searchCustID = ID;
            int searchAssignedToID = zero;

            if (User.IsInRole("Member"))
            {
                searchCustID = CustID;
            }

            string searchPhrase = "";
            string searchNoteStatus = "";
            string searchNoteType = "";
            string searchCommType = "";
            string searchCommDirection = "";

            var CustomerNoteList = db.Database.SqlQuery<CustomerNote>("exec dbo.[LB_GetNotesV2] @CustID, @ActivityID, @AssignedToID, @StartDate, @EndDate, @ScheduledStartDate, @ScheduledEndDate, @CompletedStartDate, @CompletedEndDate, @SearchPhrase, @Status, @NoteType, @CommType, @CommDirection, @AdminID",
            new SqlParameter("@CustID", searchCustID),
            new SqlParameter("@ActivityID", searchActivityID),
            new SqlParameter("@AssignedToID", searchAssignedToID),
            new SqlParameter("@StartDate", startDateTime),
            new SqlParameter("@EndDate", endDateTime),
            new SqlParameter("@ScheduledStartDate", scheduledStartDateTime),
            new SqlParameter("@ScheduledEndDate", scheduledEndDateTime),
            new SqlParameter("@CompletedStartDate", completedStartDateTime),
            new SqlParameter("@CompletedEndDate", completedEndDateTime),
            new SqlParameter("@SearchPhrase", searchPhrase),
            new SqlParameter("@Status", searchNoteStatus),
            new SqlParameter("@NoteType", searchNoteType),
            new SqlParameter("@CommType", searchCommType),
            new SqlParameter("@CommDirection", searchCommDirection),
            new SqlParameter("@AdminID", zero)
            ).ToList();

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> searchNoteTypeList = DataHelper.GetStatusSelectList(Constants.noteTypeLookupGroupID, true, true);
            IEnumerable<System.Web.Mvc.SelectListItem> searchCommTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, true);
            IEnumerable<System.Web.Mvc.SelectListItem> searchCommDirectionList = DataHelper.GetStatusSelectList(Constants.commDirectionLookupGroupID, true, true);
            IEnumerable<System.Web.Mvc.SelectListItem> searchNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, true);
            IEnumerable<System.Web.Mvc.SelectListItem> searchAssignedToList = DataHelper.GetAdminSelectList();

            var model = new CustomerNoteListViewModel
            {
                CustomerNoteList = CustomerNoteList,
                SearchCustID = searchCustID,
                SearchActivityID = searchActivityID,
                SearchAssignedToID = searchAssignedToID,
                SearchNoteType = searchNoteType,
                SearchCommType = searchCommType,
                SearchCommDirection = searchCommDirection,
                SearchPhrase = searchPhrase,
                SearchNoteStatus = searchNoteStatus,
                SearchAssignedToList = searchAssignedToList,
                SearchNoteTypeList = searchNoteTypeList,
                SearchCommTypeList = searchCommTypeList,
                SearchCommDirectionList = searchCommDirectionList,
                SearchNoteStatusList = searchNoteStatusList,
            };

            if (searchActivityID == -1)
            {
                ViewBag.ShowAddEdit = "N";
            }
            else
            {
                ViewBag.ShowAddEdit = "Y";
            }
            return PartialView("_CustomerNotes", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NotesList([Bind(Include = "SearchCustID, SearchActivityID, SearchAssignedToID, SearchStartDate, SearchEndDate, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchNoteType, SearchCommType, SearchCommDirection, SearchPhrase, SearchNoteStatus")] CustomerNoteListViewModel CustomerNoteListViewModel)
        {

            ViewBag.ShowAddEdit = "N";

            int zero = 0;

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Unable to generate note list");
            }
            else
            {
                int searchCustID = CustomerNoteListViewModel.SearchCustID;
                int searchActivityID = CustomerNoteListViewModel.SearchActivityID;
                int searchAssignedToID = CustomerNoteListViewModel.SearchAssignedToID;

                DateTime startDateTime = CustomerNoteListViewModel.SearchStartDate ?? new DateTime(1900, 1, 1);
                DateTime endDateTime = CustomerNoteListViewModel.SearchEndDate ?? new DateTime(1900, 1, 1);
                DateTime scheduledStartDateTime = CustomerNoteListViewModel.SearchScheduledStartDate ?? new DateTime(1900, 1, 1);
                DateTime scheduledEndDateTime = CustomerNoteListViewModel.SearchScheduledEndDate ?? new DateTime(1900, 1, 1);
                DateTime completedStartDateTime = CustomerNoteListViewModel.SearchCompletedStartDate ?? new DateTime(1900, 1, 1);
                DateTime completedEndDateTime = CustomerNoteListViewModel.SearchCompletedEndDate ?? new DateTime(1900, 1, 1);

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

                string searchNoteType = CustomerNoteListViewModel.SearchNoteType ?? "";
                string searchCommType = CustomerNoteListViewModel.SearchCommType ?? "";
                string searchCommDirection = CustomerNoteListViewModel.SearchCommDirection ?? "";
                string searchPhrase = CustomerNoteListViewModel.SearchPhrase ?? "";
                string searchNoteStatus = CustomerNoteListViewModel.SearchNoteStatus ?? "";

                var CustomerNoteList = db.Database.SqlQuery<CustomerNote>("exec dbo.[LB_GetNotesV2] @CustID, @ActivityID, @AssignedToID, @StartDate, @EndDate, @ScheduledStartDate, @ScheduledEndDate, @CompletedStartDate, @CompletedEndDate, @SearchPhrase, @Status, @NoteType, @CommType, @CommDirection, @AdminID",
                new SqlParameter("@CustID", searchCustID),
                new SqlParameter("@ActivityID", searchActivityID),
                new SqlParameter("@AssignedToID", searchAssignedToID),
                new SqlParameter("@StartDate", startDateTime),
                new SqlParameter("@EndDate", endDateTime),
                new SqlParameter("@ScheduledStartDate", scheduledStartDateTime),
                new SqlParameter("@ScheduledEndDate", scheduledEndDateTime),
                new SqlParameter("@CompletedStartDate", completedStartDateTime),
                new SqlParameter("@CompletedEndDate", completedEndDateTime),
                new SqlParameter("@SearchPhrase", searchPhrase),
                new SqlParameter("@Status", searchNoteStatus),
                new SqlParameter("@NoteType", searchNoteType),
                new SqlParameter("@CommType", searchCommType),
                new SqlParameter("@CommDirection", searchCommDirection),
                new SqlParameter("@AdminID", zero)
                ).ToList();

                CustomerNoteListViewModel.CustomerNoteList = CustomerNoteList;

                DateTime nullDateTime = new DateTime(1900, 1, 1);
                DateTime anyDateTime = new DateTime(2900, 1, 1);

                if (startDateTime == nullDateTime || startDateTime == anyDateTime)
                {
                    CustomerNoteListViewModel.SearchStartDate = null;
                    CustomerNoteListViewModel.SearchEndDate = null;            
                }
                if (searchActivityID > -1)
                {
                    ViewBag.ShowAddEdit = "Y";
                }

            }

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> searchNoteTypeList = DataHelper.GetStatusSelectList(Constants.noteTypeLookupGroupID, true, true);
            IEnumerable<System.Web.Mvc.SelectListItem> searchCommTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, true);
            IEnumerable<System.Web.Mvc.SelectListItem> searchCommDirectionList = DataHelper.GetStatusSelectList(Constants.commDirectionLookupGroupID, true, true);
            IEnumerable<System.Web.Mvc.SelectListItem> searchNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, true);
            IEnumerable<System.Web.Mvc.SelectListItem> searchAssignedToList = DataHelper.GetAdminSelectList();

            CustomerNoteListViewModel.SearchAssignedToList = searchAssignedToList;
            CustomerNoteListViewModel.SearchNoteTypeList = searchNoteTypeList;
            CustomerNoteListViewModel.SearchCommTypeList = searchCommTypeList;
            CustomerNoteListViewModel.SearchCommDirectionList = searchCommDirectionList;
            CustomerNoteListViewModel.SearchNoteStatusList = searchNoteStatusList;


            return PartialView("_CustomerNotes", CustomerNoteListViewModel);
        }

        [HttpGet]
        public ActionResult AddNoteInit()
        {

            var DataHelper = new DataHelpers();

            var CustomerNoteUpdateViewModel = new CustomerNoteUpdateViewModel();

            CustomerNoteUpdateViewModel.CustID = 0;
            if (User.IsInRole("Member"))
            {
                CustomerNoteUpdateViewModel.CustID = CustID;
            }

            CustomerNoteUpdateViewModel.ActivityID = 0;
            CustomerNoteUpdateViewModel.AssignedTo = 0;

            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommDirectionList = DataHelper.GetStatusSelectList(Constants.commDirectionLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditAssignedToList = DataHelper.GetAdminSelectList();


            return PartialView("_CustomerNoteAdd", CustomerNoteUpdateViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNoteInit([Bind(Include = "NoteType, SearchCustID, SearchActivityID, SearchVendorID, SearchAssignedToID, SearchStartDate, SearchEndDate, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchNoteType, SearchCommType, SearchCommDirection, SearchPhrase, SearchNoteStatus")] CustomerNoteUpdateViewModel CustomerNoteUpdateViewModel)
        {
            ModelState.Remove("NoteText");

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Unable to initialize Note add Form");
            }
            
            var DataHelper = new DataHelpers();

            CustomerNoteUpdateViewModel.CustID = CustomerNoteUpdateViewModel.SearchCustID;
            if (User.IsInRole("Member"))
            {
                CustomerNoteUpdateViewModel.CustID = CustID;
            }


            CustomerNoteUpdateViewModel.ActivityID = CustomerNoteUpdateViewModel.SearchActivityID;
            CustomerNoteUpdateViewModel.AssignedTo = AdminID;

            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommDirectionList = DataHelper.GetStatusSelectList(Constants.commDirectionLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditAssignedToList = DataHelper.GetAdminSelectList();


            return PartialView("_CustomerNoteAdd", CustomerNoteUpdateViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNote([Bind(Include = "CustID, ActivityID, NoteType, CommType, CommDirection, NoteText, ScheduledDate, AssignedTo, SearchCustID, SearchActivityID, SearchVendorID, SearchAssignedToID, SearchStartDate, SearchEndDate, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchNoteType, SearchCommType, SearchCommDirection, SearchPhrase, SearchNoteStatus")] CustomerNoteUpdateViewModel CustomerNoteUpdateViewModel)
        {
            ViewBag.Success = "N";

            if (ModelState.IsValid)
            {
                int custID = CustomerNoteUpdateViewModel.CustID;
                if (User.IsInRole("Member"))
                {
                    custID = CustID;
                }

                int activityID = CustomerNoteUpdateViewModel.ActivityID;
                string noteType = CustomerNoteUpdateViewModel.NoteType ?? "";
                string commType = CustomerNoteUpdateViewModel.CommType ?? "";
                string commDirection = CustomerNoteUpdateViewModel.CommDirection ?? "";
                string noteText = CustomerNoteUpdateViewModel.NoteText ?? "";
                Nullable<System.DateTime> scheduledDate = CustomerNoteUpdateViewModel.ScheduledDate;
                int assignedTo = CustomerNoteUpdateViewModel.AssignedTo ?? 0;

                if (custID == 0 && activityID == 0)
                {
                    ModelState.AddModelError("", "Unable to identify the Member and/or Activity.");
                }
                else
                {
                    try
                    {
                        if (activityID == 0)
                        {
                            activityID = -1;
                        }
                        db.LB_InsertNoteMinV2(custID, AdminID, activityID, noteType, commType, commDirection, noteText, scheduledDate, assignedTo, AdminID);

                        ViewBag.Success = "Y";
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Error encountered trying to add Note");
                    }
                }
            }

            var DataHelper = new DataHelpers();

            CustomerNoteUpdateViewModel.EditNoteTypeList = DataHelper.GetStatusSelectList(Constants.noteTypeLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommDirectionList = DataHelper.GetStatusSelectList(Constants.commDirectionLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditAssignedToList = DataHelper.GetAdminSelectList();

            return PartialView("_CustomerNoteAdd", CustomerNoteUpdateViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddTaskInit([Bind(Include = "NoteType, SearchCustID, SearchActivityID, SearchVendorID, SearchAssignedToID, SearchStartDate, SearchEndDate, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchNoteType, SearchCommType, SearchCommDirection, SearchPhrase, SearchNoteStatus")] CustomerNoteUpdateViewModel CustomerNoteUpdateViewModel)
        {
            ModelState.Remove("NoteText");

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Unable to initialize Note add Form");
            }

            var DataHelper = new DataHelpers();

            CustomerNoteUpdateViewModel.CustID = CustomerNoteUpdateViewModel.SearchCustID;
            if (User.IsInRole("Member"))
            {
                CustomerNoteUpdateViewModel.CustID = CustID;
            }


            CustomerNoteUpdateViewModel.ActivityID = CustomerNoteUpdateViewModel.SearchActivityID;
            CustomerNoteUpdateViewModel.AssignedTo = AdminID;

            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommDirectionList = DataHelper.GetStatusSelectList(Constants.commDirectionLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditAssignedToList = DataHelper.GetAdminSelectList();


            return PartialView("_TaskListAdd", CustomerNoteUpdateViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddTask([Bind(Include = "CustID, ActivityID, NoteType, CommType, CommDirection, NoteText, ScheduledDate, AssignedTo, SearchCustID, SearchActivityID, SearchVendorID, SearchAssignedToID, SearchStartDate, SearchEndDate, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchNoteType, SearchCommType, SearchCommDirection, SearchPhrase, SearchNoteStatus")] CustomerNoteUpdateViewModel CustomerNoteUpdateViewModel)
        {
            ViewBag.Success = "N";
            
            if (ModelState.IsValid)
            {
                int custID = CustomerNoteUpdateViewModel.CustID;
                if (User.IsInRole("Member"))
                {
                    custID = CustID;
                }

                int activityID = CustomerNoteUpdateViewModel.ActivityID;
                string noteType = CustomerNoteUpdateViewModel.NoteType ?? "";
                string commType = CustomerNoteUpdateViewModel.CommType ?? "";
                string commDirection = CustomerNoteUpdateViewModel.CommDirection ?? "";
                string noteText = CustomerNoteUpdateViewModel.NoteText ?? "";
                Nullable<System.DateTime> scheduledDate = CustomerNoteUpdateViewModel.ScheduledDate;
                int assignedTo = CustomerNoteUpdateViewModel.AssignedTo ?? 0;

                if (custID == 0 && activityID == 0)
                {
                    ModelState.AddModelError("", "Unable to identify the Member and/or Activity.");
                }
                else
                {
                    try
                    {
                        if (activityID == 0)
                        {
                            activityID = -1;
                        }
                        db.LB_InsertNoteMinV2(custID, AdminID, activityID, noteType, commType, commDirection, noteText, scheduledDate, assignedTo, AdminID);

                        ViewBag.Success = "Y";
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Error encountered trying to add Note");
                    }
                }
            }

            var DataHelper = new DataHelpers();

            CustomerNoteUpdateViewModel.EditNoteTypeList = DataHelper.GetStatusSelectList(Constants.noteTypeLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommDirectionList = DataHelper.GetStatusSelectList(Constants.commDirectionLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditAssignedToList = DataHelper.GetAdminSelectList();

            return PartialView("_TaskListAdd", CustomerNoteUpdateViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNoteInit([Bind(Include = "NoteID, SearchCustID, SearchActivityID, SearchVendorID, SearchAssignedToID, SearchStartDate, SearchEndDate, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchNoteType, SearchCommType, SearchCommDirection, SearchPhrase, SearchNoteStatus")] CustomerNoteUpdateViewModel CustomerNoteUpdateViewModel)
        {
            ModelState.Remove("NoteText");

            var CustomerNoteSingle = db.Database.SqlQuery<CustomerNote>("exec dbo.[LB_GetNoteByID] @NoteID",
            new SqlParameter("@NoteID", CustomerNoteUpdateViewModel.NoteID)
            ).FirstOrDefault();

            string noteText = CustomerNoteSingle.NoteText ?? "";
            string editNoteStatus = CustomerNoteSingle.Status ?? "";

            if (CustomerNoteSingle != null)
            {
                CustomerNoteUpdateViewModel.CustID = CustomerNoteSingle.CustID;
                CustomerNoteUpdateViewModel.OwnerID = CustomerNoteSingle.OwnerID;
                CustomerNoteUpdateViewModel.ActivityID = CustomerNoteSingle.ActivityID;
                CustomerNoteUpdateViewModel.NoteType = CustomerNoteSingle.NoteType;
                CustomerNoteUpdateViewModel.CommType = CustomerNoteSingle.CommType;
                CustomerNoteUpdateViewModel.CommDirection = CustomerNoteSingle.CommDirection;
                CustomerNoteUpdateViewModel.NoteText = CustomerNoteSingle.NoteText;
                CustomerNoteUpdateViewModel.ScheduledDate = CustomerNoteSingle.ScheduledDate;
                CustomerNoteUpdateViewModel.EndDate = CustomerNoteSingle.EndDate;
                CustomerNoteUpdateViewModel.AssignedTo = CustomerNoteSingle.AssignedTo;
                CustomerNoteUpdateViewModel.AssignedDate = CustomerNoteSingle.AssignedDate;
                CustomerNoteUpdateViewModel.Status = CustomerNoteSingle.Status;
            }
            else
            {
                ModelState.AddModelError("", "Note could not be found");
            }

            var DataHelper = new DataHelpers();

            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommDirectionList = DataHelper.GetStatusSelectList(Constants.commDirectionLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditAssignedToList = DataHelper.GetAdminSelectList();

            ViewBag.ShowEditform = true;

            return PartialView("_CustomerNoteEdit", CustomerNoteUpdateViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNote([Bind(Include = "NoteID, CustID, OwnerID, ActivityID, Notetype, CommType, CommDirection, NoteText, ScheduledDate, EndDate, AssignedTo, AssignedDate, Status, SearchCustID, SearchActivityID, SearchVendorID, SearchAssignedToID, SearchStartDate, SearchEndDate, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchNoteType, SearchCommType, SearchCommDirection, SearchPhrase, SearchNoteStatus")] CustomerNoteUpdateViewModel CustomerNoteUpdateViewModel)
        {
            ViewBag.Success = "N";

            if (ModelState.IsValid)
            {
                int thisNoteID = CustomerNoteUpdateViewModel.NoteID;
                int thisCustID = CustomerNoteUpdateViewModel.CustID;
                int thisOwnerID = CustomerNoteUpdateViewModel.OwnerID;
                int thisActivityID = CustomerNoteUpdateViewModel.ActivityID;
                string thisNoteType = CustomerNoteUpdateViewModel.NoteType ?? "";
                string thisCommType = CustomerNoteUpdateViewModel.CommType ?? "";
                string thisCommDirection = CustomerNoteUpdateViewModel.CommDirection ?? "";
                string thisNoteText = CustomerNoteUpdateViewModel.NoteText ?? "";
                int thisAssignedTo = CustomerNoteUpdateViewModel.AssignedTo ?? 0;
                Nullable<System.DateTime> thisNoteScheduledDate = CustomerNoteUpdateViewModel.ScheduledDate;
                Nullable<System.DateTime> thisNoteEndDate = CustomerNoteUpdateViewModel.EndDate;
                string thisStatus = CustomerNoteUpdateViewModel.Status;

                if (thisStatus.ToUpper() == "COMPLETE" && thisNoteEndDate == null)
                {
                    ModelState.AddModelError("", "Please specify the task completed date");
                }
                else
                {
                    try
                    {
                        ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                        ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                        db.LB_UpdateNoteV2(thisNoteID, thisCustID, thisOwnerID, thisActivityID, thisNoteType, thisCommType, thisCommDirection, thisNoteText, thisNoteScheduledDate, thisNoteEndDate, thisAssignedTo, thisStatus, returnID, returnMessage);

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
                        ModelState.AddModelError("", "Error encountered trying to update note");
                    }
                }
            }

            var DataHelper = new DataHelpers();

            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommDirectionList = DataHelper.GetStatusSelectList(Constants.commDirectionLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditAssignedToList = DataHelper.GetAdminSelectList();


            return PartialView("_CustomerNoteEdit", CustomerNoteUpdateViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTaskInit([Bind(Include = "NoteID, SearchCustID, SearchActivityID, SearchVendorID, SearchAssignedToID, SearchStartDate, SearchEndDate, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchNoteType, SearchCommType, SearchCommDirection, SearchPhrase, SearchNoteStatus")] CustomerNoteUpdateViewModel CustomerNoteUpdateViewModel)
        {
            ModelState.Remove("NoteText");

            var CustomerNoteSingle = db.Database.SqlQuery<CustomerNote>("exec dbo.[LB_GetNoteByID] @NoteID",
            new SqlParameter("@NoteID", CustomerNoteUpdateViewModel.NoteID)
            ).FirstOrDefault();

            string noteText = CustomerNoteSingle.NoteText ?? "";
            string editNoteStatus = CustomerNoteSingle.Status ?? "";

            if (CustomerNoteSingle != null)
            {
                CustomerNoteUpdateViewModel.CustID = CustomerNoteSingle.CustID;
                CustomerNoteUpdateViewModel.OwnerID = CustomerNoteSingle.OwnerID;
                CustomerNoteUpdateViewModel.ActivityID = CustomerNoteSingle.ActivityID;
                CustomerNoteUpdateViewModel.NoteType = CustomerNoteSingle.NoteType;
                CustomerNoteUpdateViewModel.CommType = CustomerNoteSingle.CommType;
                CustomerNoteUpdateViewModel.CommDirection = CustomerNoteSingle.CommDirection;
                CustomerNoteUpdateViewModel.NoteText = CustomerNoteSingle.NoteText;
                CustomerNoteUpdateViewModel.ScheduledDate = CustomerNoteSingle.ScheduledDate;
                CustomerNoteUpdateViewModel.EndDate = CustomerNoteSingle.EndDate;
                CustomerNoteUpdateViewModel.AssignedTo = CustomerNoteSingle.AssignedTo;
                CustomerNoteUpdateViewModel.AssignedDate = CustomerNoteSingle.AssignedDate;
                CustomerNoteUpdateViewModel.Status = CustomerNoteSingle.Status;
            }
            else
            {
                ModelState.AddModelError("", "Note could not be found");
            }

            var DataHelper = new DataHelpers();

            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommDirectionList = DataHelper.GetStatusSelectList(Constants.commDirectionLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditAssignedToList = DataHelper.GetAdminSelectList();

            ViewBag.ShowEditform = true;

            return PartialView("_TaskListEdit", CustomerNoteUpdateViewModel);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTask([Bind(Include = "NoteID, CustID, OwnerID, ActivityID, Notetype, CommType, CommDirection, NoteText, ScheduledDate, EndDate, AssignedTo, AssignedDate, Status, SearchCustID, SearchActivityID, SearchVendorID, SearchAssignedToID, SearchStartDate, SearchEndDate, SearchScheduledStartDate, SearchScheduledEndDate, SearchCompletedStartDate, SearchCompletedEndDate, SearchNoteType, SearchCommType, SearchCommDirection, SearchPhrase, SearchNoteStatus")] CustomerNoteUpdateViewModel CustomerNoteUpdateViewModel)
        {
            ViewBag.Success = "N";

            if (ModelState.IsValid)
            {
                int thisNoteID = CustomerNoteUpdateViewModel.NoteID;
                int thisCustID = CustomerNoteUpdateViewModel.CustID;
                int thisOwnerID = CustomerNoteUpdateViewModel.OwnerID;
                int thisActivityID = CustomerNoteUpdateViewModel.ActivityID;
                string thisNoteType = CustomerNoteUpdateViewModel.NoteType ?? "";
                string thisCommType = CustomerNoteUpdateViewModel.CommType ?? "";
                string thisCommDirection = CustomerNoteUpdateViewModel.CommDirection ?? "";
                string thisNoteText = CustomerNoteUpdateViewModel.NoteText ?? "";
                int thisAssignedTo = CustomerNoteUpdateViewModel.AssignedTo ?? 0;
                Nullable<System.DateTime> thisNoteScheduledDate = CustomerNoteUpdateViewModel.ScheduledDate;
                Nullable<System.DateTime> thisNoteEndDate = CustomerNoteUpdateViewModel.EndDate;
                string thisStatus = CustomerNoteUpdateViewModel.Status;

                if (thisStatus.ToUpper() == "COMPLETE" && thisNoteEndDate == null)
                {
                    ModelState.AddModelError("", "Please specify the task completed date");
                }
                else
                {
                    try
                    {
                        ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                        ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                        db.LB_UpdateNoteV2(thisNoteID, thisCustID, thisOwnerID, thisActivityID, thisNoteType, thisCommType, thisCommDirection, thisNoteText, thisNoteScheduledDate, thisNoteEndDate, thisAssignedTo, thisStatus, returnID, returnMessage);

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
                        ModelState.AddModelError("", "Error encountered trying to update note");
                    }
                }
            }

            var DataHelper = new DataHelpers();

            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommTypeList = DataHelper.GetStatusSelectList(Constants.activityTypeLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditCommDirectionList = DataHelper.GetStatusSelectList(Constants.commDirectionLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditNoteStatusList = DataHelper.GetStatusSelectList(Constants.noteStatusLookupGroupID, true, false);
            CustomerNoteUpdateViewModel.EditAssignedToList = DataHelper.GetAdminSelectList();


            return PartialView("_TaskListEdit", CustomerNoteUpdateViewModel);
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
