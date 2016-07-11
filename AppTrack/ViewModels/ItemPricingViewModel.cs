using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AppTrack.Models;

// Caution if we include System.Web.Mvc for use with SelectListItem, 
// the Compare attribute becomes ambiguous as it is in 2 namespaces.
// So, just specify System.Web.Mvc for SelectListItem

namespace AppTrack.ViewModels
{
    public class ItemPricingViewModel : C_ItemPricing
    {
        public ItemPricingViewModel()
        {
            CustTypeID = 1;

            PriceLevel = 1; 
            StartDate = DateTime.Now; 
            EndDate = DateTime.Now.AddYears(100); 
            RetailPrice = 0; 
            TaxableAmount = 0; 
            ShippingValue = 0; 
            SalesPrice = 0; 
            
            Volume1 = 0; 
            Volume2 = 0; 

            StatusID = 1;
            PostDate = DateTime.Now;           
        }

        public List<C_ItemPricing> ItemPricingList { get; set; }

        public int ID { get; set; }

        public int ItemID { get; set; }

        /*
        [Display(Name = "SKU")]
        [MaxLength(50)]
        public string SKU { get; set; }
        */

        [Display(Name = "Customer Type")]
        public int CustTypeID { get; set; }

        /*
        [Display(Name = "Pricing Type")]
        public Nullable<int> PricingTypeID { get; set; }
        */

        [Display(Name = "Price Level")]
        public int PriceLevel { get; set; }

        [Required(ErrorMessage = "Required"), Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime StartDate { get; set; }
                
        [Required(ErrorMessage = "Required"), Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public System.DateTime EndDate { get; set; }

        [Display(Name = "Retail Price")]
        public Nullable<decimal> RetailPrice { get; set; }

        [Display(Name = "Tax")]
        public Nullable<decimal> TaxableAmount { get; set; }

        [Display(Name = "Shipping")]
        public Nullable<decimal> ShippingValue { get; set; }

        [Display(Name = "Sales Price")]
        public Nullable<decimal> SalesPrice { get; set; }

        /*
        [Display(Name = "Volume")]
        public Nullable<decimal> Volume { get; set; }
        */

        [Display(Name = "Volume 1")]
        public Nullable<decimal> Volume1 { get; set; }

        [Display(Name = "Volume 2")]
        public Nullable<decimal> Volume2 { get; set; }

        [Display(Name = "Status")]
        public Nullable<int> StatusID { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> PostDate { get; set; }
    }
}