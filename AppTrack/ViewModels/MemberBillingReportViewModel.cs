using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace AppTrack.ViewModels
{
    public class MemberBillingReportViewModel
    {
        public List<MemberBillingReport> MemberBillingReportList { get; set; }
        
        [Display(Name = "Member Type")]
        [MaxLength(10)]
        public string MemberType { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchMemberTypeList { get; set; }

        [Display(Name = "Product Type")]
        public int ProductType { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchProductList { get; set; }

        [Display(Name = "Vendor")]
        public int SearchVendorID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchVendorList { get; set; }

        [Display(Name = "Period")]
        public int searchPeriodID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchPeriodList { get; set; }

    }

    public class MemberBillingReport
    {
        public int PeriodID { get; set; }
        public string PeriodName { get; set; }
        public int StoreID { get; set; }
        public string StoreName { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal SalesTotal { get; set; }
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PaidTotal { get; set; }
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal CancelTotal { get; set; }
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal DiscountTotal { get; set; }
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal BalanceDue { get; set; }
        public string Status { get; set; }
    }


    public class StackedBarSalesData
    {
        public int ID { get; set; }
        public string Month { get; set; }
        public string Measure { get; set; }
        public decimal Value1 { get; set; }
        public decimal Value2 { get; set; }
        public decimal Value3 { get; set; }

        public StackedBarSalesData(int iD, string month, string measure, decimal value1, decimal value2, decimal value3)
        {
            ID = iD;
            Month = month;
            Measure = measure;
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
        }
    }


}