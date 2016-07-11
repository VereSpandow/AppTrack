using AppTrack.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppTrack.ViewModels
{
    public class AutoShipHeaderListViewModel
    {
        public List<AutoShipHeader> AutoShipHeaderList { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> SearchStartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> SearchEndDate { get; set; }

        [Display(Name = "Customer ID")]
        public string SearchCustID { get; set; }

        [Display(Name = "Status")]
        public string SearchStatus { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> SearchStatusList { get; set; }
    }
    public class AutoShipHeaderUpdateViewModel
    {
        public AutoShipHeader AutoShipHeader { get; set; }

        public List<AutoShipDetail> AutoShipDetailList { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> EditNextDate { get; set; }

        [Display(Name = "Status")]
        public string EditStatus { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> EditStatusList { get; set; }
    }

    public class OrderListViewModel
    {
        public List<C_OrderHeader> OrderHeaderList { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> SearchStartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> SearchEndDate { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{c:2}")]
        public decimal SearchStartBalance { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{c:2}")]
        public decimal SearchEndBalance { get; set; }

        [Display(Name = "Customer ID")]
        public int? SearchCustID { get; set; }

        [Display(Name = "Order ID")]
        public int? SearchOrderID { get; set; }

        [Display(Name = "Status")]
        public string SearchStatus { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> SearchStatusList { get; set; }

        [Display(Name = "Product")]
        public int? SearchItemID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> SearchItemList { get; set; }
    }

    public class OrderUpdateViewModel
    {
        public C_OrderHeader OrderHeader { get; set; }

        public List<C_OrderDetail> OrderDetailList { get; set; }

        public List<C_PaymentHistory> PaymentHistoryList { get; set; }

        public List<C_PaymentHistory> ReturnPaymentList { get; set; }

        public List<C_PaymentHistory> RefundPaymentList { get; set; }

        public List<C_OrderHistory> DiscountList { get; set; }

        public C_PaymentMethod PaymentMethod { get; set; } 

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> PaymentDate { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{c:2}")]
        [Range(.01, 9999)]
        public decimal PaymentAmount { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentType { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{c:2}")]
        public decimal DiscountAmount { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{c:2}")]
        [Range(.01, 9999)]
        public decimal RefundAmount { get; set; }

        [Required]
        [Display(Name = "Refund Reason")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Discount Reason")]
        public string DiscountDescription { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        [Display(Name = "Check Date")]
        public Nullable<System.DateTime> CheckDate { get; set; }

        [Required]
        [Display(Name = "Check Number")]
        public int? CheckNumber { get; set; }

        [Required]
        [Display(Name = "Transaction ID")]
        public string TransactionID { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{c:2}")]
        [Range(.01, 9999)]
        public decimal CheckAmount { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        public int OrderID { get; set; }

        [Required(ErrorMessage="Select a payment")]
        public int PaymentID { get; set; }

    }

    public class FailedPaymentList : C_OrderHeader
    {
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{MMMM 0:dd yyyy")]
        public Nullable<System.DateTime> PaymentDate { get; set; }

        [Display(Name = "Payment ID")]
        public int? PaymentID { get; set; }

        [Display(Name = "ResponseString")]
        public string ResponseString { get; set; }

        [Display(Name = "PaymentStatus")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Payment Amount")]
        public decimal Amount{ get; set; }

        [Display(Name = "PaymentType")]
        public string PaymentType { get; set; }

        [Display(Name = "CardNumber")]
        public string CardNumber { get; set; }

    }

    public class FailedPaymentListViewModel
    {
        public List<FailedPaymentList> OrderHeaderList { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> SearchStartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> SearchEndDate { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{c:2}")]
        public decimal SearchStartBalance { get; set; }

        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{c:2}")]
        public decimal SearchEndBalance { get; set; }

        [Display(Name = "Customer ID")]
        public int? SearchCustID { get; set; }

        [Display(Name = "Order ID")]
        public int? SearchOrderID { get; set; }

        [Display(Name = "Status")]
        public string SearchStatus { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> SearchStatusList { get; set; }

        [Display(Name = "Product")]
        public int? SearchItemID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> SearchItemList { get; set; }
    }


}