using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
using AppTrack.SharedModels;
using AppTrack.Models;

namespace AppTrack.ViewModels
{
    public class ProcessRebatesViewModel
    {
            public IEnumerable<System.Web.Mvc.SelectListItem> RebateProcessingPeriodList { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> RebatePostingPeriodList { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> ManualRebatePostingPeriodList { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> VendorList { get; set; }

            public int? AdminID { get; set; }
            public int? PeriodID { get; set; }
            public int? PostPeriodID { get; set; }
            public string ProcessRebatesConfirmation { get; set; }
            public string PostRebatesConfirmation { get; set; }
            public int? ManualRebatePostPeriodID { get; set; }
            public int? VendorID { get; set; }
            public int? MemberID { get; set; }
            public Nullable<decimal> Amount { get; set; }
            public string Comments { get; set; }
    }

    public class RebateExport
    {
        public string SageNo{ get; set; }
        public int InvoiceNo { get; set; }
        public string Comment { get; set; }
        public decimal Amount { get; set; }
        public string KeyAccountID { get; set; }
        public int BatchID { get; set; }
    }


}