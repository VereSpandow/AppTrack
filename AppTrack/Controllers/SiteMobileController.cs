using AuthorizeNet;
using AppTrack.Helpers;
using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppTrack.Controllers
{
    public class SiteMobileController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
        DataHelpers DataHelper = new DataHelpers();

        [HttpGet]
        public ActionResult Error()
        {
            if (TempData["ErrorMessage"] == null)
            {
                TempData["ErrorMessage"] = "";
            }
            var viewMessage = TempData["ErrorMessage"];
            ViewBag.ErrorMessage = viewMessage;
            return View();
        }

        [HttpGet]
        public ActionResult Index(string E)
        {
            var EditCookie = "";
            if (E != null)
            {
                if (E == "Y")
                {
                    EditCookie = "Y";
                }
                else
                {
                    EditCookie = "N";
                }
                // check if cookie exists and if yes update
                HttpCookie existingCookie = Request.Cookies["isEditCookie"];
                if (existingCookie != null)
                {
                    // force to expire it
                    existingCookie.Value = "";
                    existingCookie.Expires = DateTime.Now.AddHours(-20);
                }

                // create a cookie
                HttpCookie newCookie = new HttpCookie("isEditCookie", E);
                newCookie.Expires = DateTime.Today.AddDays(1);
                Response.Cookies.Add(newCookie);
            }

            ViewBag.EditCookie = EditCookie;
            return View();
        }

        public ActionResult Home()
        {
            RedirectToAction("Index");
            return View();
        }

        public ActionResult GrowthSolutions()
        {
            return View();
        }

        public ActionResult Education()
        {
            return View();
        }

        public ActionResult SavingsAndRebates()
        {
            return View();
        }

        public ActionResult AppTrack()
        {
            return View();
        }

        public ActionResult AppTrackSelect()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        public ActionResult Blog()
        {
            ViewBag.Message = "Your AppTrack Blog.";
            return View();
        }

        public ActionResult VideoGallery()
        {
            ViewBag.Message = "Your Video Gallery";
            return View();
        }

        public ActionResult AppTrackQuarterly()
        {
            ViewBag.Message = "Your AppTrack Quarterly.";
            return View();
        }

        public ActionResult Conference()
        {

            var model = new ConferenceRegistrationViewModel
            {
            };

            ViewBag.Message = "Your AppTrack Annual Conference";
            IEnumerable<System.Web.Mvc.SelectListItem> staffAttendeeList = DataHelper.GetCounterList(10);
            model.StaffAttendeeList = staffAttendeeList;

            return View(model);
        }

        public ActionResult MembershipOptions()
        {
            ViewBag.Message = "Your AppTrack Membership Options";
            return View();
        }

        public ActionResult TermsAndConditions()
        {
            return View();
        }

        public ActionResult PrivacyPolicy()
        {
            return View();
        }

        public ActionResult FAQs()
        {
            return View();
        }

        public ActionResult LocalMeetingList()
        {

            var localMeetingRows = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventsByRegisterDate]").ToList();
            var model = new LocalMeetingList
            {
                localMeetings = localMeetingRows
            };
            ViewBag.Message = "Your AppTrack Annual Conference";
            return View(model);
        }

        public ActionResult LocalMeetingListPartial()
        {

            var localMeetingRows = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeaderStudyGroups]").ToList();
            var model = new LocalMeetingList
            {
                localMeetings = localMeetingRows
            };
            ViewBag.Message = "Your AppTrack Annual Conference";
            return PartialView("_LocalMeetingListPartial", model);
        }

        [HttpGet]
        public ActionResult MeetingRegistration(int id = 0, int SponsorID = 0)
        {
            if (id == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Meeting ID supplied to registration page";
                return RedirectToAction("Error", "SiteMobile");
            }

            var localMeetingRecord = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeaderStudyGroupByID]  @ID",
            new SqlParameter("@ID", id)
            ).FirstOrDefault();

            if (localMeetingRecord == null)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Meeting ID supplied to registration page";
                return RedirectToAction("Error", "SiteMobile");
            }
            var model = new MeetingRegistrationViewModel { };

            model.meetingEvent = localMeetingRecord;
            model.meetingRegistration = new MeetingRegistration();

            model.meetingRegistration.EventID = id;
            model.meetingRegistration.SponsorID = SponsorID;

            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;

            if (TempData["Message"] == null)
            {
                TempData["Message"] = "";
            }
            var viewMessage = TempData["Message"];
            ViewBag.Message = viewMessage;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MeetingRegistration([Bind(Include = "EventID, CustID, SponsorID, NameTitle, FirstName, LastName, JobTitle, SponsorName, Email, Phone, Flag1")] MeetingRegistration meetingRegistration)
        {
            if (ModelState.IsValid)
            {
                if (meetingRegistration.EventID == 0)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Meeting ID supplied to registration page";
                    return RedirectToAction("Error", "SiteMobile");
                }

                int SponsorID = meetingRegistration.SponsorID;

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
                    if (SponsorID == 0)
                    {
                        SponsorID = scalarID;
                    }
                    TempData["Message"] = "Thank You. Please register any additional guests you wish to bring with you.";

                    return RedirectToAction("MeetingRegistration", new { id = meetingRegistration.EventID, SponsorID = SponsorID });
                }
            }
            return View(meetingRegistration);
        }

        [ChildActionOnly]
        public ActionResult ContactSubscribe()
        {
            var model = new ContactMe { };
            return PartialView("_ContactSubscribe", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public String Subscribe([Bind(Include = "contactEmail")] ContactMe contactMe)
        {
            contactMe.contactFirstName = "AppTrack";
            contactMe.contactLastName = "Quarterly";
            contactMe.contactSource = "Website Side Panel- Quarterly";
            contactMe.contactSubject = "AppTrack Quarterly Subscription";
            contactMe.contactMessage = "AppTrack Quarterly Subscription";
            try
            {
                db.LB_InsertContactsMin(contactMe.contactFirstName, contactMe.contactLastName, contactMe.contactEmail, contactMe.contactSource, contactMe.contactMessage, contactMe.contactSubject);
                string returnString = "Thank You, You Subscription is activated.";
                return returnString;
            }
            catch
            {
                string returnString = "We're sorry. Please check the form and try again.";
                return returnString;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public String ContactMe([Bind(Include = "contactDisplayName,contactEmail,contactSubject,contactMessage")] ContactMe contactMe)
        {
            string returnString = "Ooops, Request not received. Please try again.";
            if (ModelState.IsValid)
            {
                contactMe.contactDisplayName = contactMe.contactDisplayName.Trim();
                string[] nameArray = contactMe.contactDisplayName.Split(' ');
                if (nameArray.Length >= 2)
                {
                    contactMe.contactFirstName = nameArray[0];
                    contactMe.contactLastName = contactMe.contactDisplayName.Replace(contactMe.contactFirstName, "");
                }
                if (nameArray.Length == 1)
                {
                    contactMe.contactFirstName = nameArray[0];
                    contactMe.contactLastName = "None Given";
                }
                contactMe.contactSource = "Website Side Panel";
                db.LB_InsertContactsMin(contactMe.contactFirstName, contactMe.contactLastName, contactMe.contactEmail, contactMe.contactSource, contactMe.contactMessage, contactMe.contactSubject);

                returnString = "Thank You, We will contact you soon.";

            }

            return returnString;
        }


    }
}
