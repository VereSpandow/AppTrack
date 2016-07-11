using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Security;
using AppTrack.SharedModels;
using AppTrack.Models;
// Caution if we include System.Web.Mvc for use with SelectListItem, the Compare attribute becomes ambiguous as it is in 2 namespaces.
// So, just specify System.Web.Mvc for SelectListItem

namespace AppTrack.ViewModels
{
    public class RebateImportBatch : C_ImportDataLog
    {
        [Display(Name = "Vendor")]
        public string VendorName { get; set; }

        public int PendingCount { get; set; }
    }

    public class RebateBatchImportListViewModel
    {
        [Display(Name = "Vendor")]
        public int VendorID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> VendorList { get; set; }

        [Display(Name = "Rebate Type")]
        public int VolumeID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> VolumeList { get; set; }

        [Display(Name = "Status")]
        public string SearchStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public System.DateTime SearchStartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public System.DateTime SearchEndDate { get; set; }

        public List<RebateImportBatch> RebateImportBatchList { get; set; }
    }

    public class RebateBatchImportDetailViewModel
    {
        public int BatchID { get; set; }

        public string VendorName { get; set; }
        public string RebateType { get; set; }
        public string BatchStatus { get; set; }

        public int PendingCount { get; set; }

        [Display(Name = "Vendor Payee ID")]
        public string VendorPayeeID { get; set; }

        [Display(Name = "Payee Name")]
        public string PayeeName { get; set; }

        [Display(Name = "Status")]
        public string SearchStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }

        public List<C_VendorRebateTransactions> RebateTransactionList { get; set; }
    }

    public class RebateFileUploadViewModel
    {

        [Display(Name = "Vendor")]
        public int VendorID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> VendorList { get; set; }

        [Display(Name = "Rebate Type")]
        public int VolumeID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> VolumeList { get; set; }

        public Document RebateFile { get; set; }

    }

    public class RebateFindPayeeModel
    {

        public int TransactionID { get; set; }

        public C_VendorRebateTransactions VendorRebateTransaction { get; set; }

        public int BatchID { get; set; }
        public int VendorID { get; set; }

        [Display(Name = "TIN Name")]
        [MaxLength(50)]
        public string SearchCompany { get; set; }

        [Display(Name = "Practice Name")]
        [MaxLength(50)]
        public string SearchDisplayName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(50)]
        public string SearchLastName { get; set; }

        [Display(Name = "Address Line 1")]
        [MaxLength(50)]
        public string SearchAddress1 { get; set; }

        [Display(Name = "City")]
        [MaxLength(50)]
        public string SearchCity { get; set; }

        [Display(Name = "State")]
        [MaxLength(50)]
        public string SearchState { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }

        [Display(Name = "Zip Code")]
        [DataType(DataType.PostalCode)]
        public string SearchPostalCode { get; set; }

        [Display(Name = "Phone")]
        [DataType(DataType.PhoneNumber)]
        public string SearchPhone { get; set; }

        [Display(Name = "AppTrack ID")]
        public string SearchSecID { get; set; }

        public List<CustomerBasic> MemberList { get; set; }


    }


}