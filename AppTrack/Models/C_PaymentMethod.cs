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
    
    public partial class C_PaymentMethod
    {
        public Nullable<int> ID { get; set; }
        public int PMID { get; set; }
        public string PaymentType { get; set; }
        public Nullable<int> CustID { get; set; }
        public string PaymentProfileID { get; set; }
        public Nullable<int> BatchID { get; set; }
        public string PRefNumber { get; set; }
        public string PFirstName { get; set; }
        public string PMiddleName { get; set; }
        public string PLastName { get; set; }
        public string PName { get; set; }
        public string PAddress1 { get; set; }
        public string PAddress2 { get; set; }
        public string PCity { get; set; }
        public string PState { get; set; }
        public string PPostalCode { get; set; }
        public string PCountry { get; set; }
        public string PPhone { get; set; }
        public string PCardType { get; set; }
        public string PBinNumber { get; set; }
        public string PCardNumber { get; set; }
        public string PExpirationDate { get; set; }
        public string PExpirationMonth { get; set; }
        public string PExpirationYear { get; set; }
        public string PBankName { get; set; }
        public string PRoutingNumber { get; set; }
        public string PAccountNumber { get; set; }
        public string PAccountType { get; set; }
        public string PCheckNumber { get; set; }
        public Nullable<int> PPriority { get; set; }
        public Nullable<System.DateTime> PStartDate { get; set; }
        public Nullable<System.DateTime> PEndDate { get; set; }
        public string Status { get; set; }
        public Nullable<decimal> CreditAmount { get; set; }
        public Nullable<decimal> CreditBalance { get; set; }
        public Nullable<int> StatusID { get; set; }
        public Nullable<System.DateTime> PostDate { get; set; }
        public string ShippingProfileID { get; set; }
        public string CreditDescription { get; set; }
    }
}
