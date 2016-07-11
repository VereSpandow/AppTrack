using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppTrack.ViewModels
{
    public class PromotionListViewModel
    {
        public int ItemID { get; set; }

        public int PromotionID { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> PromotionList { get; set; }

    }
}