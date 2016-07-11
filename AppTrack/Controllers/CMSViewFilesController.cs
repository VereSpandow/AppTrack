using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AppTrack.Models;
using AppTrack.Helpers;

namespace CMS.Controllers
{
    [AuthorizeAdminRedirect(Roles = "Accounting, Finance,Website")]
    public class CMSViewFilesController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        // GET: CMSViewFiles
        public ActionResult Index()
        {
            return View(db.CMSViewFiles.ToList());
        }

        // GET: CMSViewFiles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CMSViewFile cMSViewFile = db.CMSViewFiles.Find(id);
            if (cMSViewFile == null)
            {
                return HttpNotFound();
            }
            return View(cMSViewFile);
        }

        // GET: CMSViewFiles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CMSViewFiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ViewLabel,ViewRights,LastUpdateDate,LastUpdatedBy,OwnerID,AdminID,StatusID,Status,PostDate,ViewPath,ViewFileName")] CMSViewFile cMSViewFile)
        {
            if (ModelState.IsValid)
            {
                db.CMSViewFiles.Add(cMSViewFile);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(cMSViewFile);
        }

        // GET: CMSViewFiles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CMSViewFile cMSViewFile = db.CMSViewFiles.Find(id);
            if (cMSViewFile == null)
            {
                return HttpNotFound();
            }
            return View(cMSViewFile);
        }

        // POST: CMSViewFiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ViewLabel,ViewRights,LastUpdateDate,LastUpdatedBy,OwnerID,AdminID,StatusID,Status,PostDate,ViewPath,ViewFileName")] CMSViewFile cMSViewFile)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cMSViewFile).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cMSViewFile);
        }

        // GET: CMSViewFiles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CMSViewFile cMSViewFile = db.CMSViewFiles.Find(id);
            if (cMSViewFile == null)
            {
                return HttpNotFound();
            }
            return View(cMSViewFile);
        }

        // POST: CMSViewFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CMSViewFile cMSViewFile = db.CMSViewFiles.Find(id);
            db.CMSViewFiles.Remove(cMSViewFile);
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
