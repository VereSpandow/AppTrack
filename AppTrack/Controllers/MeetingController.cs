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
using System.Data;
using System.Web;
using System.IO;
using System.Text;
using FastMember;

namespace AppTrack.Controllers
{

    [AuthorizeAdminRedirect(Roles = Constants.adminRoles)]
    public class MeetingController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        //
        // MEETING SECTION
        //
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult MeetingList(int CustID = 0, int EventID = 0, string ActionType = "")
        {
            var DataHelper = new DataHelpers();

            if (CustID > 0)
            {
                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.memberDirectorCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ViewBag.ErrorCode = Constants.fatalErrorCode;
                    ViewBag.ErrorMessage = "Invalid Member Director ID provided, unable to retrieve Meeting List";
                    return PartialView("_MeetingList");
                }

                ViewBag.DisplayName = checkCustomerResult.C_Info.DisplayName;
            }

            var meetingEvent = new MeetingEvent()
            {
                CustID = CustID,
                EventStartDate = DateTime.Now
            };

            int thisHour = 0;
            int thisMinute = 0;
            string thisAMPM = "AM";

            if (EventID > 0)
            {
                // Check to see if Action is Delete
                if (ActionType == "D")
                {
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_DeleteEventHeader(EventID, AdminID,
                        returnID, returnMessage);

                    var scalarID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                else
                {
                    // Check Event
                    meetingEvent = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeader] @EventID",
                    new SqlParameter("@EventID", EventID)
                    ).First();

                    if (meetingEvent == null)
                    {
                        // Event not found so error
                        ModelState.AddModelError("", "Meeting was not found and could not be retrieved");
                    }
                    else
                    {
                        // Set the time variables for the select list
                        thisHour = System.Convert.ToInt32(meetingEvent.EventStartDate.ToString("hh"));
                        thisMinute = System.Convert.ToInt32(meetingEvent.EventStartDate.ToString("mm"));

                        if (System.Convert.ToInt32(meetingEvent.EventStartDate.ToString("HH")) >= 12)
                        {
                            thisAMPM = "PM";
                        }
                    }
                }
            }

            // Initalize the View Model

            var meetingViewModel = new MeetingViewModel();

            int zero = 0;
            DateTime startDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now.AddDays(30);

            var meetingEventList = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeader] @EventID, @CategoryID, @CustID, @SponsorName, @EventTitle, @StartDate, @EndDate, @LocationTitle, @City, @State, @Status, @StatusID",
            new SqlParameter("@EventID", zero),
            new SqlParameter("@CategoryID", Constants.categoryStudyGroupMeeting),
            new SqlParameter("@CustID", CustID),
            new SqlParameter("@SponsorName", ""),
            new SqlParameter("@EventTitle", ""),
            new SqlParameter("@StartDate", startDate),
            new SqlParameter("@EndDate", endDate),
            new SqlParameter("@LocationTitle", ""),
            new SqlParameter("@City", ""),
            new SqlParameter("@State", ""),
            new SqlParameter("@Status", ""),
            new SqlParameter("@StatusID", zero)
            ).ToList();

            meetingViewModel.MeetingEventList = meetingEventList;

            // meetingEvent could be blank or be populated if an event id was supplied to edit
            meetingViewModel.meetingEvent = meetingEvent;

            meetingViewModel.eventStartHour = thisHour;
            meetingViewModel.eventStartMinute = thisMinute;
            meetingViewModel.eventStartAMPM = thisAMPM;

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            meetingViewModel.StateList = stateList;

            IEnumerable<System.Web.Mvc.SelectListItem> statusList = DataHelper.GetStatusSelectList(Constants.meetingStatusLookupGroupID, true, false);
            meetingViewModel.StatusList = statusList;

            IEnumerable<System.Web.Mvc.SelectListItem> timeHourList = DataHelper.GetTimeHourList();
            meetingViewModel.TimeHourList = timeHourList;

            IEnumerable<System.Web.Mvc.SelectListItem> timeMinuteList = DataHelper.GetTimeMinuteList();
            meetingViewModel.TimeMinuteList = timeMinuteList;

            IEnumerable<System.Web.Mvc.SelectListItem> timeAMPMList = DataHelper.GetTimeAMPMList();
            meetingViewModel.TimeAMPMList = timeAMPMList;

            IEnumerable<System.Web.Mvc.SelectListItem> searchMemberDirectorList = DataHelper.GetMemberDirectorSelectList(false, true);
            meetingViewModel.SearchMemberDirectorList = searchMemberDirectorList;
            
            IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.meetingStatusLookupGroupID, false, true);
            meetingViewModel.SearchStatusList = searchStatusList;

            meetingViewModel.SearchCustID = CustID;
            meetingViewModel.SearchStartDate = startDate;
            meetingViewModel.SearchEndDate = endDate;            
            meetingViewModel.SearchPhrase = " ";
            meetingViewModel.SearchStatus = " ";
            meetingViewModel.SearchState = " ";

            return PartialView("_MeetingList", meetingViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MeetingList([Bind(Include = "SearchCustID, SearchStartDate, SearchEndDate, SearchPhrase, SearchStatus, SearchState")] MeetingViewModel meetingViewModel)
        {
            var DataHelper = new DataHelpers();

            int zero = 0;
            int thisHour = 0;
            int thisMinute = 1;
            // value of 1 causes select list to display Min, can't use 0 because 0 is one of the choices.
            string thisAMPM = "AM";

            var meetingEventList = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeader] @EventID, @CategoryID, @CustID, @SponsorName, @EventTitle, @StartDate, @EndDate, @LocationTitle, @City, @State, @Status, @StatusID",
            new SqlParameter("@EventID", zero),
            new SqlParameter("@CategoryID", Constants.categoryStudyGroupMeeting),
            new SqlParameter("@CustID", meetingViewModel.SearchCustID),
            new SqlParameter("@SponsorName", ""),
            new SqlParameter("@EventTitle", meetingViewModel.SearchPhrase),
            new SqlParameter("@StartDate", meetingViewModel.SearchStartDate),
            new SqlParameter("@EndDate", meetingViewModel.SearchEndDate),
            new SqlParameter("@LocationTitle", ""),
            new SqlParameter("@City", ""),
            new SqlParameter("@State", meetingViewModel.SearchState),
            new SqlParameter("@Status", meetingViewModel.SearchStatus),
            new SqlParameter("@StatusID", zero)
            ).ToList();

            meetingViewModel.MeetingEventList = meetingEventList;

            var emptyMeetingEvent = new MeetingEvent();
            meetingViewModel.meetingEvent = emptyMeetingEvent;

            meetingViewModel.eventStartHour = thisHour;
            meetingViewModel.eventStartMinute = thisMinute;
            meetingViewModel.eventStartAMPM = thisAMPM;

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            meetingViewModel.StateList = stateList;

            IEnumerable<System.Web.Mvc.SelectListItem> statusList = DataHelper.GetStatusSelectList(Constants.meetingStatusLookupGroupID, true, false);
            meetingViewModel.StatusList = statusList;

            IEnumerable<System.Web.Mvc.SelectListItem> timeHourList = DataHelper.GetTimeHourList(true, false);
            meetingViewModel.TimeHourList = timeHourList;

            IEnumerable<System.Web.Mvc.SelectListItem> timeMinuteList = DataHelper.GetTimeMinuteList(true, false);
            meetingViewModel.TimeMinuteList = timeMinuteList;

            IEnumerable<System.Web.Mvc.SelectListItem> timeAMPMList = DataHelper.GetTimeAMPMList();
            meetingViewModel.TimeAMPMList = timeAMPMList;

            IEnumerable<System.Web.Mvc.SelectListItem> searchMemberDirectorList = DataHelper.GetMemberDirectorSelectList(false, true);
            meetingViewModel.SearchMemberDirectorList = searchMemberDirectorList;

            IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.meetingStatusLookupGroupID, false, true);
            meetingViewModel.SearchStatusList = searchStatusList;

            return PartialView("_MeetingList", meetingViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "SalesManager, MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MeetingUpdate([Bind(Include = "eventStartHour, eventStartMinute, eventStartAMPM, meetingEvent")] MeetingViewModel meetingViewModel)
        {
            var DataHelper = new DataHelpers();

            int CustID = new int { };
            int EventID = new int { };

            DateTime startDate = new DateTime();

            if (ModelState.IsValid)
            {
                CustID = meetingViewModel.meetingEvent.CustID;

                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.memberDirectorCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ViewBag.ErrorCode = Constants.fatalErrorCode;
                    ViewBag.ErrorMessage = "Invalid Member Director ID provided, unable to retrieve Meeting List";
                    return PartialView("_MeetingList");
                }

                EventID = meetingViewModel.meetingEvent.ID;

                int startHour = meetingViewModel.eventStartHour;

                if (meetingViewModel.eventStartAMPM == "PM" && meetingViewModel.eventStartHour < 12)
                {
                    startHour = startHour + 12;
                }
                if (meetingViewModel.eventStartAMPM == "AM" && meetingViewModel.eventStartHour == 12)
                {
                    startHour = 0;
                }

                startDate = meetingViewModel.meetingEvent.EventStartDate.AddHours(startHour);

                startDate = startDate.AddMinutes(meetingViewModel.eventStartMinute);

                meetingViewModel.meetingEvent.EventStartDate = startDate;
                meetingViewModel.meetingEvent.EventEndDate = startDate;

                if (EventID == 0)
                {
                    // Add Event 
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));
                    meetingViewModel.meetingEvent.Status = "Pending";

                    db.LB_InsertEventHeader(Constants.categoryStudyGroupMeeting, CustID,
                        meetingViewModel.meetingEvent.SponsorName,
                        meetingViewModel.meetingEvent.EventTitle,
                        meetingViewModel.meetingEvent.EventDescription,
                        meetingViewModel.meetingEvent.EventStartDate,
                        meetingViewModel.meetingEvent.EventEndDate,
                        meetingViewModel.meetingEvent.EventDateTimeString,
                        meetingViewModel.meetingEvent.Capacity,
                        meetingViewModel.meetingEvent.LocationTitle,
                        meetingViewModel.meetingEvent.Address1,
                        meetingViewModel.meetingEvent.Address2,
                        meetingViewModel.meetingEvent.City,
                        meetingViewModel.meetingEvent.State,
                        meetingViewModel.meetingEvent.PostalCode,
                        meetingViewModel.meetingEvent.Status,
                        AdminID,
                        returnID, returnMessage);

                    var scalarID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                else
                {
                    // Update Event
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateEventHeader(EventID, CustID,
                        meetingViewModel.meetingEvent.SponsorName,
                        meetingViewModel.meetingEvent.EventDateTimeString,
                        meetingViewModel.meetingEvent.EventTitle,
                        meetingViewModel.meetingEvent.EventDescription,
                        meetingViewModel.meetingEvent.EventStartDate,
                        meetingViewModel.meetingEvent.EventEndDate,
                        meetingViewModel.meetingEvent.LocationTitle,
                        meetingViewModel.meetingEvent.Address1,
                        meetingViewModel.meetingEvent.Address2,
                        meetingViewModel.meetingEvent.City,
                        meetingViewModel.meetingEvent.State,
                        meetingViewModel.meetingEvent.PostalCode,
                        meetingViewModel.meetingEvent.Capacity,
                        meetingViewModel.meetingEvent.Status,
                        meetingViewModel.meetingEvent.StatusID,
                        AdminID,
                        returnID, returnMessage);

                    var scalarID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
            }

            int zero = 0;
            DateTime endDate = startDate;

            var meetingEventList = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeader] @EventID, @CategoryID, @CustID, @SponsorName, @EventTitle, @StartDate, @EndDate, @LocationTitle, @City, @State, @Status, @StatusID",
            new SqlParameter("@EventID", EventID),
            new SqlParameter("@CategoryID", Constants.categoryStudyGroupMeeting),
            new SqlParameter("@CustID", CustID),
            new SqlParameter("@SponsorName", ""),
            new SqlParameter("@EventTitle", ""),
            new SqlParameter("@StartDate", startDate),
            new SqlParameter("@EndDate", endDate),
            new SqlParameter("@LocationTitle", ""),
            new SqlParameter("@City", ""),
            new SqlParameter("@State", ""),
            new SqlParameter("@Status", ""),
            new SqlParameter("@StatusID", zero)
            ).ToList();

            meetingViewModel.MeetingEventList = meetingEventList;

            if (ModelState.IsValid)
            {
                // Add empty items to View Model and clear out Model State which is used to populate the form 
                var emptyMeetingEvent = new MeetingEvent()
                {
                    CustID = CustID,
                    EventStartDate = DateTime.Now
                };
                meetingViewModel.meetingEvent = emptyMeetingEvent;
                meetingViewModel.eventStartHour = 0;
                meetingViewModel.eventStartMinute = 0;
                meetingViewModel.eventStartAMPM = "AM";

                ModelState.Clear();
            }

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            meetingViewModel.StateList = stateList;

            IEnumerable<System.Web.Mvc.SelectListItem> statusList = DataHelper.GetStatusSelectList(Constants.meetingStatusLookupGroupID, true, false);
            meetingViewModel.StatusList = statusList;

            IEnumerable<System.Web.Mvc.SelectListItem> timeHourList = DataHelper.GetTimeHourList(true, false);
            meetingViewModel.TimeHourList = timeHourList;

            IEnumerable<System.Web.Mvc.SelectListItem> timeMinuteList = DataHelper.GetTimeMinuteList(true, false);
            meetingViewModel.TimeMinuteList = timeMinuteList;

            IEnumerable<System.Web.Mvc.SelectListItem> timeAMPMList = DataHelper.GetTimeAMPMList();
            meetingViewModel.TimeAMPMList = timeAMPMList;

            IEnumerable<System.Web.Mvc.SelectListItem> searchMemberDirectorList = DataHelper.GetMemberDirectorSelectList(false, true);
            meetingViewModel.SearchMemberDirectorList = searchMemberDirectorList;

            IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.meetingStatusLookupGroupID, false, true);
            meetingViewModel.SearchStatusList = searchStatusList;

            meetingViewModel.SearchCustID = 0;
            meetingViewModel.SearchStartDate = DateTime.Now; ;
            meetingViewModel.SearchEndDate = DateTime.Now.AddDays(30); ;
            meetingViewModel.SearchPhrase = " ";
            meetingViewModel.SearchStatus = " ";
            meetingViewModel.SearchState = " ";

            return PartialView("_MeetingList", meetingViewModel);
        }

        [HttpGet]
        public ActionResult RegistrationList(int EventID = 0)
        {
            var DataHelper = new DataHelpers();

            MeetingEvent meetingEvent = new MeetingEvent();
            MeetingRegistrationListViewModel meetingRegistrationListViewModel = new MeetingRegistrationListViewModel();

            if (EventID == 0)
            {
                // Event not found so error
                ModelState.AddModelError("", "Meeting was not found and could not be retrieved");
            }
            else
            {
                // Initalize the View Model
                meetingEvent = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeader] @EventID",
                new SqlParameter("@EventID", EventID)
                ).First();

                if (meetingEvent == null)
                {
                    // Event not found so error
                    ModelState.AddModelError("", "Meeting was not found and could not be retrieved");
                }
                else
                {
                    meetingRegistrationListViewModel.meetingEvent = meetingEvent;

                    var meetingRegistrationList = db.Database.SqlQuery<MeetingRegistration>("exec dbo.[LB_GetEventRegistration] @EventID",
                    new SqlParameter("@EventID", EventID)
                    ).ToList();

                    meetingRegistrationListViewModel.MeetingRegistrationList = meetingRegistrationList;
                }
            }
            meetingRegistrationListViewModel.meetingRegistration = new MeetingRegistration();

            meetingRegistrationListViewModel.NameTitleList = DataHelper.GetNameTitleList();
            meetingRegistrationListViewModel.meetingRegistration.EventID = EventID;

            return View(meetingRegistrationListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "SalesManager, MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCustomer(int EventID = 0, int CustID = 0)
        {
            var DataHelper = new DataHelpers();

            if (EventID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Meeting ID supplied to Delete Customer";
                return RedirectToAction("Error", "Admin");
            }
            if (CustID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Customer ID supplied to Delete Customer";
                return RedirectToAction("Error", "Admin");
            }

            ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
            ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

            try
            {
                db.LB_DeleteEventRegistration(EventID, CustID, AdminID, returnID, returnMessage);
                var scalarID = (int)returnID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
            }
            catch
            {
                ModelState.AddModelError("", "Error encountered - unable to delete Meeting");
            }

            MeetingEvent meetingEvent = new MeetingEvent();
            MeetingRegistrationListViewModel meetingRegistrationListViewModel = new MeetingRegistrationListViewModel();

            // Initalize the View Model
            try
            {
                meetingEvent = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeader] @EventID",
                new SqlParameter("@EventID", EventID)
                ).First();
            }
            catch
            {
                ModelState.AddModelError("", "Error encountered - unable to retrieve Meeting information");
            }

            if (meetingEvent == null)
            {
                // Event not found so error
                ModelState.AddModelError("", "Error encountered - unable to retrieve Meeting information");
            }
            else
            {
                meetingRegistrationListViewModel.meetingEvent = meetingEvent;

                var meetingRegistrationList = new List<MeetingRegistration>();

                try
                {
                    meetingRegistrationList = db.Database.SqlQuery<MeetingRegistration>("exec dbo.[LB_GetEventRegistration] @EventID",
                    new SqlParameter("@EventID", EventID)
                    ).ToList();
                }
                catch
                {
                    ModelState.AddModelError("", "Error encountered - unable to retrieve Meeting attendee list");
                }

                meetingRegistrationListViewModel.MeetingRegistrationList = meetingRegistrationList;
            }
            meetingRegistrationListViewModel.meetingRegistration = new MeetingRegistration();
            meetingRegistrationListViewModel.NameTitleList = DataHelper.GetNameTitleList();
            meetingRegistrationListViewModel.meetingRegistration.EventID = EventID;

            return View("RegistrationList", meetingRegistrationListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "SalesManager, MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCustomer([Bind(Include = "EventID, CustID, SponsorID, NameTitle, FirstName, LastName, JobTitle, SponsorName, Email, Phone, Flag1")] MeetingRegistration meetingRegistration)
        {
            var DataHelper = new DataHelpers();
            
            int SponsorID = meetingRegistration.SponsorID;

            if (ModelState.IsValid)
            {
                if (meetingRegistration.EventID == 0)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Meeting ID supplied to Add Attendee";
                    return RedirectToAction("Error", "Admin");
                }

                meetingRegistration.Phone = DataHelper.FixPhone(meetingRegistration.Phone);

                ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_InsertEventRegistration(
                        meetingRegistration.EventID,
                        meetingRegistration.CustID,
                        meetingRegistration.SponsorID,
                        meetingRegistration.NameTitle,
                        meetingRegistration.FirstName,
                        meetingRegistration.LastName,
                        meetingRegistration.JobTitle,
                        meetingRegistration.SponsorName,
                        meetingRegistration.Email,
                        meetingRegistration.Phone,
                        Constants.meetingAttendeeCustomerType,
                        meetingRegistration.Flag1, 
                        returnID, returnMessage
                        );

                var scalarID = (int)returnID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    SponsorID = scalarID;
                }

            }
            MeetingEvent meetingEvent = new MeetingEvent();

            MeetingRegistrationListViewModel meetingRegistrationListViewModel = new MeetingRegistrationListViewModel();

            if (ModelState.IsValid)
            {
                meetingRegistrationListViewModel.meetingRegistration = new MeetingRegistration();
                meetingRegistrationListViewModel.meetingRegistration.SponsorID = SponsorID;
            }
            else
            {
                meetingRegistrationListViewModel.meetingRegistration = meetingRegistration;
            }
            // Initalize the View Model
            meetingEvent = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeader] @EventID",
            new SqlParameter("@EventID", meetingRegistration.EventID)
            ).First();

            if (meetingEvent == null)
            {
                // Event not found so error
                ModelState.AddModelError("", "Meeting was not found and could not be retrieved");
            }
            else
            {
                meetingRegistrationListViewModel.meetingEvent = meetingEvent;

                var meetingRegistrationList = db.Database.SqlQuery<MeetingRegistration>("exec dbo.[LB_GetEventRegistration] @EventID",
                new SqlParameter("@EventID", meetingRegistration.EventID)
                ).ToList();

                meetingRegistrationListViewModel.MeetingRegistrationList = meetingRegistrationList;
            }
            meetingRegistrationListViewModel.NameTitleList = DataHelper.GetNameTitleList();

            return View("RegistrationList", meetingRegistrationListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "SalesManager, MemberServices")]
        [HttpGet]
        public ActionResult RegistrationListToCSV(int EventID = 0)
        {
            var DataHelper = new DataHelpers();

            MeetingEvent meetingEvent = new MeetingEvent();
            List<MeetingRegistration> meetingRegistrationList = new List<MeetingRegistration>();

            if (EventID == 0)
            {
                return HttpNotFound();
            }
            else
            {
                // Initalize the View Model
                meetingEvent = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeader] @EventID",
                new SqlParameter("@EventID", EventID)
                ).FirstOrDefault();

                if (meetingEvent == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    meetingRegistrationList = db.Database.SqlQuery<MeetingRegistration>("exec dbo.[LB_GetEventRegistration] @EventID",
                    new SqlParameter("@EventID", EventID)
                    ).ToList();
                    DataTable registrationTable = new DataTable();
                    using (var reader = ObjectReader.Create(meetingRegistrationList))
                    {
                        registrationTable.Load(reader);
                    }
                    return new CsvActionResult(registrationTable) { FileDownloadName = "EventID-"+EventID.ToString()+"-RegistrationList.csv" };
                }
            }
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

