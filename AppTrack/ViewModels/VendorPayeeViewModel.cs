
using AppTrack.SharedModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace AppTrack.ViewModels
{
    public class VendorPayeeListViewModel
    {
        public List<VendorPayee> VendorPayeeList { get; set; }

        [Display(Name = "Member ID")]
        public int? SearchCustID { get; set; }

        [Display(Name = "Name")]
        [MaxLength(10)]
        public string SearchDisplayName { get; set; }

        [Display(Name = "Vendor ID")]
        public int? SearchVendorID { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> SearchVendorList { get; set; }

        [Display(Name = "Vendor Payee ID")]
        [MaxLength(10)]
        public string SearchVendorPayeeID { get; set; }


    }

    public class VendorPayee 
    {
        public int ID { get; set; }
        public int VendorID { get; set; }
        public string  VendorPayeeID { get; set; }
        public int PayeeID { get; set; }

        public string VendorName { get; set; }
        public int CustID { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string DayPhone { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Status { get; set; }
        public int StatusID  { get; set; }

        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<System.DateTime> StatusDate { get; set; }
        public Nullable<System.DateTime> PostDate { get; set; }

    }

}