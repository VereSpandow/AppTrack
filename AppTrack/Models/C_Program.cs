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
    
    public partial class C_Program
    {
        public int ID { get; set; }
        public string ProgramName { get; set; }
        public string ProgramDescription { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public string Status { get; set; }
        public System.DateTime StatusDate { get; set; }
        public int StatusID { get; set; }
        public int AdminID { get; set; }
        public System.DateTime PostDate { get; set; }
        public Nullable<int> Seqno { get; set; }
    }
}
