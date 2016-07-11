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
    public class VendorPayeeController : BaseController
    {

        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
        DataHelpers DataHelper = new DataHelpers();

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;


        public VendorPayeeController()
        {
        }

        public VendorPayeeController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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

        //      [AuthorizeAdminRedirect(Roles = Constants.adminRoles)]
        [HttpGet]
        public ActionResult Index()
        {
            int? searchCustID = -1;
            int? searchVendorID = 0;
            string searchVendorPayeeID = "";


            VendorPayeeListViewModel model = new VendorPayeeListViewModel();
            using (var db = new DevProvidentIDOCEntities())
            {
                model.VendorPayeeList = db.Database.SqlQuery<VendorPayee>("exec dbo.[LB_GetVendorPayees] @CustID, @VendorID, @VendorPayeeID",
                new SqlParameter("@CustID", searchCustID),
                new SqlParameter("@VendorID", searchVendorID),
                new SqlParameter("@VendorPayeeID", searchVendorPayeeID)
               ).ToList();
            }

            IEnumerable<System.Web.Mvc.SelectListItem> vendorList = DataHelper.GetRebateVendorList(true, true);
            model.SearchVendorList = vendorList;
            model.SearchVendorID = searchVendorID;
            model.SearchCustID = searchCustID;
            model.SearchVendorPayeeID = searchVendorPayeeID;

            return View(model);
        }



        // INDEX : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "SearchCustID, SearchDisplayName, SearchVendorID, SearchVendorPayeeID")] VendorPayeeListViewModel model, string submit = "submit")
        {
            string searchDisplayName = model.SearchDisplayName ?? " ";
            int? searchVendorID = model.SearchVendorID ?? 0;
            int? searchCustID = model.SearchCustID ?? 0;
            string searchVendorPayeeID = model.SearchVendorPayeeID ?? " ";
            
            using (var db = new DevProvidentIDOCEntities())
            {
                model.VendorPayeeList = db.Database.SqlQuery<VendorPayee>("exec dbo.[LB_GetVendorPayees] @CustID, @VendorID, @VendorPayeeID",
                new SqlParameter("@CustID", searchCustID),
                new SqlParameter("@VendorID", searchVendorID),
                new SqlParameter("@VendorPayeeID", searchVendorPayeeID)
               ).ToList();
            }

            if (submit == "Download")
            {

                if (model.VendorPayeeList.Count > 0)
                {
                    string attachment = "attachment; filename=" + AdminID.ToString() + "-VendorPayeeList.csv";

                    HttpContext.Response.AddHeader("content-disposition", attachment);

                    var sw = new StreamWriter(new MemoryStream());

                    sw.WriteLine("\"Member ID\",\"Vendor ID\",\"Vendor Name\",\"Practice Name\",\"Address1\",\"Address2\",\"City\",\"State\",\"Zip\",\"Email\",\"Phone\",\"Start Date\",\"End Date\",\"Status\",\"Status Date\"");
                    foreach (var line in model.VendorPayeeList)
                    {
                        sw.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\"",
                        line.CustID,
                        line.VendorID,
                        line.VendorName,
                        line.DisplayName,
                        line.Address1,
                        line.Address2,
                        line.City,
                        line.State,
                        line.PostalCode,
                        line.Email,
                        line.DayPhone,
                        line.StartDate.Value.ToString("MM/dd/yyyy"),
                        line.EndDate.Value.ToString("MM/dd/yyyy"),
                        line.Status,
                        line.StatusDate.Value.ToString("MM/dd/yyyy"))
                        );
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
            IEnumerable<System.Web.Mvc.SelectListItem> vendorList = DataHelper.GetRebateVendorList(true, true);
            model.SearchVendorList = vendorList;
            model.SearchVendorID = searchVendorID;
            model.SearchCustID = searchCustID;
            model.SearchVendorPayeeID = searchVendorPayeeID;
            return View(model);

        }

                // INDEX : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public String NewVendorPayee(int id, string submit, int newPayeeID)
        {
            string response = "";

            if (submit.ToLower() == "update")
            {
                var vpMemberRecord = db.Database.SqlQuery<AppTrack.SharedModels.Member>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", newPayeeID)
                 ).FirstOrDefault();

                if (vpMemberRecord == null)
                {
                    response = "Invalid Member ID.  Please try again.<br /><br />";
                    response = response + "<input type=" + '\u0022' + "submit" + '\u0022' + " class=" + '\u0022' + "btn btn-info pull-left" + '\u0022' + " name=" + '\u0022' + "submit" + '\u0022' + " value=" + '\u0022' + "Update" + '\u0022' + " >";
                    return response;
                }
                string memberName = vpMemberRecord.DisplayName;
                string address1 = vpMemberRecord.Address1;
                string address2 = vpMemberRecord.Address2;
                string city = vpMemberRecord.City;
                string state = vpMemberRecord.State;
                string postalcode = vpMemberRecord.PostalCode;
                string address = address1 + " " + address2;
                string cityStateZip = city + " " + state + " " + postalcode;         
            
                response = memberName + "<br/>";
                response = response + address + "<br/>";
                response = response + cityStateZip + "<br/><br/>";
                response = response + "<strong>Please confirm your changes.</strong><br/><br/>";
                response = response + "<input type=" + '\u0022' + "submit" + '\u0022' + " class=" + '\u0022' + "btn btn-info pull-right" + '\u0022' + " name=" + '\u0022' + "submit" + '\u0022' + " value=" + '\u0022' + "Confirm" + '\u0022' + " >";
                response = response + "<input type=" + '\u0022' + "submit" + '\u0022' + " class=" + '\u0022' + "btn btn-info pull-left" + '\u0022' + " name=" + '\u0022' + "submit" + '\u0022' + " value=" + '\u0022' + "Cancel" + '\u0022' + " >";
            }
            if (submit.ToLower() == "cancel")
            {
                response = "<br><br><strong>No changes have been saved. Close this window to continue.</strong><br><br>.";
                response = response + "<div " + " class=" + '\u0022' + "modalClose btn btn-info pull-right" + '\u0022' + ">Close</div>";
            }
            if (submit.ToLower() == "confirm")
            {
                var vpMemberRecord = db.Database.SqlQuery<AppTrack.SharedModels.Member>("exec dbo.[LB_UpdateVendorPayeeByID] @ID,@NewPayeeID, @AdminID",
                 new SqlParameter("@ID", id),
                 new SqlParameter("@NewPayeeID", newPayeeID),
                 new SqlParameter("@AdminID", AdminID)
                 ).FirstOrDefault();

                if (vpMemberRecord == null)
                {
                    response = "Invalid Member ID.  Please try again.<br /><br />";
                    response = response + "<input type=" + '\u0022' + "submit" + '\u0022' + " class=" + '\u0022' + "btn btn-info pull-left" + '\u0022' + " name=" + '\u0022' + "submit" + '\u0022' + " value=" + '\u0022' + "Update" + '\u0022' + " >";
                    return response;
                }
                else
                {
                    response = "<br><br><strong>Your changes have been saved. Close this window and refresh your report to continue.</strong><br><br>.";
                }
            }
            return response ;
        }

    }
}
