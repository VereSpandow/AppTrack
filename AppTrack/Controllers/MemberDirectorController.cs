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
using System;

namespace AppTrack.Controllers
{
    [AuthorizeAdminRedirect(Roles = "SalesManager")]
    public class MemberDirectorController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public MemberDirectorController()
        {
        }

        public MemberDirectorController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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
            string searchCompanyName = "";
            string searchDisplayName = "";
            string selectedStatus = "Active";

            var memberDirectorRows = db.Database.SqlQuery<MemberDirector>("exec dbo.[LB_GetCustByNameStatusType] @DisplayName, @Company, @LastName, @Status, @CustomerType",
             new SqlParameter("@DisplayName", searchDisplayName),
             new SqlParameter("@Company", searchCompanyName),
             new SqlParameter("@LastName", ""),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@CustomerType", Constants.memberDirectorCustomerType)
             ).ToList();

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, true);

            var memberDirectorViewModel = new MemberDirectorListViewModel
            {
                MemberDirectorList = memberDirectorRows,
                SearchDisplayName = searchDisplayName,
                SelectedStatus = selectedStatus,
                StatusList = thisList
            };

            if (TempData["ErrorMessage"] != null)
            {
                string errMessage = TempData["ErrorMessage"].ToString();
                ModelState.AddModelError("", errMessage);
            }

            return View(memberDirectorViewModel);
        }

        // INDEX : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "SearchCompanyName, SearchDisplayName, SelectedStatus")] MemberDirectorListViewModel memberDirectorListViewModel)
        {
            string searchCompanyName = memberDirectorListViewModel.SearchCompanyName ?? "";
            string searchDisplayName = memberDirectorListViewModel.SearchDisplayName ?? "";
            string selectedStatus = memberDirectorListViewModel.SelectedStatus ?? "Active";

            var memberDirectorRows = db.Database.SqlQuery<MemberDirector>("exec dbo.[LB_GetCustByNameStatusType] @DisplayName, @Company, @LastName, @Status, @CustomerType",
             new SqlParameter("@DisplayName", searchDisplayName),
             new SqlParameter("@Company", searchCompanyName),
             new SqlParameter("@LastName", ""),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@CustomerType", Constants.memberDirectorCustomerType)
             ).ToList();

            memberDirectorListViewModel.MemberDirectorList = memberDirectorRows;

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, true);

            memberDirectorListViewModel.StatusList = thisList;

            return View(memberDirectorListViewModel);
        }

        //ACOUNTPROFILE : GET 
        public ActionResult AccountProfile(int id = 0)
        {
            int CustID = id;

            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.memberDirectorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Member Director ID supplied to Account Profile page";
                return RedirectToAction("Error", "Admin");
            }
            else
            {
                ViewBag.errorCode = 0;

                int zero = 0;

                var memberDirectorProfileViewModel = new MemberDirectorProfileViewModel();

                memberDirectorProfileViewModel.MemberDirectorRecord = db.Database.SqlQuery<MemberDirector>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).FirstOrDefault();

                int periodTypeID = Constants.monthlyPeriodTypeID;

                memberDirectorProfileViewModel.CommissionHeaderList = db.Database.SqlQuery<CommissionHeader>("exec dbo.[LB_GetCommissionHeaderByCustID] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @Status",
                 new SqlParameter("@CustID", CustID),
                 new SqlParameter("@PeriodTypeID", periodTypeID),
                 new SqlParameter("@StartPeriodID", zero),
                 new SqlParameter("@EndPeriodID", zero),
                 new SqlParameter("@CommissionID", zero),
                 new SqlParameter("@Status", " ")
                 ).ToList();


                DateTime meetingStartDate = DateTime.Now.AddDays(-60);
                DateTime meetingEndDate = DateTime.Now.AddDays(60);

                memberDirectorProfileViewModel.MeetingEventList = db.Database.SqlQuery<MeetingEvent>("exec dbo.[LB_GetEventHeader] @EventID, @CategoryID, @CustID, @SponsorName, @EventTitle, @StartDate, @EndDate, @LocationTitle, @City, @State, @Status, @StatusID",
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
                new SqlParameter("@StatusID", zero)
                ).ToList();

                DateTime memberStartDate = DateTime.Now.AddDays(-60);
                DateTime memberEndDate = DateTime.Now.AddDays(60);

                memberDirectorProfileViewModel.MemberList = db.Database.SqlQuery<CustomerBasic>("exec dbo.[LB_GetCustBySPonsorType] @CustomerType, @SponsorID, @StartDate, @EndDate",
                 new SqlParameter("@CustomerType", Constants.memberCustomerType),
                 new SqlParameter("@SponsorID", CustID),
                 new SqlParameter("@StartDate", memberStartDate),
                 new SqlParameter("@EndDate", memberEndDate)
                 ).ToList();

                return View(memberDirectorProfileViewModel);
            }
        }

        // CREATE : GET
        [HttpGet]
        public ActionResult Create()
        {
            var model = new MemberDirectorViewModel();

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();

            model.StateList = stateList;

            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();

            model.NameTitleList = nameTitleList;

            return View(model);

        }

        // CREATE : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SponsorID, TaxID, AccountingID, Company, DisplayName, NameTitle, FirstName, LastName, Address1, Address2, City, State, PostalCode, Email, Password, ConfirmPassword, DayPhone, Mobile, Fax")] MemberDirectorViewModel memberDirectorViewModel)
        {
            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();

            memberDirectorViewModel.StateList = stateList;

            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();

            memberDirectorViewModel.NameTitleList = nameTitleList;

            if (memberDirectorViewModel.SponsorID != null)
            {
                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(memberDirectorViewModel.SponsorID, Constants.memberCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ModelState.AddModelError("SponsorID", "Invalid Member ID");
                }
            }

            if (ModelState.IsValid)
            {
                memberDirectorViewModel.TaxID = memberDirectorViewModel.TaxID.Replace("-", "");
                memberDirectorViewModel.DayPhone = DataHelper.FixPhone(memberDirectorViewModel.DayPhone);
                memberDirectorViewModel.Mobile = DataHelper.FixPhone(memberDirectorViewModel.Mobile);
                memberDirectorViewModel.Fax = DataHelper.FixPhone(memberDirectorViewModel.Fax);
                
                ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_InsertMemberDirector(Constants.memberDirectorCustomerType, 
                    memberDirectorViewModel.SponsorID,
                    memberDirectorViewModel.Company,
                    memberDirectorViewModel.DisplayName,
                    memberDirectorViewModel.TaxID,
                    memberDirectorViewModel.AccountingID,
                    memberDirectorViewModel.NameTitle,
                    memberDirectorViewModel.FirstName,
                    memberDirectorViewModel.LastName,
                    memberDirectorViewModel.Address1,
                    memberDirectorViewModel.Address2,
                    memberDirectorViewModel.City,
                    memberDirectorViewModel.State,
                    memberDirectorViewModel.PostalCode,
                    memberDirectorViewModel.DayPhone,
                    memberDirectorViewModel.Mobile,
                    memberDirectorViewModel.Fax,
                    memberDirectorViewModel.Email, returnCustID, returnMessage);

                var scalarCustID = (int)returnCustID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    // Insert Member Director into Identity login tables
                    try
                    {
                        var user = new ApplicationUser { UserName = memberDirectorViewModel.Email, Email = memberDirectorViewModel.Email, DisplayName = memberDirectorViewModel.FirstName + " " + memberDirectorViewModel.LastName, CustID = scalarCustID };
                        IdentityResult userResult = UserManager.Create(user, memberDirectorViewModel.Password);
                        if (!userResult.Succeeded)
                        {
                            AddErrors(userResult);
                        }
                        else
                        {
                            try
                            {
                                IdentityResult roleResult = UserManager.AddToRoles(user.Id, "MemberDirector");
                                if (!roleResult.Succeeded)
                                {
                                    ModelState.AddModelError("", roleResult.Errors.First());
                                }
                            }
                            catch
                            {
                                ModelState.AddModelError("", "Error encountered adding Member Director Role");
                            }
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Error encountered adding Member Director Login");
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(memberDirectorViewModel);
                }
                return RedirectToAction("Index");
            }
            return View(memberDirectorViewModel);
        }

        // EDIT : GET
        public ActionResult Edit(int CustID = 0)
        {
            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.memberDirectorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Member Director ID supplied to Edit page";
                return PartialView("_Contact");
            }
            else
            {
                ViewBag.errorCode = 0;

                var memberDirectorViewModel = new MemberDirectorViewModel();

                memberDirectorViewModel = db.Database.SqlQuery<MemberDirectorViewModel>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).First();

                IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();

                memberDirectorViewModel.StateList = stateList;

                IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();

                memberDirectorViewModel.NameTitleList = nameTitleList;

                return PartialView("_Contact", memberDirectorViewModel);
            }
        }

        // EDIT : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustID,TaxID,Company,DisplayName,AccountingID,NameTitle,FirstName,LastName,Address1,Address2,City,State,PostalCode,DayPhone,Mobile,Fax,Email")] MemberDirectorViewModel memberDirectorViewModel)
        {
            // Need to remove any Required properties in the model that are not being supplied by form
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            ViewBag.ErrorCode = 0;

            var DataHelper = new DataHelpers();

            if (ModelState.IsValid)
            {
                ViewBag.CustID = memberDirectorViewModel.CustID;

                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(memberDirectorViewModel.CustID, Constants.memberDirectorCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ViewBag.ErrorCode = Constants.fatalErrorCode;
                    ViewBag.ErrorMessage = "Invalid Member Director ID supplied to Edit page";
                    return PartialView("_Contact", memberDirectorViewModel);
                }
                else
                {
                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateMemberDirector(Constants.memberDirectorCustomerType, memberDirectorViewModel.CustID,
                        memberDirectorViewModel.TaxID,
                        memberDirectorViewModel.Company,
                        memberDirectorViewModel.DisplayName,
                        memberDirectorViewModel.AccountingID,
                        memberDirectorViewModel.NameTitle,
                        memberDirectorViewModel.FirstName,
                        memberDirectorViewModel.LastName,
                        memberDirectorViewModel.Address1,
                        memberDirectorViewModel.Address2,
                        memberDirectorViewModel.City,
                        memberDirectorViewModel.State,
                        memberDirectorViewModel.PostalCode,
                        memberDirectorViewModel.DayPhone,
                        memberDirectorViewModel.Mobile,
                        memberDirectorViewModel.Fax,
                        memberDirectorViewModel.Email, returnCustID, returnMessage);

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Member Director information was updated successfully";
                    }
                }
            }

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();

            memberDirectorViewModel.StateList = stateList;

            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();

            memberDirectorViewModel.NameTitleList = nameTitleList;

            return PartialView("_Contact", memberDirectorViewModel);
        }

        // POST: Info/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int CustID = 0)
        {

            if (ModelState.IsValid)
            {
                var DataHelper = new DataHelpers();
            
                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.memberDirectorCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Member Director ID supplied to Delete action";
                    return RedirectToAction("Error", "Admin");
                }
                else
                {
                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_DeleteMemberDirector(CustID, AdminID, returnCustID, returnMessage);

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        TempData["ErrorMessage"] = errorMessage;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Member Director was successfully cancelled";
                        return RedirectToAction("Index");
                    }
                }
            }
            TempData["ErrMessage"] = "Member Director could not be Cancelled";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult MemberList(int CustID = 0)
        {
            string searchLastName = "";
            string selectedStatus = "Active";

            var memberRows = db.Database.SqlQuery<Member>("exec dbo.[LB_GetCustByLastNameStatusType]  @LastName, @Status, @CustomerType, @SponsorID",
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
            return PartialView("_MemberList", memberDirectorMemberListViewModel);
        }

        // INDEX : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberList([Bind(Include = "CustID, SearchDisplayName, SelectedStatus")] MemberDirectorMemberListViewModel memberDirectorMemberListViewModel)
        {
            string searchLastName = memberDirectorMemberListViewModel.SearchLastName ?? "";
            string selectedStatus = memberDirectorMemberListViewModel.SelectedStatus ?? "Active";

            var memberRows = db.Database.SqlQuery<Member>("exec dbo.[LB_GetCustByLastNameStatusType]  @LastName, @Status, @CustomerType, @SponsorID",
             new SqlParameter("@LastName", searchLastName),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@CustomerType", Constants.memberCustomerType),
             new SqlParameter("@SponsorID", memberDirectorMemberListViewModel.CustID)
             ).ToList();

            memberDirectorMemberListViewModel.MemberList = memberRows;

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, true);

            memberDirectorMemberListViewModel.StatusList = thisList;

            return PartialView("_MemberList", memberDirectorMemberListViewModel);
        }

        // Special section to use one time to add the aspnet identity records for manually added IMDS

        // CREATE : GET
        [HttpGet]
        public ActionResult CreateUser()
        {
            var memberDirectorRows = db.Database.SqlQuery<MemberDirector>("exec dbo.[LB_GetCustWithoutUserInfo] @CustomerType",
             new SqlParameter("@CustomerType", Constants.memberDirectorCustomerType)
             ).ToList();

            var memberDirectorListViewModel = new MemberDirectorListViewModel
            {
                MemberDirectorList = memberDirectorRows
            };

            return View(memberDirectorListViewModel);

        }

        // CREATE : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(MemberDirectorListViewModel memberDirectorListViewModel)
        {
            var DataHelper = new DataHelpers();

            var memberDirectorRows = db.Database.SqlQuery<MemberDirector>("exec dbo.[LB_GetCustWithoutUserInfo] @CustomerType",
             new SqlParameter("@CustomerType", Constants.memberDirectorCustomerType)
             ).ToList();

            memberDirectorListViewModel.MemberDirectorList = memberDirectorRows;

            string password = "";

            if (ModelState.IsValid)
            {

                foreach (MemberDirector IMD in memberDirectorListViewModel.MemberDirectorList)
                {
                    password = "TempIMD2015!" ;
                    // Insert Member Director into Identity login tables
                    try
                    {
                        var user = new ApplicationUser { UserName = IMD.Email, Email = IMD.Email, DisplayName = IMD.DisplayName, CustID = IMD.CustID };
                        IdentityResult userResult = UserManager.Create(user, password);
                        if (!userResult.Succeeded)
                        {
                            AddErrors(userResult);
                        }
                        else
                        {
                            try
                            {
                                IdentityResult roleResult = UserManager.AddToRoles(user.Id, "MemberDirector");
                                if (!roleResult.Succeeded)
                                {
                                    ModelState.AddModelError("", roleResult.Errors.First());
                                }
                            }
                            catch
                            {
                                ModelState.AddModelError("", "Error adding Role for " + IMD.CustID);
                            }
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Error adding User" + IMD.CustID);
                    }
                }

            }

            if (ModelState.IsValid)
            {

                memberDirectorRows = db.Database.SqlQuery<MemberDirector>("exec dbo.[LB_GetCustWithoutUserInfo] @CustomerType",
                 new SqlParameter("@CustomerType", Constants.memberDirectorCustomerType)
                 ).ToList();

                memberDirectorListViewModel.MemberDirectorList = memberDirectorRows;
            }

          return View(memberDirectorListViewModel);
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
