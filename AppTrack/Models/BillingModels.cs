using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppTrack.Models
{
    public class AutoShipHeader
    {
        public int AutoShipID { get; set; }
        public Nullable<int> AutoShipTypeID { get; set; }
        public string OrderType { get; set; }
        public int OrderID { get; set; }
        [Display(Name = "Member ID")]
        public int CustID { get; set; }
        [Display(Name = "Last Bill Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> LastDate { get; set; }

        [Display(Name = "Next Bill Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> NextDate { get; set; }

        public int Interval { get; set; }

        public string IntervalUnit { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> EndDate { get; set; }

        public string ShipName { get; set; }

        [DisplayFormat(DataFormatString = "{0:c}")]
        public Nullable<decimal> OrderTotal { get; set; }

        public string Status { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> StatusDate { get; set; }

        public int StatusID { get; set; }

    }
    public class AutoShipDetail
    {
        public int AutoShipID { get; set; }
        
        public int ItemID { get; set; }
        
        public string ItemTitle { get; set; }
        
        public int Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:c}")]
        public Nullable<decimal> UnitPrice { get; set; }

        [DisplayFormat(DataFormatString = "{0:c}")]
        public Nullable<decimal> ExtPrice { get; set; }

        public int PriceLevel { get; set; }

        public string Status { get; set; }

        public int StatusID { get; set; }

    }

}