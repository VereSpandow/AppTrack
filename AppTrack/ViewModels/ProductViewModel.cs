using AppTrack.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AppTrack.ViewModels
{
    public class ProgramItem
    {
        public int ProgramID { get; set; }

        [Display(Name = "Membership")]
        public string ProgramName { get; set; }

        public int ParentID { get; set; }

        public int ItemID { get; set; }

        [Display(Name = "Product Code")]
        public string ProductCode { get; set; }
        
        [Display(Name = "Product Title")]
        public string ItemTitle { get; set; }
        
        [Display(Name = "Product Description")]
        public string ItemDescription { get; set; }

        [Display(Name = "GL Code")]
        public string GLCode { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }
        
        public int StatusID { get; set; }
    }

    public class ProgramItemListViewModel
    {
        public List<ProgramItem> ProgramItemList { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> ProgramList { get; set; }

    }
    

    public class ItemDetailViewModel
    {
        public string ItemTitle { get; set; }
        public List<C_ItemPricing> ItemPricingList { get; set; }
        public List<C_ItemPromotion> ItemPromotionList { get; set; }

    }
}