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
    
    public partial class C_RankHeader
    {
        public int ID { get; set; }
        public int RankID { get; set; }
        public int CustID { get; set; }
        public Nullable<int> RankTypeID { get; set; }
        public Nullable<System.DateTime> AchievedDate { get; set; }
        public Nullable<System.DateTime> ExpirationDate { get; set; }
        public Nullable<int> PeriodID { get; set; }
        public Nullable<int> SourceID { get; set; }
        public Nullable<int> StatusID { get; set; }
        public System.DateTime PostDate { get; set; }
    }
}
