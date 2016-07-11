using AppTrack.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppTrack.ViewModels
{
    public class PaymentProcessViewModel
    {
    }

    public class PaymentOrderListViewModel
    {
        public List<TmpOrderHeaderForPayment> OrderHeaderList { get; set; }

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

        [Display(Name = "Submit Action")]
        public string SubmitAction { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> SearchStatusList { get; set; }

        [Display(Name = "Product")]
        public int? SearchItemID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> SearchItemList { get; set; }
    }

}