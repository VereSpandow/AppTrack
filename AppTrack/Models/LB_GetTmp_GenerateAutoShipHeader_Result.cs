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
    
    public partial class LB_GetTmp_GenerateAutoShipHeader_Result
    {
        public int AutoShipID { get; set; }
        public Nullable<int> AutoShipTypeID { get; set; }
        public string OrderType { get; set; }
        public Nullable<int> OrderID { get; set; }
        public int CustID { get; set; }
        public Nullable<int> PriceLevel { get; set; }
        public Nullable<System.DateTime> LastDate { get; set; }
        public Nullable<System.DateTime> NextDate { get; set; }
        public Nullable<int> Interval { get; set; }
        public string IntervalUnit { get; set; }
        public Nullable<int> PaymentMethodID { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<System.DateTime> LastUpdate { get; set; }
        public string ShipFirstName { get; set; }
        public string ShipLastName { get; set; }
        public string ShipName { get; set; }
        public string ShipAddress1 { get; set; }
        public string ShipAddress2 { get; set; }
        public string ShipCity { get; set; }
        public string ShipCounty { get; set; }
        public string ShipState { get; set; }
        public string ShipPostalCode { get; set; }
        public string ShipCountry { get; set; }
        public string ShipPhone { get; set; }
        public string ShipMethod { get; set; }
        public Nullable<decimal> Retailtotal { get; set; }
        public Nullable<decimal> Saletotal { get; set; }
        public Nullable<decimal> TaxableTotal { get; set; }
        public Nullable<decimal> ShippingTotal { get; set; }
        public Nullable<decimal> TaxTotal { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public Nullable<decimal> OrderTotal { get; set; }
        public Nullable<int> TemplateID { get; set; }
        public string VariantData1 { get; set; }
        public string VariantData2 { get; set; }
        public Nullable<int> AdminId { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> StatusDate { get; set; }
        public Nullable<int> StatusID { get; set; }
        public Nullable<System.DateTime> PostDate { get; set; }
    }
}
