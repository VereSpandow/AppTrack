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
    
    public partial class C_MembershipActivity
    {
        public int ID { get; set; }
        public int CustID { get; set; }
        public Nullable<int> StoreID { get; set; }
        public Nullable<int> DepartmentID { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> StatusDate { get; set; }
        public Nullable<int> StatusID { get; set; }
        public Nullable<System.DateTime> PostDate { get; set; }
        public Nullable<int> Flag1 { get; set; }
        public Nullable<int> Flag2 { get; set; }
    }
}
