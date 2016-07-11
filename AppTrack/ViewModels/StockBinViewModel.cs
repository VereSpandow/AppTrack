using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppTrack.Models;

// Caution if we include System.Web.Mvc for use with SelectListItem, 
// the Compare attribute becomes ambiguous as it is in 2 namespaces.
// So, just specify System.Web.Mvc for SelectListItem

namespace AppTrack.ViewModels
{   
    public class StockBinViewModel : C_StockBin
    {
        public StockBinViewModel()
        {   
            ItemID = 0; 

            OnHand = 0; 
            QtyCommitted = 0; 
            QtyShipped = 0; 
            Available = 0; //Computed , Numeric
            UnitOfMeasure = null;

            StatusID = 1;
            PostDate = DateTime.Now;            
        }

        public List<C_StockBin> StockBinList { get; set; }

        public int InventoryID { get; set; }

        /*
        [Display(Name = "Warehouse ID")]
        public Nullable<int> WarehouseID { get; set; }

        [Display(Name = "Location ID")]
        public Nullable<int> LocationID { get; set; }
        */

        public Nullable<int> ItemID { get; set; }               
        
        /*
        [Display(Name = "Product Code")]
        [MaxLength(50)]
        public string ProductCode { get; set; }
        
        [Display(Name = "Lot ID")]
        [MaxLength(50)]
        public string LotID { get; set; }

        [Display(Name = "Lot Cost")]
        public Nullable<decimal> LotCost { get; set; }
        
        [Display(Name = "Vendor")]
        [MaxLength(250)]
        public string Vendor { get; set; }

        [Display(Name = "Lot Receipt Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> LotReceiptDate { get; set; }

        [Display(Name = "Product Name")]
        [MaxLength(50)]
        public string ProductName { get; set; }
        */

        [Display(Name = "On Hand")]
        public Nullable<decimal> OnHand { get; set; }

        [Display(Name = "Qty Committed")]
        public Nullable<decimal> QtyCommitted { get; set; }

        [Display(Name = "Qty Shipped")]
        public Nullable<decimal> QtyShipped { get; set; }

        [Display(Name = "Available")]
        public Nullable<decimal> Available { get; set; }

        [Display(Name = "Unit of Measure")]
        [MaxLength(50)]
        public string UnitOfMeasure { get; set; }

        public Nullable<int> AdminID { get; set; }               

        [Display(Name = "Status")]
        public Nullable<int> StatusID { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> PostDate { get; set; }
    }
}