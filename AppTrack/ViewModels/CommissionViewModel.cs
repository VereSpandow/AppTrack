using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppTrack.Models;
using AppTrack.SharedModels;
using System.ComponentModel.DataAnnotations;

namespace AppTrack.ViewModels
{

    public class PayoutListViewModel
    {
        public List<C_PayOut> PayoutList { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public System.DateTime SearchStartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public System.DateTime SearchEndDate { get; set; }
    }

    public class CommissionSummary
    {
        public int PeriodID { get; set; }
        public string PeriodName { get; set; }

        public int CommissionID { get; set; }
        public string CommissionName { get; set; }

        public string Status { get; set; }
 
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Commission { get; set; }
    }

    public class CommissionSummaryViewModel
    {
        [Display(Name = "Start Period")]
        public int StartPeriodID { get; set; }

        [Display(Name = "End Period")]
        public int EndPeriodID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> PeriodList { get; set; }

        [Display(Name = "Commission Type")]
        public int CommissionID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> CommissionList { get; set; }

        public List<CommissionSummary> CommissionSummaryList { get; set; }
    }


    public class CommissionHeaderListViewModel
    {
        public List<CommissionHeader> CommissionHeaderList { get; set; }

        public int StartPeriodID { get; set; }

        public int EndPeriodID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> SearchPeriodIDList { get; set; }

        [Display(Name = "Payee")]
        public int SearchCustID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchCustIDList { get; set; }

        [Display(Name = "Commission Type")]
        public int SearchCommissionID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchCommissionIDList { get; set; }

        [Display(Name = "Status")]
        public string SearchStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchStatusList { get; set; }

    }
    public class CommissionDetailListViewModel
    {
        public List<CommissionDetail> CommissionDetailList { get; set; }

        public int StartPeriodID { get; set; }

        public int EndPeriodID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> SearchPeriodIDList { get; set; }

        [Display(Name = "Payee")]
        public int SearchCustID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchCustIDList { get; set; }

        [Display(Name = "Commission Type")]
        public int SearchCommissionID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchCommissionIDList { get; set; }

        [Display(Name = "Status")]
        public string SearchStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchStatusList { get; set; }

    }
    public class VolumeDetailListViewModel
    {
        public List<VolumeDetail> VolumeDetailList { get; set; }
    }
}