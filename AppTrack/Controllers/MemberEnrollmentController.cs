
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
using System.Collections.Generic;
using AuthorizeNet;
using System;
using System.Net.Mail;

namespace AppTrack.Controllers
{
    [AuthorizeAdminRedirect(Roles = "Accounting,MemberEnrollment")]
    public class MemberEnrollmentController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
        DataHelpers DataHelper = new DataHelpers();

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public MemberEnrollmentController()
        {
        }

        public MemberEnrollmentController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        [HttpGet]
        public ActionResult MemberEnrollmentTest(int ID = 5, int custID = 148778)
        {
            if (ID == 2)
            {
                var model = new MemberEnrollmentLocations
                {
                };
                IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                model.StateList = stateList;
                return PartialView("MemberEnrollmentLocations", model);
            }
            if (ID == 3)
            {
                var paymentrecord = db.Database.SqlQuery<MemberEnrollmentThankYou>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", custID)
                 ).FirstOrDefault();

                var model = new MemberEnrollmentPaymentMethod
                {
                    CustID = custID,
                    PName = paymentrecord.FirstName + ' ' + paymentrecord.LastName
                };
                IEnumerable<System.Web.Mvc.SelectListItem> cardTypeList = DataHelper.GetCardTypeSelectList();
                model.CardTypeList = cardTypeList;
                IEnumerable<System.Web.Mvc.SelectListItem> cardExpMonthList = DataHelper.GetCardExpMonthsSelectList();
                model.CardExpMonthList = cardExpMonthList;
                IEnumerable<System.Web.Mvc.SelectListItem> cardExpYearList = DataHelper.GetCardExpYearsSelectList();
                model.CardExpYearList = cardExpYearList;
                return PartialView("MemberEnrollmentPaymentMethod", model);
            }
            if (ID == 4)
            {
                var enrollmentVendorRows = db.Database.SqlQuery<MemberEnrollmentVendor>("exec dbo.[LB_GetVendorsAllByCategory]").ToList();
                var model = new MemberEnrollmentVendorSelect
                {
                    CustID = custID,
                    MemberEnrollmentVendorList = enrollmentVendorRows
                };
                return PartialView("MemberEnrollmentVendorSelect", model);
            }
            if (ID == 5)
            {
                var thankYouCustID = custID;

                var thankYouModel = db.Database.SqlQuery<MemberEnrollmentThankYou>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).FirstOrDefault();

                thankYouModel.Locations = 5;
            
            return PartialView("MemberEnrollmentThankYou", thankYouModel);

            }
 
            return View("MemberEnrollment");
        }

 

        // GET: MemberEnrollment Prep
        public ActionResult Index()
        {
            return View("MemberEnrollment");
        }

        // CREATE : GET
        [HttpGet]
        public ActionResult MemberEnrollment()
        {
            return View();
        }


        // GET: MemberEnrollment Promotion
        [HttpGet]
        public ActionResult MemberEnrollmentPromotion(int secSponsorID = 0)
        {
            var model = new MemberEnrollmentPrep            {
                SecSponsorID = secSponsorID
            };

            IEnumerable<System.Web.Mvc.SelectListItem> itemList = DataHelper.GetItemSelectList();
            model.ItemList = itemList;
            IEnumerable<System.Web.Mvc.SelectListItem> promotionList = DataHelper.GetItemPromotionSelectList(1);
            model.PromotionList = promotionList;
            return PartialView("MemberEnrollmentPromotion", model);
        }

        // POST: Get dynamic Promotion list
        [HttpGet]
        public ActionResult GetPromotionList(int id)
        {
            PromotionListViewModel model = new PromotionListViewModel();
            IEnumerable<System.Web.Mvc.SelectListItem> promotionList = DataHelper.GetItemPromotionSelectList(id);
            model.PromotionList = promotionList;

            return PartialView("_PromotionListView", model);
        }

        // POST: MemberEnrollment Post Prep Goto IMD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberEnrollmentPromotion([Bind(Include = "ItemID,PromotionID")] MemberEnrollmentPrep model)
        {
            if (ModelState.IsValid)
            {
                int? promotionID = model.PromotionID;
                int? SponsorID = model.SponsorID;
                int? SecSponsorID = model.SecSponsorID;
                IEnumerable<System.Web.Mvc.SelectListItem> iMDList = DataHelper.GetCustIDList(Constants.memberDirectorCustomerType);
                model.IMDList = iMDList;
                IEnumerable<System.Web.Mvc.SelectListItem> repList = DataHelper.GetCustIDList(Constants.salesRepCustomerType);
                model.RepList = repList;
                return PartialView("MemberEnrollmentIMD", model);
            }
            else
            {
                IEnumerable<System.Web.Mvc.SelectListItem> promotionList = DataHelper.GetItemPromotionSelectList(1);
                model.PromotionList = promotionList;
                return PartialView("MemberEnrollmentPromotion", model);
            }
        }

        // POST: MemberEnrollment Post Prep Goto IMD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberEnrollmentIMD([Bind(Include = "ItemID,PromotionID,SponsorID,SecSponsorID")] MemberEnrollmentPrep model)
        {
            if (ModelState.IsValid)
            {
                int promotionID = 0;
                if (!Int32.TryParse(model.PromotionID.ToString(), out promotionID))
                    promotionID = 0;

                int? SponsorID = model.SponsorID;
                if (SponsorID == 0)
                {
                    SponsorID = Constants.orphanSalesRepID;
                    model.SponsorID = SponsorID;
                }
                int? SecSponsorID = model.SecSponsorID;
                if (SecSponsorID == 0)
                {
                    if (promotionID == Constants.IMDEnrollmentPromotionID)
                    {
                        ModelState.AddModelError("", "You must select the IMD you are enrolling");

                        IEnumerable<System.Web.Mvc.SelectListItem> iMDList = DataHelper.GetCustIDList(Constants.memberDirectorCustomerType);
                        model.IMDList = iMDList;
                        IEnumerable<System.Web.Mvc.SelectListItem> repList = DataHelper.GetCustIDList(Constants.salesRepCustomerType);
                        model.RepList = repList;
                        return PartialView("MemberEnrollmentIMD", model);

                    }
                    SecSponsorID = Constants.orphanIMDID;
                    model.SecSponsorID = SecSponsorID;
                }
                if (SecSponsorID > 0){
                    if (promotionID == Constants.IMDEnrollmentPromotionID)
                    {
                        CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(model.SecSponsorID, Constants.memberDirectorCustomerType);

                        if (checkCustomerResult.ErrorCode != 0)
                        {
                            ModelState.AddModelError("", "The IMD Selected was not found.");  
                        }
                        else
                        {
                            if (checkCustomerResult.C_Info.SponsorID != null  && checkCustomerResult.C_Info.SponsorID != 0)
                            {
                                ModelState.AddModelError("", "The IMD selected is already a Member with the Member ID: " + checkCustomerResult.C_Info.SponsorID); 
                            }
                        }

                        if (!ModelState.IsValid)
                        {
                            IEnumerable<System.Web.Mvc.SelectListItem> iMDList = DataHelper.GetCustIDList(Constants.memberDirectorCustomerType);
                            model.IMDList = iMDList;
                            IEnumerable<System.Web.Mvc.SelectListItem> repList = DataHelper.GetCustIDList(Constants.salesRepCustomerType);
                            model.RepList = repList;
                            return PartialView("MemberEnrollmentIMD", model);
                        }
                    }
                    IEnumerable<System.Web.Mvc.SelectListItem> meetingList = DataHelper.GetMeetingFormCommissionsList();
                    model.EnrollmentMeetingList = meetingList;
                    return PartialView("MemberEnrollmentMeeting", model);
                }
                else
                {
                    DateTime now = DateTime.Now;

                    
                    var modelMEP = new MemberEnrollmentPrimary
                    {
                        SecSponsorID = model.SecSponsorID,
                        SponsorID = model.SponsorID,
                        EventID = Constants.orphanMeetingID,
                        ItemID = model.ItemID,
                        PromotionID = model.PromotionID,
                        StartDate = now
                    };

                    IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                    modelMEP.StateList = stateList;
                    IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
                    modelMEP.NameTitleList = nameTitleList;

                    return PartialView("MemberEnrollmentPrimary", modelMEP);

                }
            }
            else
            {
                IEnumerable<System.Web.Mvc.SelectListItem> iMDList = DataHelper.GetCustIDList(Constants.memberDirectorCustomerType);
                model.IMDList = iMDList;
                IEnumerable<System.Web.Mvc.SelectListItem> repList = DataHelper.GetCustIDList(Constants.salesRepCustomerType);
                model.RepList = repList;
                return PartialView("MemberEnrollmentIMD", model);
            }
        }

        // POST: MemberEnrollment Post IMD Goto Meeting
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberEnrollmentMeeting([Bind(Include = "ItemID,PromotionID,SponsorID,SecSponsorID,EventID")] MemberEnrollmentPrep model)
        {
            if (ModelState.IsValid)
            {
                int? promotionID = model.PromotionID;
                int? SponsorID = model.SponsorID;
                int? SecSponsorID = model.SecSponsorID;
                DateTime now = DateTime.Now;

                var modelMEP = new MemberEnrollmentPrimary
                {
                    SecSponsorID = model.SecSponsorID,
                    SponsorID = model.SponsorID,
                    EventID = model.EventID,
                    ItemID = model.ItemID,
                    PromotionID = model.PromotionID,
                    StartDate = now
                };

                IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                modelMEP.StateList = stateList;
                IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
                modelMEP.NameTitleList = nameTitleList;
 
                return PartialView("MemberEnrollmentPrimary", modelMEP);

            }
            else
            {
                return PartialView("MemberEnrollmentMeeting", model);
            }
        }

        // CREATE : GET
        // this is not currently used
        [HttpGet]
        public ActionResult MemberEnrollmentPrimary(int secSponsorID = 0)
        {
            DateTime now = DateTime.Now;
            var model = new MemberEnrollmentPrimary
            {
                SecSponsorID = secSponsorID,
                StartDate = now
            };

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;

            return PartialView("MemberEnrollmentPrimary", model);

        }


        // CREATE : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberEnrollmentPrimary([Bind(Include = "SponsorID, SecSponsorID, EventID, ItemID, PromotionID, MemberID, TaxID, DisplayName, Company,NameTitle, FirstName, LastName, Address1, Address2, City, State, PostalCode, Password, ConfirmPassword, DayPhone, Mobile, Fax, Email, AccountingID, Multilocation, StartDate")] MemberEnrollmentPrimary memberEnrollmentPrimary)
        {
            DateTime now = DateTime.Now;
            DateTime StartDate = memberEnrollmentPrimary.StartDate ?? DateTime.Now;
            if (StartDate > now && AdminID != 50)
            {
                ModelState.AddModelError("", "Start Date cannot be after today");
            }

            if (!ModelState.IsValid)
            {
                IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                memberEnrollmentPrimary.StateList = stateList;
                IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
                memberEnrollmentPrimary.NameTitleList = nameTitleList;
                return PartialView("MemberEnrollmentPrimary", memberEnrollmentPrimary);
            }
            else
            {
                if (memberEnrollmentPrimary.TaxID != null)
                {
                    memberEnrollmentPrimary.TaxID = memberEnrollmentPrimary.TaxID.Replace("-", "");
                }
                memberEnrollmentPrimary.DayPhone = DataHelper.FixPhone(memberEnrollmentPrimary.DayPhone);
                memberEnrollmentPrimary.Fax = DataHelper.FixPhone(memberEnrollmentPrimary.Fax);

                memberEnrollmentPrimary.ParentID = 0;

                int pricingLevel = 1;

                // If we are enrolling an IMD based on the promotion selected, then set the Pricing Level accordingly
                if (memberEnrollmentPrimary.PromotionID == Constants.IMDEnrollmentPromotionID)
                {
                    pricingLevel = Constants.IMDPricingLevel;                    
                }

                ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_InsertMemberV2(Constants.memberCustomerType, 
                    memberEnrollmentPrimary.DisplayName,
                    memberEnrollmentPrimary.NameTitle,
                    memberEnrollmentPrimary.FirstName,
                    memberEnrollmentPrimary.LastName,
                    memberEnrollmentPrimary.Email, 
                    memberEnrollmentPrimary.Address1,
                    memberEnrollmentPrimary.Address2,
                    memberEnrollmentPrimary.City,
                    memberEnrollmentPrimary.State,
                    memberEnrollmentPrimary.PostalCode,
                    memberEnrollmentPrimary.DayPhone,
                    memberEnrollmentPrimary.DayPhone,
                    "",
                    memberEnrollmentPrimary.Fax,
                    memberEnrollmentPrimary.Company,
                    memberEnrollmentPrimary.TaxID,
                    memberEnrollmentPrimary.SponsorID,
                    memberEnrollmentPrimary.SecSponsorID,
                    memberEnrollmentPrimary.ParentID,
                    memberEnrollmentPrimary.ItemID,
                    memberEnrollmentPrimary.PromotionID,
                    memberEnrollmentPrimary.AccountingID,
                    memberEnrollmentPrimary.EventID,
                    memberEnrollmentPrimary.Password,
                    pricingLevel,
                    StartDate,
                    returnCustID, returnMessage);

                var scalarCustID = (int)returnCustID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID <= -1)
                {
                    ModelState.AddModelError("", errorMessage);

                    IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                    memberEnrollmentPrimary.StateList = stateList;
                    IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
                    memberEnrollmentPrimary.NameTitleList = nameTitleList;
                    return PartialView("MemberEnrollmentPrimary", memberEnrollmentPrimary);
                }
                else
                {
                    // Insert Member Director into Identity login tables
                    try
                    {
                        var user = new ApplicationUser { UserName = memberEnrollmentPrimary.Email, Email = memberEnrollmentPrimary.Email, DisplayName = memberEnrollmentPrimary.FirstName + " " + memberEnrollmentPrimary.LastName, CustID = scalarCustID };
                        IdentityResult userResult = UserManager.Create(user, memberEnrollmentPrimary.Password);
                        if (!userResult.Succeeded)
                        {
                            AddErrors(userResult);
                        }
                        else
                        {
                            try
                            {
                                IdentityResult roleResult = UserManager.AddToRoles(user.Id, "Member");
                                if (!roleResult.Succeeded)
                                {
                                    ModelState.AddModelError("", roleResult.Errors.First());
                                }
                            }
                            catch
                            {
                                ModelState.AddModelError("", "Error encountered adding Member Role");
                            }
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Error encountered adding Member Login");
                    }
                }

                Response.Cookies["PrimaryEmail"].Value = memberEnrollmentPrimary.Email;
                Response.Cookies["FirstName"].Value = memberEnrollmentPrimary.FirstName;
                Response.Cookies["LastName"].Value = memberEnrollmentPrimary.LastName;

                if (memberEnrollmentPrimary.Multilocation == "Y")
                {
                    ModelState.Clear();
                    var model = new MemberEnrollmentLocations
                    {
                        SponsorID = memberEnrollmentPrimary.SponsorID,
                        SecSponsorID = memberEnrollmentPrimary.SponsorID,
                        ParentID = scalarCustID,
                        SecID = memberEnrollmentPrimary.SecID,
                        DisplayName = "",
                        FirstName = "",
                        LastName = "",
                        Company = memberEnrollmentPrimary.Company,
                        TaxID = memberEnrollmentPrimary.TaxID,
                        AccountingID = memberEnrollmentPrimary.AccountingID,
                        Email = memberEnrollmentPrimary.Email,
                        Address1 = "",
                        Address2 = "",
                        City = "",
                        State = memberEnrollmentPrimary.State,
                        PostalCode = "",
                        DayPhone = "",
                        Fax = memberEnrollmentPrimary.Fax,
                        StartDate = StartDate,
                        AddLocation = "Y"
                    };

                    IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                    model.StateList = stateList;
                    return PartialView("MemberEnrollmentLocations", model);
                }
                else
                {
                    if (memberEnrollmentPrimary.PromotionID == 5)
                        // This was a temporary promotion until the Prima integration was completed.
                    {
                        db.LB_UpdateMemberIncomplete(scalarCustID);

                        db.LB_UpdateAutoShipDiscount(scalarCustID,25);
                        
                        // Existing Prima Member gets Free AppTrack Membership and we do not want to capture credit card
                        var thankYouModel = new MemberEnrollmentThankYou
                        {
                            CustID = scalarCustID,
                            DisplayName = memberEnrollmentPrimary.DisplayName,
                            FirstName = memberEnrollmentPrimary.FirstName,
                            LastName = memberEnrollmentPrimary.LastName,
                            DayPhone = memberEnrollmentPrimary.DayPhone,
                            Email = memberEnrollmentPrimary.Email,
                            Locations = 1
                        };

                        return PartialView("MemberEnrollmentThankYou", thankYouModel);
                    }
                    var model = new MemberEnrollmentPaymentMethod
                    {
                        CustID = scalarCustID,
                        PName = memberEnrollmentPrimary.FirstName + ' ' + memberEnrollmentPrimary.LastName
                    };
                    IEnumerable<System.Web.Mvc.SelectListItem> cardTypeList = DataHelper.GetCardTypeSelectList();
                    model.CardTypeList = cardTypeList;
                    IEnumerable<System.Web.Mvc.SelectListItem> cardExpMonthList = DataHelper.GetCardExpMonthsSelectList();
                    model.CardExpMonthList = cardExpMonthList;
                    IEnumerable<System.Web.Mvc.SelectListItem> cardExpYearList = DataHelper.GetCardExpYearsSelectList();
                    model.CardExpYearList = cardExpYearList;
                    return PartialView("MemberEnrollmentPaymentMethod", model);
                }

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberEnrollmentLocations([Bind(Include = "SponsorID,ParentID,SecSponsorID,EventID,DisplayName,FirstName,LastName,Email,Address1,Address2,City,State,PostalCode,DayPhone,Fax,Company,TaxID,StartDate,AddLocation")] MemberEnrollmentLocations memberEnrollmentLocations)
        {
            var submitAction = Request.Form["Submit"];
            string nextPage = "";

            if (submitAction == "Discard and Continue")
            {
                ModelState.Remove("DisplayName");
                ModelState.Remove("FirstName");
                ModelState.Remove("LastName");
                ModelState.Remove("Email");
                ModelState.Remove("Address1");
                ModelState.Remove("City");
                ModelState.Remove("State");
                ModelState.Remove("PostalCode");
                ModelState.Remove("DayPhone");
                ModelState.Remove("Company");
                ModelState.Remove("TaxID");

                nextPage = "Payment";
            }
            else
            {
                if (submitAction != "Add Another Location")
                {
                    memberEnrollmentLocations.AddLocation = "N";
                    nextPage = "Payment";
                }
                else
                {
                    memberEnrollmentLocations.AddLocation = "Y";
                    nextPage = "Location";
                }

                if (memberEnrollmentLocations.SecSponsorID == 0)
                {
                    memberEnrollmentLocations.SecSponsorID = Constants.orphanIMDID;
                }
                // Need to remove any Required properties in the model that are not being supplied by form
                ModelState.Remove("Password");
                ModelState.Remove("ConfirmPassword");

                if (ModelState.IsValid)
                {
                    memberEnrollmentLocations.DayPhone = DataHelper.FixPhone(memberEnrollmentLocations.DayPhone);
                    memberEnrollmentLocations.Fax = DataHelper.FixPhone(memberEnrollmentLocations.Fax);

                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_InsertMemberV2(66,
                        memberEnrollmentLocations.DisplayName, "",
                        memberEnrollmentLocations.FirstName,
                        memberEnrollmentLocations.LastName,
                        memberEnrollmentLocations.Email,
                        memberEnrollmentLocations.Address1,
                        memberEnrollmentLocations.Address2,
                        memberEnrollmentLocations.City,
                        memberEnrollmentLocations.State,
                        memberEnrollmentLocations.PostalCode,
                        memberEnrollmentLocations.DayPhone,
                        memberEnrollmentLocations.DayPhone,
                        "",
                        memberEnrollmentLocations.Fax,
                        memberEnrollmentLocations.Company,
                        memberEnrollmentLocations.TaxID,
                        memberEnrollmentLocations.ParentID,
                        memberEnrollmentLocations.SecSponsorID,
                        memberEnrollmentLocations.ParentID,
                        0,
                        0,
                        memberEnrollmentLocations.AccountingID,
                        0,
                        "",
                        1,
                        memberEnrollmentLocations.StartDate,
                        returnCustID, returnMessage);

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID <= -1)
                    {
                        scalarCustID = -1;
                        ModelState.AddModelError("", errorMessage);
                        IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                        memberEnrollmentLocations.StateList = stateList;
                        return PartialView("MemberEnrollmentLocations", memberEnrollmentLocations);
                    }
                }
                else
                {
                    // Model State not valid
                    IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                    memberEnrollmentLocations.StateList = stateList;
                    return PartialView("MemberEnrollmentLocations", memberEnrollmentLocations);
                }
            }

            if (nextPage == "Location")
            {
                ModelState.Clear();
                memberEnrollmentLocations.FirstName = "";
                memberEnrollmentLocations.LastName = "";
                memberEnrollmentLocations.DisplayName = "";
                memberEnrollmentLocations.Address1 = "";
                memberEnrollmentLocations.Address2 = "";
                memberEnrollmentLocations.City = "";
                memberEnrollmentLocations.PostalCode = "";
                memberEnrollmentLocations.DayPhone = "";
                memberEnrollmentLocations.StartDate = memberEnrollmentLocations.StartDate;
                IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                memberEnrollmentLocations.StateList = stateList;
                ViewBag.Message = "The Location was successfully saved. Complete the form below to add another.";
                ModelState.AddModelError("", "The Location was successfully saved. Complete the form below to add another.");
                return PartialView("MemberEnrollmentLocations", memberEnrollmentLocations);
            }
            else
            {
                var paymentrecord = db.Database.SqlQuery<MemberEnrollmentThankYou>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", memberEnrollmentLocations.ParentID)
                 ).FirstOrDefault();

                var model = new MemberEnrollmentPaymentMethod
                {
                    CustID = memberEnrollmentLocations.ParentID,
                    PName = paymentrecord.FirstName + ' ' + paymentrecord.LastName
                };

                IEnumerable<System.Web.Mvc.SelectListItem> cardTypeList = DataHelper.GetCardTypeSelectList();
                model.CardTypeList = cardTypeList;
                IEnumerable<System.Web.Mvc.SelectListItem> cardExpMonthList = DataHelper.GetCardExpMonthsSelectList();
                model.CardExpMonthList = cardExpMonthList;
                IEnumerable<System.Web.Mvc.SelectListItem> cardExpYearList = DataHelper.GetCardExpYearsSelectList();
                model.CardExpYearList = cardExpYearList;
                return PartialView("MemberEnrollmentPaymentMethod", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberEnrollmentPaymentMethod([Bind(Include = "CustID,PName,PCardNumber,PCardType,PExpirationMonth,PExpirationYear,CustomerProfile,PaymentProfile,CardCode")] MemberEnrollmentPaymentMethod memberEnrollmentPaymentMethod)
        {

            IEnumerable<System.Web.Mvc.SelectListItem> cardTypeList = DataHelper.GetCardTypeSelectList();
            memberEnrollmentPaymentMethod.CardTypeList = cardTypeList;
            IEnumerable<System.Web.Mvc.SelectListItem> cardExpMonthList = DataHelper.GetCardExpMonthsSelectList();
            memberEnrollmentPaymentMethod.CardExpMonthList = cardExpMonthList;
            IEnumerable<System.Web.Mvc.SelectListItem> cardExpYearList = DataHelper.GetCardExpYearsSelectList();
            memberEnrollmentPaymentMethod.CardExpYearList = cardExpYearList;
            if (memberEnrollmentPaymentMethod.CustomerProfile == null)
            {
                memberEnrollmentPaymentMethod.CustomerProfile = "1";
            }
            if (memberEnrollmentPaymentMethod.PaymentProfile == null)
            {
                memberEnrollmentPaymentMethod.PaymentProfile = "1";
            }
            string CustomerProfile = memberEnrollmentPaymentMethod.CustomerProfile;

            string PaymentProfile = memberEnrollmentPaymentMethod.PaymentProfile;
            
            if (ModelState.IsValid)
            {
                string CardFirstSix = memberEnrollmentPaymentMethod.PCardNumber.Substring(0, 6);
                string CardType = memberEnrollmentPaymentMethod.PCardType;
                int CardNumberLength = memberEnrollmentPaymentMethod.PCardNumber.Length;
                string CardLastFour = memberEnrollmentPaymentMethod.PCardNumber.Substring(CardNumberLength - 4, 4);
                int? custID = memberEnrollmentPaymentMethod.CustID;
                string expMonth = memberEnrollmentPaymentMethod.PExpirationMonth.ToString();
                string expYear = memberEnrollmentPaymentMethod.PExpirationYear.ToString();


                var CInfoRecord = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", custID)
                 ).FirstOrDefault();

                int promotionID = 0;
                int autoshipID = 0;
                int AppTrackItemID = 0;
                int PRIMAItemID = 0;

                if (!Int32.TryParse(CInfoRecord.CustomerQualificationLevel.ToString(), out promotionID))
                    promotionID = 0;

                if (!Int32.TryParse(CInfoRecord.Flag1.ToString(), out autoshipID))
                    autoshipID = 0;

                if (!Int32.TryParse(CInfoRecord.Flag2.ToString(), out AppTrackItemID))
                    AppTrackItemID = 0;

                if (!Int32.TryParse(CInfoRecord.Flag3.ToString(), out PRIMAItemID))
                    PRIMAItemID = 0;


                if (AppTrackItemID > 0 && PRIMAItemID > 0)
                {
                    db.LB_UpdateAutoShipDiscountAppTrack(autoshipID);
                }

                if (promotionID > 0)
                {
                    db.LB_UpdateAutoShipPromotionFree(autoshipID, promotionID);
                }

                if (PRIMAItemID > 0)
                {
                    // Need to get autoship record with amount > 0 in case we just added a free one for the promotion
                    var AutoshipHeaderRecord = db.Database.SqlQuery<C_AutoShipHeader>("exec dbo.[LB_GetAutoshipHeaderToProrate] @CustID",
                     new SqlParameter("@CustID", custID)
                     ).FirstOrDefault();

                    autoshipID = AutoshipHeaderRecord.AutoShipID;

                    db.LB_GenerateOrderProrated(autoshipID);
                }

                string customerEmail = CInfoRecord.Email;
                string customerFirstName = CInfoRecord.FirstName;
                string customerLastName = CInfoRecord.LastName;
                string customerAddress1 = CInfoRecord.Address1;
                string customerCity = CInfoRecord.City;
                string customerState = CInfoRecord.State;
                string customerPostalCode = CInfoRecord.PostalCode;
                string customerPayPhone = CInfoRecord.DayPhone;

                ServiceMode serviceMode = ServiceMode.Test;
                string apiLogin = Constants.apiLogin;
                string transactionKey = Constants.transactionKey;
                if (Constants.AuthorizeNetStatus == "Live")
                {
                    serviceMode = ServiceMode.Live;
                    apiLogin = Constants.apiLoginLive;
                    transactionKey = Constants.transactionKeyLive;
                }

                string customerDescription = Request.Cookies["FirstName"].Value + " " + Request.Cookies["LastName"].Value;
                string paymentTransactionID = "Love it.";
                string newCustID = memberEnrollmentPaymentMethod.CustID.ToString();

                CustomerGateway customerGateway = new CustomerGateway(apiLogin, transactionKey, serviceMode);
                try
                {
                    if (CustomerProfile == "1")
                    {
                        Customer newcustomer = customerGateway.CreateCustomer(customerEmail, customerDescription, newCustID);
                        CustomerProfile = newcustomer.ProfileID;
                        memberEnrollmentPaymentMethod.CustomerProfile = CustomerProfile;
                        newcustomer.Email = customerEmail;
                        newcustomer.Description = "This is the record for " + customerDescription;
                        int intCustomerProfile = Int32.Parse(CustomerProfile);
                        db.LB_UpdateC_InfoCustomerProfileID(custID, intCustomerProfile);
                    }
                    string thisPCardNumber = memberEnrollmentPaymentMethod.PCardNumber;
                    int thisPExpMonth = Int32.Parse(memberEnrollmentPaymentMethod.PExpirationMonth);
                    int thisPExpYear = Int32.Parse(memberEnrollmentPaymentMethod.PExpirationYear);
                    //bool updateCustomer = customerGateway.UpdateCustomer(newcustomer);
                    string CardCode = memberEnrollmentPaymentMethod.CardCode;
                    try
                    {

                        Address address = new Address();
                        address.First = customerFirstName;
                        address.Last = customerLastName;
                        address.Street = customerAddress1;
                        address.City = customerCity;
                        address.State = customerState;
                        address.Zip = customerPostalCode;
                        address.Phone = customerPayPhone;
                        try
                        {
                            if (CardType == "Visa")
                            {
                                string addCreditCard = customerGateway.AddCreditCard(CustomerProfile, thisPCardNumber, thisPExpMonth, thisPExpYear, CardCode, address);
                                PaymentProfile = addCreditCard;
                            }
                            else
                            {
                                string addCreditCard = customerGateway.AddCreditCard(CustomerProfile, thisPCardNumber, thisPExpMonth, thisPExpYear, CardCode);
                                PaymentProfile = addCreditCard;
                            }

                            if (Constants.isAuthorizeOn == "Yes")
                            {
                                Order order = new Order(CustomerProfile, PaymentProfile, "");
                                order.Amount = 1.01M;
                                order.InvoiceNumber = "Initial Enrollment";
                                order.Description = customerDescription;
                                order.CustomerProfileID = CustomerProfile;
                                order.PaymentProfileID = PaymentProfile;

                                // IGatewayResponse authorize = customerGateway.Authorize(CustomerProfile, PaymentProfile, 1.57m);
                                IGatewayResponse authorize = customerGateway.Authorize(order);
                                paymentTransactionID = authorize.TransactionID;
                            }
                        }
                        catch (Exception e)
                        {
                            string s = e.Message;
                            ModelState.AddModelError("", s);
                            paymentTransactionID = "No so Good";
                            PaymentProfile = "1";
                        }
                    }
                    catch (Exception e)
                    {
                        string s = e.Message;
                        ModelState.AddModelError("", s);
                        PaymentProfile = "1";
                    }
                }
                catch (Exception e)
                {
                    string s = e.Message;
                    ModelState.AddModelError("", s);
                    CustomerProfile = "1";
                }
                ViewBag.CustomerProfile = CustomerProfile;
                ViewBag.PaymentProfile = PaymentProfile;

                if ((CustomerProfile == "1") || (PaymentProfile == "1"))
                {
                    memberEnrollmentPaymentMethod.PCardNumber = "";
                    memberEnrollmentPaymentMethod.CardCode = "";
                    return PartialView("MemberEnrollmentPaymentMethod", memberEnrollmentPaymentMethod);
                }
                else
                {

                    string shippingProfileID = "";
                    if (Constants.isShippingProfileRequired == "Yes")
                    {
                        shippingProfileID = customerGateway.AddShippingAddress(CustomerProfile, customerFirstName, customerLastName, customerAddress1, customerCity, customerState, customerPostalCode, "US", customerPayPhone);
                    }

                    db.LB_InsertPaymentMethodCIMShippingProfile(custID, memberEnrollmentPaymentMethod.PName, memberEnrollmentPaymentMethod.PCardType,
                                            CardFirstSix, CardLastFour, expMonth, expYear, PaymentProfile, CustomerProfile, shippingProfileID
                                             );

                    // LB_InsertMembershipActivity populates C_MembershipActivity with new membership records
                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_InsertMembershipActivity(custID,
                        returnCustID,
                        returnMessage);

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID < 0)
                    {
                        ModelState.AddModelError("", errorMessage);                       
                    }
                    var memberEnrollmentVendorRows = db.Database.SqlQuery<MemberEnrollmentVendor>("exec dbo.[LB_GetVendorsAllByCategory]").ToList();
                    var model = new MemberEnrollmentVendorSelect
                    {
                        CustID = memberEnrollmentPaymentMethod.CustID,
                        MemberEnrollmentVendorList = memberEnrollmentVendorRows
                    };
                    string VendorString = "0";

                    //db.LB_InsertMemberVendorParse(custID, VendorString, 1);

                    var thankYourecord = db.Database.SqlQuery<MemberEnrollmentThankYou>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                    new SqlParameter("@CustID", custID)
                    ).FirstOrDefault();

                    int isEmailOn = Constants.emailSwitch;
                    if (isEmailOn == 1)
                    {
                        int emailId = Constants.emailIDNewMember;
                        var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                        new SqlParameter("@ID", emailId)
                        ).FirstOrDefault();
                        string subject = emailRow.Subject;
                        string body = emailRow.Content;
                        string strCustID = thankYourecord.CustID.ToString();
                        SmtpClient client = new SmtpClient();
                        MailMessage mailMessage = new MailMessage();

                        if (AppTrackItemID > 0 && PRIMAItemID == 0)
                        {
                            subject = subject.Replace("#CUSTID#", strCustID);
                            subject = subject.Replace("#FIRSTNAME#", thankYourecord.FirstName);
                            subject = subject.Replace("#LASTNAME#", thankYourecord.LastName);
                            subject = subject.Replace("#NAMETITLE#", thankYourecord.NameTitle);
                            body = body.Replace("#CUSTID#", strCustID);
                            body = body.Replace("#FIRSTNAME#", thankYourecord.FirstName);
                            body = body.Replace("#LASTNAME#", thankYourecord.LastName);
                            body = body.Replace("#NAMETITLE#", thankYourecord.NameTitle);
                            body = body.Replace("#EMAIL#", thankYourecord.Email);
                            body = body.Replace("#PASSWORD#", thankYourecord.Password);
                            body = Constants.emailTemplate.Replace("#BODYCOPY#", body);
                            body = body.Replace("#SITEURL#", Constants.siteURL);
                            MailAddress FromAddress = new MailAddress(Constants.memberEmailFrom, "AppTrack Member Services");
                            mailMessage.From = FromAddress;
//                            Attachment attachment = new Attachment(Path.Combine(Server.MapPath("~/Documents"), "a.txt"));
//                            mailMessage.Attachments.Add(attachment);
                            mailMessage.To.Add(thankYourecord.Email);
                            mailMessage.Subject = subject;
                            mailMessage.Body = body;
                            mailMessage.IsBodyHtml = true;
                            client.Send(mailMessage);
                            //await client.SendMailAsync(mailMessage);
                        }

                        emailId = Constants.emailIDNewMemberNotification;
                        emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                        new SqlParameter("@ID", emailId)
                        ).FirstOrDefault();
                        subject = emailRow.Subject;
                        body = emailRow.Content;
                        subject = subject.Replace("#CUSTID#", strCustID);
                        subject = subject.Replace("#FIRSTNAME#", thankYourecord.FirstName);
                        subject = subject.Replace("#LASTNAME#", thankYourecord.LastName);
                        subject = subject.Replace("#NAMETITLE#", thankYourecord.NameTitle);
                        body = body.Replace("#CUSTID#", strCustID);
                        body = body.Replace("#DISPLAYNAME#", thankYourecord.DisplayName);
                        body = body.Replace("#FIRSTNAME#", thankYourecord.FirstName);
                        body = body.Replace("#LASTNAME#", thankYourecord.LastName);
                        body = body.Replace("#TITLE#", thankYourecord.NameTitle ?? "");
                        body = body.Replace("#STARTDATE#", DateTime.Now.ToShortDateString());
                        body = body.Replace("#DAYPHONE#", thankYourecord.DayPhone);
                        body = Constants.emailTemplate.Replace("#BODYCOPY#", body);
                        body = body.Replace("#SITEURL#", Constants.siteURL);
                        MailMessage mailMessageNotification = new MailMessage();
                        MailAddress FromAddressNotification = new MailAddress(Constants.adminEmailFrom, "AppTrack Member Services");
                        mailMessageNotification.From = FromAddressNotification;
                        mailMessageNotification.To.Add(Constants.notificationEmailTo);
                        mailMessageNotification.Subject = subject;
                        mailMessageNotification.Body = body;
                        mailMessageNotification.IsBodyHtml = true;
                        client.Send(mailMessageNotification);
                        //await client.SendMailAsync(mailMessage);
                    }


                    var thankYouModel = new MemberEnrollmentThankYou
                    {
                        CustID = custID,
                        DisplayName = thankYourecord.DisplayName,
                        FirstName = thankYourecord.FirstName,
                        LastName = thankYourecord.LastName,
                        DayPhone = thankYourecord.DayPhone,
                        Email = thankYourecord.Email,
                        Locations = thankYourecord.Volume1
                    };

                    return PartialView("MemberEnrollmentThankYou", thankYouModel);
                }
            }
            else
            {
                memberEnrollmentPaymentMethod.PCardType = "";
                memberEnrollmentPaymentMethod.PCardNumber = "";
                memberEnrollmentPaymentMethod.PExpirationMonth = "";
                memberEnrollmentPaymentMethod.PExpirationYear = "";
                memberEnrollmentPaymentMethod.CardCode = "";

                return PartialView("MemberEnrollmentPaymentMethod", memberEnrollmentPaymentMethod);
            }
        }

        // VHS 02/06/16 Removed returns above to MemberEnrollmentVendorSelect partial view as AppTrack does not capture this info on enrollment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberEnrollmentVendorSelect([Bind(Include = "CustID")] MemberEnrollmentVendorSelect memberEnrollmentVendorSelect, params string[] SelectedVendors)
        {
            string VendorString = "0";
            if (SelectedVendors != null)
            {         
                VendorString = string.Join(",", SelectedVendors);
            }
            int programID = 1;

            if (ModelState.IsValid)
            {
                db.LB_InsertMemberVendorParse(memberEnrollmentVendorSelect.CustID, VendorString, programID);
            }
            else
            {
                var thisCustID = memberEnrollmentVendorSelect.CustID;
                var memberEnrollmentVendorRows = db.Database.SqlQuery<MemberEnrollmentVendor>("exec dbo.[LB_GetVendorsAllByCategory]").ToList();

                var model = new MemberEnrollmentVendorSelect
                {
                    CustID = thisCustID,
                    MemberEnrollmentVendorList = memberEnrollmentVendorRows
                };
                return PartialView("MemberEnrollmentVendorSelect", model);
            }
            
            var thankYouCustID = memberEnrollmentVendorSelect.CustID;

            var thankYourecord = db.Database.SqlQuery<MemberEnrollmentThankYou>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                new SqlParameter("@CustID", thankYouCustID)
                ).FirstOrDefault();

            var thankYouModel = new MemberEnrollmentThankYou
            {
                CustID = thankYouCustID,
                DisplayName = thankYourecord.DisplayName,
                FirstName = thankYourecord.FirstName,
                LastName = thankYourecord.LastName,
                DayPhone = thankYourecord.DayPhone,
                Email = thankYourecord.Email,
                Locations = thankYourecord.Volume1
            };
            
            return PartialView("MemberEnrollmentThankYou", thankYouModel);
        }

    }
}