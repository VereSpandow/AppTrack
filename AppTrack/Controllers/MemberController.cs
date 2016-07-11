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
using System.Data;
using FastMember;
using System.IO;

namespace AppTrack.Controllers
{
    [AuthorizeAdminRedirect(Roles = Constants.adminRoles)]
    public class MemberController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
        DataHelpers DataHelper = new DataHelpers();

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public MemberController()
        {
        }

        public MemberController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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

        public FileStreamResult Export(string searchText, string searchTextSite, string StartDate, string EndDate)
        {

            HttpContext.Response.AddHeader("content-disposition", "attachment; filename=Export.csv");

            var sw = new StreamWriter(new MemoryStream());

            return new FileStreamResult(sw.BaseStream, "text/csv");
            // return File(sw.BaseStream, "text/csv", "report.csv"); Renders the same result

        }
        // INDEX : GET
        [HttpGet]
        public ActionResult Index()
        {
            string searchDisplayName = "";
            string searchCompany = "";
            string searchLastName = "";
            int zero = 0;
            string selectedStatus = " ";
            int customerType = Constants.memberCustomerType;

            var memberRows = db.Database.SqlQuery<AppTrack.SharedModels.Member>("exec dbo.[LB_GetCustByNameStatusType]  @DisplayName, @Company, @LastName, @Status, @CustomerType, @SponsorID, @MaxCount",
             new SqlParameter("@DisplayName", searchDisplayName),
             new SqlParameter("@Company", searchCompany),
             new SqlParameter("@LastName", searchLastName),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@CustomerType", customerType),
             new SqlParameter("@SponsorID", zero),
             new SqlParameter("@MaxCount", 100)
             ).ToList();


            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, false, true);
            IEnumerable<System.Web.Mvc.SelectListItem> adminList = DataHelper.GetAccountManagerSelectList(false, true);
            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList(false, true);

            var model = new MemberListViewModel
            {
                MemberList = memberRows,
                SearchCustID = " ",
                SearchDisplayName = searchDisplayName,
                SearchCompany = searchCompany,
                SearchLastName = searchLastName,
                SelectedStatus = selectedStatus,
                StatusList = thisList,
                AccountManagerList = adminList,
                StateList = stateList
            };

            return View(model);
        }

        // INDEX : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "SearchCustID, SearchDisplayName, SearchCompany, SearchLastName, SearchAddress1, SearchCity, SearchState, SearchPostalCode, SearchPhone, SearchEmail, SearchAccountManagerID, SelectedStatus")] MemberListViewModel model, string submit = "submit")
        {
            int zero = 0;
            string searchDisplayName = model.SearchDisplayName ?? " ";
            string searchCompany = model.SearchCompany ?? " ";
            string searchLastName = model.SearchLastName ?? " ";
            string searchAddress1 = model.SearchAddress1 ?? " ";
            string searchCity = model.SearchCity ?? " ";
            string searchState = model.SearchState ?? " ";
            string searchPostalCode = model.SearchPostalCode ?? " ";
            string searchPhone = model.SearchPhone ?? " ";
            searchPhone = DataHelper.FixPhone(searchPhone);
            string searchEmail = model.SearchEmail ?? " ";

            string selectedStatus = model.SelectedStatus ?? "Active";
            int searchAccountManagerID = model.SearchAccountManagerID;

            int intCustID = 0;

            if (model.SearchCustID != "")
            {
                if (!Int32.TryParse(model.SearchCustID, out intCustID))
                    intCustID = 0;
            }

            if(intCustID > 0)
            {
                model.MemberList = db.Database.SqlQuery<AppTrack.SharedModels.Member>("exec dbo.[LB_GetC_InfoByCustIDCustomerType] @CustID, @CustomerType",
                 new SqlParameter("@CustID", intCustID),
                 new SqlParameter("@CustomerType", Constants.memberCustomerType)
                 ).ToList();
            }
            else
            {
                model.MemberList = db.Database.SqlQuery<AppTrack.SharedModels.Member>("exec dbo.[LB_GetMemberList]  @DisplayName, @Company, @LastName, @Address1, @City, @State, @PostalCode, @Phone, @Email, @Status, @CustomerType, @AdminID, @MaxCount",
                 new SqlParameter("@DisplayName", searchDisplayName),
                 new SqlParameter("@Company", searchCompany),
                 new SqlParameter("@LastName", searchLastName),
                 new SqlParameter("@Address1", searchAddress1),
                 new SqlParameter("@City", searchCity),
                 new SqlParameter("@State", searchState),
                 new SqlParameter("@PostalCode", searchPostalCode),
                 new SqlParameter("@Phone", searchPhone),
                 new SqlParameter("@Email", searchEmail),
                 new SqlParameter("@Status", selectedStatus),
                 new SqlParameter("@CustomerType", Constants.memberCustomerType),
                 new SqlParameter("@AdminID", searchAccountManagerID),
                 new SqlParameter("@MaxCount", zero)
                 ).ToList();

                if (submit == "Download")
                {

                    if (model.MemberList.Count > 0)
                    {
                        string attachment = "attachment; filename=" + AdminID.ToString() + "-MemberList.csv";

                        HttpContext.Response.AddHeader("content-disposition", attachment);

                        var sw = new StreamWriter(new MemoryStream());

                        sw.WriteLine("\"Member ID\",\"Customer Type\",\"Parent ID\",\"Sales Rep ID\",\"IMD ID\",\"Sage ID\",\"Meeting ID\",\"Sales Force ID\",\"Name Title\",\"First Name\",\"Last Name\",\"TIN Name\",\"Practice Name\",\"Tax ID\",\"Address 1\",\"Address 2\",\"City\",\"State\",\"Postal Code\",\"Email\",\"Company Phone\",\"Day Phone\",\"Mobile\",\"Fax\",\"WebSite Name\",\"Start Date\",\"Status\",\"Status Date\",\"Status ID\",\"Posted Date\"");
                        foreach (var line in model.MemberList)
                        {
                            sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\",\"{23}\",\"{24}\",\"{25}\",\"{26}\",\"{27}\",\"{28}\",\"{29}\"",
                            line.CustID,
                            line.CustomerType,
                            line.ParentID,
                            line.SponsorID,
                            line.SecSponsorID,
                            line.AccountingID,
                            line.SourceID,
                            line.SalesForceID,
                            line.NameTitle,
                            line.FirstName,
                            line.LastName,
                            line.Company,
                            line.DisplayName,
                            line.TaxID,
                            line.Address1,
                            line.Address2,
                            line.City,
                            line.State,
                            line.PostalCode,
                            line.Email,
                            line.CompanyPhone,
                            line.DayPhone,
                            line.Mobile,
                            line.Fax,
                            line.SiteName,
                            line.StartDate.Value.ToString("MM/dd/yyyy"),
                            line.Status,
                            line.StatusDate.Value.ToString("MM/dd/yyyy"),
                            line.StatusID,
                            line.PostDate.Value.ToString("MM/dd/yyyy")));
                        }
                        sw.Flush();
                        sw.BaseStream.Seek(0, SeekOrigin.Begin);

                        //return new FileStreamResult(sw.BaseStream, "text/csv");
                        return File(sw.BaseStream, "text/csv");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Cannot download an empty list");
                    }
                }
            }

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, false,true);
            IEnumerable<System.Web.Mvc.SelectListItem> adminList = DataHelper.GetAccountManagerSelectList(false, true);
            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList(false, true);

            model.StatusList = thisList;
            model.AccountManagerList = adminList;
            model.StateList = stateList;

            return View(model);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StatusUpdate()
        {
            var formCustID = Request.Form["CustID"];
            var formStatusID = Request.Form["StatusID"];
            var formAdminID = Request.Form["AdminID"];
            var status = Request.Form["Status"];
            int custID = Convert.ToInt32(formCustID);
            int statusID = Convert.ToInt32(formStatusID);
            int adminID = Convert.ToInt32(formAdminID);

            ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
            ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

            db.LB_UpdateC_InfoStatusID(custID,
                statusID,
                status,
                adminID,
                returnID,returnMessage);

            var scalarCustID = (int)returnID.Value;

            if (scalarCustID == -1)
            {
                ModelState.AddModelError("", "There was an error updating the status");
            }
            else
            {
                TempData["ErrMessage"] = "Member Status successfully updated";
                return RedirectToAction("AccountProfile", new { id = custID });
            }

            return RedirectToAction("AccountProfile", new { id = custID });
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        // LOCATION: GET
        [HttpGet]
        public ActionResult LocationList(int? id = 0)
        {
            ViewBag.ErrorCode = 0;
            string searchLastName = "";
            string selectedStatus = "Active";

            var locationlistmodel = new LocationListViewModel { };

            var locationRows = db.Database.SqlQuery<Location>("exec dbo.[LB_GetCustByParentID]  @ParentID, @CustomerType",
             new SqlParameter("@ParentID", id),
             new SqlParameter("@CustomerType", 66)
             ).ToList();

            if ((locationRows == null) || (locationRows.Count() == 0))
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "No Locations Exists for This Member";
                ViewBag.ParentID = id;
                return PartialView("_LocationList");
            }
            else
            {


                locationlistmodel.LocationList = locationRows;
                locationlistmodel.SearchLastName = searchLastName;
                locationlistmodel.SelectedStatus = selectedStatus;
                IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, true);
                locationlistmodel.StatusList = thisList;

                return PartialView("_LocationList", locationlistmodel);
            }
        }

        //ACOUNTPROFILE : GET 
        public ActionResult AccountProfile(int id = 0, string tab = "")
        {
            int CustID = id;

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.memberCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.locationCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Member  ID supplied to Account Profile page";
                    return RedirectToAction("Error", "Admin");
                }
                else
                {
                    return RedirectToAction("LocationProfileMain", "Member", new { id = CustID });
                }
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
                 new SqlParameter("@CustID", CustID)
                 ).First();

                model.CustID = model.MemberRecord.CustID;
                model.Status = model.MemberRecord.Status;
                model.OriginaStatus = model.MemberRecord.Status;
                model.AdminID = 6;

                model.AutoshipBasicList = db.Database.SqlQuery<AutoshipBasic>("exec dbo.[LB_GetAutoshipHeaderByCustIDAll] @CustID",
                 new SqlParameter("@CustID", CustID)
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

                ViewBag.Tab = tab;

                return View(model);
            }
        }
        //ACOUNTPROFILE : GET 
        public ActionResult LocationProfileMain(int id = 0)
        {
            int CustID = id;
            ViewBag.CustID = CustID;
            return View();
        }

        //ACOUNTPROFILE : GET 
        public ActionResult LocationProfile(int id = 0)
        {
            int CustID = id;


            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.locationCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Location ID supplied to Profile page";
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

                var model = new LocationProfileViewModel();

                model.LocationRecord = db.Database.SqlQuery<Location>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).First();

                model.CommissionHeaderList = db.Database.SqlQuery<CommissionHeader>("exec dbo.[LB_GetCommissionDetailByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
                 new SqlParameter("@CustID", CustID),
                 new SqlParameter("@PeriodTypeID", periodTypeID),
                 new SqlParameter("@StartPeriodID", startPeriodID),
                 new SqlParameter("@EndPeriodID", endPeriodID),
                 new SqlParameter("@CommissionID", commissionID),
                 new SqlParameter("@Status", thisStatus)
                 ).ToList();

                IEnumerable<System.Web.Mvc.SelectListItem> statusCodes = DataHelper.GetMemberStatusList();
                model.statusList = statusCodes;

                IEnumerable<System.Web.Mvc.SelectListItem> cancelReasonCodesList = DataHelper.GetCancelReasonCodesSelectList();
                model.CancelReasonCodesList = cancelReasonCodesList;
                
                if (TempData["ErrorMessage"] != null)
                {
                    ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
                }
                return PartialView("_LocationProfile", model);
            }
        }

        // CONTACT LIST: GET
        public ActionResult ContactList(int id = 0)
        {
            ModelState.Clear();
            var contactRows = db.Database.SqlQuery<Contact>("exec dbo.[LB_GetContacts]   @SponsorID, @CustomerType",
             new SqlParameter("@SponsorID", id),
             new SqlParameter("@CustomerType", 61)
             ).ToList();

            var model = new ContactListViewModel
            {
                CustID = id,
                ContactList = contactRows,
            };

            if(TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }
            return PartialView("_ContactsList", model);
        }

        // CREATE: GET
        public ActionResult ContactCreate(int? id = 0)
        {

            var model = new MemberContactViewModel();
            var contactRecordRow = new Contact();
            model.contactRecord = contactRecordRow;
            model.contactRecord.SponsorID = id;

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;
            IEnumerable<System.Web.Mvc.SelectListItem> contactTypeList = DataHelper.GetContactRolesSelectList();
            model.ContactTypeList = contactTypeList;

            return PartialView("_ContactCreate", model);

        }

        // CREATE: POST
        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
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
                    model.contactRecord.ContactType,
                    model.contactRecord.VariantData2,
                    model.contactRecord.VariantData3,
                    returnCustID, returnMessage);

                var scalarCustID = (int)returnCustID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                    IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                    model.StateList = stateList;
                    IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
                    model.NameTitleList = nameTitleList;
                    IEnumerable<System.Web.Mvc.SelectListItem> contactTypeList = DataHelper.GetContactRolesSelectList();
                    model.ContactTypeList = contactTypeList;
                    return PartialView("_Contact", model);
                }
                else
                {
                    ViewBag.ErrorMessage = "Member Contact information was updated successfully";
                }
            }

            return RedirectToAction("ContactList", new { id = model.contactRecord.SponsorID });
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        // EDIT: GET
        public ActionResult ContactEdit(int? CustID = 0)
        {

            var model = new MemberContactViewModel();

            var contactRecordRow = db.Database.SqlQuery<Contact>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
             new SqlParameter("@CustID", CustID)
             ).FirstOrDefault();


            model.contactRecord = contactRecordRow;

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;
            IEnumerable<System.Web.Mvc.SelectListItem> contactTypeList = DataHelper.GetContactRolesSelectList();
            model.ContactTypeList = contactTypeList;

            return PartialView("_Contact", model);

        }

        // EDIT : POST
        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactEdit([Bind(Include = "contactRecord")] MemberContactViewModel model)
        {
            ViewBag.ErrorCode = 0;

            if (ModelState.IsValid)
            {
                ViewBag.CustID = model.contactRecord.CustID;

                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(model.contactRecord.CustID, Constants.memberContactCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ViewBag.ErrorCode = Constants.fatalErrorCode;
                    ViewBag.ErrorMessage = "Invalid Member Contact ID supplied to Edit page";
                    return PartialView("_Contact", model);
                }
                else
                {
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
                        IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                        model.StateList = stateList;
                        IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
                        model.NameTitleList = nameTitleList;
                        IEnumerable<System.Web.Mvc.SelectListItem> contactTypeList = DataHelper.GetContactRolesSelectList();
                        model.ContactTypeList = contactTypeList;

                        return PartialView("_Contact", model);
                    }
                    else
                    {
                        ModelState.Clear();
                        ViewBag.ErrorMessage = "Member information was updated successfully";
                    }
                }
            }

            return RedirectToAction("ContactList", new { id = model.contactRecord.SponsorID });
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
                TempData["ErrorMessage"] = errorMessage;
            }

            return RedirectToAction("ContactList", new { id = checkCustomerResult.C_Info.SponsorID });
        }


        // POST: Info/Delete/5
        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus([Bind(Include = "StatusID, Status, CustID, cancelReasonCode, Comments")] MemberStatusViewModel model)
        {
            if (model.StatusID == 1)
            {
                model.cancelReasonCode = "Reactivation";
            }
            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(model.CustID, Constants.memberCustomerType);
            if (AdminID == null)
            {
                AdminID = 0;
            }

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Member ID supplied to Status Update action";
                return RedirectToAction("Error", "Admin");
            }
            else
            {

                db.LB_UpdateC_InfoStatusIDAppTrack(
                    model.CustID,
                    model.StatusID,
                    model.Status,
                    model.AdminID,
                    model.cancelReasonCode,
                    model.Comments
                   );
            }
            TempData["ErrMessage"] = "Member Status successfully updated";
            return RedirectToAction("AccountProfile", new { id = model.CustID });

        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance,MemberServices")]
        // GET: OrderList
        public ActionResult OrderList(int CustID = 0)
        {
            DateTime now = DateTime.Now;
            DateTime searchStartDate = DateTime.Now.AddDays(-180); ;
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

            return PartialView("_OrderList", orderListViewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OrderList([Bind(Include = "SearchStartDate, SearchEndDate, SearchStartBalance, SearchEndBalance, SearchCustID, SearchStatus, SearchItemID, SearchOrderID")] 
            OrderListViewModel orderListViewModel)
        {
            if (ModelState.IsValid)
            {
                DateTime now = DateTime.Now;
                DateTime searchStartDate = orderListViewModel.SearchStartDate ?? DateTime.Now;
                DateTime searchEndDate = orderListViewModel.SearchEndDate ?? DateTime.Now;
                decimal searchStartBalance = orderListViewModel.SearchStartBalance;
                decimal searchEndBalance = orderListViewModel.SearchEndBalance;
                int searchCustID = orderListViewModel.SearchCustID ?? 0;
                string searchStatus = orderListViewModel.SearchStatus ?? " ";
                int searchItemID = orderListViewModel.SearchItemID ?? 0;
                int searchOrderID = orderListViewModel.SearchOrderID ?? 0;

                if (searchStartDate > searchEndDate)
                {
                    ModelState.AddModelError("", "Start Date cannot be after End Date");
                    // Return below using empty model
                    orderListViewModel.OrderHeaderList = new List<C_OrderHeader>();
                }
                else
                {
                    if (searchStartBalance > searchEndBalance)
                    {
                        ModelState.AddModelError("", "Starting Balance cannot be greater than Ending Balance");
                        // Return below using empty model
                        orderListViewModel.OrderHeaderList = new List<C_OrderHeader>();
                    }
                    else
                    {
                        orderListViewModel.OrderHeaderList = db.Database.SqlQuery<C_OrderHeader>("exec dbo.[LB_GetOrderHeaderList] @StartDate, @EndDate, @StartBalance, @EndBalance, @CustID, @Status, @ItemID, @OrderID",
                            new SqlParameter("@StartDate", searchStartDate),
                            new SqlParameter("@EndDate", searchEndDate),
                            new SqlParameter("@StartBalance", searchStartBalance),
                            new SqlParameter("@EndBalance", searchEndBalance),
                            new SqlParameter("@CustID", searchCustID),
                            new SqlParameter("@Status", searchStatus),
                            new SqlParameter("@ItemID", searchItemID),
                            new SqlParameter("@OrderID", searchOrderID)
                            ).ToList();

                    }
                } // Search Start and End Date valid
            } // ModelState.IsValid
            IEnumerable<System.Web.Mvc.SelectListItem> searchItemList = DataHelper.GetAllMembershipItemsSelectList(false, true);

            orderListViewModel.SearchItemList = searchItemList;

            IEnumerable<System.Web.Mvc.SelectListItem> searchStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, false, true);

            orderListViewModel.SearchStatusList = searchStatusList;
            return PartialView("_OrderList", orderListViewModel);

        }

        [HttpGet]
        public ActionResult MemberVendorSelect(int? id = 0)
        {

            // Debug.WriteLine("Get-" + searchLastName + "|" + searchStatus + "|" + startDateTime + "|" + endDateTime);
            var vendorRows = db.Database.SqlQuery<MemberVendor>("exec dbo.[LB_GetMemberVendorListByCustID]  @CustID, @CustomerType",
             new SqlParameter("@CustID", id),
             new SqlParameter("@CustomerType", Constants.vendorCustomerType)
             ).ToList();

            vendorRows.Sort((x, y) => string.Compare(x.Company, y.Company));

            var model = new MemberVendorViewModel
            {
                MemberVendorList = vendorRows,
                CustID = id,
            };

            /*                var memberVendorRows = db.Database.SqlQuery<MemberEnrollmentVendor>("exec dbo.[LB_GetVendorsAllByCategory]").ToList();
                            var model = new MemberEnrollmentVendorSelect
                            {
                                CustID = id,
                                MemberEnrollmentVendorList = memberEnrollmentVendorRows
                            };
             */
            return PartialView("_MemberVendors", model);
        }

        [HttpPost]
        public ActionResult MemberVendorProgramSelect(int CustID, int VendorID, string Participation, int ItemID)
        {

            db.LB_UpdateMemberVendor(CustID,
                VendorID,
                Participation);

            var thisMessage = "Thank You Vendor " + VendorID + " has been changed to " + Participation;

            return Content(thisMessage);
            //return PartialView("_ThankYou");
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpPost]
        public ActionResult MemberVendorPayee(int CustID, int VendorID, string Payee)
        {

            db.LB_UpdateVendorPayeeNoKey(CustID,
                VendorID,
                Payee,
                "Active",
                1,
                0);

            var thisMessage = "Thank You Vendor " + VendorID + " has been changed to " + Payee;

            return Content(thisMessage);
            //return PartialView("_ThankYou");
        }

        [HttpGet]
        public ActionResult MemberVendorRequirement(int CustID, int VendorID)
        {
            var thisMessage = "Thank You Vendor " + VendorID + " Requirements are being gathered";
            
            var requirementRows = db.Database.SqlQuery<MemberVendorRequirement>("exec dbo.[LB_GetMemberVendorRequirementByMemberVendor] @CustID, @VendorID",
                new SqlParameter("@CustID", CustID),
                new SqlParameter("@VendorID", VendorID)
                ).ToList();

            var model = new MemberVendorRequirementViewModel();

            model.MemberVendorRequirementList = requirementRows ;
            model.CustID =  CustID;
            model.VendorID = VendorID;

            IEnumerable<System.Web.Mvc.SelectListItem> MemberVendorRequirementStatusList = DataHelper.GetMemberVendorRequirementStatusList();
            
            return PartialView("_MemberVendorRequirements", model);
        }

        // Requirement: POST
        [AuthorizeAdminRedirect(Roles = "MemberServices")]
        [HttpPost]
        public ActionResult MemberVendorRequirementUpdate(int RequirementID, string Status)
        {

            //var thisMessage = "Thank You. This Vendor Requirement has been updated to ";

            db.LB_UpdateMemberVendorRequirementByID(
                RequirementID,
                Status,
                "","");

            var thisMessage = "Thank You. This Vendor Requirement has been updated to " + Status;

            return Content("Thank You!");
            //return PartialView("_ThankYou");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DocumentUpload(HttpPostedFileBase documentFile, int MVRID = 0, int CustID = 0, int VendorID = 0)
        {
            if (ModelState.IsValid)
            {
                string thisFileName = "";

                if (MVRID != 0)
                {
                    if (documentFile != null && documentFile.ContentLength > 0)
                    {
                        string folderName = Server.MapPath("~/Documents/Requirements/");
                        if (!System.IO.Directory.Exists(folderName))
                        {
                            System.IO.Directory.CreateDirectory(folderName);
                        }
                        String fileExtension = Path.GetExtension(documentFile.FileName).ToLower();
                        String allowedExtensionsStr = ".pdf";
                        int findExt = allowedExtensionsStr.IndexOf(fileExtension);
                        if (findExt >= 0)
                        {
                            thisFileName = MVRID.ToString() + '-' + Path.GetFileName(documentFile.FileName);

                            string fullPath = Path.Combine(folderName, thisFileName);

                            if (System.IO.File.Exists(fullPath))
                            {
                                ModelState.AddModelError("", "A file with this name already exists on the server for this Requirement");
                            }
                            else
                            {
                                try
                                {
                                    documentFile.SaveAs(fullPath);
                                }
                                catch (Exception ex)
                                {
                                    ModelState.AddModelError("", "ERROR:" + ex.Message.ToString());
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "The file type was not valid. Valid file type is .pdf");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "You have not specified a file.");
                    }

                    if (ModelState.IsValid)
                    {

                        db.LB_UpdateMemberVendorRequirementByID(
                            MVRID,
                            "Completed",
                            "~/Documents/Requirements/",
                            thisFileName
                            );
                    }
                }
                else
                {
                    ModelState.AddModelError("", "You have not specified a file.");
                }
            }
            return RedirectToAction("AccountProfile", new { id = CustID, tab = "Vendor" });
        }

        [HttpGet]
        public ActionResult DocumentUploadIFrame(int MVRID = 0, int CustID = 0, int VendorID = 0)
        {
            ViewBag.Message = "";
            ViewBag.CustID = CustID;
            ViewBag.VendorID = VendorID;
            ViewBag.MVRID = MVRID;
            return View("DocumentUploadIFrame", "~/Views/Shared/_IFrameLayout.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DocumentUploadIFrame(HttpPostedFileBase documentFile, int MVRID = 0, int CustID = 0, int VendorID = 0)
        {
            if (ModelState.IsValid)
            {
                string thisFileName = "";

                if (MVRID != 0)
                {
                    if (documentFile != null && documentFile.ContentLength > 0)
                    {
                        string folderName = Server.MapPath("~/Documents/Requirements/");
                        if (!System.IO.Directory.Exists(folderName))
                        {
                            System.IO.Directory.CreateDirectory(folderName);
                        }
                        String fileExtension = Path.GetExtension(documentFile.FileName).ToLower();
                        String allowedExtensionsStr = ".pdf";
                        int findExt = allowedExtensionsStr.IndexOf(fileExtension);
                        if (findExt >= 0)
                        {
                            thisFileName = MVRID.ToString() + '-' + Path.GetFileName(documentFile.FileName);

                            string fullPath = Path.Combine(folderName, thisFileName);

                            if (System.IO.File.Exists(fullPath))
                            {
                                ModelState.AddModelError("", "A file with this name already exists on the server for this Requirement");
                            }
                            else
                            {
                                try
                                {
                                    documentFile.SaveAs(fullPath);
                                }
                                catch (Exception ex)
                                {
                                    ModelState.AddModelError("", "ERROR:" + ex.Message.ToString());
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "The file type was not valid. Valid file type is .pdf");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "You have not specified a file.");
                    }

                    if (ModelState.IsValid)
                    {

                        db.LB_UpdateMemberVendorRequirementByID(
                            MVRID,
                            "Completed",
                            "~/Documents/Requirements/",
                            thisFileName
                            );
                    }
                }
                else
        {
                    ModelState.AddModelError("", "You have not specified a file.");
                }
            }

            if (ModelState.IsValid)
            {
                ViewBag.Message = "The Document was successfully uploaded.";
            }
            else
            {
                ViewBag.Message = "";
            }

            ViewBag.CustID = CustID;
            ViewBag.VendorID = VendorID;
            ViewBag.MVRID = MVRID;

            return View("DocumentUploadIFrame","~/Views/Shared/_IFrameLayout.cshtml");
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        // ProfileUpdate : Get
        [HttpGet]
        public ActionResult ViewMembershipDetail(int id)
        {
            var autoShipHeaderUpdateViewModel = new AutoShipHeaderUpdateViewModel();

            autoShipHeaderUpdateViewModel.AutoShipDetailList = db.Database.SqlQuery<AutoShipDetail>("exec dbo.[LB_GetAutoShipDetailByAutoShipID] @AutoShipID",
             new SqlParameter("@AutoShipID", id)
             ).ToList();

            return PartialView("_ViewMembershipDetail", autoShipHeaderUpdateViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        // ProfileUpdate : Get
        [HttpGet]
        public ActionResult ProfileUpdate(int id)
        {
            var autoShipHeaderUpdateViewModel = new AutoShipHeaderUpdateViewModel();

            autoShipHeaderUpdateViewModel.AutoShipHeader = db.Database.SqlQuery<AutoShipHeader>("exec dbo.[LB_GetAutoShipHeaderByID] @AutoShipID",
             new SqlParameter("@AutoShipID", id)
             ).First();

            autoShipHeaderUpdateViewModel.AutoShipDetailList = db.Database.SqlQuery<AutoShipDetail>("exec dbo.[LB_GetAutoShipDetailByAutoShipID] @AutoShipID",
             new SqlParameter("@AutoShipID", id)
             ).ToList();

            IEnumerable<System.Web.Mvc.SelectListItem> editStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, true, false);

            autoShipHeaderUpdateViewModel.EditStatusList = editStatusList;
            autoShipHeaderUpdateViewModel.EditNextDate = autoShipHeaderUpdateViewModel.AutoShipHeader.NextDate;
            autoShipHeaderUpdateViewModel.EditStatus = autoShipHeaderUpdateViewModel.AutoShipHeader.Status;

            return PartialView("_ProfileUpdate", autoShipHeaderUpdateViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileUpdate([Bind(Include = "AutoShipHeader, EditStatus, EditNextDate")] AutoShipHeaderUpdateViewModel autoShipHeaderUpdateViewModel)
        {
            int autoShipID = autoShipHeaderUpdateViewModel.AutoShipHeader.AutoShipID;
            ViewBag.ErrorCode = 0;

            if (ModelState.IsValid)
            {

                if (autoShipHeaderUpdateViewModel.EditNextDate < DateTime.Now)
                {
                    ModelState.AddModelError("", "Next Billing Date cannot be in the past");
                    ViewBag.ErrorCode = 1;
                }
                else
                {
                    autoShipID = autoShipHeaderUpdateViewModel.AutoShipHeader.AutoShipID;

                    // Update Profile
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateAutoShipHeader(autoShipID,
                        autoShipHeaderUpdateViewModel.EditStatus,
                        autoShipHeaderUpdateViewModel.EditNextDate,
                        AdminID,
                        returnID, returnMessage);

                    var scalarID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                        ViewBag.ErrorCode = 1;
                    }
                }
            }

            autoShipHeaderUpdateViewModel.AutoShipHeader = db.Database.SqlQuery<AutoShipHeader>("exec dbo.[LB_GetAutoShipHeaderByID] @AutoShipID",
             new SqlParameter("@AutoShipID", autoShipID)
             ).First();


            autoShipHeaderUpdateViewModel.AutoShipDetailList = db.Database.SqlQuery<AutoShipDetail>("exec dbo.[LB_GetAutoShipDetailByAutoShipID] @AutoShipID",
             new SqlParameter("@AutoShipID", autoShipID)
             ).ToList();

            IEnumerable<System.Web.Mvc.SelectListItem> editStatusList = DataHelper.GetStatusSelectList(Constants.autoShipStatusLookupGroupID, true, false);

            autoShipHeaderUpdateViewModel.EditStatusList = editStatusList;

            return PartialView("_ProfileUpdate", autoShipHeaderUpdateViewModel);
        }


        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpGet]
        public ActionResult MemberPaymentMethod(int? id = 0)
        {

            PaymentMethod emptyPaymentMethod = new PaymentMethod();

            MemberPaymentMethodViewModel model = new MemberPaymentMethodViewModel();

            model.currentPaymentMethod = emptyPaymentMethod;

            var paymentMethodRow = db.Database.SqlQuery<PaymentMethod>("exec dbo.[LB_GetPaymentMethodByCustID] @CustID",
                 new SqlParameter("@CustID", id)
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
                model.currentPaymentMethod.CustID = id;
                model.currentPaymentMethod.PName = "";
                model.currentPaymentMethod.PCardNumber = "";
                model.currentPaymentMethod.PCardType = "";
                model.currentPaymentMethod.PExpirationMonth = "";
                model.currentPaymentMethod.PExpirationYear = "";
            }
            model.CustID = id;
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

            return PartialView("_MemberPaymentMethod", model);

        }

        // EDIT : POST
        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberPaymentMethod([Bind(Include = "CustID, PName, PCardNumber, PCardType, PExpirationMonth, PExpirationYear, CardCode")] MemberPaymentMethodViewModel model)
        {
            string CardFirstSix = model.PCardNumber.Substring(0, 6);
            string CardType = model.PCardType;
            int CardNumberLength = model.PCardNumber.Length;
            string CardLastFour = model.PCardNumber.Substring(CardNumberLength - 4, 4);
            int? custID = model.CustID;
            string expMonth = model.PExpirationMonth.ToString();
            string expYear = model.PExpirationYear.ToString();
            ViewBag.ErrorMessage = "";

            if (ModelState.IsValid)
            {
                var C_InfoRow = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", custID)
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
                string paymentTransactionID = "Success";
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
                if (CustomerProfile.Length <= 6)
                {
//                    try
//                   {
                        Customer newcustomer = customerGateway.CreateCustomer(customerEmail, customerDescription, newCustID);
                        CustomerProfile = newcustomer.ProfileID;
                        newcustomer.Email = customerEmail;
                        newcustomer.Description = "This is the record for " + customerDescription;
                        int intCustomerProfile = Int32.Parse(CustomerProfile);
                        db.LB_UpdateC_InfoCustomerProfileID(custID, intCustomerProfile);
                        
                    
                    //bool updateCustomer = customerGateway.UpdateCustomer(newcustomer);
//                    }
//                    catch (Exception e)
//                    {
//                        ModelState.AddModelError("", e.Message);
//                    }
                }

                if (ModelState.IsValid)
                {
                    string thisPCardNumber = model.PCardNumber;
                    int thisPExpMonth = Int32.Parse(model.PExpirationMonth);
                    int thisPExpYear = Int32.Parse(model.PExpirationYear);
                    string CardCode = model.CardCode;
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
                            if (CardType == "Visa") {
                                string addCreditCard = customerGateway.AddCreditCard(CustomerProfile, thisPCardNumber, thisPExpMonth, thisPExpYear, CardCode, address);
                                PaymentProfile = addCreditCard;
                            }
                            else{
                                string addCreditCard = customerGateway.AddCreditCard(CustomerProfile, thisPCardNumber, thisPExpMonth, thisPExpYear, CardCode);
                                PaymentProfile = addCreditCard;
                            }

                            if (Constants.isAuthorizeOn == "Yes")
                            {
                                IGatewayResponse authorize = customerGateway.Authorize(CustomerProfile, PaymentProfile, 1.01m);
                                paymentTransactionID = authorize.TransactionID;
                            }
                        }
                        catch (Exception e)
                        {
                            string s = e.Message;
                            ModelState.AddModelError("", e.Message);
                            ViewBag.ErrorMessage = s;
                            paymentTransactionID = "Error";
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

                ViewBag.CustomerProfile = CustomerProfile;
                ViewBag.PaymentProfile = PaymentProfile;
                if (ModelState.IsValid)
                {
                    string shippingProfileID = "";
                    if (Constants.isShippingProfileRequired == "Yes")
                    {
                        shippingProfileID = customerGateway.AddShippingAddress(CustomerProfile, customerFirstName, customerLastName, customerAddress1, customerCity, customerState, customerPostalCode, "US", customerPayPhone);
                    }

                    if (ViewBag.ErrorMessage == "")
                    {
                        db.LB_InsertPaymentMethodCIMShippingProfile(custID, model.PName, model.PCardType,
                                                CardFirstSix, CardLastFour, expMonth, expYear, PaymentProfile, CustomerProfile, shippingProfileID
                                                 );
                        ViewBag.ErrorMessage = "Thank You, Your new payment method has been saved.";
                    }

                    ModelState.Clear();
                }
            }

            var paymentMethodRow = db.Database.SqlQuery<PaymentMethod>("exec dbo.[LB_GetPaymentMethodByCustID] @CustID",
            new SqlParameter("@CustID", custID)
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
            IEnumerable<System.Web.Mvc.SelectListItem> cardTypeList = DataHelper.GetCardTypeSelectList();
            model.CardTypeList = cardTypeList;
            IEnumerable<System.Web.Mvc.SelectListItem> cardExpMonthList = DataHelper.GetCardExpMonthsSelectList();
            model.CardExpMonthList = cardExpMonthList;
            IEnumerable<System.Web.Mvc.SelectListItem> cardExpYearList = DataHelper.GetCardExpYearsSelectList();
            model.CardExpYearList = cardExpYearList;

            model.PName = "";
            model.PCardNumber = "";
            model.PCardType = "";
            model.CardCode = "";
            model.PExpirationMonth = "";
            model.PExpirationYear = "";
            return PartialView("_MemberPaymentMethod", model);
        }


        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices,MemberServicesManager")]
        [HttpGet]
        public ActionResult ProgramChange(int id = 0)
        {

            var model = new MemberProgramViewModel();

            var AutoshipBasicRecord = db.Database.SqlQuery<AutoshipBasic>("exec dbo.[LB_GetAutoshipHeaderByCustID] @CustID",
             new SqlParameter("@CustID", id)
             ).FirstOrDefault();

            if (AutoshipBasicRecord != null)
            {
                model.AutoshipID = AutoshipBasicRecord.AutoshipID;
            }
            else
            {
                model.AutoshipID = 0;
            }

            IEnumerable<System.Web.Mvc.SelectListItem> programList = DataHelper.GetProgramList(id,true,false);
            model.ProgramList = programList;
            IEnumerable<System.Web.Mvc.SelectListItem> promotionList = GetItemPromotionProgramList(id);
            model.PromotionList = promotionList;
            model.CustID = id;

            ObjectParameter returnIdoc = new ObjectParameter("returnIdoc", typeof(int));
            ObjectParameter returnPrima = new ObjectParameter("returnPrima", typeof(int));

            db.LB_GetMembershipType(id, returnIdoc, returnPrima);

            var scalarIdoc = (int)returnIdoc.Value;
            var scalarPrima = (int)returnPrima.Value;

            model.IsIdoc = scalarIdoc;
            model.IsPrima = scalarPrima;

            return PartialView("_ProgramChange", model);

        }

        // EDIT : POST
        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices,MemberServicesManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProgramChange([Bind(Include = "CustID,AutoshipID,ItemID")] MemberProgramViewModel model)
        {
            ViewBag.ErrorCode = 0;

            if (ModelState.IsValid)
            {
                ViewBag.CustID = model.CustID;

                ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string)); 

                db.LB_UpdateAutoshipChangeItem(
                        model.CustID,
                        model.AutoshipID,
                        model.ItemID,
                        0,
                        returnID, returnMessage
                        );

                var scalarCustID = (int)returnID.Value;

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", "There was an error updating the membership." + returnMessage);
                    IEnumerable<System.Web.Mvc.SelectListItem> programList = DataHelper.GetProgramList(model.CustID, true, false);
                    model.ProgramList = programList;
                    IEnumerable<System.Web.Mvc.SelectListItem> promotionList = GetItemPromotionProgramList(model.CustID);
                    model.PromotionList = promotionList;

                    ObjectParameter returnIdoc = new ObjectParameter("returnIdoc", typeof(int));
                    ObjectParameter returnPrima = new ObjectParameter("returnPrima", typeof(int));

                    db.LB_GetMembershipType(model.CustID, returnIdoc, returnPrima);

                    var scalarIdoc = (int)returnIdoc.Value;
                    var scalarPrima = (int)returnPrima.Value;

                    model.IsIdoc = scalarIdoc;
                    model.IsPrima = scalarPrima;

                    return PartialView("_ProgramChange", model);

                }

                var oldAutoShipID = model.AutoshipID;
                var AutoshipBasicRecord = db.Database.SqlQuery<AutoshipBasic>("exec dbo.[LB_GetAutoshipHeaderByCustID] @CustID",
                 new SqlParameter("@CustID", model.CustID)
                 ).FirstOrDefault();

                if (AutoshipBasicRecord != null)
                {
                    model.AutoshipID = AutoshipBasicRecord.AutoshipID;
                }
                else
                {
                    model.AutoshipID = oldAutoShipID;
                }

                var C_InfoRow = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", model.CustID)
                 ).FirstOrDefault();

                string customerDisplayName = C_InfoRow.DisplayName;

                int emailId = Constants.emailIDNewSelectMemberNotification;
                var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                new SqlParameter("@ID", emailId)
                ).FirstOrDefault();
                string subject = emailRow.Subject;
                string body = emailRow.Content;
                string sCustID = model.CustID.ToString();
                body = body.Replace("#CUSTID#", sCustID);
                body = body.Replace("#STARTDATE#", DateTime.Now.ToShortDateString());
                body = body.Replace("#DISPLAYNAME#", customerDisplayName);
                body = Constants.emailTemplate.Replace("#BODYCOPY#", body);
                body = body.Replace("#SITEURL#", Constants.siteURL);
                SmtpClient client = new SmtpClient();
                MailMessage mailMessageNotification = new MailMessage();
                MailAddress FromAddressNotification = new MailAddress(Constants.adminEmailFrom, "AppTrack Member Services");
                mailMessageNotification.From = FromAddressNotification;
                mailMessageNotification.To.Add(Constants.notificationEmailTo);
                mailMessageNotification.Subject = subject;
                mailMessageNotification.Body = body;
                mailMessageNotification.IsBodyHtml = true;
                client.Send(mailMessageNotification);

                ViewBag.ErrorMessage = "Program and Promotion information updated successfully";
            }
            
            return RedirectToAction("ProfileUpdate", new { id = model.AutoshipID });
            // return RedirectToAction("ProgramChange", new { id = model.CustID });
            // return RedirectToAction("AccountProfile", new { id = model.CustID });
        }

        // EDIT : POST
        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices,MemberServicesManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProgramAdd([Bind(Include = "CustID,AutoshipID,ItemID")] MemberProgramViewModel model)
        {
            ViewBag.ErrorCode = 0;

            if (ModelState.IsValid)
            {
                ViewBag.CustID = model.CustID;
                ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_UpdateAutoShipAddItem(
                        model.CustID,
                        model.AutoshipID,
                        model.ItemID,
                        returnID, returnMessage
                        );

                var scalarCustID = (int)returnID.Value;

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", "There was an error updating the membership." + returnMessage.Value);
                    IEnumerable<System.Web.Mvc.SelectListItem> programList = DataHelper.GetProgramList(model.CustID, true, false);
                    model.ProgramList = programList;
                    IEnumerable<System.Web.Mvc.SelectListItem> promotionList = GetItemPromotionProgramList(model.CustID);
                    model.PromotionList = promotionList;

                    ObjectParameter returnIdoc = new ObjectParameter("returnIdoc", typeof(int));
                    ObjectParameter returnPrima = new ObjectParameter("returnPrima", typeof(int));

                    db.LB_GetMembershipType(model.CustID, returnIdoc, returnPrima);

                    var scalarIdoc = (int)returnIdoc.Value;
                    var scalarPrima = (int)returnPrima.Value;

                    model.IsIdoc = scalarIdoc;
                    model.IsPrima = scalarPrima;

                    return PartialView("_ProgramChange", model);
                }

                var oldAutoShipID = model.AutoshipID;
                var AutoshipBasicRecord = db.Database.SqlQuery<AutoshipBasic>("exec dbo.[LB_GetAutoshipHeaderByCustID] @CustID",
                 new SqlParameter("@CustID", model.CustID)
                 ).FirstOrDefault();

                if (AutoshipBasicRecord != null)
                {
                    model.AutoshipID = AutoshipBasicRecord.AutoshipID;
                }
                else
                {
                    model.AutoshipID = oldAutoShipID;
                }

                var C_InfoRow = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", model.CustID)
                 ).FirstOrDefault();

                string customerDisplayName = C_InfoRow.DisplayName;

                var C_ItemRow = db.Database.SqlQuery<C_Item>("exec dbo.[LB_GetItemByID] @ItemID",
                 new SqlParameter("@ItemID", model.ItemID)
                 ).FirstOrDefault();

                string itemTitle = C_ItemRow.ItemTitle;

                int emailId = Constants.emailIDMembershipAddNotification;
                var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                new SqlParameter("@ID", emailId)
                ).FirstOrDefault();
                string subject = emailRow.Subject;
                string body = emailRow.Content;
                string sCustID = model.CustID.ToString();
                body = body.Replace("#CUSTID#", sCustID);
                body = body.Replace("#STARTDATE#", DateTime.Now.ToShortDateString());
                body = body.Replace("#DISPLAYNAME#", customerDisplayName);
                body = body.Replace("#MEMBERSHIPTYPE#", itemTitle);
                body = Constants.emailTemplate.Replace("#BODYCOPY#", body);
                body = body.Replace("#SITEURL#", Constants.siteURL);
                SmtpClient client = new SmtpClient();
                MailMessage mailMessageNotification = new MailMessage();
                MailAddress FromAddressNotification = new MailAddress(Constants.adminEmailFrom, "AppTrack Member Services");
                mailMessageNotification.From = FromAddressNotification;
                mailMessageNotification.To.Add(Constants.notificationEmailTo);
                mailMessageNotification.Subject = subject;
                mailMessageNotification.Body = body;
                mailMessageNotification.IsBodyHtml = true;
                client.Send(mailMessageNotification);

                ViewBag.ErrorMessage = "Membership updated successfully";
            }

            return RedirectToAction("ProfileUpdate", new { id = model.AutoshipID });
            // return RedirectToAction("ProgramChange", new { id = model.CustID });
            // return RedirectToAction("AccountProfile", new { id = model.CustID });
        }

        // EDIT : POST
        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices,MemberServicesManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProgramCancel([Bind(Include = "CustID,AutoshipID,StoreID")] MemberProgramViewModel model)
        {
            ViewBag.ErrorCode = 0;

            if (ModelState.IsValid)
            {
                ViewBag.CustID = model.CustID;
                ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_UpdateAutoshipCancelItemsByStoreID(
                        model.CustID,
                        model.AutoshipID,
                        model.StoreID,
                        returnID, returnMessage
                        );

                var scalarCustID = (int)returnID.Value;

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", "There was an error updating the membership." + returnMessage.Value);
                    IEnumerable<System.Web.Mvc.SelectListItem> programList = DataHelper.GetProgramList(model.CustID, true, false);
                    model.ProgramList = programList;
                    IEnumerable<System.Web.Mvc.SelectListItem> promotionList = GetItemPromotionProgramList(model.CustID);
                    model.PromotionList = promotionList;

                    ObjectParameter returnIdoc = new ObjectParameter("returnIdoc", typeof(int));
                    ObjectParameter returnPrima = new ObjectParameter("returnPrima", typeof(int));

                    db.LB_GetMembershipType(model.CustID, returnIdoc, returnPrima);

                    var scalarIdoc = (int)returnIdoc.Value;
                    var scalarPrima = (int)returnPrima.Value;

                    model.IsIdoc = scalarIdoc;
                    model.IsPrima = scalarPrima;

                    return PartialView("_ProgramChange", model);

                }

                var oldAutoShipID = model.AutoshipID;
                var AutoshipBasicRecord = db.Database.SqlQuery<AutoshipBasic>("exec dbo.[LB_GetAutoshipHeaderByCustID] @CustID",
                 new SqlParameter("@CustID", model.CustID)
                 ).FirstOrDefault();

                if (AutoshipBasicRecord != null)
                {
                    model.AutoshipID = AutoshipBasicRecord.AutoshipID;
                }
                else
                {
                    model.AutoshipID = oldAutoShipID;
                }

                var C_InfoRow = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", model.CustID)
                 ).FirstOrDefault();

                string customerDisplayName = C_InfoRow.DisplayName;

                string body = "";

                if(model.StoreID == Constants.AppTrackStoreID)
                {
                    body = "AppTrack Membership was cancelled for Member : #DISPLAYNAME# ID: #CUSTID#"; 
                }
                else
                {
                    body = "PRIMA Membership was cancelled for Member : #DISPLAYNAME# ID: #CUSTID#"; 

                }
                int emailId = Constants.emailIDNewSelectMemberNotification;
                var emailRow = db.Database.SqlQuery<C_EmailTemplates>("exec dbo.[LB_GetEmailTemplate]  @ID",
                new SqlParameter("@ID", emailId)
                ).FirstOrDefault();
                string subject = "AppTrack Membership Cancellation";
                string sCustID = model.CustID.ToString();
                body = body.Replace("#CUSTID#", sCustID);
                body = body.Replace("#STARTDATE#", DateTime.Now.ToShortDateString());
                body = body.Replace("#DISPLAYNAME#", customerDisplayName);
                body = Constants.emailTemplate.Replace("#BODYCOPY#", body);
                body = body.Replace("#SITEURL#", Constants.siteURL);
                SmtpClient client = new SmtpClient();
                MailMessage mailMessageNotification = new MailMessage();
                MailAddress FromAddressNotification = new MailAddress(Constants.adminEmailFrom, "AppTrack Member Services");
                mailMessageNotification.From = FromAddressNotification;
                mailMessageNotification.To.Add(Constants.notificationEmailTo);
                mailMessageNotification.Subject = subject;
                mailMessageNotification.Body = body;
                mailMessageNotification.IsBodyHtml = true;
                client.Send(mailMessageNotification);

                ViewBag.ErrorMessage = "Membership updated successfully";
            }

            return RedirectToAction("ProfileUpdate", new { id = model.AutoshipID });
            // return RedirectToAction("ProgramChange", new { id = model.CustID });
            // return RedirectToAction("AccountProfile", new { id = model.CustID });
        }


        // This utility is not current used to day.  This will need to be modified to get promotions associated with an item in the future
        public IEnumerable<SelectListItem> GetItemPromotionProgramList(int CustID = 0)
        {
            List<MemberProgramItemPromotion> memberProgramItemPromotion = new List<MemberProgramItemPromotion>();
            memberProgramItemPromotion = db.Database.SqlQuery<MemberProgramItemPromotion>("exec dbo.[LB_GetItemPromotionProgramChange] @CustID",
              new SqlParameter("@CustID", CustID)
            ).ToList();

            var optionalItemPromotion = new MemberProgramItemPromotion
            {
                CustID = CustID,
                ID = 0,
                ItemID = 0,
                PromotionID = 0,
                PromotionTitle = "(optional)"
            };

            memberProgramItemPromotion.Insert(0, optionalItemPromotion);
            var memberProgramItemPromotionList = memberProgramItemPromotion
            .Select(item => new SelectListItem
            {
                Value = item.ID.ToString(),
                Text = item.PromotionTitle
            });

            return new SelectList(memberProgramItemPromotionList, "Value", "Text");
        }


        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpGet]
        public ActionResult RebatePayee(int? id = 0)
        {

            var model = new MemberRebatePayeeViewModel();

            var rebatePayeeRow = db.Database.SqlQuery<RebatePayee>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", id)
                 ).FirstOrDefault();

            if (rebatePayeeRow != null)
            {
                model.CustID = rebatePayeeRow.CustID;
                model.ShipName = rebatePayeeRow.ShipName;
                model.ShipAddress1 = rebatePayeeRow.ShipAddress1;
                model.ShipAddress2 = rebatePayeeRow.ShipAddress2;
                model.ShipCity = rebatePayeeRow.ShipCity;
                model.ShipState = rebatePayeeRow.ShipState;
                model.ShipPostalCode = rebatePayeeRow.ShipPostalCode;
                model.ShipEmail = rebatePayeeRow.ShipEmail;
                model.ShipPhone = rebatePayeeRow.ShipPhone;
                model.ShipFirstName = rebatePayeeRow.ShipFirstName;
                model.ShipLastName = rebatePayeeRow.ShipLastName;
            }
            else
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Member ID supplied to Edit page";
                return PartialView("_RebatePayee", model);
            }

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;

            return PartialView("_RebatePayee", model);

        }

        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        // EDIT : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RebatePayee([Bind(Include = "CustID,ShipName, ShipFirstName,ShipLastName,ShipAddress1, ShipAddress2, ShipCity, ShipState, ShipPostalCode, ShipPhone, ShipEmail, PayoutCustID")] MemberRebatePayeeViewModel model)
        {
            ViewBag.ErrorCode = 0;

            if (ModelState.IsValid)
            {
                ViewBag.CustID = model.CustID;

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
            string errorMessage = "Payee information was updated successfully";
            ModelState.AddModelError("", errorMessage);
            ViewBag.ErrorMessage = errorMessage;

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;

            return PartialView("_RebatePayee", model);
            //return RedirectToAction("_RebatePayee", new { id = model.CustID });
        }


        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        // EDITACCOUNT : GET
        public ActionResult EditAccount(int CustID = 0)
        {

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.memberCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Member ID supplied to Edit page";
                return PartialView("_EditAccount");
            }
            else
            {
                ViewBag.errorCode = 0;

                var model = new MemberViewModel();

                model = db.Database.SqlQuery<MemberViewModel>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).FirstOrDefault();

                var memberInfoDetail = db.Database.SqlQuery<MemberInfoDetail>("exec dbo.[LBGetC_InfoDetailMemberData] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).FirstOrDefault();

                model.EnrollmentReason = memberInfoDetail.EnrollmentReason;
                model.PracticeSoftware = memberInfoDetail.PracticeSoftware;
                model.PracticeSize = memberInfoDetail.PracticeSize;               

                int TreeID = 60;

                var thisTreeObj = db.Database.SqlQuery<int?>("exec dbo.[LB_GetUpline] @CustID, @TreeID",
                 new SqlParameter("@CustID", CustID),
                 new SqlParameter("@TreeID", TreeID)
                 ).FirstOrDefault();

                model.UplineID = thisTreeObj ?? 0;
                
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

                IEnumerable<System.Web.Mvc.SelectListItem> iMDList = DataHelper.GetCustIDList(Constants.memberDirectorCustomerType);
                model.IMDList = iMDList;
                IEnumerable<System.Web.Mvc.SelectListItem> repList = DataHelper.GetCustIDList(Constants.salesRepCustomerType);
                model.RepList = repList;
                IEnumerable<System.Web.Mvc.SelectListItem> accountManagerList = DataHelper.GetAccountManagerSelectList(false, false);
                model.AccountManagerList = accountManagerList;


                return PartialView("_EditAccount", model);
            }
        }

        // EDIT : POST
        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAccount([Bind(Include = "CustID,TaxID, Company, DisplayName, NameTitle,FirstName,LastName,Address1, Address2, City, State, PostalCode, CompanyPhone, DayPhone, Mobile, Fax, Email,StartDate, Status, ActivationStatus, ActivationStatusDate, VariantData2, VariantData3, EnrollmentReason, PracticeSoftware, PracticeSize, Flag1, SponsorID, SecID, SecSponsorID,  SourceID, UplineID, SourceCode, AccountingID, SalesForceID, EmailFlag, TextFlag,SiteName")] MemberViewModel model)
        {
            ViewBag.ErrorCode = 0;

            if (ModelState.IsValid)
            {
                ViewBag.CustID = model.CustID;

                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(model.CustID, Constants.memberCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ViewBag.ErrorCode = Constants.fatalErrorCode;
                    ViewBag.ErrorMessage = "Invalid Member ID supplied to Edit page";
                    return PartialView("_EditAccount", model);
                }
                else
                {
                    model.CompanyPhone = DataHelper.FixPhone(model.CompanyPhone);
                    model.DayPhone = DataHelper.FixPhone(model.DayPhone);
                    model.Fax = DataHelper.FixPhone(model.Fax);

                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateMemberEditAccount(
                        Constants.memberCustomerType,
                        model.CustID,
                        model.TaxID,
                        model.Company,
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
                        model.StartDate,
                        model.ActivationStatus,
                        model.ActivationStatusDate,
                        model.Status,
                        model.VariantData2,
                        model.VariantData3,
                        model.EnrollmentReason,
                        model.PracticeSoftware,
                        model.PracticeSize,
                        model.Flag1,
                        model.SponsorID,
                        model.SecID,
                        model.SecSponsorID,
                        model.SourceID,
                        model.SourceCode,
                        model.AccountingID,
                        model.SalesForceID,
                        model.EmailFlag,
                        model.TextFlag,
                        model.SiteName,
                        returnCustID, returnMessage);

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        db.LB_UpdateC_TreeUplineID(
                            model.CustID,
                            model.UplineID,
                            60);

                            ViewBag.ErrorMessage = "Member  information was updated successfully";
                            return RedirectToAction("AccountProfile", new { id = model.CustID });
                    }
                }
            }
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

            IEnumerable<System.Web.Mvc.SelectListItem> iMDList = DataHelper.GetCustIDList(Constants.memberDirectorCustomerType);
            model.IMDList = iMDList;
            IEnumerable<System.Web.Mvc.SelectListItem> repList = DataHelper.GetCustIDList(Constants.salesRepCustomerType);
            model.RepList = repList;
            IEnumerable<System.Web.Mvc.SelectListItem> accountManagerList = DataHelper.GetAccountManagerSelectList(false, false);
            model.AccountManagerList = accountManagerList;

            return PartialView("_EditAccount", model);
        }

        // EDITACCOUNT : GET
        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpGet]
        public ActionResult LocationEditAccount(int CustID = 0)
        {

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.locationCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Location ID supplied to Edit page";
                return PartialView("_LocationEditAccount");
            }
            else
            {
                ViewBag.errorCode = 0;

                var model = new LocationViewModel();

                model = db.Database.SqlQuery<LocationViewModel>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).FirstOrDefault();

                IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                model.StateList = stateList;
                IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
                model.NameTitleList = nameTitleList;

                return PartialView("_LocationEditAccount", model);
            }
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LocationEditAccount([Bind(Include = "CustID,ParentID,TaxID,Company,DisplayName,NameTitle,FirstName,LastName,Address1,Address2,City,State,PostalCode,CompanyPhone,DayPhone,Fax,Email,StartDate,Status,SecID,AccountingID,SalesForceID,EmailFlag")] LocationViewModel model)
        {
            ViewBag.ErrorCode = 0;

            if (ModelState.IsValid)
            {
                ViewBag.CustID = model.CustID;

                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(model.CustID, Constants.locationCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ViewBag.ErrorCode = Constants.fatalErrorCode;
                    ViewBag.ErrorMessage = "Invalid Member Location ID supplied to Edit page";
                    return PartialView("_LocationEditAccount", model);
                }
                else
                {

                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateLocationEditAccount(
                        Constants.locationCustomerType,
                        model.CustID,
                        model.TaxID,
                        model.Company,
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
                        model.DayPhone,
                        model.Fax,
                        model.Email,
                        model.StartDate,
                        model.Status,
                        0,
                        model.SecID,
                        0,
                        model.AccountingID,
                        model.SalesForceID,
                        model.EmailFlag,
                        0,
                        returnCustID, returnMessage);

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        ViewBag.ErrorMessage = errorMessage;
                    }
                    else 
                    {
                        ViewBag.ErrorMessage = "Location information was updated successfully";
                    }
                }
            }
            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
            model.StateList = stateList;
            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
            model.NameTitleList = nameTitleList;
            return PartialView("_LocationEditAccount", model);
        }

        // EDITACCOUNT : GET
        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpGet]
        public ActionResult LocationCreate(int CustID = 0)
        {

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.memberCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Member ID supplied to Edit page";
                return PartialView("_LocationCreate");
            }
            else
            {
                ViewBag.errorCode = 0;

                var model = new LocationViewModel();

                model.ParentID = CustID;
                model.DisplayName = DisplayName;
                model.StartDate = DateTime.Now;

                IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();
                model.StateList = stateList;
                IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();
                model.NameTitleList = nameTitleList;

                return PartialView("_LocationCreate", model);
            }
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LocationCreate([Bind(Include = "ParentID,Company,TaxID,StartDate,DisplayName,CompanyPhone,Fax,Address1,Address2,City,State,PostalCode,NameTitle,FirstName,LastName,DayPhone,Email,AccountingID,SalesForceID,EmailFlag,SecID")] LocationViewModel model)
        {

            if (ModelState.IsValid)
            {
                ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_InsertLocationV2(66,
                    model.DisplayName, "",
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    model.Address1,
                    model.Address2,
                    model.City,
                    model.State,
                    model.PostalCode,
                    model.CompanyPhone,
                    model.DayPhone,
                    model.Mobile,
                    model.Fax,
                    model.Company,
                    model.TaxID,
                    0,
                    0,
                    model.ParentID,
                    0,
                    0,
                    model.AccountingID,
                    0,
                    model.SalesForceID,
                    returnCustID, returnMessage);

                var scalarCustID = (int)returnCustID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                    return PartialView("_LocationCreate", model);
                }
                else
                {
                    return RedirectToAction("LocationList", new { id = model.ParentID });
                }
            }
            else
            {
                return PartialView("_LocationCreate", model);
            }
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelLocation([Bind(Include = "CustID, ParentID, StatusID, Status, cancelReasonCode, Comments")] LocationProfileViewModel model)
        {
            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(model.CustID, Constants.locationCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Location ID supplied to Cancel action";
                return RedirectToAction("Error", "Admin");
            }
            else
            {
                ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_CancelLocation(
                    model.CustID,
                    AdminID,
                    returnCustID, returnMessage);

                var scalarCustID = (int)returnCustID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID == -1)
                {
                    TempData["ErrorMessage"] = errorMessage;
                }
            }
            return RedirectToAction("LocationProfileMain", new { id = model.CustID });
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance,MemberServices")]
        [HttpGet]
        public ActionResult RebateCommissionList(int id = 0)
        {
            int searchCustID = id;

            if (searchCustID > 0)
            {
                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(searchCustID);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ViewBag.ErrorCode = Constants.fatalErrorCode;
                    ViewBag.ErrorMessage = "Invalid Customer ID provided, unable to retrieve Commission List";
                    return PartialView("_CommissionList");
                }

                ViewBag.DisplayName = checkCustomerResult.C_Info.DisplayName;
            }

            // Initalize the View Model

            var model = new CommissionHeaderListViewModel();
            int periodTypeID = Constants.rebateCommissionPeriodType;

            int startPeriodID = 0;
            int endPeriodID = 0;

            int searchCommissionID = Constants.memberRebateCommissionID;
            string searchStatus = " ";

/*          
            model.CommissionDetailList = db.Database.SqlQuery<CommissionDetail>("exec dbo.[LB_GetCommissionDetailByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
             new SqlParameter("@CustID", searchCustID),
             new SqlParameter("@PeriodTypeID", periodTypeID),
             new SqlParameter("@StartPeriodID", startPeriodID),
             new SqlParameter("@EndPeriodID", endPeriodID),
             new SqlParameter("@CommissionID", searchCommissionID),
             new SqlParameter("@Status", searchStatus)
            ).ToList();
*/

            model.CommissionHeaderList = db.Database.SqlQuery<CommissionHeader>("exec dbo.[LB_GetCommissionHeaderByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
             new SqlParameter("@CustID", searchCustID),
             new SqlParameter("@PeriodTypeID", periodTypeID),
             new SqlParameter("@StartPeriodID", startPeriodID),
             new SqlParameter("@EndPeriodID", endPeriodID),
             new SqlParameter("@CommissionID", searchCommissionID),
             new SqlParameter("@Status", searchStatus)
            ).ToList();

            model.StartPeriodID = startPeriodID;
            model.EndPeriodID = endPeriodID;
            model.SearchCustID = searchCustID;
            model.SearchCommissionID = searchCommissionID;
            model.SearchStatus = searchStatus;

            model.SearchPeriodIDList = DataHelper.GetPeriodIDList(Constants.quarterlyPeriodTypeID, 12, false, true);

            model.SearchStatusList = DataHelper.GetStatusSelectList(Constants.commissionStatusLookupGroupID, false, true);

            ViewBag.SearchDisplay = "none";

            return PartialView("_CommissionList", model);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance,MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RebateCommissionList([Bind(Include = "StartPeriodID, EndPeriodID, SearchCustID, SearchCommissionID, SearchStatus")] CommissionHeaderListViewModel model)
        {
            int periodTypeID = Constants.rebateCommissionPeriodType;
            int searchCommissionID = Constants.memberRebateCommissionID;


/*            model.CommissionDetailList = db.Database.SqlQuery<CommissionDetail>("exec dbo.[LB_GetCommissionDetailByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
             new SqlParameter("@CustID", model.SearchCustID),
             new SqlParameter("@PeriodTypeID", periodTypeID),
             new SqlParameter("@StartPeriodID", model.StartPeriodID),
             new SqlParameter("@EndPeriodID", model.EndPeriodID),
             new SqlParameter("@CommissionID", model.SearchCommissionID),
             new SqlParameter("@Status", model.SearchStatus)
            ).ToList();
 */

            model.CommissionHeaderList = db.Database.SqlQuery<CommissionHeader>("exec dbo.[LB_GetCommissionHeaderByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
            new SqlParameter("@CustID", model.SearchCustID),
            new SqlParameter("@PeriodTypeID", periodTypeID),
            new SqlParameter("@StartPeriodID", model.StartPeriodID),
            new SqlParameter("@EndPeriodID", model.EndPeriodID),
            new SqlParameter("@CommissionID", searchCommissionID),
            new SqlParameter("@Status", model.SearchStatus)
            ).ToList();
            
            model.SearchPeriodIDList = DataHelper.GetPeriodIDList(Constants.quarterlyPeriodTypeID, 12, false, true);

            model.SearchStatusList = DataHelper.GetStatusSelectList(Constants.commissionStatusLookupGroupID, false, true);

            ViewBag.SearchDisplay = "block";

            return PartialView("_CommissionList", model);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance,MemberServices")]
        [HttpGet]
        public ActionResult RebateVolumeDetailList(int id = 0)
        {
            int CHID = id;

            if (CHID == 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Customer ID provided, unable to retrieve Commission List";
                return PartialView("_RebateVolumeDetailListWithHeader");
            }

            // Initalize the View Model

            VolumeDetailListViewModel volumeDetailListViewModel = new VolumeDetailListViewModel();

            volumeDetailListViewModel.VolumeDetailList = db.Database.SqlQuery<VolumeDetail>("exec dbo.[LB_GetVolumeDetailRebateByCHID] @CHID",
             new SqlParameter("@CHID", CHID)
            ).ToList();

            return PartialView("_RebateVolumeDetailListWithHeader", volumeDetailListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance,MemberServices")]
        [HttpGet]
        public ActionResult Review(int id = 0)
        {
            var memberReviewRow = db.Database.SqlQuery<MemberReview>("exec dbo.[LB_GetMemberReviewByCustID] @CustID",
            new SqlParameter("@CustID", id)).FirstOrDefault();
            var model = new MemberReviewViewModel();

            if (memberReviewRow == null)
            {
                model.CustID = id;
                model.Status = "New";
                model.ReviewReasonList = DataHelper.GetAccountReviewReasonCodesSelectList();
                return PartialView("_Review", model);
            }
            else { 
                model.ID = memberReviewRow.ID;
                model.CustID = memberReviewRow.CustID;
                model.Status = memberReviewRow.Status;
                model.ReviewReason = memberReviewRow.ReviewReason;
                model.Description = memberReviewRow.Description;
                model.ChangeAlliance = memberReviewRow.ChangeAlliance;
                if (model.Status == "New")
                {
                    model.ReviewStatusList = DataHelper.GetReviewStatusList();
                }
                else
                {
                    model.ReviewStatusList = DataHelper.GetReview2StatusList();
                }
                model.ReviewReasonList = DataHelper.GetAccountReviewReasonCodesSelectList();
                model.ReviewOutcomeList = DataHelper.GetReviewOutcomeList();
                model.ChangeAllianceList = DataHelper.GetOtherAlliancesSelectList();

                return PartialView("_ReviewStep2", model);
            }
        }


        [AuthorizeAdminRedirect(Roles = "Accounting,Finance,MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Review([Bind(Include = "CustID, ReviewReason, Description")] MemberReviewViewModel thismodel)
        {
            int thisCustID = thismodel.CustID;
            string thisReviewReason = thismodel.ReviewReason;
            string thisDescription = thismodel.Description;

            db.LB_AccountReviewStep1(thismodel.CustID,
               thismodel.ReviewReason,
               thismodel.Description,
               AdminID);


//            var memberReviewRow = db.Database.SqlQuery<MemberReview>("exec dbo.[LB_GetMemberReviewByCustID] @CustID",
//            new SqlParameter("@CustID", CustID)).FirstOrDefault();
//            var model = new MemberReviewViewModel();

           thismodel.ID = thisCustID;
           thismodel.Status = "New";
           thismodel.ReviewReasonList = DataHelper.GetAccountReviewReasonCodesSelectList();
           thismodel.ReviewStatusList = DataHelper.GetReviewStatusList();
           thismodel.ReviewReasonList = DataHelper.GetAccountReviewReasonCodesSelectList();
           thismodel.ReviewOutcomeList = DataHelper.GetReviewOutcomeList();
           thismodel.ChangeAllianceList = DataHelper.GetOtherAlliancesSelectList();

           return PartialView("_ReviewStep2", thismodel);
        }

        [AuthorizeAdminRedirect(Roles = "Accounting,Finance,MemberServices")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Review2([Bind(Include = "ID, CustID, Status, ReviewReason, Description, Outcome, OutcomeReasonCode, ChangeAlliance, OutcomeDescription ")] MemberReviewViewModel model)
        {
            int thisCustID = model.CustID;
            int thisID = model.ID;
            string thisReviewStatus = model.Status;
            string thisReviewReason = model.ReviewReason;
            string thisDescription = model.Description;
            string thisOutcome = model.Outcome;
            string thisOutcomeReasonCode = model.OutcomeReasonCode;
            string thisChangeAlliance = model.ChangeAlliance ;
            string thisOutcomeDescription = model.OutcomeDescription ;
            if (thisOutcome != null)
            {
                thisReviewStatus = "Completed";
            }

            db.LB_AccountReviewStep2(thisCustID,
               thisReviewStatus, 
               thisReviewReason ,
               thisDescription,
               thisChangeAlliance,
               1,
               AdminID);

//            var memberReviewRow = db.Database.SqlQuery<MemberReview>("exec dbo.[LB_GetMemberReviewByCustID] @CustID",
//            new SqlParameter("@CustID", CustID)).FirstOrDefault();
//            var model = new MemberReviewViewModel();

            if (thisOutcome == null){

                if (model.Status == "New")
                {
                    model.ReviewStatusList = DataHelper.GetReviewStatusList();
                }
                else
                {
                    model.ReviewStatusList = DataHelper.GetReview2StatusList();
                }
                model.ReviewReasonList = DataHelper.GetAccountReviewReasonCodesSelectList();
                model.ReviewReasonList = DataHelper.GetAccountReviewReasonCodesSelectList();
                model.ReviewOutcomeList = DataHelper.GetReviewOutcomeList();
                model.ChangeAllianceList = DataHelper.GetOtherAlliancesSelectList();

                return RedirectToAction("AccountProfile", new { id = thisCustID });

            }
            else
            {

            DateTime thisOutcomeDate = DateTime.Now;
           
            db.LB_AccountReviewStep3(thisCustID,
               thisOutcome,
               thisOutcomeDate,
               thisOutcomeReasonCode,
               thisOutcomeDescription,
               AdminID);
            }
            return RedirectToAction("AccountProfile", new { id = thisCustID });
        }

        // CREATE : GET
        [HttpGet]
        public ActionResult CreateUser()
        {
            var memberRows = db.Database.SqlQuery<AppTrack.SharedModels.Member>("exec dbo.[LB_GetCustWithoutUserInfo] @CustomerType",
             new SqlParameter("@CustomerType", Constants.memberCustomerType)
             ).ToList();

            var memberListViewModel = new MemberListViewModel
            {
                MemberList = memberRows
            };

            return View(memberListViewModel);

        }

        // CREATE : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(MemberListViewModel memberListViewModel)
        {
            var DataHelper = new DataHelpers();

            var memberRows = db.Database.SqlQuery<AppTrack.SharedModels.Member>("exec dbo.[LB_GetCustWithoutUserInfo] @CustomerType",
             new SqlParameter("@CustomerType", Constants.memberCustomerType)
             ).ToList();

            memberListViewModel.MemberList = memberRows;

            string password = "";

            if (ModelState.IsValid)
            {

                foreach (AppTrack.SharedModels.Member Member in memberListViewModel.MemberList)
                {
                    password = Member.Email;
                    // Insert Member into Identity login tables
                    try
                    {
                        var user = new ApplicationUser { UserName = Member.Email, Email = Member.Email, DisplayName = Member.DisplayName, CustID = Member.CustID };
                        IdentityResult userResult = UserManager.Create(user, password);
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
                                ModelState.AddModelError("", "Error adding Role for " + Member.CustID);
                            }
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Error adding User" + Member.CustID);
                    }
                }

            }

            if (ModelState.IsValid)
            {

                memberRows = db.Database.SqlQuery<AppTrack.SharedModels.Member>("exec dbo.[LB_GetCustWithoutUserInfo] @CustomerType",
                 new SqlParameter("@CustomerType", Constants.memberCustomerType)
                 ).ToList();

                memberListViewModel.MemberList = memberRows;
            }

            return View(memberListViewModel);
        }

        [HttpGet]
        public ActionResult MemberSite(int id = 0)
        {
            // create a cookie
            string sCustID = id.ToString();

            if (id == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Member  ID supplied to Account Profile page";
                return RedirectToAction("Error", "Admin");
            }
            else
            {
                // create a cookie
                HttpCookie newCookie = new HttpCookie("CustID", sCustID);
                newCookie.Expires = DateTime.Now.AddHours(4);
                Response.Cookies.Add(newCookie);
                return RedirectToAction("Index", "SiteMember");
            }                
        }
        [HttpGet]
        public ActionResult IMDSite(int id = 0)
        {
            // create a cookie
            string sCustID = id.ToString();

            if (id == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Member  ID supplied to Account Profile page";
                return RedirectToAction("Error", "Admin");
            }
            else
            {
                // create a cookie
                HttpCookie newCookie = new HttpCookie("CustID", sCustID);
                newCookie.Expires = DateTime.Now.AddHours(4);
                Response.Cookies.Add(newCookie);
                return RedirectToAction("Index", "SiteIMD");
            }
        }

        [HttpGet]
        public ActionResult LineChartView(int searchCustID = 0)
        {
            MembershipActivityViewModel model = new MembershipActivityViewModel();

            if (searchCustID == 0)
            {
                ModelState.AddModelError("", "Error finding Member with ID = " + searchCustID);
            }
            else
            {
                using (var db = new DevProvidentIDOCEntities())
                {
                    var membershipActivityList = db.Database.SqlQuery<MembershipActivityList>("exec dbo.[LB_GetMembershipActivityByCustID] @CustID",
                    new SqlParameter("@CustID", searchCustID)
                    ).ToList();

                    model.MembershipActivityList = membershipActivityList;
                }            
            }
            return View(model);

        }

        [HttpGet]
        public JsonResult LineChart2(int searchCustID = 0)
        {
            int zero = 0;
            DateTime now = DateTime.Now;

            using (var db = new DevProvidentIDOCEntities())
            {
                var model = db.Database.SqlQuery<MembershipActivityForChart>("exec dbo.[LB_GetMembershipActivityByCustIDForChart] @CustID",
                new SqlParameter("@CustID", searchCustID)
               ).ToList(); 

                if (model != null)
                {
                    List<MembershipActivityData> thisData = new List<MembershipActivityData>();
                    foreach (var line in model)
                    {
                        thisData.Add(new MembershipActivityData(line.SeriesName, line.PeriodName, line.StartDate, line.Count));
                    }
                    return Json(thisData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<MembershipActivityData> thisData = new List<MembershipActivityData>();
                    thisData.Add(new MembershipActivityData("", "", now, 0));

                    return Json(thisData, JsonRequestBehavior.AllowGet);
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


