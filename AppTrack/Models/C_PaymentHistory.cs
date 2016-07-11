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
    
    public partial class C_PaymentHistory
    {
        public int PaymentID { get; set; }
        public Nullable<int> PaymentMethodID { get; set; }
        public Nullable<System.DateTime> PaymentDate { get; set; }
        public Nullable<int> OrderID { get; set; }
        public Nullable<int> CustID { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public string Description { get; set; }
        public string PaymentType { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string CardType { get; set; }
        public string BankName { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public Nullable<int> Priority { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string ExtCode { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseState { get; set; }
        public Nullable<int> CheckNumber { get; set; }
        public Nullable<int> ResponseCode { get; set; }
        public string ResponseString { get; set; }
        public int ReturnPaymentID { get; set; }
        public string ReturnPaymentString { get; set; }
        public string ExtTransactionID { get; set; }
        public Nullable<decimal> RefundAmount { get; set; }
        public string Status { get; set; }
        public System.DateTime PostDate { get; set; }
        public int StatusID { get; set; }
        public Nullable<byte> EmailFlag { get; set; }
        public Nullable<int> AdminID { get; set; }
    }
}
