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
using System.IO;
using System.Text;

namespace AppTrack.Controllers
{
            //[AuthorizeAdminRedirect(Roles = "Sales,Accounting,Finance")]
    public class ProcessRebatesController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
        private DataHelpers DataHelper = new DataHelpers();
        
        
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public ProcessRebatesController()
        {
        }

        public ProcessRebatesController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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

        
        // GET: ProcessCommissions
        [AuthorizeAdminRedirect(Roles = "Finance")]
        public ActionResult Index()
        {
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
                TempData["ErrorMessage"] = "";
            }

            var model = new ProcessRebatesViewModel
            {
                AdminID = 0,
                PeriodID = 0,
                ProcessRebatesConfirmation = "Y"
            };

            IEnumerable<System.Web.Mvc.SelectListItem> rebateProcessingPeriodList = DataHelper.GetRebateProcessingPeriodSelectList();
            IEnumerable<System.Web.Mvc.SelectListItem> rebatePostingPeriodList = DataHelper.GetRebatePostingPeriodSelectList();
            IEnumerable<System.Web.Mvc.SelectListItem> manualRebatePostingPeriodList = DataHelper.GetPeriodIDList(Constants.quarterlyPeriodTypeID);
            IEnumerable<System.Web.Mvc.SelectListItem> vendorList = DataHelper.GetRebateVendorsList();
            
            
            model.RebateProcessingPeriodList = rebateProcessingPeriodList;
            model.RebatePostingPeriodList = rebatePostingPeriodList;
            model.ManualRebatePostingPeriodList = manualRebatePostingPeriodList;
            model.VendorList = vendorList;
         
            return View(model);
        }
    
        // POST: ProcessCommissions
        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        public ActionResult ProcessRebates(int AdminID, int PeriodID, string ProcessRebatesConfirmation)
        {
            ViewBag.ErrorCode = 0;
            if (ModelState.IsValid)
            {

                db.LB_ProcessRebatesStageOne(PeriodID, AdminID, 
                    Constants.rebateVolumeType,
                    Constants.memberRebateCommissionID,
                    Constants.corporateRebateCommissionID,
                    Constants.quarterlyPeriodTypeID
                    );

                TempData["ErrorMessage"] = "Rebates have been successfully processed";
            }
            else
            {
                TempData["ErrorMessage"] = "Rebate processing encountered an error";
            }
            return RedirectToAction("Index", "ProcessRebates");
        }

        [AuthorizeAdminRedirect(Roles = "Executive")]
        public ActionResult PostRebates(int AdminID, int PeriodID, int? BatchID, string ProcessRebatesConfirmation)
        {
              //todo: get Rebate Data

            BatchID = 0;

            var rebateExportList = db.Database.SqlQuery<RebateExport>("exec dbo.[LB_ProcessBucketAndPostFromCommissionRebates]  @PeriodID, @AdminID",
                     new SqlParameter("@PeriodID", PeriodID),
                     new SqlParameter("@AdminID", AdminID)
                     ).ToList();

            if (rebateExportList.Count() == 0)
            {
                TempData["ErrorMessage"] = "Rebates have been successfully posted. Corporate Rebates only in this batch. Sage export file did not need to be created.";
            }
            else
            {
                BatchID = rebateExportList[0].BatchID;
                StringBuilder sb = new StringBuilder();
                foreach (RebateExport r in rebateExportList)
                {
                    sb.AppendLine(r.SageNo + ",P" + r.InvoiceNo + "," + r.Comment + "," + r.Amount.ToString("0.00") + "," + r.KeyAccountID + "," + r.BatchID);
                }

                var string_with_your_data = sb.ToString();
                var exportFileName = "SageRebateInvoiceBatch-" + BatchID + ".csv";
                var path = Server.MapPath("~/App_Data/ExportFiles/" + exportFileName);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                System.IO.File.WriteAllText(path, string_with_your_data);
                TempData["ErrorMessage"] = "Rebates have been successfully posted. Sage export file " + exportFileName + " was created.";
            }
// Code used when this returned a FileStreamResult
//                var byteArray = Encoding.ASCII.GetBytes(string_with_your_data);
//                var stream = new MemoryStream(byteArray);
//                return File(stream, "text/plain", exportFileName);

            return RedirectToAction("Index", "ProcessRebates");
        }

        [AuthorizeAdminRedirect(Roles = "Executive")]
        public ActionResult CreateSageFile()
        {
            //todo: get Rebate Data

            int BatchID = 1;

            var rebateExportList = db.Database.SqlQuery<RebateExport>("exec dbo.[LB_GetRebatesForExportByBatchID] @BatchID",
                     new SqlParameter("@BatchID", BatchID)
                     ).ToList();

            if (rebateExportList.Count() == 0)
            {
                TempData["ErrorMessage"] = "No Rebates found. Sage export file was not created.";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (RebateExport r in rebateExportList)
                {
                    sb.AppendLine(r.SageNo + ",P" + r.InvoiceNo + "," + r.Comment + "," + r.Amount.ToString("0.00") + "," + r.KeyAccountID + "," + r.BatchID);
                }

                var string_with_your_data = sb.ToString();
                var exportFileName = "SageRebateInvoiceBatch-" + BatchID + ".csv";
                var path = Server.MapPath("~/App_Data/ExportFiles/" + exportFileName);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                System.IO.File.WriteAllText(path, string_with_your_data);
                TempData["ErrorMessage"] = "Rebates have been successfully exported. Sage export file " + exportFileName + " was created.";
            }
            // Code used when this returned a FileStreamResult
            //                var byteArray = Encoding.ASCII.GetBytes(string_with_your_data);
            //                var stream = new MemoryStream(byteArray);
            //                return File(stream, "text/plain", exportFileName);

            return RedirectToAction("Index", "ProcessRebates");
        }

        // POST: ProcessRebates
        [AuthorizeAdminRedirect(Roles = "Finance")]
        [HttpPost]
        public ActionResult PostManualRebates(int AdminID, int PeriodID, int VendorID, int MemberID, decimal Amount, string Comments)
        {
            ViewBag.ErrorCode = 0;
            if (ModelState.IsValid)
            {

                db.LB_ProcessPostManualRebates(PeriodID, 
                    AdminID, 
                    VendorID,
                    MemberID,
                    Amount,
                    Comments
                    );

                var isDone = "Y";

                ViewBag.Message = "The Manual Rebate was posted and will be processed with the next processing batch";
                return RedirectToAction("Index", "ProcessRebates");
            }
            else
            {

                ViewBag.Message = "Ooops the Manual Rebate was not posted";
                return RedirectToAction("Index", "ProcessRebates");

            }
        }

    }
}