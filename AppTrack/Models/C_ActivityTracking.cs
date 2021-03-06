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
    
    public partial class C_ActivityTracking
    {
        public int ActivityID { get; set; }
        public Nullable<int> CustID { get; set; }
        public Nullable<int> OwnerID { get; set; }
        public Nullable<int> VendorID { get; set; }
        public Nullable<int> ActivityTypeID { get; set; }
        public string ActivityType { get; set; }
        public Nullable<int> CategoryID { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Outcome { get; set; }
        public Nullable<System.DateTime> TriggerDate { get; set; }
        public Nullable<System.DateTime> ScheduledDate { get; set; }
        public Nullable<System.DateTime> CompletionDate { get; set; }
        public Nullable<System.DateTime> AssignedDate { get; set; }
        public Nullable<int> AssignedToID { get; set; }
        public string AssignedToName { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> StatusChangeDate { get; set; }
        public Nullable<int> Flag1 { get; set; }
        public Nullable<int> Flag2 { get; set; }
        public Nullable<decimal> Volume1 { get; set; }
        public Nullable<decimal> Volume2 { get; set; }
        public string VariantData1 { get; set; }
        public string VariantData2 { get; set; }
        public Nullable<int> StatusID { get; set; }
        public Nullable<int> AdminID { get; set; }
        public Nullable<System.DateTime> PostDate { get; set; }
    }
}
