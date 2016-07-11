using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

using System.Web;

using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using AppTrack.Helpers;
using AuthorizeNet;
using System.Collections.Generic;
using System;
using System.Net.Mail;

namespace AppTrack.Controllers
{
    // [Authorize(Roles = "MemberDirector")]
    public class SiteIMDController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
        DataHelpers DataHelper = new DataHelpers();

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string topic = "")
        {
            if (topic == "")
            {
                ViewBag.ErrorMessage = "Please enter a topic.";
            }
            else
            {
                string emailSubject = "Suggested Topic from IMD";
                string body = "Suggested Topic from IMD via IMD website<br><br>IMD Name: " + DisplayName + "<br><br>Topic: " + topic;
                SmtpClient client = new SmtpClient();
                MailMessage mailMessage = new MailMessage();
                MailAddress FromAddress = new MailAddress(Constants.adminEmailFrom, "AppTrack Member Services");
                mailMessage.From = FromAddress;
                mailMessage.To.Add(Constants.notificationEmailTo);
                mailMessage.To.Add("Vere@MotionGrid.com");
                mailMessage.Subject = emailSubject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                try
                {
                    client.Send(mailMessage);
                    ViewBag.ErrorMessage = "Thank you for your suggestion!";
                }
                catch 
                {
                    ViewBag.ErrorMessage = "An unexpected error was encountered. Please try again.";
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult MeetingList(int EventID = 0, string ActionType = "")
        {
            // Parameters are left over from copied code from Admin site
            // IMDs cannot edit or delete meetings so no need to use these
            var DataHelper = new DataHelpers();

            var meetingEvent = new MeetingEvent()
            {
                CustID = CustID,
                EventStartDate = DateTime.Now.AddDays(42)
            };

            int thisHour = 0;
            int thisMinute = 1;
            // value of 1 causes select list to display Min, can't use 0 because 0 is one of the choices.
            string thisAMPM = "AM";

            // Initalize the View Model
            var meetingViewModel = new MeetingViewModel();

            int zero = 0;
            DateTime startDate = DateTime.Now.AddDays(-90);
            DateTime endDate = DateTime.Now.AddDays(90);

            var meetingEventList = new List<MeetingEvent>();

            try
            {
                meetingEventList = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeader] @EventID, @CategoryID, @CustID, @SponsorName, @EventTitle, @StartDate, @EndDate, @LocationTitle, @City, @State, @Status, @StatusID",
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
            }
            catch
            {
                ModelState.AddModelError("", "Error encountered - unable to retrieve Meeting List");
            }

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

            return View(meetingViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MeetingList([Bind(Include = "SearchCustID, SearchStartDate, SearchEndDate, SearchPhrase, SearchStatus, SearchState")] MeetingViewModel meetingViewModel)
        {
            var DataHelper = new DataHelpers();
            
            int zero = 0;
            int thisHour = 0;
            int thisMinute = 0;
            string thisAMPM = "AM";
            meetingViewModel.SearchPhrase = meetingViewModel.SearchPhrase.Trim();

            var meetingEventList = new List<MeetingEvent>();

            try
            {
                meetingEventList = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeader] @EventID, @CategoryID, @CustID, @SponsorName, @EventTitle, @StartDate, @EndDate, @LocationTitle, @City, @State, @Status, @StatusID",
                new SqlParameter("@EventID", zero),
                new SqlParameter("@CategoryID", Constants.categoryStudyGroupMeeting),
                new SqlParameter("@CustID", CustID),
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
            }
            catch
            {
                ModelState.AddModelError("", "Error encountered - unable to retrieve Meeting List");
            }

            meetingViewModel.MeetingEventList = meetingEventList;

            var emptyMeetingEvent = new MeetingEvent();
            meetingViewModel.meetingEvent = emptyMeetingEvent;

            meetingViewModel.meetingEvent.EventStartDate = DateTime.Now.AddDays(42);

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

            return View(meetingViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MeetingUpdate([Bind(Include = "eventStartHour, eventStartMinute, eventStartAMPM, meetingEvent")] MeetingViewModel meetingViewModel)
        {
            var DataHelper = new DataHelpers();

            int EventID = 0;

            DateTime startDate = new DateTime();

            if (ModelState.IsValid)
            {
                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.memberDirectorCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ModelState.AddModelError("", "Invalid Member Director ID provided, unable to retrieve Meeting List");
                }

                if (meetingViewModel.meetingEvent.EventStartDate < DateTime.Now.AddDays(41))
                {
                    ModelState.AddModelError("", "Meetings must be scheduled at least 6 weeks from today");
                }

                if (ModelState.IsValid)
                {
                    int startHour = meetingViewModel.eventStartHour;

                    if (meetingViewModel.eventStartAMPM == "PM")
                    {
                        startHour = startHour + 12;
                    }

                    startDate = meetingViewModel.meetingEvent.EventStartDate.AddHours(startHour);

                    startDate = startDate.AddMinutes(meetingViewModel.eventStartMinute);

                    meetingViewModel.meetingEvent.EventStartDate = startDate;
                    meetingViewModel.meetingEvent.EventEndDate = startDate;

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
                    else
                    {
                        int emailId = Constants.emailIDMeetingUpdateNotification;
                        var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                        new SqlParameter("@ID", emailId)
                        ).FirstOrDefault();
                        string subject = emailRow.Subject;
                        string body = emailRow.Content;
                        string sCustID = CustID.ToString();

                        body = body.Replace("#IMDID#", sCustID);
                        body = body.Replace("#NAME#", meetingViewModel.meetingEvent.SponsorName);
                        body = body.Replace("#EVENTTITLE#", meetingViewModel.meetingEvent.EventTitle);
                        body = body.Replace("#EVENTDESCRIPTION#", meetingViewModel.meetingEvent.EventDescription);
                        body = body.Replace("#EVENTDATE#",meetingViewModel.meetingEvent.EventStartDate.ToShortDateString());
                        body = body.Replace("#LOCATION#", meetingViewModel.meetingEvent.LocationTitle);

                        body = Constants.emailTemplate.Replace("#BODYCOPY#", body);
                        body = body.Replace("#SITEURL#", Constants.siteURL);
                        SmtpClient client = new SmtpClient();
                        MailMessage mailMessage = new MailMessage();
                        MailAddress FromAddress = new MailAddress(Constants.adminEmailFrom, "AppTrack Member Services");
                        mailMessage.From = FromAddress;
                        mailMessage.To.Add(Constants.salesEmailTo);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        client.Send(mailMessage);

                    }
                }
            }

            int zero = 0;
            startDate = DateTime.Now.AddDays(-90);
            DateTime endDate = DateTime.Now.AddDays(90);

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

            return View("MeetingList", meetingViewModel);
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
                    if (meetingEvent.CustID != CustID)
                    {
                        ViewBag.ErrorCode = Constants.fatalErrorCode;
                        ViewBag.ErrorMessage = "You do not have permission to access this meetings attendee list.";
                        return View();
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
            }
            return View(meetingRegistrationListViewModel);
        }

        //ACOUNTPROFILE : GET 
        public ActionResult AccountProfile(int id = 0)
        {
            var DataHelper = new DataHelpers();

            ViewBag.errorCode = 0;

            int zero = 0;

            var memberDirectorProfileViewModel = new MemberDirectorProfileViewModel();

            memberDirectorProfileViewModel.MemberDirectorRecord = db.Database.SqlQuery<MemberDirector>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                new SqlParameter("@CustID", CustID)
                ).FirstOrDefault();

            int periodTypeID = Constants.monthlyPeriodTypeID;

            int RowCount = 10;

            memberDirectorProfileViewModel.CommissionHeaderList = db.Database.SqlQuery<CommissionHeader>("exec dbo.[LB_GetCommissionHeaderByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status, @RowCount",
                new SqlParameter("@CustID", CustID),
                new SqlParameter("@PeriodTypeID", periodTypeID),
                new SqlParameter("@StartPeriodID", zero),
                new SqlParameter("@EndPeriodID", zero),
                new SqlParameter("@CommissionID", zero),
                new SqlParameter("@Status", " "),
                new SqlParameter("@RowCount", RowCount)
                ).ToList();

            DateTime meetingStartDate = DateTime.Now.AddDays(-60);
            DateTime meetingEndDate = DateTime.Now.AddDays(60);

            memberDirectorProfileViewModel.MeetingEventList = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeader] @EventID, @CategoryID, @CustID, @SponsorName, @EventTitle, @StartDate, @EndDate, @LocationTitle, @City, @State, @Status, @StatusID, @RowCount",
            new SqlParameter("@EventID", zero),
            new SqlParameter("@CategoryID", Constants.categoryStudyGroupMeeting),
            new SqlParameter("@CustID", CustID),
            new SqlParameter("@SponsorName", ""),
            new SqlParameter("@EventTitle", ""),
            new SqlParameter("@StartDate", meetingStartDate),
            new SqlParameter("@EndDate", meetingEndDate),
            new SqlParameter("@LocationTitle", ""),
            new SqlParameter("@City", ""),
            new SqlParameter("@State", ""),
            new SqlParameter("@Status", ""),
            new SqlParameter("@StatusID", zero),
                new SqlParameter("@RowCount", 5)

            ).ToList();

            string searchDisplayName = "";
            string searchCompanyName = "";
            string searchLastName = "";
            string selectedStatus = "Active";

            memberDirectorProfileViewModel.MemberList = db.Database.SqlQuery<CustomerBasic>("exec dbo.[LB_GetCustByNameStatusSecSponsorIDType]  @DisplayName, @Company, @LastName, @Status, @CustomerType, @SponsorID, @RowCount",
                new SqlParameter("@DisplayName", searchDisplayName),
                new SqlParameter("@Company", searchCompanyName),
                new SqlParameter("@LastName", searchLastName),
                new SqlParameter("@Status", selectedStatus),
                new SqlParameter("@CustomerType", Constants.memberCustomerType),
                new SqlParameter("@SponsorID", CustID),
                new SqlParameter("@RowCount", RowCount)
                ).ToList();

            return View(memberDirectorProfileViewModel);
        }
        [HttpGet]
        public ActionResult MemberList()
        {
            string searchDisplayName = "";
            string searchCompanyName = "";
            string searchLastName = "";
            string selectedStatus = "Active";

            var memberRows = db.Database.SqlQuery<Member>("exec dbo.[LB_GetCustByNameStatusSecSponsorIDType]  @DisplayName, @Company, @LastName, @Status, @CustomerType, @SponsorID",
             new SqlParameter("@DisplayName", searchDisplayName),
             new SqlParameter("@Company", searchCompanyName),
             new SqlParameter("@LastName", searchLastName),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@CustomerType", Constants.memberCustomerType),
             new SqlParameter("@SponsorID", CustID)
             ).ToList();

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, true);

            var memberDirectorMemberListViewModel = new MemberDirectorMemberListViewModel
            {
                CustID = CustID,
                MemberList = memberRows,
                SearchLastName = searchLastName,
                SelectedStatus = selectedStatus,
                StatusList = thisList
            };
            return View(memberDirectorMemberListViewModel);
        }

        // INDEX : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberList([Bind(Include = "SearchDisplayNameName, SearchLastName, SelectedStatus")] MemberDirectorMemberListViewModel memberDirectorMemberListViewModel)
        {
            string searchDisplayName = memberDirectorMemberListViewModel.SearchDisplayName ?? "";
            string searchCompanyName = "";
            string searchLastName = memberDirectorMemberListViewModel.SearchLastName ?? "";
            string selectedStatus = memberDirectorMemberListViewModel.SelectedStatus ?? "Active";

            var memberRows = db.Database.SqlQuery<Member>("exec dbo.[LB_GetCustByNameStatusSecSponsorIDType]  @DisplayName, @Company, @LastName, @Status, @CustomerType, @SponsorID",
             new SqlParameter("@DisplayName", searchDisplayName),
             new SqlParameter("@Company", searchCompanyName),
             new SqlParameter("@LastName", searchLastName),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@CustomerType", Constants.memberCustomerType),
             new SqlParameter("@SponsorID", CustID)
             ).ToList();

            memberDirectorMemberListViewModel.MemberList = memberRows;

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, true);

            memberDirectorMemberListViewModel.StatusList = thisList;

            return View(memberDirectorMemberListViewModel);
        }

        [HttpGet]
        public ActionResult PayoutList()
        {
            // Initalize the View Model

            var payoutListViewModel = new PayoutListViewModel();

            payoutListViewModel.SearchStartDate = DateTime.Now.AddDays(-7);
            payoutListViewModel.SearchEndDate = DateTime.Now.AddDays(1);

            payoutListViewModel.PayoutList = db.Database.SqlQuery<C_PayOut>("exec dbo.[LB_GetPayoutByCustID] @CustID, @StartDate, @EndDate",
             new SqlParameter("@CustID", CustID),
             new SqlParameter("@StartDate", payoutListViewModel.SearchStartDate),
             new SqlParameter("@EndDate", payoutListViewModel.SearchEndDate)
            ).ToList();

            return View(payoutListViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayoutList([Bind(Include = "SearchStartDate, SearchEndDate")] PayoutListViewModel payoutListViewModel)
        {
            // Initalize the View Model
            if (ModelState.IsValid)
            {
                DateTime endDate = payoutListViewModel.SearchEndDate.AddDays(1);

                payoutListViewModel.PayoutList = db.Database.SqlQuery<C_PayOut>("exec dbo.[LB_GetPayoutByCustID] @CustID, @StartDate, @EndDate",
                 new SqlParameter("@CustID", CustID),
                 new SqlParameter("@StartDate", payoutListViewModel.SearchStartDate),
                 new SqlParameter("@EndDate", endDate)
                ).ToList();
            }

            return View(payoutListViewModel);
        }

        [HttpGet]
        public ActionResult CommissionList()
        {
            // Initalize the View Model

            var commissionDetailListViewModel = new CommissionDetailListViewModel();

            int startPeriodID = 0;
            int endPeriodID = 0;

            int searchCommissionID = 0;
            string searchStatus = " ";

            int periodTypeID = Constants.monthlyPeriodTypeID;

            commissionDetailListViewModel.CommissionDetailList = db.Database.SqlQuery<CommissionDetail>("exec dbo.[LB_GetCommissionDetailByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
             new SqlParameter("@CustID", CustID),
             new SqlParameter("@PeriodTypeID", periodTypeID),
             new SqlParameter("@StartPeriodID", startPeriodID),
             new SqlParameter("@EndPeriodID", endPeriodID),
             new SqlParameter("@CommissionID", searchCommissionID),
             new SqlParameter("@Status", searchStatus)
            ).ToList();

            commissionDetailListViewModel.StartPeriodID = startPeriodID;
            commissionDetailListViewModel.EndPeriodID = endPeriodID;
            commissionDetailListViewModel.SearchCustID = CustID;
            commissionDetailListViewModel.SearchCommissionID = searchCommissionID;
            commissionDetailListViewModel.SearchStatus = searchStatus;

            commissionDetailListViewModel.SearchPeriodIDList = DataHelper.GetPeriodIDList(Constants.monthlyPeriodTypeID, 24, false, true);

            return View(commissionDetailListViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CommissionList([Bind(Include = "StartPeriodID, EndPeriodID")] CommissionDetailListViewModel commissionDetailListViewModel)
        {
            int searchCommissionID = 0;
            string searchStatus = " ";

            commissionDetailListViewModel.CommissionDetailList = db.Database.SqlQuery<CommissionDetail>("exec dbo.[LB_GetCommissionDetailByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
             new SqlParameter("@CustID", CustID),
             new SqlParameter("@PeriodTypeID", Constants.monthlyPeriodTypeID),
             new SqlParameter("@StartPeriodID", commissionDetailListViewModel.StartPeriodID),
             new SqlParameter("@EndPeriodID", commissionDetailListViewModel.EndPeriodID),
             new SqlParameter("@CommissionID", searchCommissionID),
             new SqlParameter("@Status", searchStatus)
            ).ToList();

            commissionDetailListViewModel.SearchPeriodIDList = DataHelper.GetPeriodIDList(Constants.monthlyPeriodTypeID, 24, false, true);

            return View(commissionDetailListViewModel);
        }

        // Does Child Action only work when Ajax used to get/post form
        [ChildActionOnly]
        [HttpGet]
        public ActionResult VolumeDetailList(int id = 0)
        {
            int CDID = id;

            if (CDID == 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Commission Type provided, unable to retrieve Commission Detail List";
                return PartialView("~/Views/Commission/_VolumeDetailList.cshtml");
            }

            // Initalize the View Model

            VolumeDetailListViewModel volumeDetailListViewModel = new VolumeDetailListViewModel();

            volumeDetailListViewModel.VolumeDetailList = db.Database.SqlQuery<VolumeDetail>("exec dbo.[LB_GetVolumeDetailByCDID] @CDID",
             new SqlParameter("@CDID", CDID)
            ).ToList();

            return PartialView("~/Views/Commission/_VolumeDetailListWithHeader.cshtml", volumeDetailListViewModel);
        }

        [HttpGet]
        public ActionResult Resources()
        {
            return View();
        }

        [HttpGet]
        public ActionResult MemberVendorList()
        {

            // Debug.WriteLine("Get-" + searchLastName + "|" + searchStatus + "|" + startDateTime + "|" + endDateTime);
            var vendorRows = db.Database.SqlQuery<MemberVendor>("exec dbo.[LB_GetVendorProgramsByCategoryList]"
             ).ToList();

            var model = new MemberVendorViewModel
            {
                MemberVendorList = vendorRows,
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult VendorDetail(int id = 0, int programID = 0)
        {
            if (id == 0 || programID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Vendor or Program ID supplied to page";
                return RedirectToAction("Error", "Site");
            }

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(id, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Vendor ID supplied to page";
                return RedirectToAction("Error", "Site");
            }
            else
            {
                if (checkCustomerResult.C_Info.Flag1 == 0 || checkCustomerResult.C_Info.Flag1 == null)
                {

                }
                var model = new VendorDetaiLViewModel();

                model = db.Database.SqlQuery<VendorDetaiLViewModel>("exec dbo.[LB_GetVendorPageDetailByProgram] @VendorID, @ProgramID",
                new SqlParameter("@VendorID", id),
                new SqlParameter("@ProgramID", programID)
                ).First();

                model.documentList = db.Database.SqlQuery<Document>("exec dbo.[LB_GetVendorDocumentsByProgram] @VendorID, @ProgramID",
                new SqlParameter("@VendorID", id),
                new SqlParameter("@ProgramID", programID)
                ).ToList();

                ViewBag.CustID = CustID;
                return View("VendorDetail", model);
            }
        }

    }
}