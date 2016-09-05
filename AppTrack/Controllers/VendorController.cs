using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;

using System.Collections.Generic;
using System.Web;
using System.Web.UI.DataVisualization.Charting;
using System.Text;
using System.IO;

using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using AppTrack.Helpers;

namespace AppTrack.Controllers
{
    [AuthorizeAdminRedirect(Roles = Constants.adminRoles)]
    public class VendorController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
       
        [HttpGet]
        public ActionResult Index(int ID = 0)
        {
            string searchCompany = "";
            string selectedStatus = "Active";

            // Debug.WriteLine("Get-" + searchLastName + "|" + searchStatus + "|" + startDateTime + "|" + endDateTime);
            var vendorRows = db.Database.SqlQuery<Vendor>("exec dbo.[LB_GetVendorList]  @Company, @Status, @CustomerType",
             new SqlParameter("@Company", searchCompany),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@CustomerType", Constants.vendorCustomerType)
             ).ToList();

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.vendorStatusLookupGroupID, true);

            var vendorListViewModel = new VendorListViewModel
            {
                VendorList = vendorRows,
                SearchCompany = searchCompany,
                SelectedStatus = selectedStatus,
                StatusList = thisList
            };

            return View(vendorListViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "SearchCompany, SelectedStatus")] VendorListViewModel vendorListViewModel)
        {
            string searchCompany = vendorListViewModel.SearchCompany ?? "";
            string selectedStatus = vendorListViewModel.SelectedStatus ?? "Active";

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

            var vendorRows = db.Database.SqlQuery<Vendor>("exec dbo.[LB_GetVendorList]  @Company, @Status, @CustomerType",
             new SqlParameter("@Company", searchCompany),
             new SqlParameter("@Status", selectedStatus),
             new SqlParameter("@CustomerType", Constants.vendorCustomerType)
             ).ToList();

            vendorListViewModel.VendorList = vendorRows;

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> thisList = DataHelper.GetStatusSelectList(Constants.vendorStatusLookupGroupID, true);

            vendorListViewModel.StatusList = thisList;

            return View(vendorListViewModel);
        }

        public ActionResult AccountProfile(int id = 0)
        {
            int CustID = id;
            ViewBag.VendorId = CustID;

            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Vendor ID supplied to Contact List";
                return RedirectToAction("Error", "Admin");
            }
            else
            {

                ViewBag.errorCode = 0;

                var vendorProfileViewModel = new VendorProfileViewModel();

                // Vendor Record
                vendorProfileViewModel.VendorRecord = db.Database.SqlQuery<Vendor>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                new SqlParameter("@CustID", CustID)
                ).First();

                // Program Summary List
                vendorProfileViewModel.VendorProgramList = db.Database.SqlQuery<VendorProfileProgram>("exec dbo.[LB_GetVendorProgramSummary] @CustID",
                new SqlParameter("@CustID", CustID)
                ).ToList();

                vendorProfileViewModel.VendorContactList = db.Database.SqlQuery<VendorContact>("exec dbo.[LB_GetVendorContacts] @CustID, @CustomerType, @ContactType",
                new SqlParameter("@CustID", CustID),
                new SqlParameter("@CustomerType", Constants.vendorContactCustomerType),
                new SqlParameter("@ContactType", "Program")
                ).ToList();

                vendorProfileViewModel.VendorRequirementList = db.Database.SqlQuery<VendorRequirement>("exec dbo.[LB_GetVendorRequirements] @CustID",
                new SqlParameter("@CustID", CustID)
                ).ToList();

                return View(vendorProfileViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateVendorStatus(int CustID = 0, int StatusID = 0)
        {
            if (ModelState.IsValid)
            {
                var DataHelper = new DataHelpers();

                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.vendorCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Vendor ID supplied to Delete action";
                    return RedirectToAction("Error", "Admin");
                }
                else
                {

                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    int scalarCustID = 0;
                    string errorMessage = "";

                    switch (StatusID)
                    {
                        case 1:
                            db.LB_UnCancelVendor(CustID, AdminID, returnCustID, returnMessage);

                            scalarCustID = (int)returnCustID.Value;
                            errorMessage = (string)returnMessage.Value ?? "";

                            if (scalarCustID == -1)
                            {
                                ModelState.AddModelError("", errorMessage);
                            }
                            else
                            {
                                ModelState.AddModelError("", "Vendor was successfully set to InActive");
                            }
                            break;
                        case 3:
                            db.LB_CancelVendor(CustID, AdminID, returnCustID, returnMessage);

                            scalarCustID = (int)returnCustID.Value;
                            errorMessage = (string)returnMessage.Value ?? "";
                            if (scalarCustID == -1)
                            {
                                ModelState.AddModelError("", errorMessage);
                            }
                            else
                            {
                                ModelState.AddModelError("", "Vendor was successfully set to InActive");
                            }
                            break;
                        default:
                            TempData["ErrorCode"] = Constants.fatalErrorCode;
                            TempData["ErrorMessage"] = "Invalid Status ID supplied to Update Vendor Status action";
                            return RedirectToAction("Error", "Admin");
                    }
                }
            }
            var vendorProfileViewModel = new VendorProfileViewModel();

            // Vendor Record
            vendorProfileViewModel.VendorRecord = db.Database.SqlQuery<Vendor>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
            new SqlParameter("@CustID", CustID)
            ).First();

            // Program Summary List
            vendorProfileViewModel.VendorProgramList = db.Database.SqlQuery<VendorProfileProgram>("exec dbo.[LB_GetVendorProgramSummary] @CustID",
            new SqlParameter("@CustID", CustID)
            ).ToList();

            vendorProfileViewModel.VendorContactList = db.Database.SqlQuery<VendorContact>("exec dbo.[LB_GetVendorContacts] @CustID, @CustomerType",
            new SqlParameter("@CustID", CustID),
            new SqlParameter("@CustomerType", Constants.vendorContactCustomerType)
            ).ToList();

            vendorProfileViewModel.VendorRequirementList = db.Database.SqlQuery<VendorRequirement>("exec dbo.[LB_GetVendorRequirements] @CustID",
            new SqlParameter("@CustID", CustID)
            ).ToList();

            return View("AccountProfile", vendorProfileViewModel);
        }

        // This is for reference for another time - we are not using this today
        public ActionResult MemberRebateChart(int CustID = 0)
        {
            var DataHelper = new DataHelpers();
            var ChartHelper = new ChartHelpers();

            List<ChartItem> chartList = new List<ChartItem>();

            chartList = db.Database.SqlQuery<ChartItem>("exec dbo.[LB_GetVendorRebateChartMember] @CustID, @CommissionID",
            new SqlParameter("@CustID", CustID),
            new SqlParameter("@CommissionID", Constants.memberRebateCommissionID)
            ).ToList();

            if (chartList.Count == 0)
            {
                return Content("No recent activity");
            }
            List<ChartSeriesItem> results = new List<ChartSeriesItem>();

            foreach (ChartItem x in chartList)
            {
                results.Add(new ChartSeriesItem(x.ChartLabel, Convert.ToDouble(x.ChartValue)));
            }
            int height = 400;
            int width = 400;
            string chartTitle = "";
            string AxisXTitle = "";
            string AxisYTitle = "";

            var chart = new Chart();

            chart.Width = width;
            chart.Height = height;

            // Create chart here 
            chart.Titles.Add(ChartHelper.CreateTitle(chartTitle));
            // 			chart.Legends.Add(CreateLegend()); 
            chart.ChartAreas.Add(ChartHelper.CreateChartArea(chartTitle, AxisXTitle, AxisYTitle));
            string seriesName1 = "series1";

            chart.Series.Add(ChartHelper.CreateSeries(results, SeriesChartType.Column, chartTitle, seriesName1));

            StringBuilder result = new StringBuilder();
            result.Append(ChartHelper.getChartImage(chart));
            result.Append(chart.GetHtmlImageMap("ImageMap"));
            return Content(result.ToString());
        }

        // This is for reference for another time - we are not using this today
        public ActionResult CorporateRebateChart()
        {
            var DataHelper = new DataHelpers();
            var ChartHelper = new ChartHelpers();

            List<ChartItem> chartList = new List<ChartItem>();

            chartList = db.Database.SqlQuery<ChartItem>("exec dbo.[LB_GetVendorRebateChartMember] @CustID, @CommissionID",
            new SqlParameter("@CustID", CustID),
            new SqlParameter("@CommissionID", Constants.corporateRebateCommissionID)
            ).ToList();

            if (chartList.Count == 0)
            {
                return Content("No recent activity");
            }
            List<ChartSeriesItem> results = new List<ChartSeriesItem>();

            foreach (ChartItem x in chartList)
            {
                results.Add(new ChartSeriesItem(x.ChartLabel, Convert.ToDouble(x.ChartValue)));
            }

            int height = 400;
            int width = 400;
            string chartTitle = "";
            string AxisXTitle = "";
            string AxisYTitle = "";

            var chart = new Chart();

            chart.Width = width;
            chart.Height = height;

            // Create chart here 
            chart.Titles.Add(ChartHelper.CreateTitle(chartTitle));
            // 			chart.Legends.Add(CreateLegend()); 
            chart.ChartAreas.Add(ChartHelper.CreateChartArea(chartTitle, AxisXTitle, AxisYTitle));
            string seriesName2 = "series2";

            chart.Series.Add(ChartHelper.CreateSeries(results, SeriesChartType.Column, chartTitle, seriesName2));

            StringBuilder result = new StringBuilder();
            result.Append(ChartHelper.getChartImage(chart));
            result.Append(chart.GetHtmlImageMap("ImageMap"));
            return Content(result.ToString());
        }


        // The bind here uses a prefix becuase the VendorProfileViewModel used in the Account Profile View 
        // creates the input parameter with the name VendorRecord.CustID.  The Prefix option below strips that off 
        // before it tries to bind the fields to the Vendor model.  I was not able to bind to the VendorProfileViewModel 
        // for reasons I gave up on.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete([Bind(Prefix = "VendorRecord")] Vendor model)
        {

            if (ModelState.IsValid)
            {
                if (model.CustID != null)
                {
                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    //                    try
                    //                    {
                    //                    db.LB_DeleteVendor(model.CustID, model.AdminID, returnCustID, returnMessage);

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        TempData["ErrMessage"] = errorMessage;
                    }
                    else
                    {
                        TempData["ErrMessage"] = "Vendor was successfully deleted";
                    }
                    //                    }
                    //                    catch
                    //                    {
                    //                        TempData["ErrMessage"] = "Vendor could not be deleted";
                    //                    }
                }
            }
            //            return View("../Vendor/AccountProfile", vendor);
            return RedirectToAction("AccountProfile", model.CustID);
        }

        [HttpGet]
        public ActionResult Company(int VendorID = 0, string pageLayout = "")
        {
            int CustID = VendorID;

            ViewBag.CustID = CustID;
            ViewBag.PageLayout = pageLayout;

            var vendorCompanyViewModel = new VendorCompanyViewModel();

            if (CustID == 0)
            {
                vendorCompanyViewModel.Flag1 = 1;
                vendorCompanyViewModel.Flag2 = 1;
                vendorCompanyViewModel.HideFlag = 0;

            }
            else
            {
                vendorCompanyViewModel = db.Database.SqlQuery<VendorCompanyViewModel>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                new SqlParameter("@CustID", CustID)
                ).First();

                if (vendorCompanyViewModel == null)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Vendor ID supplied to Contact List";
                    return RedirectToAction("Error", "Admin");
                }
            }

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();

            vendorCompanyViewModel.StateList = stateList;

            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();

            vendorCompanyViewModel.NameTitleList = nameTitleList;
            if (pageLayout == "I")
            {
                ViewBag.NextAction = "";
                return View("Company", "~/Views/Shared/_IFrameLayout.cshtml", vendorCompanyViewModel);
            }
            else
            {
                ViewBag.NextAction = "Contacts";
                return View(vendorCompanyViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Company([Bind(Include = "CustID,Company,DisplayName,Address1,Address2,City,State,PostalCode,CompanyPhone,Fax,Flag1,NameTitle,FirstName,LastName,Email,DayPhone,Mobile,Flag2,SiteName,HideFlag")] VendorCompanyViewModel vendorCompanyViewModel, string NextAction = "Contacts", string pageLayout = "")
        {
            var DataHelper = new DataHelpers();

            int CustID = vendorCompanyViewModel.CustID;

            ViewBag.VendorDisplayName = "";
            ViewBag.PageLayout = pageLayout;

            int scalarCustID = 0;
            string errorMessage = "";

            if (CustID > 0)
            {
                CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.vendorCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Vendor ID supplied to Contact List";
                    return RedirectToAction("Error", "Admin");
                }
                else
                {
                    ViewBag.VendorDisplayName = checkCustomerResult.DisplayName;

                    // Update Vendor

                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateVendor(vendorCompanyViewModel.CustID, Constants.vendorCustomerType, vendorCompanyViewModel.Company, vendorCompanyViewModel.DisplayName, vendorCompanyViewModel.Address1, vendorCompanyViewModel.Address2, vendorCompanyViewModel.City, vendorCompanyViewModel.State, vendorCompanyViewModel.PostalCode, vendorCompanyViewModel.CompanyPhone, vendorCompanyViewModel.Fax, vendorCompanyViewModel.Flag1, vendorCompanyViewModel.NameTitle, vendorCompanyViewModel.FirstName, vendorCompanyViewModel.LastName, vendorCompanyViewModel.Email, vendorCompanyViewModel.DayPhone, vendorCompanyViewModel.Mobile, vendorCompanyViewModel.Flag2, vendorCompanyViewModel.SiteName, vendorCompanyViewModel.HideFlag, returnCustID, returnMessage);

                    scalarCustID = (int)returnCustID.Value;
                    errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
            }
            else
            {
                ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_InsertVendorV2(Constants.vendorCustomerType, 3, "", vendorCompanyViewModel.Company, vendorCompanyViewModel.DisplayName, 
                    vendorCompanyViewModel.Address1, vendorCompanyViewModel.Address2, vendorCompanyViewModel.City, vendorCompanyViewModel.State, 
                    vendorCompanyViewModel.PostalCode, vendorCompanyViewModel.CompanyPhone, vendorCompanyViewModel.Fax, vendorCompanyViewModel.Flag1, 
                    vendorCompanyViewModel.NameTitle, vendorCompanyViewModel.FirstName, vendorCompanyViewModel.LastName, vendorCompanyViewModel.Email, 
                    vendorCompanyViewModel.DayPhone, vendorCompanyViewModel.Mobile, vendorCompanyViewModel.Flag2, vendorCompanyViewModel.SiteName, 
                    vendorCompanyViewModel.HideFlag, returnCustID, returnMessage);

                scalarCustID = (int)returnCustID.Value;
                errorMessage = (string)returnMessage.Value ?? "";

                if (scalarCustID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
            }

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();

            vendorCompanyViewModel.StateList = stateList;

            IEnumerable<System.Web.Mvc.SelectListItem> nameTitleList = DataHelper.GetNameTitleList();

            vendorCompanyViewModel.NameTitleList = nameTitleList;

            if (ModelState.IsValid)
            {
                ViewBag.ErrorCode = 0;
                if (CustID == 0)
                {
                    // Adding a new vendor so go to next step which is Contacts (default value of parameter)
                    return RedirectToAction(NextAction, "Vendor", new { ID = scalarCustID });
                }
            }
            else
            {
                ViewBag.ErrorCode = 1;
            }

            if (pageLayout == "I")
            {
                ViewBag.NextAction = "";
                return View("Company", "~/Views/Shared/_IFrameLayout.cshtml", vendorCompanyViewModel);
            }
            else
            {
                ViewBag.NextAction = "Contacts";
                return View(vendorCompanyViewModel);
            }
        }

        //
        // CONTACT SECTION
        //

        [HttpGet]
        public ActionResult Contacts(int ID = 0)
        {
            int VendorID = ID;
            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Vendor ID supplied to Contact List";
                return RedirectToAction("Error", "Admin");
            }
            ViewBag.VendorID = VendorID;

            ViewBag.NextAction = "Rebates";

            return View();
        }

        [HttpGet]
        public ActionResult ContactList(int VendorID = 0, int ContactID = 0, string ActionType = "")
        {
            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Vendor ID provided, unable to retrieve to Contact List";
                return PartialView("_ContactList");
            }

            ViewBag.VendorDisplayName = checkCustomerResult.DisplayName;

            var vendorContact = new VendorContact()
            {
                SponsorID = VendorID
            };

            if (ContactID > 0)
            {
                // Verify the Contact 
                checkCustomerResult = DataHelper.CheckCustomer(ContactID, Constants.vendorContactCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    ViewBag.ErrorCode = Constants.fatalErrorCode;
                    ViewBag.ErrorMessage = "Invalid Contact ID provided, unable to retrieve to Contact List";
                    return PartialView("_ContactList");
                }
                // Check to see if Action is Delete
                if (ActionType == "D")
                {
                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_DeleteC_Info(ContactID, "Contact", AdminID,
                        returnCustID, returnMessage);

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                else
                {
                    vendorContact = db.Database.SqlQuery<VendorContact>("exec dbo.[LB_GetC_InfoByCustID] @CustID",
                    new SqlParameter("@CustID", ContactID)
                    ).First();
                }
            }

            // Initalize the View Model

            var vendorContactViewModel = new VendorContactViewModel();

            vendorContactViewModel.vendorContact = vendorContact;

            var ContactList = db.Database.SqlQuery<VendorContact>("exec dbo.[LB_GetVendorContacts] @CustID, @CustomerType",
            new SqlParameter("@CustID", VendorID),
            new SqlParameter("@CustomerType", Constants.vendorContactCustomerType)
            ).ToList();

            vendorContactViewModel.VendorContactList = ContactList;

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();

            vendorContactViewModel.StateList = stateList;

            IEnumerable<System.Web.Mvc.SelectListItem> contactTypeList = DataHelper.GetVendorContactTypeList();

            vendorContactViewModel.ContactTypeList = contactTypeList;

            return PartialView("_ContactList", vendorContactViewModel);
        }



        [HttpPost]
        // [ChildActionOnly] can't use this because we use AJAX to post to this Action
        [ValidateAntiForgeryToken]
        public ActionResult ContactList([Bind(Include = "vendorContact")] VendorContactViewModel vendorContactViewModel)
        {
            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList();

            vendorContactViewModel.StateList = stateList;

            IEnumerable<System.Web.Mvc.SelectListItem> contactTypeList = DataHelper.GetVendorContactTypeList();

            vendorContactViewModel.ContactTypeList = contactTypeList;

            int ContactID = new int { };
            int VendorID = new int { };

            ViewBag.VendorDisplayName = "";

            // validate Vendor ID
            VendorID = vendorContactViewModel.vendorContact.SponsorID;

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Contact ID provided, unable to retrieve to Contact List";
                return PartialView("_ContactList");
            }
            else
            {
                ViewBag.VendorDisplayName = checkCustomerResult.DisplayName;
            }

            if (ModelState.IsValid)
            {

                ContactID = vendorContactViewModel.vendorContact.CustID;
                // if custId = 0 then this is a new contact 

                if (ContactID == 0)
                {
                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_InsertVendorContact(VendorID, Constants.vendorContactCustomerType,
                        vendorContactViewModel.vendorContact.ContactType,
                        vendorContactViewModel.vendorContact.NameTitle,
                        vendorContactViewModel.vendorContact.FirstName,
                        vendorContactViewModel.vendorContact.LastName,
                        vendorContactViewModel.vendorContact.Email,
                        vendorContactViewModel.vendorContact.Address1,
                        vendorContactViewModel.vendorContact.Address2,
                        vendorContactViewModel.vendorContact.City,
                        vendorContactViewModel.vendorContact.State,
                        vendorContactViewModel.vendorContact.PostalCode,
                        vendorContactViewModel.vendorContact.DayPhone,
                        vendorContactViewModel.vendorContact.Mobile,
                        vendorContactViewModel.vendorContact.Fax,
                        vendorContactViewModel.vendorContact.Flag2,
                        returnCustID, returnMessage);

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                else
                {
                    // Update contact
                    ObjectParameter returnCustID = new ObjectParameter("returnCustID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateVendorContact(VendorID, ContactID, Constants.vendorContactCustomerType,
                    vendorContactViewModel.vendorContact.ContactType,
                    vendorContactViewModel.vendorContact.NameTitle,
                    vendorContactViewModel.vendorContact.FirstName,
                    vendorContactViewModel.vendorContact.LastName,
                    vendorContactViewModel.vendorContact.Email,
                    vendorContactViewModel.vendorContact.Address1,
                    vendorContactViewModel.vendorContact.Address2,
                    vendorContactViewModel.vendorContact.City,
                    vendorContactViewModel.vendorContact.State,
                    vendorContactViewModel.vendorContact.PostalCode,
                    vendorContactViewModel.vendorContact.DayPhone,
                    vendorContactViewModel.vendorContact.Mobile,
                    vendorContactViewModel.vendorContact.Fax,
                    vendorContactViewModel.vendorContact.Flag2,
                    returnCustID, returnMessage);

                    var scalarCustID = (int)returnCustID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarCustID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
            }

            var ContactList = db.Database.SqlQuery<VendorContact>("exec dbo.[LB_GetVendorContacts] @CustID, @CustomerType",
            new SqlParameter("@CustID", VendorID),
            new SqlParameter("@CustomerType", Constants.vendorContactCustomerType)
            ).ToList();

            vendorContactViewModel.VendorContactList = ContactList;

            if (ModelState.IsValid)
            {
                // Add empty items to View Model and clear out Model State which is what the browser uses to populate the form
                var emptyVendorContact = new VendorContact() { SponsorID = VendorID };

                vendorContactViewModel.vendorContact = emptyVendorContact;
                ModelState.Clear();

                ViewBag.ErrorCode = 0;
            }
            else
            {
                ViewBag.ErrorCode = 1;
            }
            return PartialView("_ContactList", vendorContactViewModel);
        }

        //
        // PROGRAM SECTION
        //

        [HttpGet]
        public ActionResult Rebates(int VendorID = 0)
        {
            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Vendor ID supplied to Rebate List";
                return RedirectToAction("Error", "Admin");
            }
            ViewBag.VendorID = VendorID;

            ViewBag.NextAction = "Programs";

            return View();
        }

        [HttpGet]
        public ActionResult RebateList(int VendorID = 0, int VolumeID = 0, string ActionType = "")
        {
            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Vendor ID provided, unable to retrieve Rebate List";
                return PartialView("_ProgramList");
            }

            ViewBag.VendorDisplayName = checkCustomerResult.DisplayName;

            var vendorRebate = new VendorRebate()
                {
                    CustID = VendorID
                };

            if (VolumeID > 0)
            {
                // Check to see if Action is Delete
                if (ActionType == "D")
                {
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_DeleteVolume(VolumeID, VendorID,
                        AdminID,
                        returnID, returnMessage);

                    var scalarID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                else
                {
                    // Check ProgramTypeCust
                    vendorRebate = db.Database.SqlQuery<VendorRebate>("exec dbo.[LB_GetVolumesByType] @VolumeType, @CustID, @VolumeID",
                    new SqlParameter("@VolumeType", Constants.memberRebateVolumeType),
                    new SqlParameter("@CustID", VendorID),
                    new SqlParameter("@VolumeID", VolumeID)
                    ).FirstOrDefault();

                    if (vendorRebate == null)
                    {
                        // Volume not found so error
                        ModelState.AddModelError("", "Rebate Type was not found and could not be retrieved");
                    }
                }
            }

            // Initalize the View Model

            var vendorRebateViewModel = new VendorRebateViewModel();

            vendorRebateViewModel.VendorRebate = vendorRebate;
            ViewBag.VolumeID = vendorRebate.VolumeID;

            var vendorRebateList = db.Database.SqlQuery<VendorRebate>("exec dbo.[LB_GetVolumesByType] @VolumeType, @VendorID",
            new SqlParameter("@VolumeType", Constants.memberRebateVolumeType),
            new SqlParameter("@VendorID", VendorID)
            ).ToList();

            vendorRebateViewModel.VendorRebateList = vendorRebateList;

            return PartialView("_RebateList", vendorRebateViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RebateList([Bind(Include = "VendorRebate")] VendorRebateViewModel vendorRebateViewModel)
        {
            var DataHelper = new DataHelpers();

            int VendorID = new int { };
            int VolumeID = new int { };

            ViewBag.VendorDisplayName = "";

            VendorID = vendorRebateViewModel.VendorRebate.CustID ?? 0;

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Vendor ID provided, unable to retrieve Rebate List";
                return PartialView("_RebateList");
            }
            else
            {
                ViewBag.VendorDisplayName = checkCustomerResult.DisplayName;
            }

            if (ModelState.IsValid)
            {
                VolumeID = vendorRebateViewModel.VendorRebate.VolumeID;

                if (VolumeID == 0)
                {
                    // Add Rebate 
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_InsertVolume(VendorID,
                        Constants.memberRebateVolumeType,
                        vendorRebateViewModel.VendorRebate.VolumeName,
                        vendorRebateViewModel.VendorRebate.VolumeDesc,
                        returnID, returnMessage);

                    var scalarID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                else
                {
                    // Update Rebate
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    try
                    {
                        db.LB_UpdateVolume(VolumeID,
                            vendorRebateViewModel.VendorRebate.VolumeName,
                            vendorRebateViewModel.VendorRebate.VolumeDesc,
                            returnID, returnMessage);
                    }
                    catch
                    {
                        int catchid = 0;
                    }

                    var scalarID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
            }

            var vendorRebateList = db.Database.SqlQuery<VendorRebate>("exec dbo.[LB_GetVolumesByType] @VolumeType, @VendorID",
            new SqlParameter("@VolumeType", Constants.memberRebateVolumeType),
            new SqlParameter("@VendorID", VendorID)
            ).ToList();

            vendorRebateViewModel.VendorRebateList = vendorRebateList;

            if (ModelState.IsValid)
            {
                // Add empty items to View Model and clear out Model State
                // Need to populate the CustID property because the View form needs this
                var emptyVendorRebate = new VendorRebate()
                {
                    CustID = VendorID
                };

                vendorRebateViewModel.VendorRebate = emptyVendorRebate;
                ModelState.Clear();

                ViewBag.ErrorCode = 0;
            }
            else
            {
                ViewBag.ErrorCode = 1;
            }
            return PartialView("_RebateList", vendorRebateViewModel);
        }

        //
        // PROGRAM SECTION
        //
        [HttpGet]
        public ActionResult Programs(int VendorID = 0)
        {
            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Vendor ID supplied to Program List";
                return RedirectToAction("Error", "Admin");
            }
            ViewBag.VendorID = VendorID;

            ViewBag.NextAction = "Documents";

            return View();
        }

        [HttpGet]
        public ActionResult ProgramList(int VendorID = 0, int ProgramID = 0, int PVendorID = 0, string ActionType = "")
        {
            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Vendor ID provided, unable to retrieve Program List";
                return PartialView("_ProgramList");
            }

            ViewBag.VendorDisplayName = checkCustomerResult.DisplayName;

            var vendorProgram = new VendorProgram()
            {
                CustID = VendorID
            };

            if (ProgramID > 0)
            {
                // Check to see if Action is Delete
                if (ActionType == "D")
                {
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_DeleteVendorProgram(ProgramID, AdminID,
                        returnID, returnMessage);

                    var scalarID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                else
                {
                    // Check Program
                    vendorProgram = db.Database.SqlQuery<VendorProgram>("exec dbo.[LB_GetVendorProgramByID] @ProgramID",
                    new SqlParameter("@ProgramID", ProgramID)
                    ).FirstOrDefault();

                    if (vendorProgram == null)
                    {
                        // Program not found so error
                        ModelState.AddModelError("", "Program was not found and could not be retrieved");
                    }
                }
            }

            // Initalize the View Model

            var vendorProgramViewModel = new VendorProgramViewModel();

            vendorProgramViewModel.vendorProgram = vendorProgram;

            var programList = db.Database.SqlQuery<VendorProgram>("exec dbo.[LB_GetVendorPrograms] @CustID",
            new SqlParameter("@CustID", VendorID)
            ).ToList();

            vendorProgramViewModel.VendorProgramList = programList;

            IEnumerable<System.Web.Mvc.SelectListItem> companyProgramList = DataHelper.GetCompanyProgramSelectList(false, false);

            vendorProgramViewModel.CompanyProgramList = companyProgramList;

            return PartialView("_ProgramList", vendorProgramViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProgramList([Bind(Include = "vendorProgram")] VendorProgramViewModel vendorProgramViewModel)
        {
            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> companyProgramList = DataHelper.GetCompanyProgramSelectList(false, false);

            vendorProgramViewModel.CompanyProgramList = companyProgramList;

            int VendorID = new int { };
            int ProgramID = new int { };

            ViewBag.VendorDisplayName = "";

            VendorID = vendorProgramViewModel.vendorProgram.CustID;

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Vendor ID provided, unable to retrieve Program List";
                return PartialView("_ProgramList");
            }
            else
            {
                ViewBag.VendorDisplayName = checkCustomerResult.DisplayName;
            }

            if (ModelState.IsValid)
            {
                ProgramID = vendorProgramViewModel.vendorProgram.ProgramID;

                if (ProgramID == 0)
                {
                    // Add Program 
                    ObjectParameter returnProgramID = new ObjectParameter("returnProgramID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_InsertVendorProgram(VendorID,
                        vendorProgramViewModel.vendorProgram.C_ProgramID,
                        vendorProgramViewModel.vendorProgram.ProgramName,
                        vendorProgramViewModel.vendorProgram.ProgramSummary,
                        vendorProgramViewModel.vendorProgram.ProgramDescription,
                        vendorProgramViewModel.vendorProgram.ProgramRequirements,
                        vendorProgramViewModel.vendorProgram.ProgramDirections,
                        vendorProgramViewModel.vendorProgram.MemberParticipationRequired,
                        vendorProgramViewModel.vendorProgram.MemberRebate,
                        vendorProgramViewModel.vendorProgram.CorporateRebate,
                        AdminID,
                        returnProgramID, returnMessage);

                    var scalarProgramID = (int)returnProgramID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarProgramID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                else
                {
                    // Update Program
                    ObjectParameter returnProgramID = new ObjectParameter("returnProgramID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateVendorProgram(VendorID, ProgramID,
                        vendorProgramViewModel.vendorProgram.C_ProgramID,
                        vendorProgramViewModel.vendorProgram.ProgramName,
                        vendorProgramViewModel.vendorProgram.ProgramSummary,
                        vendorProgramViewModel.vendorProgram.ProgramDescription,
                        vendorProgramViewModel.vendorProgram.ProgramRequirements,
                        vendorProgramViewModel.vendorProgram.ProgramDirections,
                        vendorProgramViewModel.vendorProgram.MemberParticipationRequired,
                        vendorProgramViewModel.vendorProgram.MemberRebate,
                        vendorProgramViewModel.vendorProgram.CorporateRebate,
                        AdminID,
                        returnProgramID, returnMessage);

                    var scalarProgramID = (int)returnProgramID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarProgramID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
            }

            var programList = db.Database.SqlQuery<VendorProgram>("exec dbo.[LB_GetVendorPrograms] @CustID",
            new SqlParameter("@CustID", VendorID)
            ).ToList();

            vendorProgramViewModel.VendorProgramList = programList;

            if (ModelState.IsValid)
            {
                // Add empty items to View Model and clear out Model State
                // Need to populate the CustID property because the View form needs this
                var emptyVendorProgram = new VendorProgram()
                {
                    CustID = VendorID
                };

                vendorProgramViewModel.vendorProgram = emptyVendorProgram;
                ModelState.Clear();

                ViewBag.ErrorCode = 0;
            }
            else
            {
                ViewBag.ErrorCode = 1;
            }
            return PartialView("_ProgramList", vendorProgramViewModel);
        }

        //
        // DOCUMENT SECTION
        //
        [HttpGet]
        public ActionResult Documents(int VendorID = 0, int DocumentID = 0, string ActionType = "", string pageLayout = "")
        {
            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Vendor ID supplied to Document List";
                return RedirectToAction("Error", "Admin");
            }

            ViewBag.VendorDisplayName = checkCustomerResult.DisplayName;
            ViewBag.PageLayout = pageLayout;

            var vendorDocument = new Document()
            {
                CustID = VendorID
            };

            if (DocumentID > 0)
            {
                // Check Document
                vendorDocument = db.Database.SqlQuery<Document>("exec dbo.[LB_GetDocumentByID] @DocumentID",
                new SqlParameter("@DocumentID", DocumentID)
                ).First();

                if (vendorDocument == null)
                {
                    // document not found so error
                    ModelState.AddModelError("", "Document was not found and could not be retrieved");
                }
                else
                {
                    // Check to see if Action is Delete
                    if (ActionType == "D")
                    {
                        // Try to delete the record in C_Documents first
                        ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                        ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                        db.LB_DeleteDocumentByVendor(VendorID, DocumentID, AdminID, returnID, returnMessage);

                        var scalarID = (int)returnID.Value;
                        var errorMessage = (string)returnMessage.Value ?? "";

                        if (scalarID == -1)
                        {
                            ModelState.AddModelError("", errorMessage);
                        }
                        else
                        {
                            // Get the filename and try to delete it

                            string folderName = Server.MapPath(Constants.DocumentFolderPath) + VendorID.ToString();
                            //                        folderName = Constants.DocumentFolderPath;


                            //                        string fullPath = Request.MapPath(folderName + vendorDocument.FileName);
                            string fullPath = Server.MapPath(Constants.DocumentFolderPath + VendorID.ToString()) + "\\" + vendorDocument.FileName;

                            if (System.IO.File.Exists(fullPath))
                            {
                                try
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                                catch
                                {
                                    ModelState.AddModelError("", "The Document file could not be deleted on the server");
                                }
                            }
                        }
                        // Reset the vendorDocument object to null
                        vendorDocument = new Document();
                    }
                }
            }

            // Initalize the View Model

            var vendorDocumentViewModel = new VendorDocumentViewModel();

            vendorDocumentViewModel.vendorDocument = vendorDocument;

            var DocumentList = db.Database.SqlQuery<Document>("exec dbo.[LB_GetVendorDocuments] @CustID",
            new SqlParameter("@CustID", VendorID)
            ).ToList();

            vendorDocumentViewModel.VendorDocumentList = DocumentList;

            if (pageLayout == "I")
            {
                ViewBag.NextAction = "";
                return View("Documents", "~/Views/Shared/_IFrameLayout.cshtml", vendorDocumentViewModel);
            }
            else
            {
                ViewBag.NextAction = "Requirements";
                return View(vendorDocumentViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Documents(HttpPostedFileBase documentFile, [Bind(Include = "vendorDocument")]VendorDocumentViewModel vendorDocumentViewModel, string pageLayout = "")
        {
            ModelState.Remove("vendorDocument.TemplateID");
    
            var DataHelper = new DataHelpers();

            int VendorID = new int { };
            int DocumentID = new int { };

            ViewBag.VendorDisplayName = "";
            ViewBag.PageLayout = pageLayout;

            VendorID = vendorDocumentViewModel.vendorDocument.CustID;

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Vendor ID supplied to Document List";
                return RedirectToAction("Error", "Admin");
            }
            else
            {
                ViewBag.VendorDisplayName = checkCustomerResult.DisplayName;
            }

            if (ModelState.IsValid)
            {
                DocumentID = vendorDocumentViewModel.vendorDocument.DocumentID;

                if (DocumentID == 0)
                {
                    if (documentFile != null && documentFile.ContentLength > 0)
                    {
                        string folderName = Server.MapPath(Constants.DocumentFolderPath) + VendorID.ToString();
                        if (!System.IO.Directory.Exists(folderName))
                        {
                            System.IO.Directory.CreateDirectory(folderName);
                        }
                        String fileExtension = Path.GetExtension(documentFile.FileName).ToLower();
                        String allowedExtensionsStr = ".doc.docx.pdf.xls.xlsx";
                        int findExt = allowedExtensionsStr.IndexOf(fileExtension);
                        if (findExt >= 0)
                        {
                            string fullPath = Path.Combine(folderName, Path.GetFileName(documentFile.FileName));

                            if (System.IO.File.Exists(fullPath))
                            {
                                ModelState.AddModelError("", "A file with this name already exists on the server for this Vendor");
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
                            ModelState.AddModelError("", "The file type was not valid. Valid file types are .doc, .docx and .pdf");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "You have not specified a file.");
                    }

                    if (ModelState.IsValid)
                    {

                        ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                        ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                        string documentPath = Constants.DocumentFolderPath + VendorID.ToString();

                        db.LB_InsertDocument(VendorID, 0, "", 0,
                             Path.GetFileName(documentFile.FileName),
                            documentPath,
                            vendorDocumentViewModel.vendorDocument.TemplateID,
                            "Vendor", "",
                            vendorDocumentViewModel.vendorDocument.DocumentName,
                            vendorDocumentViewModel.vendorDocument.DocumentDescription,
                            vendorDocumentViewModel.vendorDocument.HideFlag,
                            AdminID,
                            returnID, returnMessage);

                        var scalarDocumentID = (int)returnID.Value;
                        var errorMessage = (string)returnMessage.Value ?? "";

                        if (scalarDocumentID == -1)
                        {
                            ModelState.AddModelError("", errorMessage);
                        }
                    }
                }
                else
                {
                    //
                    //
                    // Need to decide whether we want to allow edit to upload a new version of the file or not
                    //
                    //

                    // Update Program
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateDocument(VendorID, DocumentID, "", 0,
                        vendorDocumentViewModel.vendorDocument.TemplateID,
                        "Vendor", "",
                        vendorDocumentViewModel.vendorDocument.DocumentName,
                        vendorDocumentViewModel.vendorDocument.DocumentDescription,
                        vendorDocumentViewModel.vendorDocument.HideFlag,
                        AdminID,
                        returnID, returnMessage);

                    var scalarDocumentID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarDocumentID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
            }

            var thisDocumentList = db.Database.SqlQuery<Document>("exec dbo.[LB_GetDocumentsByCustID] @CustID",
            new SqlParameter("@CustID", VendorID)
            ).ToList();

            vendorDocumentViewModel.VendorDocumentList = thisDocumentList;

            if (ModelState.IsValid)
            {
                var emptyVendorDocument = new Document() { CustID = VendorID };
                vendorDocumentViewModel.vendorDocument = emptyVendorDocument;
                ModelState.Clear();
                ViewBag.ErrorCode = 0;
            }
            else
            {
                ViewBag.ErrorCode = 1;
            }

            if (pageLayout == "I")
            {
                ViewBag.NextAction = "";
                return View("Documents", "~/Views/Shared/_IFrameLayout.cshtml", vendorDocumentViewModel);
            }
            else
            {
                ViewBag.NextAction = "Requirements";
                return View(vendorDocumentViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DocuSign([Bind(Include = "vendorDocument")]VendorDocumentViewModel vendorDocumentViewModel, string pageLayout = "")
        {
            var DataHelper = new DataHelpers();

            int VendorID = new int { };
            int DocumentID = new int { };

            ViewBag.VendorDisplayName = "";
            ViewBag.PageLayout = pageLayout;

            VendorID = vendorDocumentViewModel.vendorDocument.CustID;

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Vendor ID supplied to Document List";
                return RedirectToAction("Error", "Admin");
            }
            else
            {
                ViewBag.VendorDisplayName = checkCustomerResult.DisplayName;
            }

            if (ModelState.IsValid)
            {
                DocumentID = vendorDocumentViewModel.vendorDocument.DocumentID;

                if (DocumentID == 0)
                {
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    string documentPath = Constants.DocumentFolderPath + VendorID.ToString();

                    db.LB_InsertDocument(VendorID, 0, "", 0,
                        "",
                        "",
                        vendorDocumentViewModel.vendorDocument.TemplateID,
                        "Vendor", "",
                        vendorDocumentViewModel.vendorDocument.DocumentName,
                        vendorDocumentViewModel.vendorDocument.DocumentDescription,
                        vendorDocumentViewModel.vendorDocument.HideFlag,
                        AdminID,
                        returnID, returnMessage);

                    var scalarDocumentID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarDocumentID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                else
                {
                    // Update Document
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateDocument(VendorID, DocumentID, "", 0,
                        vendorDocumentViewModel.vendorDocument.TemplateID,
                        "Vendor", "",
                        vendorDocumentViewModel.vendorDocument.DocumentName,
                        vendorDocumentViewModel.vendorDocument.DocumentDescription,
                        vendorDocumentViewModel.vendorDocument.HideFlag,
                        AdminID,
                        returnID, returnMessage);

                    var scalarDocumentID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarDocumentID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
            }

            var thisDocumentList = db.Database.SqlQuery<Document>("exec dbo.[LB_GetDocumentsByCustID] @CustID",
            new SqlParameter("@CustID", VendorID)
            ).ToList();

            vendorDocumentViewModel.VendorDocumentList = thisDocumentList;

            if (ModelState.IsValid)
            {
                var emptyVendorDocument = new Document() { CustID = VendorID };
                vendorDocumentViewModel.vendorDocument = emptyVendorDocument;
                ModelState.Clear();
                ViewBag.ErrorCode = 0;
            }
            else
            {
                ViewBag.ErrorCode = 1;
            }

            if (pageLayout == "I")
            {
                ViewBag.NextAction = "";
                return View("Documents", "~/Views/Shared/_IFrameLayout.cshtml", vendorDocumentViewModel);
            }
            else
            {
                ViewBag.NextAction = "Requirements";
                return View("Documents", vendorDocumentViewModel);
            }
        }


        //
        // REQUIREMENT SECTION
        //

        [HttpGet]
        public ActionResult Requirements(int VendorID = 0)
        {
            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Vendor ID supplied to Requirement List";
                return RedirectToAction("Error", "Admin");
            }
            ViewBag.VendorID = VendorID;
            ViewBag.NextAction = "AccountProfile";
            return View();
        }

        [HttpGet]
        public ActionResult RequirementList(int VendorID = 0, int RequirementID = 0, string ActionType = "")
        {
            var DataHelper = new DataHelpers();

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Vendor ID provided, unable to retrieve Requirement List";
                return PartialView("_RequirementList");
            }

            ViewBag.VendorDisplayName = checkCustomerResult.DisplayName;

            var vendorRequirement = new VendorRequirement()
            {
                CustID = VendorID
            };

            if (RequirementID > 0)
            {
                // Check to see if Action is Delete
                if (ActionType == "D")
                {
                    ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_DeleteVendorRequirement(RequirementID, AdminID,
                        returnID, returnMessage);

                    var scalarID = (int)returnID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
                else
                {
                    // Check Requirement
                    vendorRequirement = db.Database.SqlQuery<VendorRequirement>("exec dbo.[LB_GetVendorRequirementByID] @RequirementID",
                    new SqlParameter("@RequirementID", RequirementID)
                    ).First();

                    if (vendorRequirement == null)
                    {
                        // Requirement not found so error
                        ModelState.AddModelError("", "Requirement was not found and could not be retrieved");
                    }
                }
            }

            // Initalize the View Model

            var vendorRequirementViewModel = new VendorRequirementViewModel();

            vendorRequirementViewModel.vendorRequirement = vendorRequirement;

            var requirementList = db.Database.SqlQuery<VendorRequirement>("exec dbo.[LB_GetVendorRequirements] @CustID",
            new SqlParameter("@CustID", VendorID)
            ).ToList();

            vendorRequirementViewModel.VendorRequirementList = requirementList;

            IEnumerable<System.Web.Mvc.SelectListItem> documentList = DataHelper.GetVendorDocumentSelectList(VendorID, false, false);

            vendorRequirementViewModel.VendorDocumentList = documentList;

            IEnumerable<System.Web.Mvc.SelectListItem> requirementTypeList = DataHelper.GetVendorRequirementTypeList();

            IEnumerable<System.Web.Mvc.SelectListItem> programList = DataHelper.GetVendorProgramSelectList(VendorID, true, false);

            vendorRequirementViewModel.VendorProgramList = programList;

            vendorRequirementViewModel.VendorRequirementTypeList = requirementTypeList;

            return PartialView("_RequirementList", vendorRequirementViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequirementList([Bind(Include = "vendorRequirement")] VendorRequirementViewModel vendorRequirementViewModel)
        {
            var DataHelper = new DataHelpers();

            int VendorID = new int { };
            int RequirementID = new int { };

            ViewBag.VendorDisplayName = "";

            VendorID = vendorRequirementViewModel.vendorRequirement.CustID;

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(VendorID, Constants.vendorCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                ViewBag.ErrorCode = Constants.fatalErrorCode;
                ViewBag.ErrorMessage = "Invalid Vendor ID provided, unable to retrieve Requirement List";
                return PartialView("_RequirementList");
            }
            else
            {
                ViewBag.VendorDisplayName = checkCustomerResult.DisplayName;
            }

            if (ModelState.IsValid)
            {
                RequirementID = vendorRequirementViewModel.vendorRequirement.RequirementID;

                if (RequirementID == 0)
                {
                    // Make sure if RequirementType is Document that we have a Document ID

                    if (vendorRequirementViewModel.vendorRequirement.RequirementType == "Document" && vendorRequirementViewModel.vendorRequirement.DocumentID == 0)
                    {
                        ModelState.AddModelError("", "A Document must be selected for Requirements of Type Document");

                    }
                    else
                    {
                        // Add Requirement 
                        ObjectParameter returnRequirementID = new ObjectParameter("returnRequirementID", typeof(int));
                        ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                        db.LB_InsertVendorRequirement(VendorID,
                            vendorRequirementViewModel.vendorRequirement.ProgramID,
                            vendorRequirementViewModel.vendorRequirement.RequirementType,
                            vendorRequirementViewModel.vendorRequirement.RequirementName,
                            vendorRequirementViewModel.vendorRequirement.RequirementDescription,
                            vendorRequirementViewModel.vendorRequirement.DocumentID,
                            AdminID,
                            returnRequirementID, returnMessage);

                        var scalarRequirementID = (int)returnRequirementID.Value;
                        var errorMessage = (string)returnMessage.Value ?? "";

                        if (scalarRequirementID == -1)
                        {
                            ModelState.AddModelError("", errorMessage);
                        }
                    }
                }
                else
                {
                    // Update Requirement
                    ObjectParameter returnRequirementID = new ObjectParameter("returnRequirementID", typeof(int));
                    ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                    db.LB_UpdateVendorRequirement(VendorID,
                        vendorRequirementViewModel.vendorRequirement.ProgramID,
                        vendorRequirementViewModel.vendorRequirement.RequirementID,
                        vendorRequirementViewModel.vendorRequirement.RequirementName,
                        vendorRequirementViewModel.vendorRequirement.RequirementDescription,
                        AdminID,
                        returnRequirementID, returnMessage);

                    var scalarRequirementID = (int)returnRequirementID.Value;
                    var errorMessage = (string)returnMessage.Value ?? "";

                    if (scalarRequirementID == -1)
                    {
                        ModelState.AddModelError("", errorMessage);
                    }
                }
            }

            IEnumerable<System.Web.Mvc.SelectListItem> documentList = DataHelper.GetVendorDocumentSelectList(VendorID, false, false);

            vendorRequirementViewModel.VendorDocumentList = documentList;

            IEnumerable<System.Web.Mvc.SelectListItem> programList = DataHelper.GetVendorProgramSelectList(VendorID, true, false);

            vendorRequirementViewModel.VendorProgramList = programList;

            IEnumerable<System.Web.Mvc.SelectListItem> requirementTypeList = DataHelper.GetVendorRequirementTypeList();

            vendorRequirementViewModel.VendorRequirementTypeList = requirementTypeList;

            var requirementList = db.Database.SqlQuery<VendorRequirement>("exec dbo.[LB_GetVendorRequirements] @CustID",
            new SqlParameter("@CustID", VendorID)
            ).ToList();

            vendorRequirementViewModel.VendorRequirementList = requirementList;

            if (ModelState.IsValid)
            {
                // Add empty items to View Model and clear out Model State
                // Need to populate the CustID property because the View form needs this
                var emptyVendorRequirement = new VendorRequirement()
                {
                    CustID = VendorID
                };

                vendorRequirementViewModel.vendorRequirement = emptyVendorRequirement;
                ModelState.Clear();

                ViewBag.ErrorCode = 0;
            }
            else
            {
                ViewBag.ErrorCode = 1;
            }

            return PartialView("_RequirementList", vendorRequirementViewModel);
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
