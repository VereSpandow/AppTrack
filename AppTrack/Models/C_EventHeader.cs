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
    
    public partial class C_EventHeader
    {
        public int ID { get; set; }
        public Nullable<int> CategoryID { get; set; }
        public Nullable<int> CustID { get; set; }
        public string SponsorName { get; set; }
        public string EventTitle { get; set; }
        public string EventDescription { get; set; }
        public System.DateTime EventStartDate { get; set; }
        public Nullable<System.DateTime> EventEndDate { get; set; }
        public Nullable<int> PaymentFlag { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string EventURL { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> RegisterStartDate { get; set; }
        public Nullable<System.DateTime> RegisterEndDate { get; set; }
        public string Status { get; set; }
        public System.DateTime StatusDate { get; set; }
        public int StatusID { get; set; }
        public int AdminID { get; set; }
        public System.DateTime PostDate { get; set; }
        public string EventDateTimeString { get; set; }
        public Nullable<int> Capacity { get; set; }
        public Nullable<int> Reserved { get; set; }
        public Nullable<int> Available { get; set; }
        public string LocationTitle { get; set; }
    }
}
