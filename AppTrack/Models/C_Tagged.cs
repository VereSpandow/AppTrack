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
    
    public partial class C_Tagged
    {
        public int ID { get; set; }
        public int CampaignID { get; set; }
        public int TaggedID { get; set; }
        public byte TagType { get; set; }
        public int StatusID { get; set; }
        public int AdminID { get; set; }
        public System.DateTime PostDate { get; set; }
    }
}
