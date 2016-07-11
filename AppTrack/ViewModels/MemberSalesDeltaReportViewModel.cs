using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace AppTrack.ViewModels
{
    public class MemberSalesDeltaReportViewModel
    {
            public List<MemberSalesDeltaReport> MemberSalesDeltaReportList { get; set; }
            [Display(Name = "Member ID")]
            public int searchCustID { get; set; }
            [Display(Name = "Vendor ID")]
            public int vid { get; set; }
            [Display(Name = "Start Period")]
            public int spid { get; set; }
            [Display(Name = "End Period")]
            public int epid { get; set; }
            [Display(Name = "Grouped or SeparatePeriods")]
            public int isGrouped { get; set; }

            public IEnumerable<System.Web.Mvc.SelectListItem> SearchVendorList { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> SearchPeriodList { get; set; }

        }

        public class MemberSalesDeltaReport
        {
            public int CustID { get; set; }
            public int VendorID { get; set; }
            public string MemberName { get; set; }
            public string VendorName { get; set; }
            public int PeriodID1 { get; set; }
            public string PeriodName1 { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal Sales1 { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal MemberRebateAmount1 { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal CorporateRebateAmount1 { get; set; }
            public int PeriodID2 { get; set; }
            public string PeriodName2 { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal Sales2 { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal MemberRebateAmount2 { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal CorporateRebateAmount2 { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal SalesDelta { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal MemberRebateAmountDelta { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal CorporateRebateAmountDelta { get; set; }
            [DisplayFormat(DataFormatString = "{0:##.#%}")]
            public double SalesDeltaPercent { get; set; }
            [DisplayFormat(DataFormatString = "{0:##.#%}")]
            public double MemberRebateAmountPercent { get; set; }
            [DisplayFormat(DataFormatString = "{0:##.#%}")]
            public double CorporateRebateAmountPercent { get; set; }
            [DataType(DataType.Date)]
            public Nullable<System.DateTime> MemberStatusDate { get; set; }
            public string MemberStatus { get; set; }

        }
}
