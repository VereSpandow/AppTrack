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

    public class MemberAttritionSummary
    {
        public int PeriodID { get; set; }

        public string PeriodName { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }

        public int BeginMemberCount { get; set; }
        public int NewMemberCount { get; set; }
        public int CancelMemberCount { get; set; }
        public int EndMemberCount { get; set; }

    }

    public class MemberAttritionSummaryViewModel
    {
        [Display(Name = "Start Period")]
        public int StartPeriodID { get; set; }

        [Display(Name = "End Period")]
        public int EndPeriodID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> PeriodList { get; set; }

        [Display(Name = "Customer Type")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Customer Type value")]
        public int CustomerType { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> CustomerTypeList { get; set; }

        public List<MemberAttritionSummary> MemberAttritionSummaryList { get; set; }
    }

    public class RebateSummaryByMember
    {
        public int PeriodID { get; set; }

        public string PeriodName { get; set; }

        public int CustID { get; set; }

        [Display(Name = "Sage ID")]
        public string AccountingID { get; set; }

        public string PayeeName { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Commission { get; set; }
    }

    public class RebateSummaryByMemberViewModel
    {
        [Display(Name = "Start Period")]
        public int StartPeriodID { get; set; }

        [Display(Name = "End Period")]
        public int EndPeriodID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> PeriodList { get; set; }

        [Display(Name = "Member ID")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Please enter a valid Member ID value")]
        public string CustID { get; set; }

        public List<RebateSummaryByMember> RebateSummaryByMemberList { get; set; }
    }

}