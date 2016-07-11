using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data.Entity.Core.Objects;


using System.Collections.Generic;

using AppTrack.Models;
using AppTrack.SharedModels;
using AppTrack.ViewModels;
using AppTrack.Helpers;
using System.Data;
using System.Web;
using System.IO;
using System.Text;
using FastMember;

namespace AppTrack.Controllers
{

    [AuthorizeAdminRedirect(Roles = Constants.adminRoles)]
    public class ProductController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        //
        // MEETING SECTION
        //
        [HttpGet]
        public ActionResult Index()
        {
            var programItemRows = db.Database.SqlQuery<ProgramItem>("exec dbo.[LB_GetItemsInStore] @StoreID, @MinStatusID, @ParentFlag",
            new SqlParameter("@StoreID", Constants.memberStoreID),
            new SqlParameter("@MinStatusID", 3),
            new SqlParameter("@PArentFlag", 1)
            ).ToList();

            var DataHelper = new DataHelpers();

            IEnumerable<System.Web.Mvc.SelectListItem> programList = DataHelper.GetProgramList(Constants.memberStoreID);

            var programItemListViewModel = new ProgramItemListViewModel
            {
                ProgramItemList = programItemRows,
                ProgramList = programList
            };

            return View(programItemListViewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ItemDetail(int ItemID = 0)
        {
            var DataHelper = new DataHelpers();

            ItemDetailViewModel itemDetailViewModel = new ItemDetailViewModel();

            if (ModelState.IsValid) 
            {
                if (ItemID == 0)
                {

                }
                else
                {
                    C_Item itemRecord = db.Database.SqlQuery<C_Item>("exec dbo.[LB_GetItemByID] @ItemID",
                    new SqlParameter("@ItemID", ItemID)
                    ).FirstOrDefault();

                    var itemPricingRows = db.Database.SqlQuery<C_ItemPricing>("exec dbo.[LB_GetItemPricing] @ItemID",
                    new SqlParameter("@ItemID", ItemID)
                    ).ToList();

                    var itemPromotionRows = db.Database.SqlQuery<C_ItemPromotion>("exec dbo.[LB_GetItemPromotion] @ItemID",
                    new SqlParameter("@ItemID", ItemID)
                    ).ToList();

                    itemDetailViewModel.ItemTitle = itemRecord.ItemTitle;
                    itemDetailViewModel.ItemPricingList = itemPricingRows;
                    itemDetailViewModel.ItemPromotionList = itemPromotionRows;
                }
            }

            return PartialView("_ItemDetail", itemDetailViewModel);
        }
    }
}