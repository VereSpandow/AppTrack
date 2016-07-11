using System.Linq;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

using System.Collections.Generic;
using System.Web;

using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using AppTrack.Helpers;

namespace AppTrack.Controllers
{
    [AuthorizeAdminRedirect(Roles="SalesManager,Accounting,Finance")]
    public class SalesRepController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public SalesRepController()
        {
        }

        public SalesRepController(ApplicationUserManager userManager, ApplicationRoleManager roleManager )
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
        public ActionResult Index(int ID = 0)
        {
            string searchLastName = "";
            string selectedStatus = "Active";

            // Debug.WriteLine("Get-" + searchLastName + "|" + searchStatus + "|" + startDateTime + "|" + endDateTime);
            var salesRepRows = db.Database.SqlQuery<SalesRep>("exec dbo.[LB_GetCustByLastNameStatusType]  @LastName, @Status, @CustomerType",
             new SqlParameter("@LastName", searchLastName),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@CustomerType", Constants.salesRepCustomerType)
             ).ToList();

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, false, true);

            var salesRepListViewModel = new SalesRepListViewModel
            {
                SalesRepList = salesRepRows,
                SearchLastName = searchLastName,
                SelectedStatus = selectedStatus,
                StatusList = thisList
            };

            return View(salesRepListViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "SearchLastName, SelectedStatus")] SalesRepListViewModel salesRepListViewModel)
        {
            string searchLastName = salesRepListViewModel.SearchLastName ?? "";
            string selectedStatus = salesRepListViewModel.SelectedStatus ?? "Active";

//            DateTime startDateTime = new DateTime();
//            if (DateTime.TryParse(searchStartDate, out startDateTime))
//            {
//                startDateTime = DateTime.Parse(searchStartDate);
//           }
//            else
//            {
//                startDateTime = new DateTime(2015, 5, 1);
//            }
//            DateTime endDateTime = new DateTime();
//            if (DateTime.TryParse(searchEndDate, out endDateTime))
//            {
//                endDateTime = DateTime.Parse(searchEndDate);
//            }
//            else
//            {
//                endDateTime = new DateTime(2017, 5, 1);
//            }

//            Debug.WriteLine("Post-" + searchLastName + "|" + searchStatus + "|" + startDateTime + "|" + endDateTime);

            var salesRepRows = db.Database.SqlQuery<SalesRep>("exec dbo.[LB_GetCustByLastNameStatusType]  @LastName, @Status, @CustomerType",
             new SqlParameter("@LastName", searchLastName),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@CustomerType", Constants.salesRepCustomerType)
             ).ToList();

            salesRepListViewModel.SalesRepList = salesRepRows;
            
            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, true);

            salesRepListViewModel.StatusList = thisList;

            return View(salesRepListViewModel);
        }

        public ActionResult AccountProfile(int id = 0)
        {
            int CustID = id;

            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.salesRepCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Sales Rep ID supplied to Account Profile page";
                return RedirectToAction("Error", "Admin");
            }
            else
            {
                ViewBag.errorCode = 0;

                var salesRepProfileViewModel = new SalesRepProfileViewModel();

                salesRepProfileViewModel.SalesRepRecord = db.Database.SqlQuery<SalesRep>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                new SqlParameter("@CustID", CustID)
                ).First();

                return View(salesRepProfileViewModel);
            }
        }


        // GET: Info/SalesRepPage1
        public ActionResult Create()
        {
           var salesRepViewModel = new SalesRepViewModel();

           return View(salesRepViewModel);

        }

        // POST: Info/SalesRepPage1
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SalesForceID,TaxID,FirstName,LastName,Email,Password,ConfirmPassword")] SalesRepViewModel salesRepViewModel)
        {
            if (ModelState.IsValid)
            {
                ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_InsertSalesRep(salesRepViewModel.SalesForceID, salesRepViewModel.TaxID, salesRepViewModel.FirstName, salesRepViewModel.LastName, salesRepViewModel.Email, returnCustID, returnMessage);

                var scalarCustID = (int)returnCustID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    TempData["Pin"] = scalarCustID;
                    // Insert Sales Rep into Identity login tables
                    try 
                    {
                        var user = new ApplicationUser { UserName = salesRepViewModel.Email, Email = salesRepViewModel.Email, DisplayName = salesRepViewModel.FirstName + " " + salesRepViewModel.LastName, CustID = scalarCustID };
                        IdentityResult userResult = UserManager.Create(user, salesRepViewModel.Password);
                        if (!userResult.Succeeded)
                        {
                            AddErrors(userResult);
                        }
                        else
                        {
                            try 
                            {
                                IdentityResult roleResult = UserManager.AddToRoles(user.Id, "SalesRep");
                                if (!roleResult.Succeeded)
                                {
                                    ModelState.AddModelError("", roleResult.Errors.First());
                                }
                            }
                            catch
                            {
                                ModelState.AddModelError("", "Error encountered adding Sales Rep Role");
                            }
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Error encountered adding Sales Rep Login");
                    }
                }
                if (!ModelState.IsValid)
                {
                    return View(salesRepViewModel);
                }
                return RedirectToAction("Index");
            }
            return View(salesRepViewModel);
        }


           // GET: Info/Edit/5
        public ActionResult Edit(int id = 0)
        {
            int CustID = id;

            ViewBag.CustID = CustID;

            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.salesRepCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Sales Rep ID supplied to Edit page";
                return RedirectToAction("Error", "Admin");
            }
            else
            {
                ViewBag.errorCode = 0;

                var salesRepViewModel = new SalesRepViewModel();

                salesRepViewModel.CustID = (int)checkCustomerResult.C_Info.CustID;
                salesRepViewModel.FirstName = checkCustomerResult.C_Info.FirstName;
                salesRepViewModel.LastName = checkCustomerResult.C_Info.LastName;
                salesRepViewModel.Email = checkCustomerResult.C_Info.Email;
                salesRepViewModel.TaxID = checkCustomerResult.C_Info.TaxID;
                salesRepViewModel.SalesForceID = checkCustomerResult.C_Info.SalesForceID;
                salesRepViewModel.StatusID = checkCustomerResult.C_Info.StatusID;
                salesRepViewModel.Status = checkCustomerResult.C_Info.Status;
                salesRepViewModel.AdminID = (int)checkCustomerResult.C_Info.AdminID;

                return View(salesRepViewModel);
            }
        }

        
        // POST: Info/MemberEnrollPage2
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustID,SalesForceID,FirstName,LastName,Email,TaxID")] SalesRepViewModel salesRepViewModel)
        {
            // Need to remove any Required properties in the model that are not being supplied by form
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            ViewBag.errorCode = 0;

            if (ModelState.IsValid)
            {
                var DataHelper = new DataHelpers();

                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(salesRepViewModel.CustID, Constants.salesRepCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Sales Rep ID supplied to Edit page";
                    return RedirectToAction("Error", "Admin");
                }
                else
                {
                    salesRepViewModel.TaxID = salesRepViewModel.TaxID.Replace("-", "");

                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateSalesRep(salesRepViewModel.CustID, salesRepViewModel.SalesForceID, salesRepViewModel.TaxID, salesRepViewModel.FirstName, salesRepViewModel.LastName, salesRepViewModel.Email, returnCustID, returnMessage);

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                        return View(salesRepViewModel);
                    }
                    else
                    {
                        return RedirectToAction("AccountProfile", new { id = salesRepViewModel.CustID } );
                    }
                }
            }
            return View(salesRepViewModel);
        }

        // The bind here uses a prefix becuase the SalesRepProfileViewModel used in the Account Profile View 
        // creates the input parameter with the name SalesRepRecord.CustID.  The Prefix option below strips that off 
        // before it tries to bind the fields to the SalesRep model.  I was not able to bind to the SalesRepProfileViewModel 
        // for reasons I gave up on.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Suspend([Bind(Include = "SalesRepRecord")] SalesRepProfileViewModel model)
        {
            ModelState.Remove("SalesRepRecord.FirstName");
            ModelState.Remove("SalesRepRecord.LastName");
            ModelState.Remove("SalesRepRecord.Email");

            if (ModelState.IsValid)
            {
                var DataHelper = new DataHelpers();

                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(model.SalesRepRecord.CustID, Constants.salesRepCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Sales Rep ID supplied to Suspend action";
                    return RedirectToAction("Error", "Admin");
                }
                else
                {
                    int statusID = 2;
                    string status = "Suspended";

                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateC_InfoStatusID(model.SalesRepRecord.CustID,
                        statusID,
                        status,
                        AdminID,
                        returnID, returnMessage);

                    var scalarCustID = (int)returnID.Value;

                    var errorMessage = (string)returnMessage.Value ?? "";
                    if (scalarCustID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        TempData["ErrMessage"] = "Sales Rep was successfully suspended";
                        return RedirectToAction("Index");
                    }
                }
            }
            var salesRepProfileViewModel = new SalesRepProfileViewModel();

            salesRepProfileViewModel.SalesRepRecord = db.Database.SqlQuery<SalesRep>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
            new SqlParameter("@CustID", model.SalesRepRecord.CustID)
            ).FirstOrDefault();

            return View("AccountProfile", salesRepProfileViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnSuspend([Bind(Include = "SalesRepRecord")] SalesRepProfileViewModel model)
        {
            ModelState.Remove("SalesRepRecord.FirstName");
            ModelState.Remove("SalesRepRecord.LastName");
            ModelState.Remove("SalesRepRecord.Email");

            if (ModelState.IsValid)
            {
                var DataHelper = new DataHelpers();

                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(model.SalesRepRecord.CustID, Constants.salesRepCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Sales Rep ID supplied to UnSuspend action";
                    return RedirectToAction("Error", "Admin");
                }
                else
                {
                    int statusID = 1;
                    string status = "Active";

                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateC_InfoStatusID(model.SalesRepRecord.CustID,
                        statusID,
                        status,
                        AdminID,returnID, returnMessage);

                    var scalarCustID = (int)returnID.Value;

                    var errorMessage = (string)returnMessage.Value ?? "";
                    if (scalarCustID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        TempData["ErrMessage"] = "Sales Rep was successfully unsuspended";
                        return RedirectToAction("Index");
                    }
                }
            }
            var salesRepProfileViewModel = new SalesRepProfileViewModel();

            salesRepProfileViewModel.SalesRepRecord = db.Database.SqlQuery<SalesRep>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
            new SqlParameter("@CustID", model.SalesRepRecord.CustID)
            ).FirstOrDefault();

            return View("AccountProfile", salesRepProfileViewModel);
        }

        [HttpGet]
        public ActionResult MemberActivityList()
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model

            var salesRepMemberActivityViewModel = new SalesRepMemberActivityViewModel();

            int zero = 0;

            salesRepMemberActivityViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.monthlyPeriodTypeID, 12, true, false);
            salesRepMemberActivityViewModel.SalesRepList = DataHelper.GetIMDByContactNameList(3, false, true);
            salesRepMemberActivityViewModel.StartPeriodID = zero;
            salesRepMemberActivityViewModel.EndPeriodID = zero;
            salesRepMemberActivityViewModel.SelectedSalesRepID = 0;

            salesRepMemberActivityViewModel.SalesRepMemberList = db.Database.SqlQuery<SalesRepMember>("exec dbo.[LB_GetSalesRepMemberActivity] @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CustID",
            new SqlParameter("@PeriodTypeID", Constants.monthlyPeriodTypeID),
            new SqlParameter("@StartPeriodID", salesRepMemberActivityViewModel.StartPeriodID),
            new SqlParameter("@EndPeriodID", salesRepMemberActivityViewModel.EndPeriodID),
            new SqlParameter("@CustID", salesRepMemberActivityViewModel.SelectedSalesRepID)
            ).ToList();

            return View(salesRepMemberActivityViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberActivityList(SalesRepMemberActivityViewModel salesRepMemberActivityViewModel)
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model
            if (ModelState.IsValid)
            {
                salesRepMemberActivityViewModel.SalesRepMemberList = db.Database.SqlQuery<SalesRepMember>("exec dbo.[LB_GetSalesRepMemberActivity] @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CustID",
                new SqlParameter("@PeriodTypeID", Constants.monthlyPeriodTypeID),
                new SqlParameter("@StartPeriodID", salesRepMemberActivityViewModel.StartPeriodID),
                new SqlParameter("@EndPeriodID", salesRepMemberActivityViewModel.EndPeriodID),
                new SqlParameter("@CustID", salesRepMemberActivityViewModel.SelectedSalesRepID)
                ).ToList();
            }

            salesRepMemberActivityViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.monthlyPeriodTypeID, 12, true, false);
            salesRepMemberActivityViewModel.SalesRepList = DataHelper.GetIMDByContactNameList(3, false, true);

            return View(salesRepMemberActivityViewModel);
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
