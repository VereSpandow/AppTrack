using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;

using System.Web;

using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using AppTrack.Helpers;
using System.Collections.Generic;
using System;

namespace AppTrack.Controllers
{
    [AuthorizeAdminRedirect(Roles = "ContractAdmin")]
    public class ContractProviderController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        // INDEX : GET
        [HttpGet]
        public ActionResult Index()
        {
            string searchCompany = "";
            string selectedStatus = "Active";

            var contractProviderRows = db.Database.SqlQuery<Contact>("exec dbo.[LB_GetContractProviderList] @Company, @Status, @CustomerType, @ContactType",
             new SqlParameter("@Company", searchCompany),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@CustomerType", Constants.contractProviderCustomerType),
             new SqlParameter("@ContactType", "")
             ).ToList();

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, false, true);
            IEnumerable<System.Web.Mvc.SelectListItem> providerTypeList = DataHelper.GetStatusSelectList(Constants.providerTypeLookupGroupID, false, true);

            var contractProviderViewModel = new ContractProviderListViewModel
            {
                ContractProviderList = contractProviderRows,
                SearchCompany = searchCompany,
                SelectedStatus = selectedStatus,
                StatusList = thisList,
                ProviderTypeList = providerTypeList
            };

            if (TempData["ErrorMessage"] != null)
            {
                string errMessage = TempData["ErrorMessage"].ToString();
                ModelState.AddModelError("", errMessage);
            }

            return View(contractProviderViewModel);
        }

        // INDEX : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "SearchCompany, SelectedType, SelectedStatus")] ContractProviderListViewModel contractProviderListViewModel)
        {
            string searchCompany = contractProviderListViewModel.SearchCompany ?? "";
            string selectedType = contractProviderListViewModel.SelectedType ?? "";
            string selectedStatus = contractProviderListViewModel.SelectedStatus ?? "Active";

            var contractProviderRows = db.Database.SqlQuery<Contact>("exec dbo.[LB_GetContractProviderList] @Company, @Status, @CustomerType, @ContactType",
             new SqlParameter("@Company", searchCompany),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@CustomerType", Constants.contractProviderCustomerType),
             new SqlParameter("@ContactType", selectedType)
             ).ToList();

            contractProviderListViewModel.ContractProviderList = contractProviderRows;

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.statusLookupGroupID, false, true);
            contractProviderListViewModel.StatusList = thisList;

            IEnumerable<System.Web.Mvc.SelectListItem> providerTypeList = DataHelper.GetStatusSelectList(Constants.providerTypeLookupGroupID, false, true);
            contractProviderListViewModel.ProviderTypeList = providerTypeList;

            return View(contractProviderListViewModel);
        }

        //ACOUNTPROFILE : GET 
        public ActionResult AccountProfile(int id = 0)
        {
            int CustID = id;

            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.contractProviderCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Contract Provider ID supplied to Account Profile page";
                return RedirectToAction("Error", "Admin");
            }
            else
            {
                ViewBag.errorCode = 0;

                int zero = 0;

                var contractProviderProfileViewModel = new ContractProviderViewModel();

                return View(contractProviderProfileViewModel);
            }
        }

        // CREATE : GET
        [HttpGet]
        public ActionResult Create()
        {
            var model = new ContractProviderViewModel();

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> providerTypeList = DataHelper.GetStatusSelectList(Constants.providerTypeLookupGroupID, true, false);
            model.ProviderTypeList = providerTypeList;

            IEnumerable<System.Web.Mvc.SelectListItem> vendorList = DataHelper.GetRebateVendorList(false, false);
            model.VendorList = vendorList;

            return View(model);

        }

        // CREATE : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SponsorID, Company, ContactType")] ContractProviderViewModel contractProviderViewModel)
        {
            ModelState.Remove("FirstName");
            ModelState.Remove("LastName");

            var DataHelper = new DataHelpers();

            if (contractProviderViewModel.SponsorID != null && contractProviderViewModel.SponsorID != 0)
            {
                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(contractProviderViewModel.SponsorID, Constants.vendorCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ModelState.AddModelError("SponsorID", "Invalid Vendor ID");
                }
            }

            if (ModelState.IsValid)
            {
                ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_InsertContractProvider(
                    contractProviderViewModel.SponsorID,
                    Constants.contractProviderCustomerType,
                    contractProviderViewModel.Company,
                    contractProviderViewModel.ContactType,
                    returnCustID, returnMessage);

                var scalarCustID = (int)returnCustID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
                if (!ModelState.IsValid)
                {
                    return View(contractProviderViewModel);
                }
                return RedirectToAction("Index");
            }
            IEnumerable<System.Web.Mvc.SelectListItem> providerTypeList = DataHelper.GetStatusSelectList(Constants.providerTypeLookupGroupID, true, false);
            contractProviderViewModel.ProviderTypeList = providerTypeList;

            IEnumerable<System.Web.Mvc.SelectListItem> vendorList = DataHelper.GetRebateVendorList(false, false);
            contractProviderViewModel.VendorList = vendorList;

            return View(contractProviderViewModel);
        }

        // EDIT : GET
        public ActionResult Edit(int CustID = 0)
        {
            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.contractProviderCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Contract Provider ID supplied to Edit page";
                return PartialView("_Contact");
            }
            else
            {
                ViewBag.errorCode = 0;

                var contractProviderViewModel = new ContractProviderViewModel();

                contractProviderViewModel = db.Database.SqlQuery<ContractProviderViewModel>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                 new SqlParameter("@CustID", CustID)
                 ).First();

                return PartialView("_Contact", contractProviderViewModel);
            }
        }

        // EDIT : POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustID, SponsorID, Company, ContactType")] ContractProviderViewModel contractProviderViewModel)
        {

            ViewBag.ErrorCode = 0;

            var DataHelper = new DataHelpers();

            if (ModelState.IsValid)
            {
                ViewBag.CustID = contractProviderViewModel.CustID;

                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(contractProviderViewModel.CustID, Constants.contractProviderCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ViewBag.ErrorCode = Constants.fatalErrorCode;
                    ViewBag.ErrorMessage = "Invalid Contract Provider ID supplied to Edit page";
                    return PartialView("_Contact", contractProviderViewModel);
                }
                else
                {
                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateContractProvider(Constants.contractProviderCustomerType, contractProviderViewModel.CustID,
                        contractProviderViewModel.SponsorID,
                        contractProviderViewModel.Company,
                        contractProviderViewModel.ContactType,
                        returnCustID, returnMessage);

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Provider information was updated successfully";
                    }
                }
            }

            return PartialView("_Contact", contractProviderViewModel);
        }

        // POST: Info/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int CustID = 0)
        {

            if (ModelState.IsValid)
            {
                var DataHelper = new DataHelpers();

                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.contractProviderCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Contract Provider ID supplied to Delete action";
                    return RedirectToAction("Error", "Admin");
                }
                else
                {
                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_DeleteVendor(CustID, AdminID, returnCustID, returnMessage);

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        TempData["ErrorMessage"] = errorMessage;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Contract Provider was successfully cancelled";
                        return RedirectToAction("Index");
                    }
                }
            }
            TempData["ErrMessage"] = "Contract Provider could not be Cancelled";
            return RedirectToAction("Index");
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
