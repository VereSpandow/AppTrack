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


        //ACOUNTPROFILE : GET 
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

        // LOCATION: GET
        [HttpGet]
        public ActionResult LocationList()
        {

            ViewBag.ErrorCode = 0;
            string searchLastName = "";
            string selectedStatus = "Active";

            var locationlistmodel = new LocationListViewModel { };

            var locationRows = db.Database.SqlQuery<Location>("exec dbo.[LB_GetCustByParentID]  @ParentID, @CustomerType",
             new SqlParameter("@ParentID", CustID),
             new SqlParameter("@CustomerType", 66)
             ).ToList();

            locationlistmodel.LocationList = locationRows;
            locationlistmodel.SearchLastName = searchLastName;
            locationlistmodel.SelectedStatus = selectedStatus;
            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, true);
            locationlistmodel.StatusList = thisList;

            ViewBag.CustID = CustID;
            return View("LocationList", locationlistmodel);            
        }

        //ACOUNTPROFILE : GET 
        public ActionResult LocationProfile(int id = 0)
        {
            if (id == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Member ID supplied to Location Profile page";
                return RedirectToAction("Error", "Site");
            }

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(id, Constants.locationCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Location ID supplied to Profile page";
                return RedirectToAction("Error", "Site");
            }
            else
            {
                if(checkCustomerResult.C_Info.ParentID != CustID)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Location ID supplied to Profile page";
                    return RedirectToAction("Error", "Site");
                }
                ViewBag.errorCode = 0;

                int zero = 0;
                int commissionID = Constants.rebateCommissionID;
                int periodTypeID = Constants.rebateCommissionPeriodType;
                int startPeriodID = 10;
                int endPeriodID = 999999;
                string thisStatus = "";

                var model = new LocationProfileViewModel();

                model.LocationRecord = db.Database.SqlQuery<Location>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", id)
                 ).First();

                model.CommissionHeaderList = db.Database.SqlQuery<CommissionHeader>("exec dbo.[LB_GetCommissionDetailByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
                 new SqlParameter("@CustID", id),
                 new SqlParameter("@PeriodTypeID", periodTypeID),
                 new SqlParameter("@StartPeriodID", startPeriodID),
                 new SqlParameter("@EndPeriodID", endPeriodID),
                 new SqlParameter("@CommissionID", commissionID),
                 new SqlParameter("@Status", thisStatus)
                 ).ToList();

                IEnumerable<System.Web.Mvc.SelectListItem> statusCodes = DataHelper.GetMemberStatusList();
                model.statusList = statusCodes;

                ViewBag.CustID = CustID;
                return View("LocationProfile", model);
            }
        }

        // CONTACT LIST: GET
        [HttpGet]
        public ActionResult ContactList()
        {

            var contactRows = db.Database.SqlQuery<Contact>("exec dbo.[LB_GetContacts]  @SponsorID,@CustomerType",
             new SqlParameter("@SponsorID", CustID),
             new SqlParameter("@CustomerType", 61)
             ).ToList();

            var model = new ContactListViewModel
            {
                ContactList = contactRows,
            };

            ViewBag.CustID = CustID;
            return View("ContactList", model);
        }

        // CREATE: GET
        public ActionResult ContactCreate()
        {
            var model = new MemberContactViewModel();
            var contactRecordRow = new Contact();
            model.contactRecord = contactRecordRow;
            model.contactRecord.SponsorID = CustID;

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;
            IEnumerable<System.Web.Mvc.SelectListItem> contactTypeList = DataHelper.GetContactRolesSelectList();
            model.ContactTypeList = contactTypeList;

            ViewBag.CustID = CustID;
            return View("ContactCreate", model);

        }

        // CREATE: POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactCreate([Bind(Include = "contactRecord")] MemberContactViewModel model)
        {
            ViewBag.ErrorCode = 0;
            if (ModelState.IsValid)
            {
                ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_InsertMemberContact(Constants.memberContactCustomerType,
                    model.contactRecord.NameTitle,
                    model.contactRecord.FirstName,
                    model.contactRecord.LastName,
                    model.contactRecord.Email,
                    model.contactRecord.Address1,
                    model.contactRecord.Address2,
                    model.contactRecord.City,
                    model.contactRecord.State,
                    model.contactRecord.PostalCode,
                    model.contactRecord.DayPhone,
                    model.contactRecord.Mobile,
                    model.contactRecord.Fax,
                    model.contactRecord.SponsorID,
                    model.contactRecord.VariantData1,
                    model.contactRecord.VariantData2,
                    model.contactRecord.VariantData3,
                    returnCustID, returnMessage);

                var scalarCustID = (int)returnCustID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    return RedirectToAction("ContactList");
                }
            }
            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;
            IEnumerable<System.Web.Mvc.SelectListItem> contactTypeList = DataHelper.GetContactRolesSelectList();
            model.ContactTypeList = contactTypeList;

            return View("ContactCreate", model);
        }

        // EDIT: GET
        public ActionResult ContactEdit(int ContactID = 0)
        {
            if (ContactID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Contact ID supplied to Edit Contact page";
                return RedirectToAction("Error", "Site");
            }
            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(ContactID, Constants.memberContactCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Contact ID supplied to Edit Contact page";
                return RedirectToAction("Error", "Site");
            }
            else
            {
                if (checkCustomerResult.C_Info.SponsorID != CustID)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Contact ID supplied to Edit Contact page";
                    return RedirectToAction("Error", "Site");
                }
            }
            var model = new MemberContactViewModel();

            var contactRecordRow = db.Database.SqlQuery<Contact>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
             new SqlParameter("@CustID", ContactID)
             ).FirstOrDefault();


            model.contactRecord = contactRecordRow;

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;
            IEnumerable<System.Web.Mvc.SelectListItem> contactTypeList = DataHelper.GetContactRolesSelectList();
            model.ContactTypeList = contactTypeList;

            ViewBag.CustID = CustID;
            return View(model);
        }

        // EDIT : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactEdit([Bind(Include = "contactRecord")] MemberContactViewModel model)
        {
            ViewBag.ErrorCode = 0;

            if (ModelState.IsValid)
            {
                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(model.contactRecord.CustID, Constants.memberContactCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Contact ID supplied to Edit Contact page";
                    return RedirectToAction("Error", "Site");
                }
                else
                {
                    if (checkCustomerResult.C_Info.SponsorID != CustID)
                    {
                        TempData["ErrorCode"] = Constants.fatalErrorCode;
                        TempData["ErrorMessage"] = "Invalid Contact ID supplied to Edit Contact page";
                        return RedirectToAction("Error", "Site");
                    }
                }
                ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));
                model.contactRecord.Company = "";
                var emptyTaxID = "";

                db.LB_UpdateMemberContact(Constants.memberContactCustomerType,
                    model.contactRecord.CustID,
                    model.contactRecord.Company,
                    emptyTaxID,
                    model.contactRecord.Company,
                    model.contactRecord.NameTitle,
                    model.contactRecord.FirstName,
                    model.contactRecord.LastName,
                    model.contactRecord.Address1,
                    model.contactRecord.Address2,
                    model.contactRecord.City,
                    model.contactRecord.State,
                    model.contactRecord.PostalCode,
                    model.contactRecord.DayPhone,
                    model.contactRecord.Mobile,
                    model.contactRecord.Fax,
                    model.contactRecord.Email,
                    model.contactRecord.VariantData1,
                    model.contactRecord.VariantData2,
                    model.contactRecord.VariantData3,
                    returnCustID, returnMessage);

                var scalarCustID = (int)returnCustID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    return RedirectToAction("ContactList");
                }
            }

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;
            IEnumerable<System.Web.Mvc.SelectListItem> contactTypeList = DataHelper.GetContactRolesSelectList();
            model.ContactTypeList = contactTypeList;

            return View(model);
        }
        // EDIT : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactDelete(int ContactID = 0)
        {

            if (ContactID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Contact ID supplied to Delete Contact page";
                return RedirectToAction("Error", "Site");
            }
            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(ContactID, Constants.memberContactCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Contact ID supplied to Delete Contact page";
                return RedirectToAction("Error", "Site");
            }
            else
            {
                if (checkCustomerResult.C_Info.SponsorID != CustID)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Contact ID supplied to Delete Contact page";
                    return RedirectToAction("Error", "Site");
                }
            }

            ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
            ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

            string CustomerTypeLabel = "Contact";

            db.LB_DeleteC_Info(
                ContactID,
                CustomerTypeLabel,
                CustID,
                returnCustID, returnMessage);

            var scalarCustID = (int)returnCustID.Value;
            var errorMessage = (string)returnMessage.Value ?? "";

            if (scalarCustID == -1)
            {
                ViewBag.ErrorMessage = errorMessage;
            }
            
            var contactRows = db.Database.SqlQuery<Contact>("exec dbo.[LB_GetContacts]  @SponsorID,@CustomerType",
            new SqlParameter("@SponsorID", CustID),
            new SqlParameter("@CustomerType", 61)
            ).ToList();

            var model = new ContactListViewModel
            {
                ContactList = contactRows,
            };

            return View("ContactList", model);
        }


        // GET: OrderList
        public ActionResult OrderList()
        {
            DateTime now = DateTime.Now;
            DateTime searchStartDate = DateTime.Now.AddYears(-10); ;
            DateTime searchEndDate = DateTime.Now.AddDays(60);
            decimal searchStartBalance = -99999;
            decimal searchEndBalance = 99999;
            string searchStatus = " ";
            int searchItemID = 0;

            var orderListViewModel = new OrderListViewModel();

            orderListViewModel.OrderHeaderList = db.Database.SqlQuery<C_OrderHeader>("exec dbo.[LB_GetOrderHeaderList] @StartDate, @EndDate, @StartBalance, @EndBalance, @CustID, @Status, @ItemID",
                new SqlParameter("@StartDate", searchStartDate),
                new SqlParameter("@EndDate", searchEndDate),
                new SqlParameter("@StartBalance", searchStartBalance),
                new SqlParameter("@EndBalance", searchEndBalance),
                new SqlParameter("@CustID", CustID),
                new SqlParameter("@Status", searchStatus),
                new SqlParameter("@ItemID", searchItemID)
                ).ToList();


            orderListViewModel.SearchStartDate = DateTime.Now;
            orderListViewModel.SearchEndDate = DateTime.Now;
            orderListViewModel.SearchStartBalance = -9999;
            orderListViewModel.SearchEndBalance = 9999;
            orderListViewModel.SearchCustID = CustID;
            orderListViewModel.SearchStatus = " ";
            orderListViewModel.SearchItemID = 0;

            IEnumerable<System.Web.Mvc.SelectListItem> searchItemList = DataHelper.GetItemSelectList(false, true);

            orderListViewModel.SearchItemList = searchItemList;

            IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, false, true);

            orderListViewModel.SearchStatusList = searchStatusList;

            ViewBag.CustID = CustID;
            return View("OrderList", orderListViewModel);

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
        public ActionResult MemberVendorSelect()
        {

            // Debug.WriteLine("Get-" + searchLastName + "|" + searchStatus + "|" + startDateTime + "|" + endDateTime);
            var vendorRows = db.Database.SqlQuery<MemberVendor>("exec dbo.[LB_GetMemberVendorListByCategory]  @CustID, @CustomerType",
             new SqlParameter("@CustID", CustID),
             new SqlParameter("@CustomerType", Constants.vendorCustomerType)
             ).ToList();

            var model = new MemberVendorViewModel
            {
                MemberVendorList = vendorRows,
            };

            return View("MemberVendors", model);
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

                model.EnrollmentReason = memberInfoDetail.EnrollmentReason;
                model.PracticeSoftware = memberInfoDetail.PracticeSoftware;
                model.PracticeSize = memberInfoDetail.PracticeSize;

                IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                model.StateList = stateList;
                IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();

                model.NameTitleList = nameTitleList;
                IEnumerable<System.Web.Mvc.SelectListItem> boardingStatusList = DataHelper.GetMemberBoardingStatusList();
                
                model.BoardingStatusList = boardingStatusList;
                IEnumerable<System.Web.Mvc.SelectListItem> statusList = DataHelper.GetMemberStatusList();
                model.StatusList = statusList;
                IEnumerable<System.Web.Mvc.SelectListItem> practiceSizeList = DataHelper.GetPracticeSizeList();
                model.PracticeSizeList = practiceSizeList;
                IEnumerable<System.Web.Mvc.SelectListItem> practiceManagementSoftwareList = DataHelper.GetPracticeManagementSoftwareSelectList();
                model.PracticeManagementSoftwareList = practiceManagementSoftwareList;
                IEnumerable<System.Web.Mvc.SelectListItem> cancelReasonCodesList = DataHelper.GetCancelReasonCodesSelectList();
                model.CancelReasonCodesList = cancelReasonCodesList;

                ViewBag.CustID = CustID;
                return View("EditAccount", model);
            }
        }

        // EDIT : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAccount([Bind(Include = "CustID,NameTitle,FirstName,LastName,ShipAddress1, ShipAddress2, ShipCity, ShipState, ShipPostalCode, ShipEmail, ShipPhone,CompanyPhone, DayPhone, Mobile, Fax, Email,StartDate, Status, ActivationStatus, ActivationStatusDate, VariantData2, VariantData3, EnrollmentReason, PracticeSoftware, PracticeSize, Flag1, SiteName")] SiteMemberEditViewModel model)
        {
            ViewBag.ErrorCode = 0;
            ModelState.Remove("TaxID");
            ModelState.Remove("Company");
            ModelState.Remove("EventID");
            ModelState.Remove("StartDate");
            ModelState.Remove("Status");
            ModelState.Remove("SourceID");
            ModelState.Remove("SourceCode");
            ModelState.Remove("StatusDate");
            ModelState.Remove("SecID");
            ModelState.Remove("SecCode");
            ModelState.Remove("SponsorID");
            ModelState.Remove("SecSponsorID");
            ModelState.Remove("ParentID");
            ModelState.Remove("ShipName");
            ModelState.Remove("ActivationStatusDate");
            ModelState.Remove("DisplayName");
            ModelState.Remove("Company");
            ModelState.Remove("TaxID");
            ModelState.Remove("Address1");
            ModelState.Remove("Address2");
            ModelState.Remove("City");
            ModelState.Remove("State");
            ModelState.Remove("PostalCode");
            ModelState.Remove("EmailFlag");
            ModelState.Remove("TextFlag");


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
                        model.VariantData2,
                        model.VariantData3,
                        returnCustID, returnMessage);

                var scalarCustID = (int)returnCustID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    db.LB_UpdateC_InfoShipPayee(
                    model.CustID,
                    model.ShipName,
                    model.ShipFirstName,
                    model.ShipLastName,
                    model.ShipAddress1,
                    model.ShipAddress2,
                    model.ShipCity,
                    model.ShipState,
                    model.ShipPostalCode,
                    model.ShipPhone,
                    model.ShipEmail,
                    model.PayoutCustID);

                    db.LB_UpdateC_InfoAdditionalInfo(
                        model.CustID,
                        model.EnrollmentReason,
                        model.PracticeSize,
                        model.PracticeSoftware,
                        model.SiteName,
                        model.Flag1);

                    ModelState.AddModelError("", "Your account information was successfully updated.");
                }                
            }

            model = db.Database.SqlQuery<SiteMemberEditViewModel>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
             new SqlParameter("@CustID", CustID)
             ).FirstOrDefault();

            var memberInfoDetail = db.Database.SqlQuery<MemberInfoDetail>("exec dbo.[LBGetC_InfoDetailMemberData] @CustID",
             new SqlParameter("@CustID", CustID)
             ).FirstOrDefault();

            model.EnrollmentReason = memberInfoDetail.EnrollmentReason;
            model.PracticeSoftware = memberInfoDetail.PracticeSoftware;
            model.PracticeSize = memberInfoDetail.PracticeSize;

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;
            IEnumerable<System.Web.Mvc.SelectListItem> statusList = DataHelper.GetMemberStatusList();
            model.StatusList = statusList;
            IEnumerable<System.Web.Mvc.SelectListItem> boardingStatusList = DataHelper.GetMemberBoardingStatusList();
            model.BoardingStatusList = boardingStatusList;
            IEnumerable<System.Web.Mvc.SelectListItem> practiceSizeList = DataHelper.GetPracticeSizeList();
            model.PracticeSizeList = practiceSizeList;
            IEnumerable<System.Web.Mvc.SelectListItem> cancelReasonCodesList = DataHelper.GetCancelReasonCodesSelectList();
            model.CancelReasonCodesList = cancelReasonCodesList;
            IEnumerable<System.Web.Mvc.SelectListItem> practiceManagementSoftwareList = DataHelper.GetPracticeManagementSoftwareSelectList();
            model.PracticeManagementSoftwareList = practiceManagementSoftwareList;

            return View("EditAccount", model);
        }

        // EDITACCOUNT : GET
        public ActionResult LocationEditAccount(int id = 0)
        {
            if (id == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Location ID supplied to Location Edit page";
                return RedirectToAction("Error", "Site");
            }

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(id, Constants.locationCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Location ID supplied to Edit page";
                return RedirectToAction("Error", "Site");
            }
            else
            {
                if (checkCustomerResult.C_Info.ParentID != CustID)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Location ID supplied to Profile page";
                    return RedirectToAction("Error", "Site");
                }
            }

            ViewBag.errorCode = 0;

            var model = new SiteMemberEditLocationViewModel();

            model = db.Database.SqlQuery<SiteMemberEditLocationViewModel>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                new SqlParameter("@CustID", id)
                ).FirstOrDefault();

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;

            ViewBag.CustID = CustID;
            return View("LocationEditAccount", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LocationEditAccount([Bind(Include = "CustID,NameTitle,FirstName,LastName,DisplayName,Address1,Address2,City,State,PostalCode,ShipAddress1, ShipAddress2, ShipCity, ShipState, ShipPostalCode, ShipEmail, ShipPhone,CompanyPhone, DayPhone, Mobile, Fax, Email,StartDate, Status, ActivationStatus, ActivationStatusDate, LicenseNumber, OETracker, VariantData2, VariantData3,VariantData4,Flag1,SiteName, FormAction")] SiteMemberEditLocationViewModel model)
        {
            ViewBag.ErrorCode = 0;
            ModelState.Remove("TaxID");
            ModelState.Remove("Company");
            ModelState.Remove("EventID");
            ModelState.Remove("StartDate");
            ModelState.Remove("Status");
            ModelState.Remove("SourceID");
            ModelState.Remove("SourceCode");
            ModelState.Remove("StatusDate");
            ModelState.Remove("SecID");
            ModelState.Remove("SecCode");
            ModelState.Remove("SponsorID");
            ModelState.Remove("SecSponsorID");
            ModelState.Remove("ParentID");
            ModelState.Remove("ShipName");
            ModelState.Remove("ActivationStatusDate");
            ModelState.Remove("EmailFlag");
            ModelState.Remove("TextFlag");
            if (ModelState.IsValid)
            {
//                ViewBag.CustID = model.CustID;

                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(model.CustID, Constants.locationCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ViewBag.ErrorCode = Constants.fatalErrorCode;
                    ViewBag.ErrorMessage = "Invalid Member Location ID supplied to Edit page";
                    return View("LocationEditAccount", model);
                }
                else
                {
                ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));
                string action = model.FormAction.ToLower();
                if (action =="primarycontact")
                {
                    db.LB_UpdateLocationEditAccountBasic(
                        model.CustID,
                        model.DisplayName,
                        model.NameTitle,
                        model.FirstName,
                        model.LastName,
                        model.Address1,
                        model.Address2,
                        model.City,
                        model.State,
                        model.PostalCode,
                        model.CompanyPhone,
                        model.DayPhone,
                        model.Mobile,
                        model.Fax,
                        model.Email,
                        returnCustID, returnMessage);
                }

                if (action =="payee")
                {
                    db.LB_UpdateC_InfoShipPayee(
                        model.CustID,
                        model.ShipName,
                        model.ShipFirstName,
                        model.ShipLastName,
                        model.ShipAddress1,
                        model.ShipAddress2,
                        model.ShipCity,
                        model.ShipState,
                        model.ShipPostalCode,
                        model.ShipPhone,
                        model.ShipEmail,
                        model.PayoutCustID);
                }

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);

                        IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                        model.StateList = stateList;
                        IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
                        model.NameTitleList = nameTitleList;

                        return View("LocationEditAccount", model);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Location information was updated successfully";
                        return RedirectToAction("LocationList", new { id = model.ParentID });
                    }
                }
            }
            return RedirectToAction("LocationList", new { id = model.ParentID });
        }

        [OverrideAuthorization]
        [Authorize(Roles = "Member,MemberServices,Marketing,SalesRep")]
        [HttpGet]
        public ActionResult VendorDetail(int id = 0, int programID = 0 )
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

        [HttpGet]
        public ActionResult RebateCommissionList()
        {
            // Initalize the View Model
            var model = new CommissionHeaderListViewModel();
            int periodTypeID = Constants.rebateCommissionPeriodType;

            int startPeriodID = 0;
            int endPeriodID = 0;

            int searchCommissionID = Constants.memberRebateCommissionID;
            string searchStatus = " ";

            model.CommissionHeaderList = db.Database.SqlQuery<CommissionHeader>("exec dbo.[LB_GetCommissionHeaderByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
             new SqlParameter("@CustID", CustID),
             new SqlParameter("@PeriodTypeID", periodTypeID),
             new SqlParameter("@StartPeriodID", startPeriodID),
             new SqlParameter("@EndPeriodID", endPeriodID),
             new SqlParameter("@CommissionID", searchCommissionID),
             new SqlParameter("@Status", searchStatus)
            ).ToList();

            model.StartPeriodID = startPeriodID;
            model.EndPeriodID = endPeriodID;
            model.SearchCustID = CustID;
            model.SearchCommissionID = searchCommissionID;
            model.SearchStatus = searchStatus;

            model.SearchCustIDList = DataHelper.GetCustIDList(Constants.memberDirectorCustomerType, false, true);

            model.SearchPeriodIDList = DataHelper.GetPeriodIDList(periodTypeID, 2, false, true);

            model.SearchCommissionIDList = DataHelper.GetCommissionIDList(0, false, true);

            model.SearchStatusList = DataHelper.GetStatusSelectList(Constants.commissionStatusLookupGroupID, false, true);

            ViewBag.CustID = CustID;
            return View("CommissionList", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RebateCommissionList([Bind(Include = "StartPeriodID, EndPeriodID, SearchCustID, SearchCommissionID, SearchStatus")] CommissionHeaderListViewModel model)
        {
            int periodTypeID = Constants.rebateCommissionPeriodType;

            string searchStatus = " ";

            model.CommissionHeaderList = db.Database.SqlQuery<CommissionHeader>("exec dbo.[LB_GetCommissionHeaderByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
            new SqlParameter("@CustID", CustID),
            new SqlParameter("@PeriodTypeID", periodTypeID),
            new SqlParameter("@StartPeriodID", model.StartPeriodID),
            new SqlParameter("@EndPeriodID", model.EndPeriodID),
            new SqlParameter("@CommissionID", model.SearchCommissionID),
            new SqlParameter("@Status", searchStatus)
            ).ToList();
                
            model.SearchCustIDList = DataHelper.GetCustIDList(Constants.memberDirectorCustomerType, false, true);
            model.SearchPeriodIDList = DataHelper.GetPeriodIDList(periodTypeID, 2, false, true);
            model.SearchCommissionIDList = DataHelper.GetCommissionIDList(0, false, true);
            model.SearchStatusList = DataHelper.GetStatusSelectList(Constants.commissionStatusLookupGroupID, false, true);

            return View("CommissionList", model);
        }

        [HttpGet]
        public ActionResult RebateVolumeDetailList(int id = 0)
        {
            int CHID = id;

            if (CHID == 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Customer ID provided, unable to retrieve Rebate Detail List";
                return PartialView("_RebateVolumeDetailListWithHeader");
            }

            // Initalize the View Model

            VolumeDetailListViewModel volumeDetailListViewModel = new VolumeDetailListViewModel();

            volumeDetailListViewModel.VolumeDetailList = db.Database.SqlQuery<VolumeDetail>("exec dbo.[LB_GetVolumeDetailRebateByCHID] @CHID",
             new SqlParameter("@CHID", CHID)
            ).ToList();

            var locationRecord = db.Database.SqlQuery<Member>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
             new SqlParameter("@CustID", volumeDetailListViewModel.VolumeDetailList[0].CustID)
            ).First();

            if ((locationRecord.CustID != CustID) && (locationRecord.ParentID != CustID))
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Member/Location ID supplied to Rebates page";
                return RedirectToAction("Error", "Site");
            }

                return PartialView("_RebateVolumeDetailListWithHeader", volumeDetailListViewModel);
        }


        [HttpGet]
        public ActionResult Resources()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public String MemberReferral([Bind(Include = "contactDisplayName,contactEmail,contactPhone")] ContactMe contactMe)
        {
            string returnString = "An error was encountered.  Please try again.";
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
                    contactMe.contactLastName = " ";
                }
                contactMe.contactSource = "Member Referral";
                int customerType = 96;

                db.LB_InsertContactMemberReferral(CustID, contactMe.contactFirstName, contactMe.contactLastName, contactMe.contactPhone, contactMe.contactEmail, contactMe.contactSource, customerType);

                returnString = "<br><br><br><span class='text-success' style='font-size:1.2em'>Thank you for referring " + contactMe.contactDisplayName + "!</span><br><br><span class='text-success'>We will be sure to contact your colleague as soon as possible and let you know the outcome.</span><br><br><br>";

                // Need to send email to Member and to AppTrack admin
                int emailId = Constants.emailIDMemberReferralNotification;
                var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                new SqlParameter("@ID", emailId)
                ).FirstOrDefault();
                string subject = emailRow.Subject;
                
                string body = emailRow.Content;
                string sCustID = CustID.ToString();
                body = body.Replace("#CUSTID#", sCustID);
                body = body.Replace("#MEMBERNAME#", DisplayName);
                body = body.Replace("#COLLEAGUENAME#", contactMe.contactDisplayName);
                body = body.Replace("#COLLEAGUEEMAIL#", contactMe.contactEmail);
                body = body.Replace("#COLLEAGUEPHONE#", contactMe.contactPhone);
                body = Constants.emailTemplate.Replace("#BODYCOPY#", body);
                body = body.Replace("#SITEURL#", Constants.siteURL);
                SmtpClient client = new SmtpClient();
                MailMessage mailMessage = new MailMessage();
                MailAddress FromAddress = new MailAddress(Constants.adminEmailFrom, "AppTrack Member Services");
                mailMessage.From = FromAddress;
                mailMessage.To.Add(Constants.referralEmailTo);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;                
                try
                {
                    client.Send(mailMessage);
                }
                catch
                {
                    ViewBag.ErrorMessage = "An unexpected error was encountered. Please try again.";
                }

            }

            return returnString;
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




