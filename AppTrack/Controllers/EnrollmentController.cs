using System.Data;
using System.Data.Entity;
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
    public class EnrollmentController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
        DataHelpers DataHelper = new DataHelpers();

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public EnrollmentController()
        {
        }

        public EnrollmentController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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


        // GET: Enrollment
        public ActionResult Index()
        {
            ViewBag.Title = "AppTrack Membership Enrollment | Optometrist Alliance Group - AppTrack";
            ViewBag.MetaDescription = "Looking to become a member of AppTrack? Start the enrollment process here. Enroll in AppTrack membership in just a few easy steps via our website!";
    
            return View("Enrollment");
        }

        [HttpGet]
        public ActionResult EnrollmentRedirect(int ID = 2, int custID = 148778)
        {
            if (ID == 2)
            {
                var model = new EnrollmentLocations
                {
                };
                IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                model.StateList = stateList;
                return PartialView("_EnrollmentLocations", model);
            }
            if (ID == 3)
            {
                var paymentrecord = db.Database.SqlQuery<Member>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", custID)
                 ).FirstOrDefault();

                var model = new EnrollmentPaymentMethod
                {
                    CustID = custID,
                    Agreement = false,
                    PName = paymentrecord.FirstName + ' ' + paymentrecord.LastName
                };
                IEnumerable<System.Web.Mvc.SelectListItem> cardTypeList = DataHelper.GetCardTypeSelectList();
                model.CardTypeList = cardTypeList;
                IEnumerable<System.Web.Mvc.SelectListItem> cardExpMonthList = DataHelper.GetCardExpMonthsSelectList();
                model.CardExpMonthList = cardExpMonthList;
                IEnumerable<System.Web.Mvc.SelectListItem> cardExpYearList = DataHelper.GetCardExpYearsSelectList();
                model.CardExpYearList = cardExpYearList;
                return PartialView("_EnrollmentPaymentMethod", model);            
            }
            if (ID == 33)
            {
                ViewBag.CustID = custID;
                ViewBag.Action = "Payment";
                return View("Enrollment");

            }

            if (ID == 4)
            {
                var enrollmentVendorRows = db.Database.SqlQuery<EnrollmentVendor>("exec dbo.[LB_GetVendorsAllByCategory]").ToList();
                var model = new EnrollmentVendorSelect
                {
                    CustID = custID,
                    EnrollmentVendorList = enrollmentVendorRows
                };
                return PartialView("_EnrollmentVendorSelect", model);
            }
            if (ID == 5)
            {

                var finalModel = new EnrollmentFinal
                {
                };
                finalModel.CustID = custID;
                IEnumerable<System.Web.Mvc.SelectListItem> customerList = DataHelper.GetCustIDList(Constants.memberDirectorCustomerType);
                finalModel.IMDList = customerList;
                IEnumerable<System.Web.Mvc.SelectListItem> practiceSizeList = DataHelper.GetPracticeSizeList();
                finalModel.PracticeSizeList = practiceSizeList;

                return PartialView("_EnrollmentFinal", finalModel);

            }
            if (ID == 6)
            {
                var thankYouCustID = custID;

                var thankYouModel = db.Database.SqlQuery<MemberEnrollmentThankYou>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", thankYouCustID)
                 ).FirstOrDefault();

                thankYouModel.Locations = 5;

                return PartialView("_EnrollmentThankYou", thankYouModel);

            }
            return View("Enrollment");
        }

        [HttpGet]
        public ActionResult EnrollmentPrimary(int repID = 0)
        {
            var model = new EnrollmentPrimary {
                RepID = repID
            };

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;
            
            return PartialView("_EnrollmentPrimary", model);
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnrollmentPrimary([Bind(Include = "RepID,ParentID,EventID,DisplayName,NameTitle,FirstName,LastName,Email,Address1,Address2,City,State,PostalCode,Phone,Fax, Password, ConfirmPassword, Multilocation")] EnrollmentPrimary enrollmentPrimary)
        {
            DateTime StartDate = DateTime.Now;
            if (!ModelState.IsValid)
            {
                IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                enrollmentPrimary.StateList = stateList;
                IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
                enrollmentPrimary.NameTitleList = nameTitleList;
                return PartialView("_EnrollmentPrimary", enrollmentPrimary);
            }
            else
            {
                var company = "";
                var taxID = "";
                int sponsorID = enrollmentPrimary.RepID;
                int secSponsorID = 0;
                int mainMemberItemID = 1;

                enrollmentPrimary.Phone = DataHelper.FixPhone(enrollmentPrimary.Phone);
                enrollmentPrimary.Fax = DataHelper.FixPhone(enrollmentPrimary.Fax);

                ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_InsertMember(Constants.memberCustomerType, 
                    enrollmentPrimary.DisplayName, 
                    enrollmentPrimary.NameTitle, 
                    enrollmentPrimary.FirstName, 
                    enrollmentPrimary.LastName, 
                    enrollmentPrimary.Email,
                    enrollmentPrimary.Address1, 
                    enrollmentPrimary.Address2, 
                    enrollmentPrimary.City,  
                    enrollmentPrimary.State, 
                    enrollmentPrimary.PostalCode, 
                    enrollmentPrimary.Phone,
                    enrollmentPrimary.Phone,
                    "",
                    enrollmentPrimary.Fax, 
                    company, 
                    taxID, 
                    sponsorID, 
                    secSponsorID, 
                    enrollmentPrimary.ParentID, 
                    mainMemberItemID, 
                    0,
                    "",
                    enrollmentPrimary.EventID,
                    enrollmentPrimary.Password,
                    StartDate,
                    returnCustID, 
                    returnMessage);


                var scalarCustID = (int)returnCustID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                    IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                    enrollmentPrimary.StateList = stateList;
                    IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
                    enrollmentPrimary.NameTitleList = nameTitleList;
                    return PartialView("_EnrollmentPrimary", enrollmentPrimary);                    
                }
                else
                {
                    Response.Cookies["PrimaryEmail"].Value = enrollmentPrimary.Email;
                    Response.Cookies["FirstName"].Value  = enrollmentPrimary.FirstName;
                    Response.Cookies["LastName"].Value = enrollmentPrimary.LastName;

                    if (enrollmentPrimary.Multilocation == "Y")
                    {
                        ModelState.Clear();
                        var model = new EnrollmentLocations {
                            RepID = enrollmentPrimary.RepID,
                            ParentID = scalarCustID,
                            EventID = enrollmentPrimary.EventID,
                            DisplayName = "NEW LOCATION",
                            FirstName = "",
                            LastName = "",
                            Email = enrollmentPrimary.Email,
                            Address1 = "",
                            Address2 = "",                            
                            City = "",
                            State = enrollmentPrimary.State,
                            PostalCode = "",
                            Phone = "",
                            Fax = "",
                            AddLocation = "Y"
                        };
                        IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                        model.StateList = stateList;
                        return PartialView("_EnrollmentLocations", model);
                    }
                    else
                    {
                        var model = new EnrollmentPaymentMethod
                        {
                            CustID = scalarCustID,
                            PName = enrollmentPrimary.FirstName + ' ' + enrollmentPrimary.LastName
                        };
                        IEnumerable<System.Web.Mvc.SelectListItem> cardTypeList = DataHelper.GetCardTypeSelectList();
                        model.CardTypeList = cardTypeList;
                        IEnumerable<System.Web.Mvc.SelectListItem> cardExpMonthList = DataHelper.GetCardExpMonthsSelectList();
                        model.CardExpMonthList = cardExpMonthList;
                        IEnumerable<System.Web.Mvc.SelectListItem> cardExpYearList = DataHelper.GetCardExpYearsSelectList();
                        model.CardExpYearList = cardExpYearList;
                        return PartialView("_EnrollmentPaymentMethod", model);
                    }
                  }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnrollmentLocations([Bind(Include = "RepID,ParentID,EventID,DisplayName,FirstName,LastName,Email,Address1,Address2,City,State,PostalCode,Phone,Fax,AddLocation")] EnrollmentLocations enrollmentLocations)
        {
            DateTime StartDate = DateTime.Now;

            var submitAction = Request.Form["Submit"];
            submitAction = submitAction.ToLower();
            if (submitAction != "add another location")
            {
                enrollmentLocations.AddLocation = "N";
            }
            else{
                enrollmentLocations.AddLocation = "Y";
            }
            if (submitAction == "discard and continue")
            {
                var paymentrecord = db.Database.SqlQuery<MemberEnrollmentThankYou>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", enrollmentLocations.ParentID)
                 ).FirstOrDefault();

                var model = new EnrollmentPaymentMethod
                {
                    CustID = enrollmentLocations.ParentID,
                    PName = paymentrecord.FirstName + ' ' + paymentrecord.LastName
                };
                IEnumerable<System.Web.Mvc.SelectListItem> cardTypeList = DataHelper.GetCardTypeSelectList();
                model.CardTypeList = cardTypeList;
                IEnumerable<System.Web.Mvc.SelectListItem> cardExpMonthList = DataHelper.GetCardExpMonthsSelectList();
                model.CardExpMonthList = cardExpMonthList;
                IEnumerable<System.Web.Mvc.SelectListItem> cardExpYearList = DataHelper.GetCardExpYearsSelectList();
                model.CardExpYearList = cardExpYearList;
                return PartialView("_EnrollmentPaymentMethod", model);
            }
            if (ModelState.IsValid)
            {

                    
                ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                enrollmentLocations.Phone = DataHelper.FixPhone(enrollmentLocations.Phone);
                enrollmentLocations.Fax = DataHelper.FixPhone(enrollmentLocations.Fax);

                var company = "";
                var taxID = "";
                int sponsorID = enrollmentLocations.RepID;
                int secSponsorID = 0;
                int mainMemberItemID = 1;

                db.LB_InsertMember(Constants.locationCustomerType,
                    enrollmentLocations.DisplayName,
                    "",
                    enrollmentLocations.FirstName,
                    enrollmentLocations.LastName,
                    enrollmentLocations.Email,
                    enrollmentLocations.Address1,
                    enrollmentLocations.Address2,
                    enrollmentLocations.City,
                    enrollmentLocations.State,
                    enrollmentLocations.PostalCode,
                    enrollmentLocations.Phone,
                    enrollmentLocations.Phone,
                    "",
                    enrollmentLocations.Fax,
                    company,
                    taxID,
                    sponsorID,
                    secSponsorID,
                    enrollmentLocations.ParentID,
                    mainMemberItemID,
                    0,
                    "",
                    0,
                    "",
                    StartDate,
                    returnCustID,
                    returnMessage);


                var scalarCustID = (int)returnCustID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID <= -1)
                {
                    ModelState.AddModelError("", errorMessage);
                    IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                    enrollmentLocations.StateList = stateList;
                    return PartialView("_EnrollmentLocations", enrollmentLocations);                    
                }
                else
                {
                    if (enrollmentLocations.AddLocation == "Y")
                    {
                        ModelState.Clear();
                        enrollmentLocations.DisplayName = "NEW LOCATION";
                        enrollmentLocations.Address1 = "";
                        enrollmentLocations.Address2 = "";
                        enrollmentLocations.FirstName = "";
                        enrollmentLocations.LastName = "";
                        enrollmentLocations.Address1 = "";
                        enrollmentLocations.Address2 = "";
                        enrollmentLocations.City = "";
                        enrollmentLocations.PostalCode = "";
                        enrollmentLocations.Phone = "";
                        enrollmentLocations.Fax = "";
                        IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                        enrollmentLocations.StateList = stateList;
                        return PartialView("_EnrollmentLocations", enrollmentLocations);
                    }
                    else
                    {

                        var paymentrecord = db.Database.SqlQuery<MemberEnrollmentThankYou>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                         new SqlParameter("@CustID", enrollmentLocations.ParentID)
                         ).FirstOrDefault();

                        var model = new EnrollmentPaymentMethod
                        {
                            CustID = enrollmentLocations.ParentID,
                            PName  = paymentrecord.FirstName + ' ' + paymentrecord.LastName
                        };
                        IEnumerable<System.Web.Mvc.SelectListItem> cardTypeList = DataHelper.GetCardTypeSelectList();
                        model.CardTypeList = cardTypeList;
                        IEnumerable<System.Web.Mvc.SelectListItem> cardExpMonthList = DataHelper.GetCardExpMonthsSelectList();
                        model.CardExpMonthList = cardExpMonthList;
                        IEnumerable<System.Web.Mvc.SelectListItem> cardExpYearList = DataHelper.GetCardExpYearsSelectList();
                        model.CardExpYearList = cardExpYearList;
                        return PartialView("_EnrollmentPaymentMethod", model);
                    }
                }
            }
            else
            {
                IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                enrollmentLocations.StateList = stateList;
                return PartialView("_EnrollmentLocation", enrollmentLocations);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnrollmentPaymentMethod([Bind(Include = "CustID,PName,PCCNumber,PCardType,PExpMonth,PExpYear,CustomerProfile,PaymentProfile,CardCode")] EnrollmentPaymentMethod enrollmentPaymentMethod)
        {
            if (ModelState.IsValid)
            {
                string CardType = enrollmentPaymentMethod.PCardType;
                string CardFirstSix = enrollmentPaymentMethod.PCCNumber.Substring(0, 6);
                int CardNumberLength = enrollmentPaymentMethod.PCCNumber.Length;
                string CardLastFour = enrollmentPaymentMethod.PCCNumber.Substring(CardNumberLength-4, 4);
                int custID = enrollmentPaymentMethod.CustID;
                string expMonth = enrollmentPaymentMethod.PExpMonth.ToString();
                string expYear = enrollmentPaymentMethod.PExpYear.ToString();
                if (enrollmentPaymentMethod.CustomerProfile == null)
                {
                    enrollmentPaymentMethod.CustomerProfile = "1";
                }
                if (enrollmentPaymentMethod.PaymentProfile == null)
                {
                    enrollmentPaymentMethod.PaymentProfile = "1";
                }
                string CustomerProfile = enrollmentPaymentMethod.CustomerProfile;
                string PaymentProfile = enrollmentPaymentMethod.PaymentProfile;


                db.LB_InsertAutoShipPromotionalByCustID(custID);

                var CInfoRecord = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", custID)
                 ).FirstOrDefault();

                string customerEmail = CInfoRecord .Email;
                string customerFirstName = CInfoRecord.FirstName;
                string customerLastName = CInfoRecord.LastName;
                string customerAddress1 = CInfoRecord.Address1;
                string customerCity= CInfoRecord.City;
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

                string customerDescription = customerFirstName + " " + customerLastName;
                string paymentTransactionID = "0";
                string newCustID = enrollmentPaymentMethod.CustID.ToString();

                CustomerGateway customerGateway = new CustomerGateway(apiLogin, transactionKey, serviceMode);
                try
                {
                    if (CustomerProfile == "1")
                    {
                        Customer newcustomer = customerGateway.CreateCustomer(customerEmail, customerDescription, newCustID);
                        CustomerProfile = newcustomer.ProfileID;
                        newcustomer.Email = customerEmail;
                        newcustomer.Description = "This is the record for " + customerDescription ;
                        int intCustomerProfile = Int32.Parse(CustomerProfile);
                        db.LB_UpdateC_InfoCustomerProfileID(custID, intCustomerProfile);
                    }
                    string thisPCardNumber = enrollmentPaymentMethod.PCCNumber;
                    int thisPExpMonth = Int32.Parse(enrollmentPaymentMethod.PExpMonth);
                    int thisPExpYear = Int32.Parse(enrollmentPaymentMethod.PExpYear);
                    //bool updateCustomer = customerGateway.UpdateCustomer(newcustomer);
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
                        string CardCode = enrollmentPaymentMethod.CardCode;
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
                    IEnumerable<System.Web.Mvc.SelectListItem> cardTypeList = DataHelper.GetCardTypeSelectList();
                    enrollmentPaymentMethod.CardTypeList = cardTypeList;
                    IEnumerable<System.Web.Mvc.SelectListItem> cardExpMonthList = DataHelper.GetCardExpMonthsSelectList();
                    enrollmentPaymentMethod.CardExpMonthList = cardExpMonthList;
                    IEnumerable<System.Web.Mvc.SelectListItem> cardExpYearList = DataHelper.GetCardExpYearsSelectList();
                    enrollmentPaymentMethod.CardExpYearList = cardExpYearList;
                    return PartialView("_EnrollmentPaymentMethod", enrollmentPaymentMethod);
                }
                else
                {
                    string shippingProfileID = "";
                    if (Constants.isShippingProfileRequired == "Yes")
                    {
                        shippingProfileID = customerGateway.AddShippingAddress(CustomerProfile, customerFirstName, customerLastName, customerAddress1, customerCity, customerState, customerPostalCode, "US", customerPayPhone);
                    }

                    // This is where we consider a Member actually enrolled successfully and should send email.
                    // LB_InsertPaymentMethodCIMShippingProfile sets StatusID = 1 from 99 making Member active
                    db.LB_InsertPaymentMethodCIMShippingProfile(custID, enrollmentPaymentMethod.PName, enrollmentPaymentMethod.PCardType,
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

                    // Insert Member Director into Identity login tables
                    string isUserError = "";
                    try
                    {
                        var user = new ApplicationUser { UserName = CInfoRecord.Email, Email = CInfoRecord.Email, DisplayName = CInfoRecord.FirstName + " " + CInfoRecord.LastName, CustID = custID };
                        IdentityResult userResult = UserManager.Create(user, CInfoRecord.Password);
                        if (!userResult.Succeeded)
                        {
                            isUserError = "Y";
                        }
                        else
                        {
                            try
                            {
                                IdentityResult roleResult = UserManager.AddToRoles(user.Id, "Member");
                                if (!roleResult.Succeeded)
                                {
                                    isUserError = "Y";
                                }
                            }
                            catch
                            {
                                isUserError = "Y";
                            }
                        }
                    }
                    catch
                    {
                        isUserError = "Y";
                    }

                    if (isUserError == "Y" || scalarCustID < 0)
                    {
                        var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                        new SqlParameter("@ID", Constants.emailIDNewMemberNotification)
                        ).FirstOrDefault();
                        string subject = emailRow.Subject;
                        string body = emailRow.Content;
                        subject = subject.Replace("#CUSTID#", custID.ToString());
                        subject = subject.Replace("#FIRSTNAME#", CInfoRecord.FirstName);
                        subject = subject.Replace("#LASTNAME#", CInfoRecord.LastName);
                        subject = subject.Replace("#NAMETITLE#", CInfoRecord.NameTitle);
                        body = "The following Member Enrollment encountered an error during the creation of their user account.  Contact Technical Support.<br><br>" + body;
                        body = body.Replace("#CUSTID#", custID.ToString());
                        body = body.Replace("#DISPLAYNAME#", CInfoRecord.DisplayName);
                        body = body.Replace("#FIRSTNAME#", CInfoRecord.FirstName);
                        body = body.Replace("#LASTNAME#", CInfoRecord.LastName);
                        body = body.Replace("#TITLE#", CInfoRecord.NameTitle ?? "");
                        body = body.Replace("#STARTDATE#", DateTime.Now.ToShortDateString());
                        body = body.Replace("#DAYPHONE#", CInfoRecord.DayPhone);
                        body = Constants.emailTemplate.Replace("#BODYCOPY#", body);
                        body = body.Replace("#SITEURL#", Constants.siteURL);
                        MailMessage mailMessageNotification = new MailMessage();
                        MailAddress FromAddressNotification = new MailAddress(Constants.adminEmailFrom, "AppTrack Member Services");
                        mailMessageNotification.From = FromAddressNotification;
                        mailMessageNotification.To.Add(Constants.notificationEmailTo);
                        mailMessageNotification.Subject = subject;
                        mailMessageNotification.Body = body;
                        mailMessageNotification.IsBodyHtml = true;
                        SmtpClient client = new SmtpClient();
                        client.Send(mailMessageNotification);
                        //await client.SendMailAsync(mailMessageNotification);

                    }
                    else
                    {
                        int isEmailOn = Constants.emailSwitch;

                        if (isEmailOn == 1)
                        {
                            int emailId = Constants.emailIDNewMember;
                            var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                            new SqlParameter("@ID", emailId)
                            ).FirstOrDefault();
                            string subject = emailRow.Subject;
                            string body = emailRow.Content;
                            string strCustID = custID.ToString();
                            subject = subject.Replace("#CUSTID#", strCustID);
                            subject = subject.Replace("#FIRSTNAME#", CInfoRecord.FirstName);
                            subject = subject.Replace("#LASTNAME#", CInfoRecord.LastName);
                            subject = subject.Replace("#NAMETITLE#", CInfoRecord.NameTitle);
                            body = body.Replace("#CUSTID#", strCustID);
                            body = body.Replace("#FIRSTNAME#", CInfoRecord.FirstName);
                            body = body.Replace("#LASTNAME#", CInfoRecord.LastName);
                            body = body.Replace("#NAMETITLE#", CInfoRecord.NameTitle);
                            body = body.Replace("#EMAIL#", CInfoRecord.Email);
                            body = body.Replace("#PASSWORD#", CInfoRecord.Password);
                            body = Constants.emailTemplate.Replace("#BODYCOPY#", body);
                            body = body.Replace("#SITEURL#", Constants.siteURL);
                            SmtpClient client = new SmtpClient();
                            MailMessage mailMessage = new MailMessage();
                            MailAddress FromAddress = new MailAddress(Constants.memberEmailFrom, "AppTrack Member Services");
                            mailMessage.From = FromAddress;
                            mailMessage.To.Add(CInfoRecord.Email);
                            mailMessage.Subject = subject;
                            mailMessage.Body = body;
                            mailMessage.IsBodyHtml = true;
                            client.Send(mailMessage);
                            //await client.SendMailAsync(mailMessage);


                            emailId = Constants.emailIDNewMemberNotification;
                            emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                            new SqlParameter("@ID", emailId)
                            ).FirstOrDefault();
                            subject = emailRow.Subject;
                            body = emailRow.Content;
                            subject = subject.Replace("#CUSTID#", strCustID);
                            subject = subject.Replace("#FIRSTNAME#", CInfoRecord.FirstName);
                            subject = subject.Replace("#LASTNAME#", CInfoRecord.LastName);
                            subject = subject.Replace("#NAMETITLE#", CInfoRecord.NameTitle);
                            body = body.Replace("#CUSTID#", strCustID);
                            body = body.Replace("#DISPLAYNAME#", CInfoRecord.DisplayName);
                            body = body.Replace("#FIRSTNAME#", CInfoRecord.FirstName);
                            body = body.Replace("#LASTNAME#", CInfoRecord.LastName);
                            body = body.Replace("#TITLE#", CInfoRecord.NameTitle ?? "");
                            body = body.Replace("#STARTDATE#", DateTime.Now.ToShortDateString());
                            body = body.Replace("#DAYPHONE#", CInfoRecord.DayPhone);
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
                            //await client.SendMailAsync(mailMessageNotification);
                        }

                    }

                    var enrollmentVendorRows = db.Database.SqlQuery<EnrollmentVendor>("exec dbo.[LB_GetVendorsAllByCategory]").ToList();
                    var model = new EnrollmentVendorSelect
                    {
                        CustID = enrollmentPaymentMethod.CustID,
                        EnrollmentVendorList = enrollmentVendorRows
                    };
                    return PartialView("_EnrollmentVendorSelect", model);
                }     
            }
            else {
                IEnumerable<System.Web.Mvc.SelectListItem> cardTypeList = DataHelper.GetCardTypeSelectList();
                enrollmentPaymentMethod.CardTypeList = cardTypeList;
                IEnumerable<System.Web.Mvc.SelectListItem> cardExpMonthList = DataHelper.GetCardExpMonthsSelectList();
                enrollmentPaymentMethod.CardExpMonthList = cardExpMonthList;
                IEnumerable<System.Web.Mvc.SelectListItem> cardExpYearList = DataHelper.GetCardExpYearsSelectList();
                enrollmentPaymentMethod.CardExpYearList = cardExpYearList;
                return PartialView("_EnrollmentPaymentMethod", enrollmentPaymentMethod);
            }
          }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnrollmentVendorSelect([Bind(Include = "CustID")] EnrollmentVendorSelect enrollmentVendorSelect, params string[] SelectedVendors)
        {
            string VendorString = "";
            if (SelectedVendors != null)
            {
              VendorString = string.Join(",", SelectedVendors);
            }
            int programID = 1;

            if (ModelState.IsValid)
            {
                db.LB_InsertMemberVendorParse(enrollmentVendorSelect.CustID, VendorString, programID);
            }
            else
            {
                var thisCustID = enrollmentVendorSelect.CustID;
                var enrollmentVendorRows = db.Database.SqlQuery<EnrollmentVendor>("exec dbo.[LB_GetVendorsAllByCategory]").ToList();

                var model = new EnrollmentVendorSelect
                {
                    CustID = thisCustID,
                    EnrollmentVendorList = enrollmentVendorRows
                };
                return PartialView("_EnrollmentVendorSelect", model);
            }

            var finalModel = new EnrollmentFinal { 
                
            };
            finalModel.CustID = enrollmentVendorSelect.CustID;
            IEnumerable<System.Web.Mvc.SelectListItem> customerList = DataHelper. GetCustIDList(Constants.memberDirectorCustomerType);
            finalModel.IMDList = customerList;
            IEnumerable<System.Web.Mvc.SelectListItem> practiceSizeList = DataHelper.GetPracticeSizeList();
            finalModel.PracticeSizeList = practiceSizeList;
            
            return PartialView("_EnrollmentFinal", finalModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EnrollmentFinal([Bind(Include = "CustID,ParentID,PracticeSize")] EnrollmentFinal model)
        {
            if (ModelState.IsValid)
            {
                db.LB_UpdateMemberEnrollmentParentPractice(model.CustID, model.ParentID, model.PracticeSize);
            }
            else
            {
                return PartialView("_EnrollmentFinal", model);
            }
            var thankYouCustID = model.CustID;

            var thankYourecord = db.Database.SqlQuery<MemberEnrollmentThankYou>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
             new SqlParameter("@CustID", thankYouCustID)
             ).FirstOrDefault();

            var thankYouModel = new MemberEnrollmentThankYou
            {
                CustID = thankYouCustID,
                DisplayName = thankYourecord.DisplayName,
                FirstName = thankYourecord.FirstName,
                LastName = thankYourecord.LastName,
                NameTitle = thankYourecord.NameTitle,
                DayPhone = thankYourecord.DayPhone,
                Email = thankYourecord.Email,
                Locations = thankYourecord.Volume1
            };

                int emailId = Constants.emailIDNewMemberNoPassword;
                var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                new SqlParameter("@ID", emailId)
                ).FirstOrDefault();
                string body = emailRow.Content;
                string strCustID = CustID.ToString();
                body = body.Replace("#CUSTID#", thankYouCustID.ToString());
                body = body.Replace("#FIRSTNAME#",thankYouModel.FirstName);
                body = body.Replace("#LASTNAME#", thankYouModel.LastName);
                body = body.Replace("#NAMETITLE#",thankYouModel.NameTitle);
                body = body.Replace("#EMAIL#", thankYouModel.Email);
                body = body.Replace("#PASSWORD#", thankYouModel.Password);
                body = body.Replace("#SITEURL#", Constants.siteURL);

                thankYouModel.ThankYouMessage = body;

            return PartialView("_EnrollmentThankYou", thankYouModel);


        }
    
    }
}