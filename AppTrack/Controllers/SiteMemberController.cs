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
using System.Threading.Tasks;
using System.Net.Mail;

namespace AppTrack.Controllers
{
    [Authorize(Roles = "Member,MemberServices,Marketing,VendorAdmin")]
    public class SiteMemberController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
        DataHelpers DataHelper = new DataHelpers();

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public SiteMemberController()
        {
        }

        public SiteMemberController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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


        // INDEX : GET
        [HttpGet]
        public ActionResult Index()
        {
           return View();
        }



        //ACOUNTPROFILE : GET 
        public ActionResult AccountProfile()
        {
            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.memberCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Member Director ID supplied to Account Profile page";
                return RedirectToAction("Error", "Site");
            }
            else
            {
                ViewBag.errorCode = 0;

                int commissionID = Constants.rebateCommissionID;
                int periodTypeID = Constants.rebateCommissionPeriodType;
                int startPeriodID = 10;
                int endPeriodID = 999999;
                string thisStatus = "";


                var model = new MemberProfileViewModel();

                model.MemberRecord = db.Database.SqlQuery<Member>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).First();

                model.CustID = model.MemberRecord.CustID;
                model.Status = model.MemberRecord.Status;
                model.OriginaStatus = model.MemberRecord.Status;
                model.AdminID = 6;

                model.AutoshipBasicRecord = db.Database.SqlQuery<AutoshipBasic>("exec dbo.[LB_GetAutoshipHeaderByCustID] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).FirstOrDefault();


                model.CommissionHeaderList = db.Database.SqlQuery<CommissionHeader>("exec dbo.[LB_GetCommissionHeaderByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
                 new SqlParameter("@CustID", CustID),
                 new SqlParameter("@PeriodTypeID", periodTypeID),
                 new SqlParameter("@StartPeriodID", startPeriodID),
                 new SqlParameter("@EndPeriodID", endPeriodID),
                 new SqlParameter("@CommissionID", commissionID),
                 new SqlParameter("@Status", thisStatus)
                 ).ToList();


                DateTime orderStartDate = DateTime.Now.AddDays(-180);
                DateTime orderEndDate = DateTime.Now.AddDays(30);
                int thisItemID = 0;
                thisStatus = "";
                model.OrderBasicList = db.Database.SqlQuery<OrderBasic>("exec dbo.[LB_GetOrderHeaderList] @StartDate, @EndDate, @StartBalance, @EndBalance, @CustID, @Status, @ItemID",
                new SqlParameter("@StartDate", orderStartDate),
                new SqlParameter("@EndDate", orderEndDate),
                new SqlParameter("@StartBalance", -999999.00),
                new SqlParameter("@EndBalance", 9999999),
                new SqlParameter("@CustID", CustID),
                new SqlParameter("@Status", thisStatus),
                new SqlParameter("@ItemID", thisItemID)
                 ).ToList();

                IEnumerable<System.Web.Mvc.SelectListItem> cancelReasonCodes = DataHelper.GetCancelReasonCodesSelectList();
                model.CancelReasonCodesList = cancelReasonCodes;
                IEnumerable<System.Web.Mvc.SelectListItem> statusCodes = DataHelper.GetMemberStatusList();
                model.statusList = statusCodes;

                ViewBag.CustID = CustID;
                return View(model);
            }
        }



        // ProfileUpdate : Get
        [HttpGet]
        public ActionResult ProfileUpdate()
        {

            // This procedure always had an id being passed in an called LB_GetAuthShipHeaderByID but I dont see how
            // this would have worked because we don't know the AutoShipID to pass to the left nav menu link.
            // this probably needs to get AutoShipHeader by CustID

            var autoShipHeaderUpdateViewModel = new AutoShipHeaderUpdateViewModel();

            autoShipHeaderUpdateViewModel.AutoShipHeader = db.Database.SqlQuery<AutoShipHeader>("exec dbo.[LB_GetAutoShipHeaderByCustID] @CustID",
             new SqlParameter("@CustID", CustID)
             ).First();

            int AutosShipID = autoShipHeaderUpdateViewModel.AutoShipHeader.AutoShipID;
            autoShipHeaderUpdateViewModel.AutoShipDetailList = db.Database.SqlQuery<AutoShipDetail>("exec dbo.[LB_GetAutoShipDetailByAutoShipID] @AutoShipID",
             new SqlParameter("@AutoShipID", AutosShipID)
             ).ToList();

            IEnumerable<System.Web.Mvc.SelectListItem> editStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, true, false);

            autoShipHeaderUpdateViewModel.EditStatusList = editStatusList;
            autoShipHeaderUpdateViewModel.EditNextDate = autoShipHeaderUpdateViewModel.AutoShipHeader.NextDate;
            autoShipHeaderUpdateViewModel.EditStatus = autoShipHeaderUpdateViewModel.AutoShipHeader.Status;

            return View("ProfileUpdate", autoShipHeaderUpdateViewModel);
        }


        [HttpGet]
        public ActionResult MemberPaymentMethod()
        {

            PaymentMethod emptyPaymentMethod = new PaymentMethod();

            MemberPaymentMethodViewModel model = new MemberPaymentMethodViewModel();

            model.currentPaymentMethod = emptyPaymentMethod;

            var paymentMethodRow = db.Database.SqlQuery<PaymentMethod>("exec dbo.[LB_GetPaymentMethodByCustID] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).FirstOrDefault();

            if (paymentMethodRow != null)
            {
                model.currentPaymentMethod.CustID = paymentMethodRow.CustID;
                model.currentPaymentMethod.PName = paymentMethodRow.PName;
                model.currentPaymentMethod.PCardNumber = paymentMethodRow.PCardNumber;
                model.currentPaymentMethod.PCardType = paymentMethodRow.PCardType;
                model.currentPaymentMethod.PExpirationDate = paymentMethodRow.PExpirationDate;
                model.currentPaymentMethod.PExpirationMonth = paymentMethodRow.PExpirationMonth;
                model.currentPaymentMethod.PExpirationYear = paymentMethodRow.PExpirationYear;
            }
            else
            {
                model.currentPaymentMethod.CustID = CustID;
                model.currentPaymentMethod.PName = "";
                model.currentPaymentMethod.PCardNumber = "";
                model.currentPaymentMethod.PCardType = "";
                model.currentPaymentMethod.PExpirationMonth = "";
                model.currentPaymentMethod.PExpirationYear = "";
            }
            model.CustID = CustID;
            model.PName = "";
            model.PCardNumber = "";
            model.PCardType = "";
            model.PExpirationMonth = "";
            model.PExpirationYear = "";

            IEnumerable<System.Web.Mvc.SelectListItem> cardTypeList = DataHelper.GetCardTypeSelectList();
            model.CardTypeList = cardTypeList;
            IEnumerable<System.Web.Mvc.SelectListItem> cardExpMonthList = DataHelper.GetCardExpMonthsSelectList();
            model.CardExpMonthList = cardExpMonthList;
            IEnumerable<System.Web.Mvc.SelectListItem> cardExpYearList = DataHelper.GetCardExpYearsSelectList();
            model.CardExpYearList = cardExpYearList;

            ViewBag.CustID = CustID;
            return View("MemberPaymentMethod", model);

        }

        // EDIT : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberPaymentMethod([Bind(Include = "CustID, PName, PCardNumber, PCardType, PExpirationMonth, PExpirationYear")] MemberPaymentMethodViewModel model)
        {
            ViewBag.ErrorMessage = "";
            string CardFirstSix = "";
            string CardLastFour = "";
            string CardType = "";
            if (ModelState.IsValid)
            {
                CardFirstSix = model.PCardNumber.Substring(0, 6);
                CardType = model.PCardType;
                int CardNumberLength = model.PCardNumber.Length;
                CardLastFour = model.PCardNumber.Substring(CardNumberLength - 4, 4);
                string expMonth = model.PExpirationMonth.ToString();
                string expYear = model.PExpirationYear.ToString();

                var C_InfoRow = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).FirstOrDefault();

                string customerEmail = C_InfoRow.Email;
                string customerFirstName = C_InfoRow.FirstName;
                string customerLastName = C_InfoRow.LastName;
                string customerAddress1 = C_InfoRow.Address1;
                string customerCity = C_InfoRow.City;
                string customerState = C_InfoRow.State;
                string customerPostalCode = C_InfoRow.PostalCode;
                string customerPayPhone = C_InfoRow.DayPhone;
                string customerDescription = model.PName;
                string CustomerProfile = C_InfoRow.CustomerProfileID.ToString();
                string PaymentProfile = "2";
                string paymentTransactionID = "Unsettled";
                string newCustID = model.CustID.ToString();

                ServiceMode serviceMode = ServiceMode.Test;
                string apiLogin = Constants.apiLogin;
                string transactionKey = Constants.transactionKey;
                if (Constants.AuthorizeNetStatus == "Live")
                {
                    serviceMode = ServiceMode.Live;
                    apiLogin = Constants.apiLoginLive;
                    transactionKey = Constants.transactionKeyLive;
                }


                CustomerGateway customerGateway = new CustomerGateway(apiLogin, transactionKey, serviceMode);
                try
                {
                    if (CustomerProfile.Length <= 6)
                    {
                        Customer newcustomer = customerGateway.CreateCustomer(customerEmail, customerDescription, newCustID);
                        CustomerProfile = newcustomer.ProfileID;
                        newcustomer.Email = customerEmail;
                        newcustomer.Description = "This is the record for " + customerDescription;
                        int intCustomerProfile = Int32.Parse(CustomerProfile);
                        db.LB_UpdateC_InfoCustomerProfileID(CustID, intCustomerProfile);
                        //bool updateCustomer = customerGateway.UpdateCustomer(newcustomer);
                    }
                    string thisPCardNumber = model.PCardNumber;
                    int thisPExpMonth = Int32.Parse(model.PExpirationMonth);
                    int thisPExpYear = Int32.Parse(model.PExpirationYear);
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
                        string CardCode = "000";
                        try
                        {

                            if (CardType == "Visa")
                            {
                                string addCreditCard = customerGateway.AddCreditCard(CustomerProfile, thisPCardNumber, thisPExpMonth, thisPExpYear, CardCode, address);
                                PaymentProfile = addCreditCard;
                            }
                            else
                            {
                                string addCreditCard = customerGateway.AddCreditCard(CustomerProfile, thisPCardNumber, thisPExpMonth, thisPExpYear);
                                PaymentProfile = addCreditCard;
                            }
                                                        
                            if (Constants.isAuthorizeOn == "Yes")
                            {
                                IGatewayResponse authorize = customerGateway.Authorize(CustomerProfile, PaymentProfile, 1.57m);
                                paymentTransactionID = authorize.TransactionID;
                            }
                        }
                        catch (Exception e)
                        {
                            string s = e.Message;
                            ModelState.AddModelError("", e.Message);
                            ViewBag.ErrorMessage = s;
                            paymentTransactionID = "No so Good";
                        }
                    }
                    catch (Exception e)
                    {
                        string s = e.Message;
                        ModelState.AddModelError("", e.Message);
                        ViewBag.ErrorMessage = s;
                        PaymentProfile = "1";
                    }
                }
                 catch (Exception e)
                {
                        string s = e.Message;
                        ModelState.AddModelError("", e.Message);
                        ViewBag.ErrorMessage = s;
                        CustomerProfile = "1";
                }
                ViewBag.CustomerProfile = CustomerProfile;
                ViewBag.PaymentProfile = PaymentProfile;

                string shippingProfileID = "";
                if (Constants.isShippingProfileRequired == "Yes")
                {
                    shippingProfileID = customerGateway.AddShippingAddress(CustomerProfile, customerFirstName, customerLastName, customerAddress1, customerCity, customerState, customerPostalCode, "US", customerPayPhone);
                }

                if (ViewBag.ErrorMessage == "")
                {
                    db.LB_InsertPaymentMethodCIMShippingProfile(CustID, model.PName, model.PCardType,
                                        CardFirstSix, CardLastFour, expMonth, expYear, PaymentProfile, CustomerProfile, shippingProfileID
                                         );
                    ViewBag.ErrorMessage = "Thank You, Your new payment method has been saved.";
                }

            }

            var paymentMethodRow = db.Database.SqlQuery<PaymentMethod>("exec dbo.[LB_GetPaymentMethodByCustID] @CustID",
            new SqlParameter("@CustID", CustID)
            ).FirstOrDefault();

            model.currentPaymentMethod = new PaymentMethod();

            if (paymentMethodRow != null)
            {
                model.currentPaymentMethod.CustID = paymentMethodRow.CustID;
                model.currentPaymentMethod.PName = paymentMethodRow.PName;
                model.currentPaymentMethod.PCardNumber = CardLastFour;
                model.currentPaymentMethod.PCardType = paymentMethodRow.PCardType;
                model.currentPaymentMethod.PExpirationMonth = paymentMethodRow.PExpirationMonth;
                model.currentPaymentMethod.PExpirationYear = paymentMethodRow.PExpirationYear;
            }
            else
            {
                model.currentPaymentMethod.CustID = CustID;
                model.currentPaymentMethod.PName = "";
                model.currentPaymentMethod.PCardNumber = "";
                model.currentPaymentMethod.PCardType = "";
                model.currentPaymentMethod.PExpirationMonth = "";
                model.currentPaymentMethod.PExpirationYear = "";
            }
            ModelState.Clear();
            model.PCardNumber = CardLastFour;

            IEnumerable<System.Web.Mvc.SelectListItem> cardTypeList = DataHelper.GetCardTypeSelectList();
            model.CardTypeList = cardTypeList;
            IEnumerable<System.Web.Mvc.SelectListItem> cardExpMonthList = DataHelper.GetCardExpMonthsSelectList();
            model.CardExpMonthList = cardExpMonthList;
            IEnumerable<System.Web.Mvc.SelectListItem> cardExpYearList = DataHelper.GetCardExpYearsSelectList();
            model.CardExpYearList = cardExpYearList;

            ViewBag.CustID = CustID;
            return View("MemberPaymentMethod", model);
        }


        // EDITACCOUNT : GET
        public ActionResult EditAccount()
        {
            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.memberCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Member ID supplied to Edit page";
                ViewBag.CustID = CustID;
                return View("EditAccount");
            }
            else
            {
                ViewBag.errorCode = 0;

                var model = new SiteMemberEditViewModel();

                model = db.Database.SqlQuery<SiteMemberEditViewModel>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).FirstOrDefault();

                var memberInfoDetail = db.Database.SqlQuery<MemberInfoDetail>("exec dbo.[LBGetC_InfoDetailMemberData] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).FirstOrDefault();

                IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                model.StateList = stateList;
                IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();

                model.NameTitleList = nameTitleList;
                IEnumerable<System.Web.Mvc.SelectListItem> boardingStatusList = DataHelper.GetMemberBoardingStatusList();
                
                ViewBag.CustID = CustID;
                return View("EditAccount", model);
            }
        }

        // EDIT : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAccount([Bind(Include = "CustID,NameTitle,FirstName,LastName,DayPhone,Email")] SiteMemberEditViewModel model)
        {
            ViewBag.ErrorCode = 0;

            if (ModelState.IsValid)
            {
                ViewBag.CustID = CustID;

                ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_UpdateC_infoPrimaryContact(
                        model.CustID,
                        model.NameTitle,
                        model.FirstName,
                        model.LastName,
                        model.DayPhone,
                        model.Email,
                        "",
                        "",
                        returnCustID, returnMessage);

                var scalarCustID = (int)returnCustID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    ModelState.AddModelError("", "Your account information was successfully updated.");
                }                
            }

            model = db.Database.SqlQuery<SiteMemberEditViewModel>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
             new SqlParameter("@CustID", CustID)
             ).FirstOrDefault();

            var memberInfoDetail = db.Database.SqlQuery<MemberInfoDetail>("exec dbo.[LBGetC_InfoDetailMemberData] @CustID",
             new SqlParameter("@CustID", CustID)
             ).FirstOrDefault();


            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;

            return View("EditAccount", model);
        }



        //ACOUNTPROFILE : GET 
        public ActionResult VendorProfile(int id = 0, string tab = "")
        {
            int VendorID = id;

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(id, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Member  ID supplied to Account Profile page";
                    return RedirectToAction("Error", "Admin");
            }
            else
            {
                ViewBag.errorCode = 0;

                int zero = 0;
                int commissionID = Constants.rebateCommissionID;
                int periodTypeID = Constants.rebateCommissionPeriodType;
                int startPeriodID = 10;
                int endPeriodID = 999999;
                string thisStatus = "";

                var model = new MemberProfileViewModel();

                model.MemberRecord = db.Database.SqlQuery<AppTrack.SharedModels.Member>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", VendorID)
                 ).First();

                model.CustID = model.MemberRecord.CustID;
                model.Status = model.MemberRecord.Status;
                model.OriginaStatus = model.MemberRecord.Status;
                model.AdminID = 6;

                model.AutoshipBasicList = db.Database.SqlQuery<AutoshipBasic>("exec dbo.[LB_GetAutoshipHeaderByCustIDAll] @CustID",
                 new SqlParameter("@CustID", VendorID)
                 ).ToList();

                /*              
                                 model.CommissionHeaderList = db.Database.SqlQuery<CommissionHeader>("exec dbo.[LB_GetCommissionDetailByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
                                 new SqlParameter("@CustID", CustID),
                                 new SqlParameter("@PeriodTypeID", periodTypeID),
                                 new SqlParameter("@StartPeriodID", startPeriodID),
                                 new SqlParameter("@EndPeriodID", endPeriodID),
                                 new SqlParameter("@CommissionID", commissionID),
                                 new SqlParameter("@Status", thisStatus)
                                 ).ToList();
                */

                model.CommissionHeaderList = db.Database.SqlQuery<CommissionHeader>("exec dbo.[LB_GetCommissionHeaderByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
                new SqlParameter("@CustID", VendorID),
                new SqlParameter("@PeriodTypeID", periodTypeID),
                new SqlParameter("@StartPeriodID", startPeriodID),
                new SqlParameter("@EndPeriodID", endPeriodID),
                new SqlParameter("@CommissionID", commissionID),
                new SqlParameter("@Status", thisStatus)
                ).ToList();

                DateTime orderStartDate = DateTime.Now.AddDays(-180);
                DateTime orderEndDate = DateTime.Now.AddDays(30);
                int thisItemID = 0;
                thisStatus = "";
                model.OrderBasicList = db.Database.SqlQuery<OrderBasic>("exec dbo.[LB_GetOrderHeaderList] @StartDate, @EndDate, @StartBalance, @EndBalance, @CustID, @Status, @ItemID",
                new SqlParameter("@StartDate", orderStartDate),
                new SqlParameter("@EndDate", orderEndDate),
                new SqlParameter("@StartBalance", -999999.00),
                new SqlParameter("@EndBalance", 9999999),
                new SqlParameter("@CustID", VendorID),
                new SqlParameter("@Status", thisStatus),
                new SqlParameter("@ItemID", thisItemID)
                 ).ToList();

                IEnumerable<System.Web.Mvc.SelectListItem> cancelReasonCodes = DataHelper.GetCancelReasonCodesSelectList();
                model.CancelReasonCodesList = cancelReasonCodes;
                IEnumerable<System.Web.Mvc.SelectListItem> statusCodes = DataHelper.GetMemberStatusList();
                model.statusList = statusCodes;

                ViewBag.Tab = tab;

                return View(model);
            }
        }

        
        
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
    }
}




