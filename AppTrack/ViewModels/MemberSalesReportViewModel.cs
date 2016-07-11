using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace AppTrack.ViewModels
{
    public class MemberSalesReportViewModel
    {
            public List<MemberSalesReport> MemberSalesReportList { get; set; }
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

        public class MemberSalesReportXTab
        {
            public int PeriodID { get; set; }
            public int VendorID { get; set; }
            public int CustID { get; set; }
            public int PeriodTypeID { get; set; }
            public string MemberName { get; set; }
            public string VendorName { get; set; }

            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal SalesPeriodOne { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal SalesPeriodTwo { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal SalesPeriodThree { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal SalesPeriodFour { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal SalesPeriodFive { get; set; }

            public int PeriodIDOne { get; set; }
            public int PeriodIDTwo { get; set; }
            public int PeriodIDThree { get; set; }
            public int PeriodIDFour { get; set; }
            public int PeriodIDFive { get; set; }

            public string PeriodLabelOne { get; set; }
            public string PeriodLabelTwo { get; set; }
            public string PeriodLabelThree { get; set; }
            public string PeriodLabelFour { get; set; }
            public string PeriodLabelFive { get; set; }

        }

            public class MemberSalesReport
        {
            public int PeriodID { get; set; }
            public int VendorID { get; set; }
            public int CustID { get; set; }
            public int PeriodTypeID { get; set; }
            public string PeriodName { get; set; }
            public string MemberName { get; set; }
            public string VendorName { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal Sales { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal MemberRebateAmount { get; set; }
            [DisplayFormat(DataFormatString = "{0:C2}")]
            public decimal CorporateRebateAmount { get; set; }
            [DataType(DataType.Date)]
            public Nullable<System.DateTime> MemberStatusDate { get; set; }
            public string MemberStatus { get; set; }


        }

}
