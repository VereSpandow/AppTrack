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
    
    public partial class C_ExportDataLogDetail
    {
        public int ID { get; set; }
        public Nullable<int> BatchID { get; set; }
        public Nullable<int> CustID { get; set; }
        public Nullable<int> EventTypeID { get; set; }
        public Nullable<int> EventID { get; set; }
        public string EventLabel { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> EventTriggerDate { get; set; }
        public Nullable<System.DateTime> EventCreationDate { get; set; }
        public Nullable<System.DateTime> EventCompletionDate { get; set; }
        public string Status { get; set; }
        public Nullable<int> Flag1 { get; set; }
        public Nullable<int> Flag2 { get; set; }
        public Nullable<int> Flag3 { get; set; }
        public Nullable<int> Flag4 { get; set; }
        public Nullable<decimal> Amount1 { get; set; }
        public Nullable<decimal> Amount2 { get; set; }
        public Nullable<decimal> Amount3 { get; set; }
        public Nullable<decimal> Amount4 { get; set; }
        public string VariantData1 { get; set; }
        public string VariantData2 { get; set; }
        public string VariantData3 { get; set; }
        public string VariantData4 { get; set; }
        public Nullable<decimal> Volume1 { get; set; }
        public Nullable<decimal> Volume2 { get; set; }
        public Nullable<decimal> Volume3 { get; set; }
        public Nullable<decimal> Volume4 { get; set; }
        public Nullable<int> StatusID { get; set; }
        public Nullable<int> AdminID { get; set; }
        public System.DateTime PostDate { get; set; }
    }
}