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
    public class StockBinController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        // GET: StockBin/Edit/5      
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            C_StockBin c_StockBin = db.C_StockBin.Find(id);
            if (c_StockBin == null)
            {
                return HttpNotFound();
            }

            var _vModelItemStock = new StockBinViewModel()
            {
                InventoryID = (int)id,
                ItemID = c_StockBin.ItemID,
                OnHand = c_StockBin.OnHand,
                QtyCommitted = c_StockBin.QtyCommitted,
                QtyShipped = c_StockBin.QtyShipped,
                Available = c_StockBin.Available,
                UnitOfMeasure = c_StockBin.UnitOfMeasure,
                StatusID = c_StockBin.StatusID,
                PostDate = (DateTime)c_StockBin.PostDate
            };

            return View(_vModelItemStock);
        }

        // POST: StockBin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "InventoryID,WarehouseID,LocationID,ItemID,ProductCode,LotID,LotCost,Vendor,LotReceiptDate,ProductName,OnHand,QtyCommitted,QtyShipped,Available,UnitOfMeasure,AdminID,StatusID,PostDate")] StockBinViewModel vModelStock)
        {
            if (ModelState.IsValid)
            {
                var _stock = db.C_StockBin.SingleOrDefault(a => a.InventoryID == vModelStock.InventoryID);

                _stock.ItemID = vModelStock.ItemID;
                _stock.OnHand = vModelStock.OnHand;
                _stock.QtyCommitted = vModelStock.QtyCommitted;
                _stock.QtyShipped = vModelStock.QtyShipped;
                _stock.Available = vModelStock.Available;
                _stock.UnitOfMeasure = vModelStock.UnitOfMeasure;
                _stock.StatusID = vModelStock.StatusID;
                _stock.PostDate = (DateTime)vModelStock.PostDate;

                db.Entry(_stock).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Edit", "Item", new { id = vModelStock.ItemID });
            }

            return View(vModelStock);
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
