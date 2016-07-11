using AppTrack.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppTrack.ViewModels
{
    public class OrderHistory : C_OrderHistory
    {
        public string DisplayName { get; set; }
        public string AccountingID { get; set; }
        public string AdjustmentFlag { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class SageExportBatchListViewModel
    {
        [Display(Name = "Batch Type")]
        public string SearchBatchType { get; set; }

        [Display(Name = "Status")]
        public string SearchStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public System.DateTime SearchStartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public System.DateTime SearchEndDate { get; set; }

        public List<C_ExportDataLog> ExportBatchList { get; set; }
    }

    public class SageOrderExportBatchDetailViewModel
    {
        public int BatchID { get; set; }

        public string BatchDescription { get; set; }
        public string BatchType { get; set; }
        public string BatchStatus { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public System.DateTime CutoffDate { get; set; }


        [Display(Name = "Status")]
        public string SearchStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }

        public List<OrderHistory> OrderHistoryList { get; set; }
    }


}