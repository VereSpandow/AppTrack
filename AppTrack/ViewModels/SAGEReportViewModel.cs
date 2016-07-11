using AppTrack.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppTrack.ViewModels
{
    public class ChangeLog
    {
        public int ID { get; set; }

        public int ChangeID { get; set; }

        public int CustomerType { get; set; }
            
        public string AccountingID { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> PostDate { get; set; }

        public int? ChangeBy { get; set; }

        public string ChangeConfirmed { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> ConfirmedDate { get; set; }
    }

    public class PayeeChangeListViewModel
    {
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> SearchStartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public Nullable<System.DateTime> SearchEndDate { get; set; }

        [Display(Name = "Confirmed")]
        public string SearchChangeConfirmed { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchChangeConfirmedList { get; set; }

        public List<ChangeLog> ChangeLogList { get; set; }

    }
}

