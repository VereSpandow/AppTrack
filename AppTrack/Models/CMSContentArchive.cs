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
    
    public partial class CMSContentArchive
    {
        public int Id { get; set; }
        public string SectionName { get; set; }
        public string ViewName { get; set; }
        public Nullable<byte> ContentType { get; set; }
        public string ContentReference { get; set; }
        public string ContentValue { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public Nullable<int> AdminID { get; set; }
        public string Status { get; set; }
        public Nullable<int> StatusID { get; set; }
        public Nullable<System.DateTime> PostDate { get; set; }
    }
}
