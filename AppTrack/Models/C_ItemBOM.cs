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
    
    public partial class C_ItemBOM
    {
        public int IBID { get; set; }
        public int ParentID { get; set; }
        public int ItemID { get; set; }
        public int Quantity { get; set; }
        public int StatusID { get; set; }
        public Nullable<System.DateTime> PostDate { get; set; }
    }
}
