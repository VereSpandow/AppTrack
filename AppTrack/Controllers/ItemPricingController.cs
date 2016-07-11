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
    public class ItemPricingController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        // GET: Item/Edit/5        
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            C_ItemPricing c_ItemPrice = db.C_ItemPricing.Find(id);
            if (c_ItemPrice == null)
            {
                return HttpNotFound();
            }

            var _model = new ItemPricingViewModel()
            {
                ID = (int)id,
                ItemID = c_ItemPrice.ItemID,
                CustTypeID = c_ItemPrice.CustTypeID,
                PriceLevel = c_ItemPrice.PriceLevel,
                StartDate = c_ItemPrice.StartDate,
                EndDate = c_ItemPrice.EndDate,
                RetailPrice = c_ItemPrice.RetailPrice,
                TaxableAmount = c_ItemPrice.TaxableAmount,
                ShippingValue = c_ItemPrice.ShippingValue,
                SalesPrice = c_ItemPrice.SalesPrice,
                Volume = c_ItemPrice.Volume,
                StatusID = c_ItemPrice.StatusID,
                PostDate = (DateTime)c_ItemPrice.PostDate,
            };

            return View(_model);
        }


        // POST: ItemPricing/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID, ItemID, CustTypeID, PriceLevel, StartDate, EndDate, RetailPrice, TaxableAmount, ShippingValue, SalesPrice, Volume1, Volume2, StatusID, PostDate")]ItemPricingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var _item = db.C_ItemPricing.SingleOrDefault(a => a.ID == model.ID);

                _item.ItemID = model.ItemID;
                _item.CustTypeID = model.CustTypeID;
                _item.PriceLevel = model.PriceLevel;
                _item.StartDate = model.StartDate;
                _item.EndDate = model.EndDate;
                _item.RetailPrice = model.RetailPrice;
                _item.TaxableAmount = model.TaxableAmount;
                _item.ShippingValue = model.ShippingValue;
                _item.SalesPrice = model.SalesPrice;
                _item.Volume = model.Volume;
                _item.StatusID = model.StatusID;
                _item.PostDate = (DateTime)model.PostDate;

                db.Entry(_item).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Edit", "Item", new { id = model.ItemID });
            }

            return View(model);
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
