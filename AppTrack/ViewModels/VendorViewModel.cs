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
    public class VendorListViewModel
    {
        public List<Vendor> VendorList { get; set; }

        [Display(Name = "Company Name")]
        [MaxLength(50)]
        public string SearchCompany { get; set; }

        [Display(Name = "Status")]
        public string SelectedStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }

    }

    public class VendorCompanyViewModel : Vendor
    {
        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }

    }

    public class VendorContactViewModel
    {
        public List<VendorContact> VendorContactList { get; set; }

        public VendorContact vendorContact { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> StateList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> NameTitleList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> ContactTypeList { get; set; }

    }

    public class VendorRebateViewModel
    {
        public List<VendorRebate> VendorRebateList { get; set; }

        public VendorRebate VendorRebate { get; set; }

    }

    public class VendorProgramViewModel
    {
        public List<VendorProgram> VendorProgramList { get; set; }

        public VendorProgram vendorProgram { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> CompanyProgramList { get; set; }

    }

    public class VendorRequirementViewModel
    {
        public List<VendorRequirement> VendorRequirementList { get; set; }

        public VendorRequirement vendorRequirement { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> VendorRequirementTypeList { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> VendorDocumentList { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> VendorProgramList { get; set; }

    }

    public class VendorDocumentViewModel
    {
        public List<Document> VendorDocumentList { get; set; }

        public Document vendorDocument { get; set; }

    }

    public class ChartItem
    {
        public string ChartLabel { get; set; }
        public decimal ChartValue { get; set; }
    }


}