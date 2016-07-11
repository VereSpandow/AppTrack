using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AppTrack.Models;

namespace AppTrack.Controllers
{
    public class C_PayOutController : Controller
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        // GET: C_PayOut
       public ActionResult Index()
        {
            return View(db.C_PayOut.ToList());
        }

        // GET: C_PayOut/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            C_PayOut c_PayOut = db.C_PayOut.Find(id);
            if (c_PayOut == null)
            {
                return HttpNotFound();
            }
            return View(c_PayOut);
        }

        // GET: C_PayOut/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: C_PayOut/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,CustID,RefNumber,PayoutAmount,PayoutDate,PayoutMethodID,PayoutTypeID,PeriodID,PayoutStatusID,BatchID,SentToProcessing,VariantData1,PayoutCounter,AdminID,StatusID,SentToProcessingID,PostDate")] C_PayOut c_PayOut)
        {
            if (ModelState.IsValid)
            {
                db.C_PayOut.Add(c_PayOut);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(c_PayOut);
        }

        // GET: C_PayOut/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            C_PayOut c_PayOut = db.C_PayOut.Find(id);
            if (c_PayOut == null)
            {
                return HttpNotFound();
            }
            return View(c_PayOut);
        }

        // POST: C_PayOut/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,CustID,RefNumber,PayoutAmount,PayoutDate,PayoutMethodID,PayoutTypeID,PeriodID,PayoutStatusID,BatchID,SentToProcessing,VariantData1,PayoutCounter,AdminID,StatusID,SentToProcessingID,PostDate")] C_PayOut c_PayOut)
        {
            if (ModelState.IsValid)
            {
                db.Entry(c_PayOut).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(c_PayOut);
        }

        // GET: C_PayOut/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            C_PayOut c_PayOut = db.C_PayOut.Find(id);
            if (c_PayOut == null)
            {
                return HttpNotFound();
            }
            return View(c_PayOut);
        }

        // POST: C_PayOut/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            C_PayOut c_PayOut = db.C_PayOut.Find(id);
            db.C_PayOut.Remove(c_PayOut);
            db.SaveChanges();
            return RedirectToAction("Index");
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
