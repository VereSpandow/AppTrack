using AppTrack.Models;
using AppTrack.SharedModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppTrack.ViewModels
{
    public class SageImportPayoutBatchListViewModel
    {
        [Display(Name = "Status")]
        public string SearchStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public System.DateTime SearchStartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public System.DateTime SearchEndDate { get; set; }

        public List<C_ImportDataLog> ImportPayoutBatchList { get; set; }
    }

    public class SageImportPayoutBatchDetailViewModel
    {
        public int ImportBatchID { get; set; }

        public string FileName { get; set; }

        public string BatchStatus { get; set; }

        [Display(Name = "Status")]
        public string SearchStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }

        public List<C_ImportPayoutTransactions> PayoutTransactionList { get; set; }
    }

    public class SagePayoutFileUploadViewModel
    {
        public Document PayoutFile { get; set; }
    }
}