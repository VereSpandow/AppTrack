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
    
    public partial class c_PaymentDetail
    {
        public int detailID { get; set; }
        public Nullable<int> paymentID { get; set; }
        public string detailname { get; set; }
        public string detailvalue { get; set; }
        public Nullable<System.DateTime> postdate { get; set; }
    }
}