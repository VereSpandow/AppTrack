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
    
    public partial class C_Regions
    {
        public int ID { get; set; }
        public int RegionTypeID { get; set; }
        public int RegionID { get; set; }
        public string RegionName { get; set; }
        public string RegionDescription { get; set; }
        public string RegionImage { get; set; }
        public int SeqNo { get; set; }
        public int StatusID { get; set; }
        public string RegionOwner { get; set; }
        public System.DateTime PostDate { get; set; }
    }
}