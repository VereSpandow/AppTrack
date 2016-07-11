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
    public class ContactController : BaseController
    {

        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
        DataHelpers DataHelper = new DataHelpers();

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;


        public ContactController()
        {
        }

        public ContactController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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

        [HttpGet]
        public ActionResult Index()
        {
            string searchDisplayName = "";
            string searchCompany = "";
            string searchLastName = "";
            int zero = 0;
            string selectedStatus = " ";
            int customerType = 61;

            var memberRows = db.Database.SqlQuery<AppTrack.SharedModels.Member>("exec dbo.[LB_GetContactList]  @DisplayName, @Company, @LastName, @Address1, @City, @State, @PostalCode, @Phone, @Email, @Status, @CustomerType, @AdminID, @MaxCount",
             new SqlParameter("@DisplayName", searchDisplayName),
             new SqlParameter("@Company", searchCompany),
             new SqlParameter("@LastName", searchLastName),
             new SqlParameter("@Address1", ""),
             new SqlParameter("@City", ""),
             new SqlParameter("@State", ""),
             new SqlParameter("@PostalCode", ""),
             new SqlParameter("@Phone", ""),
             new SqlParameter("@Email", ""),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@CustomerType", customerType),
             new SqlParameter("@AdminID", zero),
             new SqlParameter("@MaxCount", 50)
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

            if (intCustID > 0)
            {
                model.MemberList = db.Database.SqlQuery<AppTrack.SharedModels.Member>("exec dbo.[LB_GetC_InfoByCustIDCustomerType] @CustID, @CustomerType",
                 new SqlParameter("@CustID", intCustID),
                 new SqlParameter("@CustomerType", Constants.memberCustomerType)
                 ).ToList();
            }
            else
            {
                model.MemberList = db.Database.SqlQuery<AppTrack.SharedModels.Member>("exec dbo.[LB_GetContactList]  @DisplayName, @Company, @LastName, @Address1, @City, @State, @PostalCode, @Phone, @Email, @Status, @CustomerType, @AdminID, @MaxCount",
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
                 new SqlParameter("@CustomerType", 61),
                 new SqlParameter("@AdminID", searchAccountManagerID),
                 new SqlParameter("@MaxCount", zero)
                 ).ToList();

                if (submit == "Download")
                {

                    if (model.MemberList.Count > 0)
                    {
                        string attachment = "attachment; filename=" + AdminID.ToString() + "-ContactList.csv";

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

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, false, true);
            IEnumerable<System.Web.Mvc.SelectListItem> adminList = DataHelper.GetAccountManagerSelectList(false, true);
            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList(false, true);

            model.StatusList = thisList;
            model.AccountManagerList = adminList;
            model.StateList = stateList;

            return View(model);
        }

    }
}
