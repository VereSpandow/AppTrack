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
    [AuthorizeAdminRedirect(Roles = "Finance,Accounting")]
    public class SAGEController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        //
        // IMPORT UTILS SECTION
        //

        // This is not a generic version as the column headings are hardcoded.
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

            //Define the column headings for the table since file does not have col headings
            line = "InvoiceNumber,Amount,SageID,CheckNumber,PaymentDate,CheckAmount,PayeeName";
            
            strArray = r.Split(line);

            Array.ForEach(strArray, s => dt.Columns.Add(new DataColumn()));

            //Read each line and split the string at , with our regular expression in to an array and add to table
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
        // IMPORT FILE SECTION
        //
        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult ImportPayout()
        {
            var DataHelper = new DataHelpers();
            // Initalize the View Model

            var payoutFileUploadViewModel = new SagePayoutFileUploadViewModel();

            return View(payoutFileUploadViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportPayout(HttpPostedFileBase documentFile)
        {
            var DataHelper = new DataHelpers();

            string fullPath = "";
            int BatchID = 0;

            if (ModelState.IsValid)
            {
                if (documentFile != null && documentFile.ContentLength > 0)
                {
                    string folderName = Server.MapPath("~/App_Data/SAGE/ImportFiles");
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
                    db.LB_TruncateTmp_ImportPayoutTransactions();

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
                            result = ProcessBulkCopy(dt, "Tmp_ImportPayoutTransactions");
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

                        db.LB_InsertImportPayoutTransactions(Path.GetFileName(documentFile.FileName), rowCount, AdminID, returnID, returnMessage);

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

            var payoutFileUploadViewModel = new SagePayoutFileUploadViewModel();

            if (ModelState.IsValid)
            {
                return RedirectToAction("ImportPayoutDetail", "SAGE", new { id = BatchID });
            }
            else
            {
                return View(payoutFileUploadViewModel);
            }
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult ImportPayoutDetail(int id)
        {
            var DataHelper = new DataHelpers();
            // Initalize the View Model

            var importPayoutBatchDetailViewModel = new SageImportPayoutBatchDetailViewModel();

            int BatchID = id;

            importPayoutBatchDetailViewModel.PayoutTransactionList = db.Database.SqlQuery<C_ImportPayoutTransactions>("exec dbo.[LB_GetImportPayoutTransactions] @BatchID",
            new SqlParameter("@BatchID", BatchID)
             ).ToList();

            if (importPayoutBatchDetailViewModel.PayoutTransactionList == null)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Could not find Batch ID supplied to SAGE Payment Import Detail list";
                return RedirectToAction("Error", "Admin");
            }

            var payoutImportBatch = new C_ImportDataLog();

            payoutImportBatch = db.Database.SqlQuery<C_ImportDataLog>("exec dbo.[LB_GetImportDataLogByBatchID] @BatchID",
            new SqlParameter("@BatchID", BatchID)
             ).FirstOrDefault();

            importPayoutBatchDetailViewModel.ImportBatchID = payoutImportBatch.BatchID;
            importPayoutBatchDetailViewModel.FileName = payoutImportBatch.FileName;
            importPayoutBatchDetailViewModel.BatchStatus = payoutImportBatch.Status;

            importPayoutBatchDetailViewModel.SearchStatus = " ";

            IEnumerable<System.Web.Mvc.SelectListItem> searchList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

            importPayoutBatchDetailViewModel.StatusList = searchList;

            return View(importPayoutBatchDetailViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostPayoutBatch(int BatchID = 0)
        {
            var DataHelper = new DataHelpers();

            if (BatchID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Batch ID supplied to Post Payments";
                return RedirectToAction("Error", "Admin");
            }

            ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
            ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

            db.LB_UpdateSageImportPayoutTransactions(BatchID, AdminID, returnID, returnMessage);

            var scalarID = (int)returnID.Value;
            var errorMessage = (string)returnMessage.Value ?? "";

            if (scalarID == -1)
            {
                ModelState.AddModelError("", errorMessage);
            }
            var importPayoutBatchListViewModel = new SageImportPayoutBatchListViewModel();

            importPayoutBatchListViewModel.SearchStartDate = DateTime.Now.AddDays(-7);
            importPayoutBatchListViewModel.SearchEndDate = DateTime.Now.AddDays(1);
            importPayoutBatchListViewModel.SearchStatus = " ";

            importPayoutBatchListViewModel.ImportPayoutBatchList = db.Database.SqlQuery<C_ImportDataLog>("exec dbo.[LB_GetImportPayoutBatchList] @StartDate, @EndDate, @Status",
            new SqlParameter("@StartDate", importPayoutBatchListViewModel.SearchStartDate),
            new SqlParameter("@EndDate", importPayoutBatchListViewModel.SearchEndDate),
            new SqlParameter("@Status", importPayoutBatchListViewModel.SearchStatus)
            ).ToList();

            importPayoutBatchListViewModel.StatusList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

            return View("ImportList", importPayoutBatchListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult ImportList()
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model

            var importPayoutBatchListViewModel = new SageImportPayoutBatchListViewModel();

            importPayoutBatchListViewModel.SearchStartDate = DateTime.Now.AddDays(-7);
            importPayoutBatchListViewModel.SearchEndDate = DateTime.Now.AddDays(1);
            importPayoutBatchListViewModel.SearchStatus = " ";

            importPayoutBatchListViewModel.ImportPayoutBatchList = db.Database.SqlQuery<C_ImportDataLog>("exec dbo.[LB_GetImportPayoutBatchList] @StartDate, @EndDate, @Status",
            new SqlParameter("@StartDate", importPayoutBatchListViewModel.SearchStartDate),
            new SqlParameter("@EndDate", importPayoutBatchListViewModel.SearchEndDate),
            new SqlParameter("@Status", importPayoutBatchListViewModel.SearchStatus)
            ).ToList();

            importPayoutBatchListViewModel.StatusList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

            return View(importPayoutBatchListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportList(SageImportPayoutBatchListViewModel importPayoutBatchListViewModel)
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model
            if (ModelState.IsValid)
            {
                DateTime endDate = importPayoutBatchListViewModel.SearchEndDate.AddDays(1);

                importPayoutBatchListViewModel.ImportPayoutBatchList = db.Database.SqlQuery<C_ImportDataLog>("exec dbo.[LB_GetImportPayoutBatchList] @StartDate, @EndDate, @Status",
                new SqlParameter("@StartDate", importPayoutBatchListViewModel.SearchStartDate),
                new SqlParameter("@EndDate", endDate),
                new SqlParameter("@Status", importPayoutBatchListViewModel.SearchStatus)
                ).ToList();
            }

            importPayoutBatchListViewModel.StatusList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

            return View(importPayoutBatchListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePayoutBatch(int BatchID = 0)
        {
            var DataHelper = new DataHelpers();

            if (BatchID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Batch ID supplied to Delete Payment Batch";
                return RedirectToAction("Error", "Admin");
            }

            ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
            ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

            db.LB_DeleteImportPayoutBatch(BatchID, AdminID, returnID, returnMessage);

            var scalarID = (int)returnID.Value;
            var errorMessage = (string)returnMessage.Value ?? "";

            if (scalarID == -1)
            {
                ModelState.AddModelError("", errorMessage);
            }
            var importPayoutBatchListViewModel = new SageImportPayoutBatchListViewModel();

            importPayoutBatchListViewModel.SearchStartDate = DateTime.Now.AddDays(-7);
            importPayoutBatchListViewModel.SearchEndDate = DateTime.Now.AddDays(1);
            importPayoutBatchListViewModel.SearchStatus = " ";

            importPayoutBatchListViewModel.ImportPayoutBatchList = db.Database.SqlQuery<C_ImportDataLog>("exec dbo.[LB_GetImportPayoutBatchList] @StartDate, @EndDate, @Status",
            new SqlParameter("@StartDate", importPayoutBatchListViewModel.SearchStartDate),
            new SqlParameter("@EndDate", importPayoutBatchListViewModel.SearchEndDate),
            new SqlParameter("@Status", importPayoutBatchListViewModel.SearchStatus)
            ).ToList();

            importPayoutBatchListViewModel.StatusList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

            return View("ImportList", importPayoutBatchListViewModel);
        }

        
        //
        // EXPORT FILE SECTION
        //

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult DownloadFile(int id)
        {
            int BatchID = id;

            var exportDataLog = new C_ExportDataLog();

            exportDataLog = db.Database.SqlQuery<C_ExportDataLog>("exec dbo.[LB_GetExportDataLogByBatchID] @BatchID",
            new SqlParameter("@BatchID", BatchID)
             ).FirstOrDefault();

            if (exportDataLog == null)
            {
                return HttpNotFound();
            }
            string fileName = exportDataLog.FileName;
            string fileFolder = exportDataLog.Path;

            string fullPath = Server.MapPath(String.Format("~/App_Data/" + fileFolder + "/{0}", fileName));
            if (System.IO.File.Exists(fullPath))
            {
                byte[] filedata = System.IO.File.ReadAllBytes(fullPath);
                string contentType = MimeMapping.GetMimeMapping(fullPath);

                System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = fileName,
                    Inline = true,
                };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(filedata, contentType);
            }
            return HttpNotFound();
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult ExportDetailRouter(int BatchID, string BatchType)
        {
            switch (BatchType.ToUpper())
            {
                case "SAGE INVOICE":
                    return RedirectToAction("ExportOrderDetail", "SAGE", new { id = BatchID });
                case "SAGE PAYMENT":
                    return RedirectToAction("ExportPaymentDetail", "SAGE", new { id = BatchID });
                case "SAGE REBATE":
                    return RedirectToAction("ExportRebateDetail", "SAGE", new { id = BatchID });
                case "SAGE IMD COMMISSIONS":
                    return RedirectToAction("ExportCommissionDetail", "SAGE", new { id = BatchID });
            }
            TempData["ErrorCode"] = Constants.fatalErrorCode;
            TempData["ErrorMessage"] = "Could not determine detail view page based on Batch Type from SAGE Export list";
            return RedirectToAction("Error", "Admin");
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult ExportList(string BatchType)
        {
            var DataHelper = new DataHelpers();

            if (BatchType == null)
            {
                BatchType = "";
            }
            // Initalize the View Model

            var exportBatchListViewModel = new SageExportBatchListViewModel();

            exportBatchListViewModel.SearchStartDate = DateTime.Now.AddDays(-7);
            exportBatchListViewModel.SearchEndDate = DateTime.Now.AddDays(1);
            exportBatchListViewModel.SearchBatchType = BatchType;
            exportBatchListViewModel.SearchStatus = " ";

            exportBatchListViewModel.ExportBatchList = db.Database.SqlQuery<C_ExportDataLog>("exec dbo.[LB_GetExportDataLog] @StartDate, @EndDate, @BatchType, @Status",
            new SqlParameter("@StartDate", exportBatchListViewModel.SearchStartDate),
            new SqlParameter("@EndDate", exportBatchListViewModel.SearchEndDate),
            new SqlParameter("@BatchType", exportBatchListViewModel.SearchBatchType),
            new SqlParameter("@Status", exportBatchListViewModel.SearchStatus)
            ).ToList();

            exportBatchListViewModel.StatusList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

            return View(exportBatchListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportList(SageExportBatchListViewModel exportBatchListViewModel)
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model
            if (ModelState.IsValid)
            {

                DateTime endDate = exportBatchListViewModel.SearchEndDate.AddDays(1);

                exportBatchListViewModel.ExportBatchList = db.Database.SqlQuery<C_ExportDataLog>("exec dbo.[LB_GetExportDataLog] @StartDate, @EndDate, @BatchType, @Status",
                new SqlParameter("@StartDate", exportBatchListViewModel.SearchStartDate),
                new SqlParameter("@EndDate", endDate),
                new SqlParameter("@BatchType", exportBatchListViewModel.SearchBatchType),
                new SqlParameter("@Status", exportBatchListViewModel.SearchStatus)
                ).ToList();
            }

            exportBatchListViewModel.StatusList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

            return View(exportBatchListViewModel);
        }

        //        
        // BEGIN ORDER EXPORT SECTION
        //
        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult ExportOrder()
        {
            var DataHelper = new DataHelpers();

            return View();
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportOrder(SageOrderExportBatchDetailViewModel orderExportBatchDetailViewModel)
        {
            var DataHelper = new DataHelpers();

            int BatchID = 0;

            // Initalize the View Model
            if (ModelState.IsValid)
            {

                string BatchDescription = orderExportBatchDetailViewModel.BatchDescription;

                ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_UpdateSageExportOrders(BatchDescription, orderExportBatchDetailViewModel.CutoffDate, returnID, returnMessage);

                var scalarID = (int)returnID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                    return View();
                }
                else
                {
                    BatchID = scalarID;
                }

                orderExportBatchDetailViewModel.OrderHistoryList = db.Database.SqlQuery<OrderHistory>("exec dbo.[LB_GetOrderHistory] @ExportBatchID",
                new SqlParameter("@ExportBatchID", BatchID)
                 ).ToList();

                if (orderExportBatchDetailViewModel.OrderHistoryList == null)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Could not find Batch ID supplied to Order Export Detail list";
                    return RedirectToAction("Error", "Admin");
                }

                var orderExportBatch = new C_ExportDataLog();

                orderExportBatch = db.Database.SqlQuery<C_ExportDataLog>("exec dbo.[LB_GetExportDataLogByBatchID] @BatchID",
                new SqlParameter("@BatchID", BatchID)
                 ).FirstOrDefault();

                orderExportBatchDetailViewModel.BatchType = orderExportBatch.BatchType.Replace("SAGE ","");
                orderExportBatchDetailViewModel.BatchDescription = orderExportBatch.Description;
                orderExportBatchDetailViewModel.BatchStatus = orderExportBatch.Status;

                orderExportBatchDetailViewModel.BatchID = BatchID;
                orderExportBatchDetailViewModel.SearchStatus = " ";

                IEnumerable<System.Web.Mvc.SelectListItem> searchList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

                orderExportBatchDetailViewModel.StatusList = searchList;

                return View("ExportOrderDetail",orderExportBatchDetailViewModel);
            }
            return View("ExportOrder", orderExportBatchDetailViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteOrderBatch(int BatchID = 0)
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

            db.LB_DeleteOrderExportBatch(BatchID, AdminID, returnID, returnMessage);

            var scalarID = (int)returnID.Value;
            var errorMessage = (string)returnMessage.Value ?? "";

            if (scalarID == -1)
            {
                ModelState.AddModelError("", errorMessage);
            }

            var orderExportBatch = new C_ExportDataLog();

            orderExportBatch = db.Database.SqlQuery<C_ExportDataLog>("exec dbo.[LB_GetExportDataLogByBatchID] @BatchID",
            new SqlParameter("@BatchID", BatchID)
             ).FirstOrDefault();


            var exportBatchListViewModel = new SageExportBatchListViewModel();

            exportBatchListViewModel.SearchStartDate = DateTime.Now.AddDays(-7);
            exportBatchListViewModel.SearchEndDate = DateTime.Now.AddDays(1);
            exportBatchListViewModel.SearchBatchType = orderExportBatch.BatchType.Replace("SAGE ", "");
            exportBatchListViewModel.SearchStatus = orderExportBatch.Status;

            exportBatchListViewModel.ExportBatchList = db.Database.SqlQuery<C_ExportDataLog>("exec dbo.[LB_GetExportDataLog] @StartDate, @EndDate, @BatchType, @Status",
            new SqlParameter("@StartDate", exportBatchListViewModel.SearchStartDate),
            new SqlParameter("@EndDate", exportBatchListViewModel.SearchEndDate),
            new SqlParameter("@BatchType", exportBatchListViewModel.SearchBatchType),
            new SqlParameter("@Status", exportBatchListViewModel.SearchStatus)
            ).ToList();

            exportBatchListViewModel.StatusList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

            return View("ExportList", exportBatchListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult ExportOrderDetail(int id)
        {
            var DataHelper = new DataHelpers();
            // Initalize the View Model

            var orderExportBatchDetailViewModel = new SageOrderExportBatchDetailViewModel();

            int BatchID = id;

            orderExportBatchDetailViewModel.OrderHistoryList = db.Database.SqlQuery<OrderHistory>("exec dbo.[LB_GetOrderHistory] @ExportBatchID",
            new SqlParameter("@ExportBatchID", BatchID)
             ).ToList();

            if (orderExportBatchDetailViewModel.OrderHistoryList == null)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Could not find Batch ID supplied to SAGE Order Export Detail list";
                return RedirectToAction("Error", "Admin");
            }

            var orderExportBatch = new C_ExportDataLog();

            orderExportBatch = db.Database.SqlQuery<C_ExportDataLog>("exec dbo.[LB_GetExportDataLogByBatchID] @BatchID",
            new SqlParameter("@BatchID", BatchID)
             ).FirstOrDefault();

            orderExportBatchDetailViewModel.BatchType = orderExportBatch.BatchType.Replace("SAGE ", "");
            orderExportBatchDetailViewModel.BatchDescription = orderExportBatch.Description;
            orderExportBatchDetailViewModel.BatchStatus = orderExportBatch.Status;

            orderExportBatchDetailViewModel.BatchID = BatchID;
            orderExportBatchDetailViewModel.SearchStatus = " ";

            IEnumerable<System.Web.Mvc.SelectListItem> searchList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

            orderExportBatchDetailViewModel.StatusList = searchList;

            return View(orderExportBatchDetailViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveOrderBatch(int BatchID = 0)
        {
            var DataHelper = new DataHelpers();

            if (BatchID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Batch ID supplied to Approve Batch";
                return RedirectToAction("Error", "Admin");
            }

            ObjectParameter returnPath = new ObjectParameter("returnPath", typeof(string));
            ObjectParameter returnFileName = new ObjectParameter("returnFileName", typeof(string));
            ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
            ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

            db.LB_ApproveSageExportOrderBatch(BatchID, AdminID, returnPath, returnFileName, returnID, returnMessage);

            var scalarID = (int)returnID.Value;
            var errorMessage = (string)returnMessage.Value ?? "";

            if (scalarID == -1)
            {
                ModelState.AddModelError("", errorMessage);
                var orderExportBatchDetailViewModel = new SageOrderExportBatchDetailViewModel();

                var rebateImportBatch = new RebateImportBatch();

                orderExportBatchDetailViewModel.OrderHistoryList = db.Database.SqlQuery<OrderHistory>("exec dbo.[LB_GetOrderHistory] @ExportBatchID",
                new SqlParameter("@ExportBatchID", BatchID)
                 ).ToList();

                if (orderExportBatchDetailViewModel.OrderHistoryList == null)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Could not find Batch ID supplied to SAGE Order Export Detail list";
                    return RedirectToAction("Error", "Admin");
                }

                var orderExportBatch = new C_ExportDataLog();

                orderExportBatch = db.Database.SqlQuery<C_ExportDataLog>("exec dbo.[LB_GetExportDataLogByBatchID] @BatchID",
                new SqlParameter("@BatchID", BatchID)
                 ).FirstOrDefault();

                orderExportBatchDetailViewModel.BatchType = orderExportBatch.BatchType.Replace("SAGE ", "");
                orderExportBatchDetailViewModel.BatchDescription = orderExportBatch.Description;
                orderExportBatchDetailViewModel.BatchStatus = orderExportBatch.Status;

                orderExportBatchDetailViewModel.BatchID = BatchID;
                orderExportBatchDetailViewModel.SearchStatus = " ";

                IEnumerable<System.Web.Mvc.SelectListItem> searchList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

                orderExportBatchDetailViewModel.StatusList = searchList;

                return View("ExportOrderDetail", orderExportBatchDetailViewModel);

            }

            var filePath = (string)returnPath.Value ?? "";
            var fileName = (string)returnFileName.Value ?? "";

            List<OrderHistory> orderHistoryList = new List<OrderHistory>();

            orderHistoryList = db.Database.SqlQuery<OrderHistory>("exec dbo.[LB_GetOrderHistoryExport] @ExportBatchID",
            new SqlParameter("@ExportBatchID", BatchID)
             ).ToList();

            if (orderHistoryList == null)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Could not find Orders to export for Batch ID supplied to SAGE Order Export Approval";
                return RedirectToAction("Error", "Admin");
            }

            StringBuilder sb = new StringBuilder();
            foreach (OrderHistory r in orderHistoryList)
            {
                sb.AppendLine("R"+r.OrderID + "," + r.AccountingID + "," + r.TransactionDate.Date.ToString("MM/dd/yyyy") + ",IN," + r.AdjustmentFlag + "," + r.ExportBatchID + "," + r.GLCode + "," + r.Comment + "," + r.BalanceChangeAmount.ToString("0.00") + "," + r.GLAccount);
            }

            var string_with_your_data = sb.ToString();
            var path = Server.MapPath("~/App_Data/" + filePath + "/" + fileName);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            System.IO.File.WriteAllText(path, string_with_your_data);

            var exportBatchListViewModel = new SageExportBatchListViewModel();

            exportBatchListViewModel.SearchStartDate = DateTime.Now.AddDays(-7);
            exportBatchListViewModel.SearchEndDate = DateTime.Now.AddDays(1);
            exportBatchListViewModel.SearchBatchType = "Invoice";
            exportBatchListViewModel.SearchStatus = " ";

            exportBatchListViewModel.ExportBatchList = db.Database.SqlQuery<C_ExportDataLog>("exec dbo.[LB_GetExportDataLog] @StartDate, @EndDate, @BatchType, @Status",
            new SqlParameter("@StartDate", exportBatchListViewModel.SearchStartDate),
            new SqlParameter("@EndDate", exportBatchListViewModel.SearchEndDate),
            new SqlParameter("@BatchType", exportBatchListViewModel.SearchBatchType),
            new SqlParameter("@Status", exportBatchListViewModel.SearchStatus)
            ).ToList();

            exportBatchListViewModel.StatusList = DataHelper.GetStatusSelectList(Constants.vendorRebateLookupGroupID, false, true);

            return View("ExportList", exportBatchListViewModel);
        }

        
        
        //        
        // BEGIN PAYMENT EXPORT SECTION
        //
        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult ExportPayment()
        {
            var DataHelper = new DataHelpers();

            return View();
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportPayment(SageOrderExportBatchDetailViewModel orderExportBatchDetailViewModel)
        {
            var DataHelper = new DataHelpers();

            int BatchID = 0;

                        // Initalize the View Model
            if (ModelState.IsValid)
            {

                string BatchDescription = orderExportBatchDetailViewModel.BatchDescription;

                ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_UpdateSageExportOrderPayments(BatchDescription, orderExportBatchDetailViewModel.CutoffDate, returnID, returnMessage);

                var scalarID = (int)returnID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                    return View();
                }
                else
                {
                    BatchID = scalarID;
                }

                orderExportBatchDetailViewModel.OrderHistoryList = db.Database.SqlQuery<OrderHistory>("exec dbo.[LB_GetOrderHistory] @ExportBatchID",
                new SqlParameter("@ExportBatchID", BatchID)
                 ).ToList();

                if (orderExportBatchDetailViewModel.OrderHistoryList == null)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Could not find Batch ID supplied to Order Export Detail list";
                    return RedirectToAction("Error", "Admin");
                }

                var orderExportBatch = new C_ExportDataLog();

                orderExportBatch = db.Database.SqlQuery<C_ExportDataLog>("exec dbo.[LB_GetExportDataLogByBatchID] @BatchID",
                new SqlParameter("@BatchID", BatchID)
                 ).FirstOrDefault();

                orderExportBatchDetailViewModel.BatchType = orderExportBatch.BatchType.Replace("SAGE ", "");
                orderExportBatchDetailViewModel.BatchDescription = orderExportBatch.Description;
                orderExportBatchDetailViewModel.BatchStatus = orderExportBatch.Status;

                orderExportBatchDetailViewModel.BatchID = BatchID;
                orderExportBatchDetailViewModel.SearchStatus = " ";

                IEnumerable<System.Web.Mvc.SelectListItem> searchList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

                orderExportBatchDetailViewModel.StatusList = searchList;

                return View("ExportPaymentDetail", orderExportBatchDetailViewModel);
            }
            return View("ExportPayment", orderExportBatchDetailViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult ExportPaymentDetail(int id)
        {
            var DataHelper = new DataHelpers();
            // Initalize the View Model

            var orderExportBatchDetailViewModel = new SageOrderExportBatchDetailViewModel();

            int BatchID = id;

            orderExportBatchDetailViewModel.OrderHistoryList = db.Database.SqlQuery<OrderHistory>("exec dbo.[LB_GetOrderHistory] @ExportBatchID",
            new SqlParameter("@ExportBatchID", BatchID)
             ).ToList();

            if (orderExportBatchDetailViewModel.OrderHistoryList == null)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Could not find Batch ID supplied to SAGE Order Export Detail list";
                return RedirectToAction("Error", "Admin");
            }

            var orderExportBatch = new C_ExportDataLog();

            orderExportBatch = db.Database.SqlQuery<C_ExportDataLog>("exec dbo.[LB_GetExportDataLogByBatchID] @BatchID",
            new SqlParameter("@BatchID", BatchID)
             ).FirstOrDefault();

            orderExportBatchDetailViewModel.BatchType = orderExportBatch.BatchType.Replace("SAGE ", "");
            orderExportBatchDetailViewModel.BatchDescription = orderExportBatch.Description;
            orderExportBatchDetailViewModel.BatchStatus = orderExportBatch.Status;

            orderExportBatchDetailViewModel.BatchID = BatchID;
            orderExportBatchDetailViewModel.SearchStatus = " ";

            IEnumerable<System.Web.Mvc.SelectListItem> searchList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

            orderExportBatchDetailViewModel.StatusList = searchList;

            return View(orderExportBatchDetailViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApprovePaymentBatch(int BatchID = 0)
        {
            var DataHelper = new DataHelpers();

            if (BatchID == 0)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Invalid Batch ID supplied to Approve Batch";
                return RedirectToAction("Error", "Admin");
            }

            ObjectParameter returnPath = new ObjectParameter("returnPath", typeof(string));
            ObjectParameter returnFileName = new ObjectParameter("returnFileName", typeof(string));
            ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
            ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

            db.LB_ApproveSageExportOrderBatch(BatchID, AdminID, returnPath, returnFileName, returnID, returnMessage);

            var scalarID = (int)returnID.Value;
            var errorMessage = (string)returnMessage.Value ?? "";

            if (scalarID == -1)
            {
                ModelState.AddModelError("", errorMessage);
                var orderExportBatchDetailViewModel = new SageOrderExportBatchDetailViewModel();

                var rebateImportBatch = new RebateImportBatch();

                orderExportBatchDetailViewModel.OrderHistoryList = db.Database.SqlQuery<OrderHistory>("exec dbo.[LB_GetOrderHistory] @ExportBatchID",
                new SqlParameter("@ExportBatchID", BatchID)
                 ).ToList();

                if (orderExportBatchDetailViewModel.OrderHistoryList == null)
                {
                    TempData["ErrorCode"] = Constants.fatalErrorCode;
                    TempData["ErrorMessage"] = "Could not find Batch ID supplied to SAGE Order Export Detail list";
                    return RedirectToAction("Error", "Admin");
                }

                var orderExportBatch = new C_ExportDataLog();

                orderExportBatch = db.Database.SqlQuery<C_ExportDataLog>("exec dbo.[LB_GetExportDataLogByBatchID] @BatchID",
                new SqlParameter("@BatchID", BatchID)
                 ).FirstOrDefault();

                orderExportBatchDetailViewModel.BatchType = orderExportBatch.BatchType.Replace("SAGE ", "");
                orderExportBatchDetailViewModel.BatchDescription = orderExportBatch.Description;
                orderExportBatchDetailViewModel.BatchStatus = orderExportBatch.Status;

                orderExportBatchDetailViewModel.BatchID = BatchID;
                orderExportBatchDetailViewModel.SearchStatus = " ";

                IEnumerable<System.Web.Mvc.SelectListItem> searchList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

                orderExportBatchDetailViewModel.StatusList = searchList;

                return View("ExportPaymentDetail", orderExportBatchDetailViewModel);

            }

            var filePath = (string)returnPath.Value ?? "";
            var fileName = (string)returnFileName.Value ?? "";

            List<OrderHistory> orderHistoryList = new List<OrderHistory>();

            orderHistoryList = db.Database.SqlQuery<OrderHistory>("exec dbo.[LB_GetPaymentHistoryExport] @ExportBatchID",
            new SqlParameter("@ExportBatchID", BatchID)
             ).ToList();

            if (orderHistoryList == null)
            {
                TempData["ErrorCode"] = Constants.fatalErrorCode;
                TempData["ErrorMessage"] = "Could not find Orders to export for Batch ID supplied to SAGE Order Export Approval";
                return RedirectToAction("Error", "Admin");
            }

            StringBuilder sb = new StringBuilder();
            foreach (OrderHistory r in orderHistoryList)
            {
                sb.AppendLine("1," + r.AccountingID + ',' + r.PaymentMethod + "," + r.ChangeAmount.ToString("0.00") + "," + r.GLAccount + "," + r.Comment + ",I,R" + r.OrderID + ",IN");
            }

            var string_with_your_data = sb.ToString();
            var path = Server.MapPath("~/App_Data/" + filePath + "/" + fileName);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            System.IO.File.WriteAllText(path, string_with_your_data);

            var exportBatchListViewModel = new SageExportBatchListViewModel();

            exportBatchListViewModel.SearchStartDate = DateTime.Now.AddDays(-7);
            exportBatchListViewModel.SearchEndDate = DateTime.Now.AddDays(1);
            exportBatchListViewModel.SearchBatchType = "Payment";
            exportBatchListViewModel.SearchStatus = " ";

            exportBatchListViewModel.ExportBatchList = db.Database.SqlQuery<C_ExportDataLog>("exec dbo.[LB_GetExportDataLog] @StartDate, @EndDate, @BatchType, @Status",
            new SqlParameter("@StartDate", exportBatchListViewModel.SearchStartDate),
            new SqlParameter("@EndDate", exportBatchListViewModel.SearchEndDate),
            new SqlParameter("@BatchType", exportBatchListViewModel.SearchBatchType),
            new SqlParameter("@Status", exportBatchListViewModel.SearchStatus)
            ).ToList();

            exportBatchListViewModel.StatusList = DataHelper.GetStatusSelectList(Constants.fileStatusLookupGroupID, false, true);

            return View("ExportList", exportBatchListViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult ExportRebateDetail(int id = 0)
        {
            var DataHelper = new DataHelpers();

            // Initalize the View Model
            var rebateDetailViewModel = new RebateDetailViewModel();

            rebateDetailViewModel.RebateDetailList = db.Database.SqlQuery<RebateDetail>("exec dbo.[LB_GetRebateDetailByBatchID] @BatchID",
            new SqlParameter("@BatchID", id)
            ).ToList();

            return View(rebateDetailViewModel);
        }

        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpGet]
        public ActionResult ExportCommissionDetail(int id = 0)
        {
            var DataHelper = new DataHelpers();

            var commissionDetailListViewModel = new CommissionDetailListViewModel();

            commissionDetailListViewModel.CommissionDetailList = db.Database.SqlQuery<CommissionDetail>("exec dbo.[LB_GetCommissionDetailByBatchID] @BatchID",
             new SqlParameter("@BatchID", id)
            ).ToList();


            return View(commissionDetailListViewModel);
        }

        [HttpGet]
        public ActionResult PayeeChangeList(string StartDate = "", string EndDate = "", int ConfirmFlag = 0)
        {
            var DataHelper = new DataHelpers();

            DateTime now = DateTime.Now;
            DateTime searchEndDate = now;
            DateTime searchStartDate = searchEndDate.AddMonths(-1);

            if (DateTime.TryParse(StartDate, out now))
            {
                searchStartDate = DateTime.Parse(StartDate);
            }

            if (DateTime.TryParse(EndDate, out now))
            {
                searchEndDate = DateTime.Parse(EndDate);
            }

            if (searchStartDate > searchEndDate)
            {
                ModelState.AddModelError("", "Start Date cannot be after End Date");
            }

            var changeLogList = db.Database.SqlQuery<ChangeLog>("exec dbo.[LB_GetChangeLog] @StartDate, @EndDate, @Table, @Field, @ChangeConfirmed",
             new SqlParameter("@StartDate", searchStartDate),
             new SqlParameter("@EndDate", searchEndDate),
             new SqlParameter("@Table", "C_Info"),
             new SqlParameter("@Field", "Payee"),
             new SqlParameter("@ChangeConfirmed", "")
             ).ToList();

            IEnumerable<System.Web.Mvc.SelectListItem> searchChangeConfirmedList = DataHelper.GetChangeConfirmedList();

            var payeeChangeListViewModel = new PayeeChangeListViewModel
            {
                ChangeLogList = changeLogList,
                SearchStartDate = searchStartDate,
                SearchEndDate = searchEndDate,
                SearchChangeConfirmed = "",
                SearchChangeConfirmedList = searchChangeConfirmedList
            };

            return View(payeeChangeListViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayeeChangeList([Bind(Include = "SearchStartDate, SearchEndDate, SearchChangeConfirmed")] PayeeChangeListViewModel payeeChangeListViewModel)
        {
            var DataHelper = new DataHelpers();

            if (ModelState.IsValid)
            {
                DateTime now = DateTime.Now;
                DateTime searchStartDate = payeeChangeListViewModel.SearchStartDate ?? DateTime.Now;
                DateTime searchEndDate = payeeChangeListViewModel.SearchEndDate ?? DateTime.Now;
                if (searchStartDate > searchEndDate)
                {
                    ModelState.AddModelError("", "Start Date cannot be after End Date");
                    // Return below using empty model
                    payeeChangeListViewModel.ChangeLogList = new List<ChangeLog>();
                }
                else
                {
                     payeeChangeListViewModel.ChangeLogList = db.Database.SqlQuery<ChangeLog>("exec dbo.[LB_GetChangeLog] @StartDate, @EndDate, @Table, @Field, @ChangeConfirmed",
                     new SqlParameter("@StartDate", searchStartDate),
                     new SqlParameter("@EndDate", searchEndDate),
                     new SqlParameter("@Table", "C_Info"),
                     new SqlParameter("@Field", "Payee"),
                     new SqlParameter("@ChangeConfirmed", payeeChangeListViewModel.SearchChangeConfirmed)
                     ).ToList();
                }
            }

            IEnumerable<System.Web.Mvc.SelectListItem> searchChangeConfirmedList = DataHelper.GetChangeConfirmedList();

            payeeChangeListViewModel.SearchChangeConfirmedList = searchChangeConfirmedList;

            return View(payeeChangeListViewModel);
        }

        [HttpGet]
        public ActionResult PayeeChangeConfirm(string StartDate = "", string EndDate = "", int ConfirmFlag = 0, int ID = 0)
        {
            var DataHelper = new DataHelpers();

            DateTime now = DateTime.Now;
            DateTime searchEndDate = now;
            DateTime searchStartDate = searchEndDate.AddMonths(-1);

            if (DateTime.TryParse(StartDate, out now))
            {
                searchStartDate = DateTime.Parse(StartDate);
            }

            if (DateTime.TryParse(EndDate, out now))
            {
                searchEndDate = DateTime.Parse(EndDate);
            }

            if (searchStartDate > searchEndDate)
            {
                searchEndDate = now;
                searchStartDate = searchEndDate.AddMonths(-1);
            }

            if (ID > 0)
            {
                ObjectParameter returnID = new ObjectParameter("returnID", typeof(int));
                ObjectParameter returnMessage = new ObjectParameter("returnMessage", typeof(string));

                db.LB_UpdateChangeLogConfirm(ID, returnID, returnMessage);

                var scalarID = (int)returnID.Value;
                var errorMessage = (string)returnMessage.Value ?? "";

                if (scalarID == -1)
                {
                    ModelState.AddModelError("", errorMessage);
                }
            }

            List<ChangeLog> changeLogList = new List<ChangeLog>();

            if (ModelState.IsValid)
            {
                changeLogList = db.Database.SqlQuery<ChangeLog>("exec dbo.[LB_GetChangeLog] @StartDate, @EndDate, @Table, @Field, @ChangeConfirmed",
                 new SqlParameter("@StartDate", searchStartDate),
                 new SqlParameter("@EndDate", searchEndDate),
                 new SqlParameter("@Table", "C_Info"),
                 new SqlParameter("@Field", "Payee"),
                 new SqlParameter("@ChangeConfirmed", "")
                 ).ToList();
            }

            IEnumerable<System.Web.Mvc.SelectListItem> searchChangeConfirmedList = DataHelper.GetChangeConfirmedList();

            var payeeChangeListViewModel = new PayeeChangeListViewModel
            {
                ChangeLogList = changeLogList,
                SearchStartDate = searchStartDate,
                SearchEndDate = searchEndDate,
                SearchChangeConfirmed = "",
                SearchChangeConfirmedList = searchChangeConfirmedList
            };

            return View("PayeeChangeList", payeeChangeListViewModel);
        }
    }
}