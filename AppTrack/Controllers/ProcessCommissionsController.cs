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
    [AuthorizeAdminRedirect(Roles = "Accounting,Finance,SalesManager")]
    public class ProcessCommissionsController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();
        private DataHelpers DataHelper = new DataHelpers();
        
        
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;

        public ProcessCommissionsController()
        {
        }

        public ProcessCommissionsController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
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
        public ActionResult Index()
        {
            var model = new ProcessCommissionsViewModel
            {
                AdminID = 0,
                PeriodID = 0,
                ProcessCommissionConfirmation = "Y"
            };

            IEnumerable<System.Web.Mvc.SelectListItem> commissionProcessingPeriodList = DataHelper.GetCommissionProcessingPeriodSelectList();
            IEnumerable<System.Web.Mvc.SelectListItem> commissionPostingPeriodList = DataHelper.GetCommissionPostingPeriodSelectList();
            IEnumerable<System.Web.Mvc.SelectListItem> manualCommissionPostingPeriodList = DataHelper.GetPeriodIDList(Constants.monthlyPeriodTypeID);
            IEnumerable<System.Web.Mvc.SelectListItem> imdCommissionIDList = DataHelper.GetManualCommissionIDSelectList();
            IEnumerable<System.Web.Mvc.SelectListItem> iMDList = DataHelper.GetCustIDList(4);
            IEnumerable<System.Web.Mvc.SelectListItem> meetingList = DataHelper.GetMeetingFormCommissionsList();
            IEnumerable<System.Web.Mvc.SelectListItem> salesRepList = DataHelper.GetCustIDList(3);
            

            model.CommissionProcessingPeriodList = commissionProcessingPeriodList;
            model.CommissionPostingPeriodList = commissionPostingPeriodList;
            model.ManualCommissionPostingPeriodList = manualCommissionPostingPeriodList;
            model.IMDCommissionIDList = imdCommissionIDList;
            model.IMDList = iMDList;
            model.MeetingList = meetingList;
            model.SalesRepList = salesRepList;
        
            return View(model);
        }

        // GET: ProcessCommissions
        public IEnumerable<SelectListItem> GetIMDMeetingList()
        {
            List<EnrollmentMeetingBasic> meetingData = new List<EnrollmentMeetingBasic>();
            meetingData = db.Database.SqlQuery<EnrollmentMeetingBasic>("exec dbo.LB_GetEventHeaderMeetingsForCommissions"
            ).ToList();

            var allMeeting = new EnrollmentMeetingBasic
            {
                EventID = 0,
                EventTitle = "(optional)"
            };
            meetingData.Insert(0, allMeeting);
            var thisList = meetingData
            .Select(listItem => new SelectListItem
            {
                Value = listItem.ID.ToString(),
                Text = listItem.City + ' ' + listItem.State + ' ' + listItem.EventStartDate + ' ' + listItem.HostName + ' ' + listItem.EventTitle
            });

            SelectList s1 = new SelectList(thisList, "Value", "Text");
            return s1;
        }


        // POST: ProcessCommissions
        [HttpPost]
        public ActionResult ProcessCommissions(int AdminID, int PeriodID, string ProcessIMDCommissionsConfirmation)
        {
            ViewBag.ErrorCode = 0;
            if (ModelState.IsValid)
            {

                db.LB_ProcessCommissionsStageOne(PeriodID, AdminID, Constants.IMDMemberEnrollmentCommissionID, Constants.IMDSixMonthMemberCommissionID, Constants.monthlyPeriodTypeID);

                var isDone = "Y";
                return RedirectToAction("Index", "ProcessCommissions");
            }
            else
            {

                return RedirectToAction("Index", "ProcessCommissions");

            }
        }

        public FileStreamResult PostCommissions(int AdminID, int PeriodID, int? BatchID, string ProcessIMDCommissionsConfirmation)
        {
              //todo: get IMDCommission Data

            BatchID = 0;

            var IMDCommissionExportList = db.Database.SqlQuery<CommissionExport>("exec dbo.[LB_ProcessBucketAndPostFromCommissionIMDCommissions]  @PeriodID, @AdminID",
                     new SqlParameter("@PeriodID", PeriodID),
                     new SqlParameter("@AdminID", AdminID)
                     ).ToList();

                BatchID = IMDCommissionExportList[0].BatchID;
                StringBuilder sb = new StringBuilder();
                foreach (CommissionExport r in IMDCommissionExportList)
                {
                    sb.AppendLine(r.SageNo + "," + r.InvoiceNo + "," + r.Comment + "," + r.Amount.ToString("0.00") + "," + r.KeyAccountID + "," + r.BatchID);
                }

                var string_with_your_data = sb.ToString();
                var exportFileName = "SageIMDCommissionInvoiceBatch-" + BatchID + ".csv";
                var path = Server.MapPath("~/App_Data/ExportFiles/" + exportFileName);
                if (!System.IO.File.Exists(path))
                {
                    System.IO.File.WriteAllText(path, string_with_your_data);
                }
                else
                {
                }

                var byteArray = Encoding.ASCII.GetBytes(string_with_your_data);
                var stream = new MemoryStream(byteArray);
                return File(stream, "text/plain", exportFileName);

        }

        // POST: ProcessRebates
        [HttpPost]
        public ActionResult PostManualCommissions(int AdminID, int PeriodID, int CustID, int CommissionID, decimal Amount, string Comments, int MemberID = 0, int SponsorID = 0, int MeetingID = 0)
        {
            ViewBag.ErrorCode = 0;
            if (CommissionID == 45)
            {
                MemberID = MeetingID;
            }
            if (CommissionID == 30)
            {
                CustID = SponsorID;
            }
            if (ModelState.IsValid)
            {

                db.LB_ProcessPostManualCommissions(PeriodID,
                    AdminID,
                    CommissionID,
                    CustID,
                    MemberID,
                    Amount,
                    Comments
                    );

                var isDone = "Y";
                return RedirectToAction("Index", "ProcessCommissions");
            }
            else
            {

                return RedirectToAction("Index", "ProcessCommissions");

            }
        }

    }
}

