using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
using AppTrack.SharedModels;
using AppTrack.Models;
// Caution if we include System.Web.Mvc for use with SelectListItem, the Compare attribute becomes ambiguous as it is in 2 namespaces.
// So, just specify System.Web.Mvc for SelectListItem

namespace AppTrack.ViewModels
{
    public class RebateSummary
    {
        public int PeriodID { get; set; }
        public string PeriodName { get; set; }

        public int CommissionID { get; set; }
        public string CommissionName { get; set; }

        public int VendorID { get; set; }
        public string VendorName { get; set; }

        public int VolumeID { get; set; }
        public string VolumeName { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Commission { get; set; }
    }

    public class RebateDetail
    {
        public int CDID { get; set; }
        public int CustID { get; set; }
        [Display(Name = "Sage ID")]
        public string AccountingID { get; set; }
        public string PayeeName { get; set; }
        public int PeriodID { get; set; }
        public string PeriodName { get; set; }

        public int CommissionID { get; set; }
        public string CommissionName { get; set; }

        public int VendorID { get; set; }
        public string VendorName { get; set; }

        public int VolumeID { get; set; }
        public string VolumeName { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Commission { get; set; }

        public int PayoutID { get; set; }
    }

    public class RebateSummaryViewModel
    {
        [Display(Name = "Start Period")]
        public int StartPeriodID { get; set; }

        [Display(Name = "End Period")]
        public int EndPeriodID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> PeriodList { get; set; }
        
        [Display(Name = "Payee Type")]
        public int CommissionID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> CommissionList { get; set; }

        [Display(Name = "Vendor")]
        public int VendorID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> VendorList { get; set; }

        [Display(Name = "Rebate Type")]
        public int VolumeID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> VolumeList { get; set; }

        public List<RebateSummary> RebateSummaryList { get; set; }
    }

    public class RebateDetailViewModel
    {
        [Display(Name = "Start Period")]
        public int StartPeriodID { get; set; }

        [Display(Name = "End Period")]
        public int EndPeriodID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> PeriodList { get; set; }

        [Display(Name = "Payee Type")]
        public int CommissionID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> CommissionList { get; set; }

        [Display(Name = "Vendor")]
        public int VendorID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> VendorList { get; set; }

        [Display(Name = "Rebate Type")]
        public int VolumeID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> VolumeList { get; set; }

        [Display(Name = "Member ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member ID value")]
        public string CustID { get; set; }

        public List<RebateDetail> RebateDetailList { get; set; }
    }


}