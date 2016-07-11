
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
using AppTrack.SharedModels;
using AppTrack.Models;

namespace AppTrack.ViewModels
{
    public class ProcessCommissionsViewModel
    {
            public IEnumerable<System.Web.Mvc.SelectListItem> CommissionProcessingPeriodList { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> CommissionPostingPeriodList { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> ManualCommissionPostingPeriodList { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> IMDCommissionIDList { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> IMDList { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> MeetingList { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> SalesRepList { get; set; }

            public int? AdminID { get; set; }
            public int? PeriodID { get; set; }
            public int? PostPeriodID { get; set; }
            public string ProcessCommissionConfirmation { get; set; }
            public string PostCommissionConfirmation { get; set; }
            public int? ManualCommissionPostPeriodID { get; set; }
            public int? CommissionID { get; set; }
            public int? CustID { get; set; }
            public int? SponsorID { get; set; }
            public int? MemberID { get; set; }
            public int? MeetingID { get; set; }        
            public Nullable<decimal> Amount { get; set; }
            public string Comments { get; set; }


    }

    public class CommissionExport
    {
        public string SageNo{ get; set; }
        public int InvoiceNo { get; set; }
        public string Comment { get; set; }
        public decimal Amount { get; set; }
        public string KeyAccountID { get; set; }
        public int BatchID { get; set; }
    }


}