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
    public class ContractProviderListViewModel
    {
        public List<Contact> ContractProviderList { get; set; }

        [Display(Name = "Provider Name")]
        [MaxLength(50)]
        public string SearchCompany { get; set; }

        [Display(Name = "Status")]
        public string SelectedStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }

        [Display(Name = "Type")]
        public string SelectedType { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> ProviderTypeList { get; set; }
    }

    public class ContractProviderViewModel : Contact
    {
        public IEnumerable<System.Web.Mvc.SelectListItem> VendorList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> ProviderTypeList { get; set; }
    }

    public class ContractListViewModel
    {
        public List<Contract> ContractList { get; set; }

        [Display(Name = "Provider Name")]
        [MaxLength(50)]
        public string SearchCompany { get; set; }

        [Display(Name = "Status")]
        public string SelectedStatus { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public System.DateTime SearchStartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public System.DateTime SearchEndDate { get; set; }

    }

    public class ContractProfileViewModel
    {
        public Contract ContractRecord { get; set; }

        public List<ContractDetail> ContractDetailList { get; set; }

    }

    public class ContractViewModel : Contract
    {
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> ContractTypeList { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> ProviderTypeList { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> ProviderList { get; set; }
    }

    public class ContractDetailViewModel
    {
        public List<ContractDetail> ContractDetailList { get; set; }

        public ContractDetail ContractDetail { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> ContractDetailTypeList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> StatusList { get; set; }

    }

    public class ContractDocumentViewModel
    {
        public List<ContractDocument> ContractDocumentList { get; set; }

        public ContractDocument ContractDocument { get; set; }
    }


}