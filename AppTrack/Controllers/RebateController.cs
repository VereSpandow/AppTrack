using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

using System.Security.Claims;
using System.Collections.Generic;
using System.Web;
using System.Web.WebPages;
using System.Web.UI.DataVisualization.Charting;
using System.Text;
using System.IO;
using System.Drawing; 

using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using AppTrack.Helpers;
using System.Text.RegularExpressions;
using System.Data;
using System.Configuration;

namespace AppTrack.Controllers

{
    [AuthorizeAdminRedirect(Roles = "Accounting,Finance")]
    public class RebateController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        private static DataTable ProcessCSV(string filename)
        {
            string line = string.Empty;
            string[] strArray;
            DataTable dt = new DataTable();

            DataRow row;

            // Read each record and import into Tmp table
            // work out where we should split on comma, but not in a sentence
            Regex r = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            //Set the filename in to our stream

            StreamReader sr = new StreamReader(filename);

            //Read the first line and split the string at , with our regular expression in to an array
            line = sr.ReadLine();
            strArray = r.Split(line);

            Array.ForEach(strArray, s => dt.Columns.Add(new DataColumn()));

            while ((line = sr.ReadLine()) != null)
            {
                row = dt.NewRow();
                row.ItemArray = r.Split(line);
                dt.Rows.Add(row);

            }

            sr.Dispose();

            return dt;
        }

        private static String ProcessBulkCopy(DataTable dt, string TableName)
        {
            string Feedback = "Error: unable to connect to database server";
            string connString = ConfigurationManager.ConnectionStrings["IdentityConnection"].ConnectionString;

            //make our connection and dispose at the end
            using (SqlConnection conn = new SqlConnection(connString))
            {
                //make our command and dispose at the end
                using (var copy = new SqlBulkCopy(conn))
                {

                    //Open our connection
                    conn.Open();

                    ///Set target table and tell the number of rows
                    copy.DestinationTableName = TableName;
                    copy.BatchSize = dt.Rows.Count;
                    try
                    {
                        //Send it to the server
                        copy.WriteToServer(dt);
                        Feedback = "Success";
                    }
                    catch (Exception ex)
                    {
                        Feedback = ex.Message;
                    }
                }
            }

            return Feedback;
        }

        //
        // IMPORT FILE UPLOAD SECTION
        //
        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult Import()
        {
            var DataHelper = new DataHelpers();
            // Initalize the View Model

            var rebateFileUploadViewModel = new RebateFileUploadViewModel();

            IEnumerable<System.Web.Mvc.SelectListItem> vendorList = DataHelper.GetRebateVendorList(true,false);

            IEnumerable<System.Web.Mvc.SelectListItem> volumeList = DataHelper.GetRebateVolumeList(Constants.memberRebateVolumeType, true, false);

            rebateFileUploadViewModel.VendorList = vendorList;

            rebateFileUploadViewModel.VolumeList = volumeList;

            return View(rebateFileUploadViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase documentFile, int VendorID = 0, int VolumeID = 0)
        {
            var DataHelper = new DataHelpers();

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

            string fullPath = "";
            int BatchID = 0;

            if (ModelState.IsValid)
            {
                if (documentFile != null && documentFile.ContentLength > 0)
                {
                    string folderName = Server.MapPath("~/App_Data/Vendor/RebateFiles");
                    if (!System.IO.Directory.Exists(folderName))
                    {
                        System.IO.Directory.CreateDirectory(folderName);
                    }
                    String fileExtension = Path.GetExtension(documentFile.FileName).ToLower();
                    String allowedExtensionsStr = ".csv";
                    int findExt = allowedExtensionsStr.IndexOf(fileExtension);
                    if (findExt >= 0)
                    {
                        fullPath = Path.Combine(folderName, Path.GetFileName(documentFile.FileName));

                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                        try
                        {
                            documentFile.SaveAs(fullPath);
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("", "ERROR:" + ex.Message.ToString());
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "The file type was not valid. Valid file type is csv");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "You have not specified a file.");
                }

                if (ModelState.IsValid)
                {
                    db.LB_TruncateTmp_VendorRebateTransactions();
                    
                    DataTable dt = new DataTable();

                    string result = "";
                    int rowCount = 0;
                    try
                    {
                        dt = ProcessCSV(fullPath);

                        rowCount = dt.Rows.Count;
                    }
                    catch
                    {
                        result = "Error reading input file" + fullPath;
                    }

                    if (result == "")
                    {
                        try
                        {
                            result = ProcessBulkCopy(dt, "Tmp_VendorRebateTransactions");
                        }
                        catch
                        {
                            result = "Error uploading file to Database";
                        }
                    }

                    dt.Dispose();

                    if (result != "Success")
                    {
                        ModelState.AddModelError("", result);
                    }
                    else
                    {
                        ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                        ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                        db.LB_InsertVendorRebateTransactions(VendorID, VolumeID, Path.GetFileName(documentFile.FileName), rowCount, AdminID, returnID, returnMessage);

                        var scalarID = (int)returnID.Value;
                        var errorMessage = (string)returnMessage.Value ?? "";

                        if (scalarID == -1)
                        {
                            ModelState.AddModelError("", errorMessage);
                        }
                        else
                        {
                            BatchID = scalarID;
                        }

                    }
                }
            }

            var rebateFileUploadViewModel = new RebateFileUploadViewModel();

            rebateFileUploadViewModel.VendorID = VendorID;
            rebateFileUploadViewModel.VolumeID = VolumeID;

            IEnumerable<System.Web.Mvc.SelectListItem> vendorList = DataHelper.GetRebateVendorList(true, false);

            IEnumerable<System.Web.Mvc.SelectListItem> volumeList = DataHelper.GetRebateVolumeList(Constants.memberRebateVolumeType, true, false);

            rebateFileUploadViewModel.VendorList = vendorList;

            rebateFileUploadViewModel.VolumeList = volumeList;

            if (ModelState.IsValid)
            {
                return RedirectToAction("BatchImportDetail", "Rebate", new { id = BatchID });
            }
            else
            {
                return View(rebateFileUploadViewModel);
            }
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult BatchImportList()
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model

            var rebateBatchImportListViewModel = new RebateBatchImportListViewModel();

            rebateBatchImportListViewModel.SearchStartDate = DateTime.Now.AddDays(-7);
            rebateBatchImportListViewModel.SearchEndDate = DateTime.Now.AddDays(1);
            rebateBatchImportListViewModel.VendorID = 0;
            rebateBatchImportListViewModel.VolumeID = 0;
            rebateBatchImportListViewModel.SearchStatus = " ";

            rebateBatchImportListViewModel.RebateImportBatchList = db.Database.SqlQuery<RebateImportBatch>("exec dbo.[LB_GetVendorRebateImportList] @StartDate, @EndDate, @VendorID, @VolumeID, @Status",
            new SqlParameter("@StartDate", rebateBatchImportListViewModel.SearchStartDate),
            new SqlParameter("@EndDate", rebateBatchImportListViewModel.SearchEndDate),
            new SqlParameter("@VendorID", rebateBatchImportListViewModel.VendorID),
            new SqlParameter("@VolumeID", rebateBatchImportListViewModel.VolumeID),
            new SqlParameter("@Status", rebateBatchImportListViewModel.SearchStatus)
            ).ToList();

            rebateBatchImportListViewModel.VendorList = DataHelper.GetRebateVendorList();

            rebateBatchImportListViewModel.VolumeList = DataHelper.GetRebateVolumeList(Constants.memberRebateVolumeType);
            rebateBatchImportListViewModel.StatusList = DataHelper.GetStatusSelectList(Constants.vendorRebateLookupGroupID, false, true);

            return View(rebateBatchImportListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BatchImportList(RebateBatchImportListViewModel rebateBatchImportListViewModel)
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model
            if (ModelState.IsValid)
            {

                DateTime endDate = rebateBatchImportListViewModel.SearchEndDate.AddDays(1);

                rebateBatchImportListViewModel.RebateImportBatchList = db.Database.SqlQuery<RebateImportBatch>("exec dbo.[LB_GetVendorRebateImportList] @StartDate, @EndDate, @VendorID, @VolumeID, @Status",
                new SqlParameter("@StartDate", rebateBatchImportListViewModel.SearchStartDate),
                new SqlParameter("@EndDate", endDate),
                new SqlParameter("@VendorID", rebateBatchImportListViewModel.VendorID),
                new SqlParameter("@VolumeID", rebateBatchImportListViewModel.VolumeID),
                new SqlParameter("@Status", rebateBatchImportListViewModel.SearchStatus)
                ).ToList();
            }

            rebateBatchImportListViewModel.VendorList = DataHelper.GetRebateVendorList();
            rebateBatchImportListViewModel.VolumeList = DataHelper.GetRebateVolumeList(Constants.memberRebateVolumeType);
            rebateBatchImportListViewModel.StatusList = DataHelper.GetStatusSelectList(Constants.vendorRebateLookupGroupID, false, true);

            return View(rebateBatchImportListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteImportBatch(int BatchID = 0)
        {
            var DataHelper = new DataHelpers();

            if (BatchID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Batch ID supplied to Delete Batch";
                return RedirectToAction("Error", "Admin");
            }

            ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
            ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

            db.LB_DeleteVendorRebateTransactionBatch(BatchID, AdminID, returnID, returnMessage);

            var scalarID = (int)returnID.Value;
            var errorMessage = (string)returnMessage.Value ?? "";

            if (scalarID == -1)
            {
                ModelState.AddModelError("", errorMessage);
            }
            var rebateBatchImportListViewModel = new RebateBatchImportListViewModel();

            rebateBatchImportListViewModel.SearchStartDate = DateTime.Now.AddDays(-7);
            rebateBatchImportListViewModel.SearchEndDate = DateTime.Now;
            rebateBatchImportListViewModel.VendorID = 0;
            rebateBatchImportListViewModel.VolumeID = 0;
            rebateBatchImportListViewModel.SearchStatus = " ";

            DateTime endDate = rebateBatchImportListViewModel.SearchEndDate.AddDays(1);

            rebateBatchImportListViewModel.RebateImportBatchList = db.Database.SqlQuery<RebateImportBatch>("exec dbo.[LB_GetVendorRebateImportList] @StartDate, @EndDate, @VendorID, @VolumeID, @Status",
            new SqlParameter("@StartDate", rebateBatchImportListViewModel.SearchStartDate),
            new SqlParameter("@EndDate", endDate),
            new SqlParameter("@VendorID", rebateBatchImportListViewModel.VendorID),
            new SqlParameter("@VolumeID", rebateBatchImportListViewModel.VolumeID),
            new SqlParameter("@Status", rebateBatchImportListViewModel.SearchStatus)
            ).ToList();

            rebateBatchImportListViewModel.VendorList = DataHelper.GetRebateVendorList();

            rebateBatchImportListViewModel.VolumeList = DataHelper.GetRebateVolumeList(Constants.memberRebateVolumeType);
            rebateBatchImportListViewModel.StatusList = DataHelper.GetStatusSelectList(Constants.vendorRebateLookupGroupID, false, true);

            return View("BatchImportList", rebateBatchImportListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveImportBatch(int BatchID = 0)
        {
            var DataHelper = new DataHelpers();

            if (BatchID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Batch ID supplied to Approve Batch";
                return RedirectToAction("Error", "Admin");
            }

            ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
            ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

            db.LB_ApproveVendorRebateTransactionBatch(BatchID, AdminID, returnID, returnMessage);

            var scalarID = (int)returnID.Value;
            var errorMessage = (string)returnMessage.Value ?? "";

            if (scalarID == -1)
            {
                ModelState.AddModelError("", errorMessage);
            }
            var rebateBatchImportListViewModel = new RebateBatchImportListViewModel();

            rebateBatchImportListViewModel.SearchStartDate = DateTime.Now.AddDays(-7);
            rebateBatchImportListViewModel.SearchEndDate = DateTime.Now;
            rebateBatchImportListViewModel.VendorID = 0;
            rebateBatchImportListViewModel.VolumeID = 0;
            rebateBatchImportListViewModel.SearchStatus = " ";

            DateTime endDate = rebateBatchImportListViewModel.SearchEndDate.AddDays(1);

            rebateBatchImportListViewModel.RebateImportBatchList = db.Database.SqlQuery<RebateImportBatch>("exec dbo.[LB_GetVendorRebateImportList] @StartDate, @EndDate, @VendorID, @VolumeID, @Status",
            new SqlParameter("@StartDate", rebateBatchImportListViewModel.SearchStartDate),
            new SqlParameter("@EndDate", endDate),
            new SqlParameter("@VendorID", rebateBatchImportListViewModel.VendorID),
            new SqlParameter("@VolumeID", rebateBatchImportListViewModel.VolumeID),
            new SqlParameter("@Status", rebateBatchImportListViewModel.SearchStatus)
            ).ToList();

            rebateBatchImportListViewModel.VendorList = DataHelper.GetRebateVendorList();

            rebateBatchImportListViewModel.VolumeList = DataHelper.GetRebateVolumeList(Constants.memberRebateVolumeType);
            rebateBatchImportListViewModel.StatusList = DataHelper.GetStatusSelectList(Constants.vendorRebateLookupGroupID, false, true);

            return View("BatchImportList", rebateBatchImportListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult BatchImportDetail(int id)
        {

            var DataHelper = new DataHelpers();
            // Initalize the View Model

            var rebateBatchImportDetailViewModel = new RebateBatchImportDetailViewModel();
            rebateBatchImportDetailViewModel.VendorName = "";

            int BatchID = id;

            var rebateImportBatch = new RebateImportBatch();

            rebateBatchImportDetailViewModel.RebateTransactionList = db.Database.SqlQuery<C_VendorRebateTransactions>("exec dbo.[LB_GetVendorRebateTransactions] @BatchID",
            new SqlParameter("@BatchID", BatchID)
             ).ToList();

            if (rebateBatchImportDetailViewModel.RebateTransactionList == null)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Could not find Batch ID supplied to Rebate Transaction list";
                return RedirectToAction("Error", "Admin");
            }

            rebateImportBatch = db.Database.SqlQuery<RebateImportBatch>("exec dbo.[LB_GetVendorRebateImportByBatchID] @BatchID",
            new SqlParameter("@BatchID", BatchID)
             ).FirstOrDefault();

            rebateBatchImportDetailViewModel.VendorName = rebateImportBatch.VendorName;
            rebateBatchImportDetailViewModel.RebateType = rebateImportBatch.Description;
            rebateBatchImportDetailViewModel.BatchStatus = rebateImportBatch.Status;
            rebateBatchImportDetailViewModel.PendingCount = rebateImportBatch.PendingCount;

            rebateBatchImportDetailViewModel.BatchID = BatchID;
            rebateBatchImportDetailViewModel.VendorPayeeID = "";
            rebateBatchImportDetailViewModel.SearchStatus = " ";

            IEnumerable<System.Web.Mvc.SelectListItem> searchList = DataHelper.GetStatusSelectList(Constants.vendorRebateLookupGroupID, false, true);

            rebateBatchImportDetailViewModel.StatusList = searchList;

            return View(rebateBatchImportDetailViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BatchImportDetail(RebateBatchImportDetailViewModel rebateBatchImportDetailViewModel)
        {

            var DataHelper = new DataHelpers();
            // Initalize the View Model

            int zero = 0;

            var rebateImportBatch = new RebateImportBatch();
            rebateImportBatch = db.Database.SqlQuery<RebateImportBatch>("exec dbo.[LB_GetVendorRebateImportByBatchID] @BatchID",
            new SqlParameter("@BatchID", rebateBatchImportDetailViewModel.BatchID)
             ).FirstOrDefault();

            if (rebateImportBatch == null)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Batch ID supplied to Batch Detail List";
                return RedirectToAction("Error", "Admin");
            }

            rebateBatchImportDetailViewModel.VendorName = rebateImportBatch.VendorName;
            rebateBatchImportDetailViewModel.RebateType = rebateImportBatch.Description;
            rebateBatchImportDetailViewModel.BatchStatus = rebateImportBatch.Status;
            rebateBatchImportDetailViewModel.PendingCount = rebateImportBatch.PendingCount;
            
            if (ModelState.IsValid)
            {
                rebateBatchImportDetailViewModel.RebateTransactionList = db.Database.SqlQuery<C_VendorRebateTransactions>("exec dbo.[LB_GetVendorRebateTransactions] @BatchID, @PeriodID, @VendorID, @VolumeID, @CustID, @SecID, @VendorPayeeID, @PayeeName, @Status",
                new SqlParameter("@BatchID", rebateBatchImportDetailViewModel.BatchID),
                new SqlParameter("@PeriodID", zero),
                new SqlParameter("@VendorID", zero),
                new SqlParameter("@VolumeID", zero),
                new SqlParameter("@CustID", zero),
                new SqlParameter("@SecID", " "),
                new SqlParameter("@PayeeName", rebateBatchImportDetailViewModel.PayeeName ?? ""),
                new SqlParameter("@VendorPayeeID", rebateBatchImportDetailViewModel.VendorPayeeID ?? ""),
                new SqlParameter("@Status", rebateBatchImportDetailViewModel.SearchStatus)
                 ).ToList();
            }
            IEnumerable<System.Web.Mvc.SelectListItem> searchList = DataHelper.GetStatusSelectList(Constants.vendorRebateLookupGroupID, false, true);

            rebateBatchImportDetailViewModel.StatusList = searchList;

            return View(rebateBatchImportDetailViewModel);
        }

        //
        // FIND PAYEE SECTION
        //
        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult FindVendorPayee(int id)
        {
            var DataHelper = new DataHelpers();
            // Initalize the View Model

            var rebateFindPayeeModel = new RebateFindPayeeModel();

            int TranID = id;

            rebateFindPayeeModel.TransactionID = TranID;

            rebateFindPayeeModel.VendorRebateTransaction = db.Database.SqlQuery<C_VendorRebateTransactions>("exec dbo.[LB_GetVendorRebateTransactionByID] @TranID",
            new SqlParameter("@TranID", TranID)
            ).FirstOrDefault();

            rebateFindPayeeModel.MemberList = db.Database.SqlQuery<CustomerBasic>("exec dbo.[LB_GetMemberListByRebateTransactionID] @TranID",
            new SqlParameter("@TranID", TranID)
             ).ToList();

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList(false, false);

            rebateFindPayeeModel.StateList = stateList;

            return PartialView("_FindVendorPayee", rebateFindPayeeModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FindVendorPayee(RebateFindPayeeModel rebateFindPayeeModel)
        {
            var DataHelper = new DataHelpers();

            if (ModelState.IsValid)
            {
                rebateFindPayeeModel.VendorRebateTransaction = db.Database.SqlQuery<C_VendorRebateTransactions>("exec dbo.[LB_GetVendorRebateTransactionByID] @TranID",
                new SqlParameter("@TranID", rebateFindPayeeModel.TransactionID)
                ).FirstOrDefault();

                if (rebateFindPayeeModel.SearchCompany == null && rebateFindPayeeModel.SearchDisplayName == null && rebateFindPayeeModel.SearchLastName == null && rebateFindPayeeModel.SearchAddress1 == null && rebateFindPayeeModel.SearchCity == null && rebateFindPayeeModel.SearchState != " " && rebateFindPayeeModel.SearchPostalCode == null && rebateFindPayeeModel.SearchPhone == null)
                {
                    ModelState.AddModelError("", "At least one of the search criteria inputs is required");

                    rebateFindPayeeModel.MemberList = db.Database.SqlQuery<CustomerBasic>("exec dbo.[LB_GetMemberListByRebateTransactionID] @TranID",
                    new SqlParameter("@TranID", rebateFindPayeeModel.TransactionID)
                     ).ToList();
                }
                else
                {
                    rebateFindPayeeModel.MemberList = db.Database.SqlQuery<CustomerBasic>("exec dbo.[LB_GetMemberLocationByNameAddressPhone] @Company, @DisplayName, @LastName, @Address1, @City, @State, @PostalCode, @Phone, @SecID",
                    new SqlParameter("@Company", rebateFindPayeeModel.SearchCompany ?? ""),
                    new SqlParameter("@DisplayName", rebateFindPayeeModel.SearchDisplayName ?? ""),
                    new SqlParameter("@LastName", rebateFindPayeeModel.SearchLastName ?? ""),
                    new SqlParameter("@Address1", rebateFindPayeeModel.SearchAddress1 ?? ""),
                    new SqlParameter("@City", rebateFindPayeeModel.SearchCity ?? ""),
                    new SqlParameter("@State", rebateFindPayeeModel.SearchState ?? ""),
                    new SqlParameter("@PostalCode", rebateFindPayeeModel.SearchPostalCode ?? ""),
                    new SqlParameter("@Phone", rebateFindPayeeModel.SearchPhone ?? ""),
                    new SqlParameter("@SecID", rebateFindPayeeModel.SearchSecID ?? "")
                    ).ToList();

                    if (rebateFindPayeeModel.MemberList.Count > 100)
                    {
                        ModelState.AddModelError("", "Search resulted in more than 100 results.  Please try again.");

                        rebateFindPayeeModel.MemberList = db.Database.SqlQuery<CustomerBasic>("exec dbo.[LB_GetMemberListByRebateTransactionID] @TranID",
                        new SqlParameter("@TranID", rebateFindPayeeModel.TransactionID)
                         ).ToList();
                    }
                }
            }

            IEnumerable<System.Web.Mvc.SelectListItem> stateList = DataHelper.GetStateSelectList(false, false);

            rebateFindPayeeModel.StateList = stateList;

            return PartialView("_FindVendorPayee", rebateFindPayeeModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateVendorPayee(int CustID = 0, int TransactionID = 0)
        {
            var DataHelper = new DataHelpers();

            if (CustID == 0 || TransactionID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Member/Location or Transaction ID supplied to Vendor Payee Update";
                return RedirectToAction("Error", "Admin");
            }

            CheckCustomerResult checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.memberCustomerType);

            if (checkCustomerResult.ErrorCode != 0)
            {
                checkCustomerResult = DataHelper.CheckCustomer(CustID, Constants.locationCustomerType);

                if (checkCustomerResult.ErrorCode != 0)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Invalid Member/Location ID supplied to Vendor Payee Update";
                    return RedirectToAction("Error", "Admin");
                }
            }

            ObjectParameter returnBatchID = new ObjectParameter("returnBatchID", typeof(int));
            ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
            ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

            db.LB_UpdateVendorPayeeByTransactionID(TransactionID, CustID, AdminID, returnBatchID, returnID, returnMessage);

            var BatchID = (int)returnBatchID.Value;
            var scalarID = (int)returnID.Value;
            var errorMessage = (string)returnMessage.Value ?? "";

            if (scalarID == -1)
            {
                ModelState.AddModelError("", errorMessage);
            }

            // Initalize the View Model

            var rebateBatchImportDetailViewModel = new RebateBatchImportDetailViewModel();

            var rebateImportBatch = new RebateImportBatch();
            rebateImportBatch = db.Database.SqlQuery<RebateImportBatch>("exec dbo.[LB_GetVendorRebateImportByBatchID] @BatchID",
            new SqlParameter("@BatchID", BatchID)
             ).FirstOrDefault();

            if (rebateImportBatch == null)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Batch ID supplied to Batch Detail List";
                return RedirectToAction("Error", "Admin");
            }

            rebateBatchImportDetailViewModel.VendorName = rebateImportBatch.VendorName;
            rebateBatchImportDetailViewModel.RebateType = rebateImportBatch.Description;
            rebateBatchImportDetailViewModel.BatchStatus = rebateImportBatch.Status;
            rebateBatchImportDetailViewModel.PendingCount = rebateImportBatch.PendingCount;

            rebateBatchImportDetailViewModel.BatchID = BatchID;
            rebateBatchImportDetailViewModel.VendorName = rebateImportBatch.VendorName;
            rebateBatchImportDetailViewModel.RebateType = rebateImportBatch.Description;

            rebateBatchImportDetailViewModel.RebateTransactionList = db.Database.SqlQuery<C_VendorRebateTransactions>("exec dbo.[LB_GetVendorRebateTransactions] @BatchID",
            new SqlParameter("@BatchID", BatchID)
             ).ToList();

            rebateBatchImportDetailViewModel.VendorPayeeID = "";
            rebateBatchImportDetailViewModel.SearchStatus = " ";

            IEnumerable<System.Web.Mvc.SelectListItem> searchList = DataHelper.GetStatusSelectList(Constants.vendorRebateLookupGroupID, false, true);

            rebateBatchImportDetailViewModel.StatusList = searchList;

            return View("BatchImportDetail", rebateBatchImportDetailViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateTransactionStatus(int TransactionID = 0, string Status = "")
        {
            var DataHelper = new DataHelpers();

            if (TransactionID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Transaction ID supplied to Void Rebate Transaction";
                return RedirectToAction("Error", "Admin");
            }

            if (Status == "")
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Status supplied to Void Rebate Transaction";
                return RedirectToAction("Error", "Admin");
            }

            ObjectParameter returnBatchID = new ObjectParameter("returnBatchID", typeof(int));
            ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
            ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

            db.LB_UpdateVendorRebateTransactionStatus(TransactionID, Status, AdminID, returnBatchID, returnID, returnMessage);

            var BatchID = (int)returnBatchID.Value;
            var scalarID = (int)returnID.Value;
            var errorMessage = (string)returnMessage.Value ?? "";

            if (scalarID == -1)
            {
                ModelState.AddModelError("", errorMessage);
            }

            // Initalize the View Model

            var rebateBatchImportDetailViewModel = new RebateBatchImportDetailViewModel();

            var rebateImportBatch = new RebateImportBatch();
            rebateImportBatch = db.Database.SqlQuery<RebateImportBatch>("exec dbo.[LB_GetVendorRebateImportByBatchID] @BatchID",
            new SqlParameter("@BatchID", BatchID)
             ).FirstOrDefault();

            if (rebateImportBatch == null)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Batch ID supplied to Batch Detail List";
                return RedirectToAction("Error", "Admin");
            }

            rebateBatchImportDetailViewModel.VendorName = rebateImportBatch.VendorName;
            rebateBatchImportDetailViewModel.RebateType = rebateImportBatch.Description;
            rebateBatchImportDetailViewModel.BatchStatus = rebateImportBatch.Status;
            rebateBatchImportDetailViewModel.PendingCount = rebateImportBatch.PendingCount;

            rebateBatchImportDetailViewModel.BatchID = BatchID;
            rebateBatchImportDetailViewModel.VendorName = rebateImportBatch.VendorName;
            rebateBatchImportDetailViewModel.RebateType = rebateImportBatch.Description;

            rebateBatchImportDetailViewModel.RebateTransactionList = db.Database.SqlQuery<C_VendorRebateTransactions>("exec dbo.[LB_GetVendorRebateTransactions] @BatchID",
            new SqlParameter("@BatchID", BatchID)
             ).ToList();

            rebateBatchImportDetailViewModel.VendorPayeeID = "";
            rebateBatchImportDetailViewModel.SearchStatus = " ";

            IEnumerable<System.Web.Mvc.SelectListItem> searchList = DataHelper.GetStatusSelectList(Constants.vendorRebateLookupGroupID, false, true);

            rebateBatchImportDetailViewModel.StatusList = searchList;

            return View("BatchImportDetail", rebateBatchImportDetailViewModel);
        }

        [HttpGet]
        public ActionResult RebateSummary()
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model

            var rebateSummaryViewModel = new RebateSummaryViewModel();

            int zero = 0;

            rebateSummaryViewModel.StartPeriodID = zero;
            rebateSummaryViewModel.EndPeriodID = zero;
            rebateSummaryViewModel.CommissionID = Constants.memberRebateCommissionID;
            rebateSummaryViewModel.VendorID = zero;
            rebateSummaryViewModel.VolumeID = zero;

            rebateSummaryViewModel.RebateSummaryList = db.Database.SqlQuery<RebateSummary>("exec dbo.[LB_GetRebateDetailSummary] @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @VendorID, @VolumeID",
            new SqlParameter("@PeriodTypeID", Constants.quarterlyPeriodTypeID),
            new SqlParameter("@StartPeriodID", rebateSummaryViewModel.StartPeriodID),
            new SqlParameter("@EndPeriodID", rebateSummaryViewModel.EndPeriodID),
            new SqlParameter("@CommissionID", rebateSummaryViewModel.CommissionID),
            new SqlParameter("@VendorID", rebateSummaryViewModel.VendorID),
            new SqlParameter("@VolumeID", rebateSummaryViewModel.VolumeID)
            ).ToList();

            rebateSummaryViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.quarterlyPeriodTypeID, 20);
            rebateSummaryViewModel.CommissionList = DataHelper.GetRebateCommissionList();
            rebateSummaryViewModel.VendorList = DataHelper.GetRebateVendorList();
            rebateSummaryViewModel.VolumeList = DataHelper.GetRebateVolumeList(Constants.memberRebateVolumeType);

            return View(rebateSummaryViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RebateSummary(RebateSummaryViewModel rebateSummaryViewModel)
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model
            if (ModelState.IsValid)
            {

                rebateSummaryViewModel.RebateSummaryList = db.Database.SqlQuery<RebateSummary>("exec dbo.[LB_GetRebateDetailSummary] @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @VendorID, @VolumeID",
                new SqlParameter("@PeriodTypeID", Constants.quarterlyPeriodTypeID),
                new SqlParameter("@StartPeriodID", rebateSummaryViewModel.StartPeriodID),
                new SqlParameter("@EndPeriodID", rebateSummaryViewModel.EndPeriodID),
                new SqlParameter("@CommissionID", rebateSummaryViewModel.CommissionID),
                new SqlParameter("@VendorID", rebateSummaryViewModel.VendorID),
                new SqlParameter("@VolumeID", rebateSummaryViewModel.VolumeID)
                ).ToList();

            }

            rebateSummaryViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.quarterlyPeriodTypeID, 20);
            rebateSummaryViewModel.CommissionList = DataHelper.GetRebateCommissionList();
            rebateSummaryViewModel.VendorList = DataHelper.GetRebateVendorList();
            rebateSummaryViewModel.VolumeList = DataHelper.GetRebateVolumeList(Constants.memberRebateVolumeType);

            return View(rebateSummaryViewModel);
        }

        [HttpGet]
        public ActionResult RebateDetail(int StartPeriodID = 0, int EndPeriodID = 0, int CommissionID = Constants.memberRebateCommissionID, int VendorID = 0, int VolumeID = 0, string CustID = "")
        {
            var DataHelper = new DataHelpers();

            int intCustID = 0;

            if (CustID != "")
            {
                if (!Int32.TryParse(CustID, out intCustID))
                    intCustID = 0;
            }
            // Initalize the View Model


            var rebateDetailViewModel = new RebateDetailViewModel();

            rebateDetailViewModel.StartPeriodID = StartPeriodID;
            rebateDetailViewModel.EndPeriodID = EndPeriodID;
            rebateDetailViewModel.CommissionID = CommissionID;
            rebateDetailViewModel.VendorID = VendorID;
            rebateDetailViewModel.VolumeID = VolumeID;
            rebateDetailViewModel.CustID = CustID;

            if (intCustID == 0 && VendorID == 0)
            {
                ModelState.AddModelError("", "Either Vendor ID or Member ID must be supplied");
            }
            else
            {
                rebateDetailViewModel.RebateDetailList = db.Database.SqlQuery<RebateDetail>("exec dbo.[LB_GetRebateDetail] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @VendorID, @VolumeID",
                new SqlParameter("@CustID", intCustID),
                new SqlParameter("@PeriodTypeID", Constants.quarterlyPeriodTypeID),
                new SqlParameter("@StartPeriodID", rebateDetailViewModel.StartPeriodID),
                new SqlParameter("@EndPeriodID", rebateDetailViewModel.EndPeriodID),
                new SqlParameter("@CommissionID", rebateDetailViewModel.CommissionID),
                new SqlParameter("@VendorID", rebateDetailViewModel.VendorID),
                new SqlParameter("@VolumeID", rebateDetailViewModel.VolumeID)
                ).ToList();
            }

            rebateDetailViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.quarterlyPeriodTypeID, 20);
            rebateDetailViewModel.CommissionList = DataHelper.GetRebateCommissionList();
            rebateDetailViewModel.VendorList = DataHelper.GetRebateVendorList();
            rebateDetailViewModel.VolumeList = DataHelper.GetRebateVolumeList(Constants.memberRebateVolumeType);

            return View(rebateDetailViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RebateDetail(RebateDetailViewModel rebateDetailViewModel)
        {
            var DataHelper = new DataHelpers();

            int intCustID = 0;

            if (rebateDetailViewModel.CustID != "")
            {
                if (!Int32.TryParse(rebateDetailViewModel.CustID, out intCustID))
                    intCustID = 0;
            }

            if (intCustID == 0 && rebateDetailViewModel.VendorID == 0)
            {
                ModelState.AddModelError("", "Either Vendor ID or Member ID must be supplied");
            }

            // Initalize the View Model
            if (ModelState.IsValid)
            {

                rebateDetailViewModel.RebateDetailList = db.Database.SqlQuery<RebateDetail>("exec dbo.[LB_GetRebateDetail] @CustID, @PeriodTypeID, @StartPeriodID, @EndPeriodID, @CommissionID, @VendorID, @VolumeID",
                new SqlParameter("@CustID", intCustID),
                new SqlParameter("@PeriodTypeID", Constants.quarterlyPeriodTypeID),
                new SqlParameter("@StartPeriodID", rebateDetailViewModel.StartPeriodID),
                new SqlParameter("@EndPeriodID", rebateDetailViewModel.EndPeriodID),
                new SqlParameter("@CommissionID", rebateDetailViewModel.CommissionID),
                new SqlParameter("@VendorID", rebateDetailViewModel.VendorID),
                new SqlParameter("@VolumeID", rebateDetailViewModel.VolumeID)
                ).ToList();

            }
            else
            {
                rebateDetailViewModel.RebateDetailList = new List<RebateDetail>();
            }
            rebateDetailViewModel.PeriodList = DataHelper.GetPeriodListVS(Constants.quarterlyPeriodTypeID, 20);
            rebateDetailViewModel.CommissionList = DataHelper.GetRebateCommissionList();
            rebateDetailViewModel.VendorList = DataHelper.GetRebateVendorList();
            rebateDetailViewModel.VolumeList = DataHelper.GetRebateVolumeList(Constants.memberRebateVolumeType);

            return View(rebateDetailViewModel);
        }
    }
}