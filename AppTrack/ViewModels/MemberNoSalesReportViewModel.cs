using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppTrack.ViewModels
{
    public class MemberNoSalesReportViewModel
    {
            public List<MemberNoSalesReport> MemberNoSalesReportList { get; set; }
            [Display(Name = "Member ID")]
            public int searchCustID { get; set; }
            [Display(Name = "Vendor ID")]
            public int vid { get; set; }
            [Display(Name = "Period")]
            public int pid { get; set; }
            [Display(Name = "Grouped or SeparatePeriods")]
            public int isGrouped { get; set; }

            public IEnumerable<System.Web.Mvc.SelectListItem> SearchVendorList { get; set; }
            public IEnumerable<System.Web.Mvc.SelectListItem> SearchPeriodList { get; set; }

    }

            public class MemberNoSalesReport
        {
            public int PeriodID { get; set; }
            public int VendorID { get; set; }
            public int CustID { get; set; }
            public string PeriodName { get; set; }
            public string MemberName { get; set; }
            public string VendorName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            [DataType(DataType.Date)]
            public Nullable<System.DateTime> MemberStatusDate { get; set; }
            public string MemberStatus { get; set; }

        }


}


