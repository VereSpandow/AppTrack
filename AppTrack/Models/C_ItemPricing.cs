//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AppTrack.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class C_ItemPricing
    {
        public int ID { get; set; }
        public int ItemID { get; set; }
        public string SKU { get; set; }
        public int CustTypeID { get; set; }
        public Nullable<int> PricingTypeID { get; set; }
        public int PriceLevel { get; set; }
        public Nullable<decimal> RetailPrice { get; set; }
        public Nullable<decimal> TaxableAmount { get; set; }
        public Nullable<decimal> ShippingValue { get; set; }
        public Nullable<decimal> SalesPrice { get; set; }
        public Nullable<decimal> Volume { get; set; }
        public Nullable<int> StatusID { get; set; }
        public Nullable<System.DateTime> PostDate { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
    }
}