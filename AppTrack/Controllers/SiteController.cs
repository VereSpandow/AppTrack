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
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace AppTrack.Controllers
{
    public class SiteController : BaseController
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

            ViewBag.Title = "";
            ViewBag.MetaDescription = "";
            
            return View();
        }

        [HttpGet]
        public ActionResult Index(string E)
        {
            if(User.IsInRole("Website"))
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
            }
            ViewBag.Title = "AppTrack - Optometric Alliance | Optometry Practice Management";
            ViewBag.MetaDescription = "AppTrack is an optometric alliance of over 2,000 optometrists offering savings from over 60 industry-leading vendors and tools to help you grow your practice.";

            return View();
        }

        public ActionResult Home()
        {
            RedirectToAction("Index");
            return View();
        }

        public ActionResult H1()
        {
            return View();
        }
        public ActionResult H2()
        {
            return View();
        }
        public ActionResult H3()
        {
            return View();
        }
        public ActionResult H4()
        {
            return View();
        }
        public ActionResult P1()
        {
            return View();
        }
        public ActionResult P2()
        {
            return View();
        }
        public ActionResult P3()
        {
            return View();
        }
        public ActionResult P4()
        {
            return View();
        }

        public ActionResult GrowthSolutions()
        {
            ViewBag.Title = "Optometric Practice Management Solutions & Consulting - AppTrack";
            ViewBag.MetaDescription = "At AppTrack we're dedicated to the success of the industry. So, how do we do it? Learn more about our practice management and consulting services here.";

            return View();
        }

        public ActionResult Education()
        {
            ViewBag.Title = "Optometric Management Conferences & Local Meetings - AppTrack";
            ViewBag.MetaDescription = "AppTrack gives you the opportunity to learn from colleagues and experts alike. Attend our events to sharpen your business skills and improve the patient experience!";

            return View();
        }

        public ActionResult SavingsAndRebates()
        {
            List<MemberVendor> vendorRows = new List<MemberVendor>();

            if (User.Identity.IsAuthenticated)
            {
                vendorRows = db.Database.SqlQuery<MemberVendor>("exec dbo.[LB_GetVendorProgramsByCategoryList]"
                 ).ToList();
            }
            else
            {
                vendorRows = db.Database.SqlQuery<MemberVendor>("exec dbo.[LB_GetVendorByCategoryList]"
                 ).ToList();
            }

            var model = new MemberVendorViewModel
            {
                MemberVendorList = vendorRows,
            };

            ViewBag.Title = "Optometrist Vendor Savings Programs | Optometry Alliance - AppTrack";
            ViewBag.MetaDescription = "Our partners include over 60 industry-leading vendors offering savings on the products you use every day. Learn how AppTrack helps you save.";

            return View(model);
        }

        public ActionResult AppTrack()
        {
            ViewBag.Title = "AppTrack Optometry Practice Management | Networking & Savings - AppTrack";
            ViewBag.MetaDescription = "We do more than support independent practices. We help them thrive. Put our education, networking and savings opportunities to work for you. Learn more here.";

            return View();
        }

        public ActionResult AppTrackSelect()
        {
            ViewBag.Title = "AppTrack Select Eye Care Practice Consulting | Practice Consultants - AppTrack";
            ViewBag.MetaDescription = "AppTrack Select is the alliance option with a difference. It gives independent eye care professionals a competitive edge and a pathway to growth. Learn more here.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Title = "Eye Care Alliance Group | About AppTrack Practice Consultants - AppTrack";
            ViewBag.MetaDescription = "Since 1999, we've been giving eye care professionals a competitive advantage through vendor programs, best practices education and networking opportunities.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Title = "Contact AppTrack | Independent Doctors of Optometric Care - AppTrack";
            ViewBag.MetaDescription = "Interested in becoming an AppTrack member? Contact us here. We look forward to hearing from you! (203) 853-3333";

            return View();
        }

        public ActionResult ContactMe()
        {
            ViewBag.Title = "Contact AppTrack | Independent Doctors of Optometric Care - AppTrack";
            ViewBag.MetaDescription = "Interested in becoming an AppTrack member? Contact us here. We look forward to hearing from you! (203) 853-3333";

            return View();
        }

        public ActionResult Blog()
        {
            ViewBag.Title = "Optometric Management Blog | Industry News & Resources - AppTrack";
            ViewBag.MetaDescription = "Visit the AppTrack Blog for the latest news, helpful resources and practice management tips. Stay up to date with industry trends, read our recent posts here.";

            return View();
        }

        public ActionResult VideoGallery()
        {
            ViewBag.Title = "AppTrack Video Gallery | Optometry Practice Management Videos - AppTrack";
            ViewBag.MetaDescription = "Visit the AppTrack Video Gallery for actual videos of our AppTrack University conferences and learn why AppTrack is a leader in practice education for optometrists.";

            return View();
        }

        public ActionResult AppTrackQuarterly()
        {
            ViewBag.Title = "The AppTrack Quarterly Education & Resources for Eye Doctors - AppTrack";
            ViewBag.MetaDescription = "The AppTrack Quarterly keeps you informed on the latest optometry industry news and trends. Check out the latest edition for helpful tips and insight.";
            
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

            ViewBag.Title = "";
            ViewBag.MetaDescription = "";

            return View(model);
        }

        public ActionResult MembershipOptions()
        {
            ViewBag.Title = "Membership Options | Optometric Vendor Programs & Discounts - AppTrack";
            ViewBag.MetaDescription = "Learn how to grow your independent practice through business management conferences, study groups, and more. Plus, 60 vendor programs from top manufacturers";
            
            return View();
        }

        public ActionResult TermsAndConditions()
        {
            ViewBag.Title = "";
            ViewBag.MetaDescription = "";

            return View();
        }

        public ActionResult PrivacyPolicy()
        {
            ViewBag.Title = "";
            ViewBag.MetaDescription = "";

            return View();
        }

        public ActionResult News()
        {
            ViewBag.Title = "";
            ViewBag.MetaDescription = "";

            return View();
        }

        public ActionResult FAQs()
        {
            ViewBag.Title = "";
            ViewBag.MetaDescription = "";

            return View();
        }

        public ActionResult LocalMeetingList()
        {

            var localMeetingRows = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventsByRegisterDate]").ToList();
            var model = new LocalMeetingList
            {
                localMeetings = localMeetingRows
            };

            ViewBag.Title = "";
            ViewBag.MetaDescription = "";
            
            return View(model);
        }

        public ActionResult LocalMeetingListPartial()
        {

            var localMeetingRows = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeaderStudyGroups]").ToList();
            var model = new LocalMeetingList
            {
                localMeetings = localMeetingRows
            };
            
            ViewBag.Title = "";
            ViewBag.MetaDescription = "";

            return PartialView("_LocalMeetingListPartial", model);
        }

        [HttpGet]
        public ActionResult MeetingRegistration(int id = 0, int SponsorID = 0)
        {
            if (id == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Meeting ID supplied to registration page";
                return RedirectToAction("Error", "Site");
            }
            
            var localMeetingRecord = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeaderStudyGroupByID]  @ID",
            new SqlParameter("@ID", id)
            ).FirstOrDefault();
            
            if (localMeetingRecord == null)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Meeting ID supplied to registration page";
                return RedirectToAction("Error", "Site");
            }
            var model = new MeetingRegistrationViewModel{};

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

            ViewBag.Title = model.meetingEvent.EventTitle + " | " + model.meetingEvent.City + ", " + model.meetingEvent.State;
            ViewBag.MetaDescription = "Join fellow ODs in " + model.meetingEvent.City + ", " + model.meetingEvent.State + " focused on best practices in Optometry. Share ideas and solutions to the issues facing all independent practitioners today.";
            
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
                    return RedirectToAction("Error", "Site");
                }

                int SponsorID = meetingRegistration.SponsorID;

                meetingRegistration.Phone = DataHelper.FixPhone(meetingRegistration.Phone);

                var localMeetingRecord = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeaderStudyGroupByID]  @ID",
                new SqlParameter("@ID", meetingRegistration.EventID)
                ).FirstOrDefault();

                if (localMeetingRecord == null)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Meeting ID supplied to registration page";
                    return RedirectToAction("Error", "Site");
                }

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

                    int emailId = Constants.emailIDMeetingRegistrationNotification;
                    var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                    new SqlParameter("@ID", emailId)
                    ).FirstOrDefault();
                    string subject = emailRow.Subject;
                    string body = emailRow.Content;
                    string sEventID = meetingRegistration.EventID.ToString();
                    string sSponsorID = meetingRegistration.SponsorID.ToString();
                    string sFlag1 = meetingRegistration.Flag1.ToString();

                    body = body.Replace("#SPONSORNAME#", localMeetingRecord.SponsorName);
                    body = body.Replace("#EVENTDATE#", localMeetingRecord.EventStartDate.ToString("MM/dd/yyyy"));
                    body = body.Replace("#LOCATIONTITLE#", localMeetingRecord.LocationTitle);
                    body = body.Replace("#ADDRESS1#", localMeetingRecord.Address1);
                    body = body.Replace("#CITY#", localMeetingRecord.City);
                    body = body.Replace("#STATE#", localMeetingRecord.State);
                    body = body.Replace("#FIRSTNAME#", meetingRegistration.FirstName);
                    body = body.Replace("#LASTNAME#", meetingRegistration.LastName);
                    body = body.Replace("#NAMETITLE#", meetingRegistration.NameTitle);
                    body = body.Replace("#EMAIL#", meetingRegistration.Email);
                    body = body.Replace("#PHONE#", meetingRegistration.Phone);
                    body = body.Replace("#PRACTICENAME#", meetingRegistration.SponsorName);
                    if (meetingRegistration.Flag1 == 0)
                    {
                        body = body.Replace("#MEMBERFLAG#", "No");
                    }
                    else
                    {
                        body = body.Replace("#MEMBERFLAG#", "Yes");
                    }
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

                    return RedirectToAction("MeetingRegistration", new { id = meetingRegistration.EventID, SponsorID = SponsorID });
                }
            }
            ViewBag.Title = "";
            ViewBag.MetaDescription = "";

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
            try { 
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

                int emailId = Constants.emailIDContactMeNotification;
                var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                new SqlParameter("@ID", emailId)
                ).FirstOrDefault();
                string body = emailRow.Content;
                string subject = emailRow.Subject;
                body = body.Replace("#DISPLAYNAME#", contactMe.contactDisplayName);
                body = body.Replace("#EMAIL#", contactMe.contactEmail);
                body = body.Replace("#SUBJECT#", contactMe.contactSubject);
                body = body.Replace("#MESSAGE#", contactMe.contactMessage);
                body = Constants.emailTemplate.Replace("#BODYCOPY#", body);
                body = body.Replace("#SITEURL#", Constants.siteURL);
                SmtpClient client = new SmtpClient();
                MailMessage mailMessage = new MailMessage();
                MailAddress FromAddress = new MailAddress(Constants.adminEmailFrom);
                mailMessage.From = FromAddress;
                mailMessage.To.Add(Constants.contactEmailTo);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                client.Send(mailMessage);

                returnString = "Thank You, We will contact you soon.";
            }

            return returnString;
        }

        [HttpGet]
        public ActionResult PromotionalOffer()
        {
            var model = new PromoContact { };

            return View("PromotionalOffer", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PromotionalOffer([Bind(Include = "contactName,contactDisplayName,contactEmail,contactPhone,contactSource")] PromoContact promoContact)
        {
            if (ModelState.IsValid)
            {
                string fullName = promoContact.contactName.Trim();
                string[] nameArray = fullName.Split(' ');
                if (nameArray.Length >= 2)
                {
                    promoContact.contactFirstName = nameArray[0];
                    promoContact.contactLastName = fullName.Replace(promoContact.contactFirstName, "");
                }
                if (nameArray.Length == 1)
                {
                    promoContact.contactFirstName = nameArray[0];
                    promoContact.contactLastName = "None";
                }
                db.LB_InsertContact(promoContact.contactFirstName, promoContact.contactLastName, promoContact.contactDisplayName, promoContact.contactEmail, promoContact.contactPhone, promoContact.contactSubject, promoContact.contactMessage, promoContact.contactSource);

                int emailId = Constants.emailIDPromotionalOfferNotification;
                var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                new SqlParameter("@ID", emailId)
                ).FirstOrDefault();
                string body = emailRow.Content;
                string subject = emailRow.Subject;
                body = body.Replace("#FIRSTNAME#", promoContact.contactFirstName);
                body = body.Replace("#LASTNAME#", promoContact.contactLastName);
                body = body.Replace("#DISPLAYNAME#", promoContact.contactDisplayName);
                body = body.Replace("#EMAIL#", promoContact.contactEmail);
                body = body.Replace("#PHONE#", promoContact.contactPhone);
                body = Constants.emailTemplate.Replace("#BODYCOPY#", body);
                body = body.Replace("#SITEURL#", Constants.siteURL);
                SmtpClient client = new SmtpClient();
                MailMessage mailMessage = new MailMessage();
                MailAddress FromAddress = new MailAddress(Constants.adminEmailFrom);
                mailMessage.From = FromAddress;
                mailMessage.To.Add(Constants.contactEmailTo);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                client.Send(mailMessage);

                promoContact = new PromoContact();
                ModelState.Clear();

                ViewBag.Message = "Success";
                return View(promoContact);
            }

            return View(promoContact);
        }

        [HttpGet]
        public ActionResult HealthCareReform2015()
        {
            var model = new PromoContactEmail { };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HealthCareReform2015([Bind(Include = "contactName,contactDisplayName,contactEmail,contactPhone,Option1,Agreement")] PromoContactEmail promoContactEmail)
        {
            if (ModelState.IsValid)
            {
                string fullName = promoContactEmail.contactName.Trim();
                string[] nameArray = fullName.Split(' ');
                if (nameArray.Length >= 2)
                {
                    promoContactEmail.contactFirstName = nameArray[0];
                    promoContactEmail.contactLastName = fullName.Replace(promoContactEmail.contactFirstName, "");
                }
                if (nameArray.Length == 1)
                {
                    promoContactEmail.contactFirstName = nameArray[0];
                    promoContactEmail.contactLastName = "None";
                }

                string Source = "Health Care Reform 2015 Promotion";
                string Message = "";

                if (promoContactEmail.Option1 == true)
                { 
                    Message = "Send Health Care Reform 2015 - Part 1 and 2";
                }
                else
                {
                    Message = "Send Health Care Reform 2015 - Part 2 only";
                }
                db.LB_InsertContact(promoContactEmail.contactFirstName, promoContactEmail.contactLastName, promoContactEmail.contactDisplayName, promoContactEmail.contactEmail, promoContactEmail.contactPhone, "", Message, Source);

                int emailId = Constants.emailIDPromotionalOfferNotification;
                var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                new SqlParameter("@ID", emailId)
                ).FirstOrDefault();
                string body = emailRow.Content;
                string subject = emailRow.Subject;
                body = body.Replace("#FIRSTNAME#", promoContactEmail.contactFirstName);
                body = body.Replace("#LASTNAME#", promoContactEmail.contactLastName);
                body = body.Replace("#DISPLAYNAME#", promoContactEmail.contactDisplayName);
                body = body.Replace("#EMAIL#", promoContactEmail.contactEmail);
                body = body.Replace("#PHONE#", promoContactEmail.contactPhone);
                body = body.Replace("#SOURCE#", Source);
                body = body.Replace("#MESSAGE#", Message);
                body = Constants.emailTemplate.Replace("#BODYCOPY#", body);
                body = body.Replace("#SITEURL#", Constants.siteURL);
                SmtpClient client = new SmtpClient();
                MailMessage mailMessage = new MailMessage();
                MailAddress FromAddress = new MailAddress(Constants.adminEmailFrom);
                mailMessage.From = FromAddress;
                mailMessage.To.Add(Constants.contactEmailTo);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                client.Send(mailMessage);

                promoContactEmail = new PromoContactEmail();
                ModelState.Clear();

                ViewBag.Message = "Success";
                return View(promoContactEmail);
            }

            return View(promoContactEmail);
        }

        [HttpGet]
        public ActionResult NationalConference2016(int page = 1)
        {
            ViewBag.Page = page;

            return View();
        }

        [HttpGet]
        public ActionResult AppTrackPrimaEyeGroup()
        {
            var model = new PromoContactEmail { };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AppTrackPrimaEyeGroup([Bind(Include = "contactName,contactDisplayName,contactEmail,contactPhone")] PromoContactEmail promoContactEmail)
        {
            if (ModelState.IsValid)
            {
                string fullName = promoContactEmail.contactName.Trim();
                string[] nameArray = fullName.Split(' ');
                if (nameArray.Length >= 2)
                {
                    promoContactEmail.contactFirstName = nameArray[0];
                    promoContactEmail.contactLastName = fullName.Replace(promoContactEmail.contactFirstName, "");
                }
                if (nameArray.Length == 1)
                {
                    promoContactEmail.contactFirstName = nameArray[0];
                    promoContactEmail.contactLastName = "None";
                }

                string Source = "AppTrack Prima Eye Care";
                string Message = "";

                Message = "Contact recieved from AppTrack Prima Eye Group page";

                db.LB_InsertContact(promoContactEmail.contactFirstName, promoContactEmail.contactLastName, promoContactEmail.contactDisplayName, promoContactEmail.contactEmail, promoContactEmail.contactPhone, "", Message, Source);

                int emailId = Constants.emailIDPromotionalOfferNotification;
                var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                new SqlParameter("@ID", emailId)
                ).FirstOrDefault();
                string body = emailRow.Content;
                string subject = emailRow.Subject;
                body = body.Replace("#FIRSTNAME#", promoContactEmail.contactFirstName);
                body = body.Replace("#LASTNAME#", promoContactEmail.contactLastName);
                body = body.Replace("#DISPLAYNAME#", promoContactEmail.contactDisplayName);
                body = body.Replace("#EMAIL#", promoContactEmail.contactEmail);
                body = body.Replace("#PHONE#", promoContactEmail.contactPhone);
                body = body.Replace("#SOURCE#", Source);
                body = body.Replace("#MESSAGE#", Message);
                body = Constants.emailTemplate.Replace("#BODYCOPY#", body);
                body = body.Replace("#SITEURL#", Constants.siteURL);
                SmtpClient client = new SmtpClient();
                MailMessage mailMessage = new MailMessage();
                MailAddress FromAddress = new MailAddress(Constants.adminEmailFrom);
                mailMessage.From = FromAddress;
                mailMessage.To.Add(Constants.contactEmailTo);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                client.Send(mailMessage);

                promoContactEmail = new PromoContactEmail();
                ModelState.Clear();

                ViewBag.Message = "Success";
                return View(promoContactEmail);
            }

            return View(promoContactEmail);
        }
        [HttpGet]
        public FileResult DownloadFile(string fileName)
        {
            string contentType = "application/pdf";

            string fullPath = Server.MapPath(String.Format("~/Documents/{0}", fileName));
            if (System.IO.File.Exists(fullPath))
            {
                contentType = MimeMapping.GetMimeMapping(fullPath);

                //return File(filedata, contentType);

                //Parameters to file are
                //1. The File Path on the File Server
                //2. The content type MIME type
                //3. The parameter for the file save by the browser

            }
            return File(fullPath, contentType, fileName);
        }

    }
}
