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
    
    public partial class C_FeeDetail
    {
        public int ID { get; set; }
        public int CustID { get; set; }
        public int FeeID { get; set; }
        public decimal FeeAmount { get; set; }
        public int PeriodID { get; set; }
        public Nullable<System.DateTime> FeeDate { get; set; }
        public Nullable<System.DateTime> PostDate { get; set; }
    }
}
