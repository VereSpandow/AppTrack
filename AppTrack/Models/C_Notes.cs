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
    
    public partial class C_Notes
    {
        public int NoteID { get; set; }
        public int CustID { get; set; }
        public Nullable<int> OwnerID { get; set; }
        public Nullable<int> ActivityID { get; set; }
        public string NoteType { get; set; }
        public string CommType { get; set; }
        public string CommDirection { get; set; }
        public string Title { get; set; }
        public string NoteReason { get; set; }
        public string NoteText { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> ScheduledDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<int> AssignedTo { get; set; }
        public Nullable<System.DateTime> AssignedDate { get; set; }
        public string ResultCode { get; set; }
        public string ResultDescription { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> StatusDate { get; set; }
        public int StatusID { get; set; }
        public Nullable<int> AccessLevel { get; set; }
        public int AdminID { get; set; }
        public System.DateTime PostDate { get; set; }
    }
}