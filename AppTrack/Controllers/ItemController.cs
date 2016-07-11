using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AppTrack.Models;
using AppTrack.ViewModels;
using AppTrack.Helpers;

namespace AppTrack.Controllers
{
    [AuthorizeAdminRedirect(Roles = Constants.adminRoles)]
    public class ItemController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        // GET: Item
        public ActionResult Index()
        {

            var itemListViewModel = new ItemListViewModel()
            {
                ItemList = db.C_Item.ToList()
            };

            return View(itemListViewModel);
        }

        // GET: Item/Create
        public ActionResult Create()
        {
            var _vmItem = new ItemViewModel();
            return View(_vmItem);
        }

        // POST: Item/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ItemID,ParentID,ProductCode,ItemName,ItemTitle,ItemDescription,ItemImage1,ItemImage2,ItemDetailPage,ItemVideo,IsShip,MinQuantity,MaxQuantity,ToItemID,FromItemID,Status,isAutoShip,isShipping,StartDate,EndDate,StatusID,IsPrePackaged,DisplayFlag,PostDate")]ItemViewModel vModelItem)
        {
            var _item = new C_Item
            {
                ProgramID = vModelItem.ProgramID,
                ProductCode = vModelItem.ProductCode,
                ItemName = vModelItem.ItemName,
                ItemTitle = vModelItem.ItemTitle,
                ItemDescription = vModelItem.ItemDescription,
                ItemImage1 = vModelItem.ItemImage1,
                MinQuantity = vModelItem.MinQuantity,
                MaxQuantity = vModelItem.MaxQuantity,
                FromItemID = vModelItem.FromItemID,
                ToItemID = vModelItem.ToItemID,
                Status = vModelItem.Status,
                isAutoShip = vModelItem.isAutoShip,
                StartDate = vModelItem.StartDate,
                EndDate = vModelItem.EndDate,
                DisplayFlag = vModelItem.DisplayFlag,
                StatusID = vModelItem.StatusID,
                PostDate = (DateTime)vModelItem.PostDate
            };

            if (ModelState.IsValid)
            {
                db.C_Item.Add(_item);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(vModelItem);
        }

        [HttpPost]
        public ActionResult FileUpload(HttpPostedFileBase file, ItemViewModel model)
        {
            int _itemID = model.ItemID;
            string _imageURL = "~/images/";
            if (file != null)
            {
                string _imgNameLoaded = file.FileName.Substring(0, file.FileName.IndexOf('.'));
                string _imgNameCurrent = file.FileName.Replace(_imgNameLoaded, "item_" + _itemID);
                string _imgServerPath = System.IO.Path.Combine(Server.MapPath(_imageURL), _imgNameCurrent);
                file.SaveAs(_imgServerPath);

                var _item = db.C_Item.SingleOrDefault(a => a.ItemID == _itemID);
                _item.ItemImage1 = _imageURL + _imgNameCurrent;
                db.Entry(_item).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Edit", "Item", new { id = _itemID });
        }

        // GET: Index & GET: Price/Create
        public ActionResult _pricing(int? itemID)
        {
            var _vModelItemPrice = new ItemPricingViewModel()
            {
                ItemID = (int)itemID,
                ItemPricingList = db.C_ItemPricing.Where(p => p.ItemID == itemID).ToList()
            };

            return PartialView(_vModelItemPrice);
        }

        // POST: Price/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _createPrice([Bind(Include = "ID, ItemID, CustTypeID, PriceLevel, StartDate, EndDate, RetailPrice, TaxableAmount, ShippingValue, SalesPrice, Volume1, Volume2, StatusID, PostDate")]ItemPricingViewModel vModelItemPrice)
        {
            var _itemPrice = new C_ItemPricing
            {
                ItemID = vModelItemPrice.ItemID,
                CustTypeID = vModelItemPrice.CustTypeID,
                PriceLevel = vModelItemPrice.PriceLevel,
                StartDate = vModelItemPrice.StartDate,
                EndDate = vModelItemPrice.EndDate,
                RetailPrice = vModelItemPrice.RetailPrice,
                TaxableAmount = vModelItemPrice.TaxableAmount,
                ShippingValue = vModelItemPrice.ShippingValue,
                SalesPrice = vModelItemPrice.SalesPrice,
                Volume = vModelItemPrice.Volume,
                StatusID = vModelItemPrice.StatusID,
                PostDate = (DateTime)vModelItemPrice.PostDate
            };

            if (ModelState.IsValid)
            {
                db.C_ItemPricing.Add(_itemPrice);
                db.SaveChanges();
                return RedirectToAction("Edit", new { id = vModelItemPrice.ItemID });
            }

            return PartialView(vModelItemPrice);
        }

        // GET: Index & GET: Stock/Create
        public ActionResult _stock(int? itemID)
        {
            var _vModelItemStock = new StockBinViewModel()
            {
                ItemID = (int)itemID,
                StockBinList = db.C_StockBin.Where(p => p.ItemID == itemID).ToList()
            };

            return PartialView(_vModelItemStock);
        }

        // POST: Price/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _createStock([Bind(Include = "InventoryID, ItemID, OnHand, QtyCommitted, QtyShipped, Available, UnitOfMeasure, StatusID, PostDate")]StockBinViewModel vModelItemStock)
        {
            var _itemStock = new C_StockBin
            {
                ItemID = vModelItemStock.ItemID,
                OnHand = vModelItemStock.OnHand,
                QtyCommitted = vModelItemStock.QtyCommitted,
                QtyShipped = vModelItemStock.QtyShipped,
                Available = vModelItemStock.Available,
                UnitOfMeasure = vModelItemStock.UnitOfMeasure,
                StatusID = vModelItemStock.StatusID,
                PostDate = (DateTime)vModelItemStock.PostDate
            };

            if (ModelState.IsValid)
            {
                db.C_StockBin.Add(_itemStock);
                db.SaveChanges();
                return RedirectToAction("Edit", new { id = vModelItemStock.ItemID });
            }

            return PartialView(vModelItemStock);
        }


        // GET: Item/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            C_Item c_Item = db.C_Item.Find(id);
            if (c_Item == null)
            {
                return HttpNotFound();
            }

            var _vModelItem = new ItemViewModel()
            {
                ItemID = (int)id,
                ProgramID = c_Item.ProgramID,
                ProductCode = c_Item.ProductCode,
                ItemName = c_Item.ItemName,
                ItemTitle = c_Item.ItemTitle,
                ItemDescription = c_Item.ItemDescription,
                ItemImage1 = c_Item.ItemImage1,
                MinQuantity = c_Item.MinQuantity,
                MaxQuantity = c_Item.MaxQuantity,
                FromItemID = c_Item.FromItemID,
                ToItemID = c_Item.ToItemID,
                Status = c_Item.Status,
                StartDate = c_Item.StartDate,
                EndDate = c_Item.EndDate,
                DisplayFlag = c_Item.DisplayFlag,
                isAutoShip = c_Item.isAutoShip,
                StatusID = c_Item.StatusID,
                PostDate = (DateTime)c_Item.PostDate
            };

            return View(_vModelItem);
        }

        // POST: Item/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ItemID,ParentID,ProductCode,ItemName,ItemTitle,ItemDescription,ItemImage1,ItemImage2,ItemDetailPage,ItemVideo,IsShip,MinQuantity,MaxQuantity,ToItemID,FromItemID,Status,isAutoShip,isShipping,StartDate,EndDate,StatusID,IsPrePackaged,DisplayFlag,PostDate")]ItemViewModel vModelItem)
        {
            if (ModelState.IsValid)
            {
                var _item = db.C_Item.SingleOrDefault(a => a.ItemID == vModelItem.ItemID);

                _item.ProgramID = vModelItem.ProgramID;
                _item.ProductCode = vModelItem.ProductCode;
                _item.ItemName = vModelItem.ItemName;
                _item.ItemTitle = vModelItem.ItemTitle;
                _item.ItemDescription = vModelItem.ItemDescription;
                _item.ItemImage1 = vModelItem.ItemImage1;
                _item.MinQuantity = vModelItem.MinQuantity;
                _item.MaxQuantity = vModelItem.MaxQuantity;
                _item.FromItemID = vModelItem.FromItemID;
                _item.ToItemID = vModelItem.ToItemID;
                _item.Status = vModelItem.Status;
                _item.StartDate = vModelItem.StartDate;
                _item.EndDate = vModelItem.EndDate;
                _item.DisplayFlag = vModelItem.DisplayFlag;
                _item.isAutoShip = vModelItem.isAutoShip;
                _item.StatusID = vModelItem.StatusID;
                _item.PostDate = (DateTime)vModelItem.PostDate;

                db.Entry(_item).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(vModelItem);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
