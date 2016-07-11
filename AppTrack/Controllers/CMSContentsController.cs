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
using System.Data.SqlClient;
using AppTrack.Helpers;

namespace AppTrack.Controllers
{
    public class CMSContentsController : BaseController
    {
        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        // GET: CMSContents
        public ActionResult Index()
        {
            return View(db.CMSContents.ToList());
        }

        // GET: CMSContents/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CMSContent cMSContent = db.CMSContents.Find(id);
            if (cMSContent == null)
            {
                return HttpNotFound();
            }
            return View(cMSContent);
        }

        // GET: CMSContents/Create
        [AuthorizeAdminRedirect(Roles = "Website")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: CMSContents/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [AuthorizeAdminRedirect(Roles = "Website")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,SectionName,ViewName,ContentType,ContentReference,ContentValue,LastUpdated,AdminID,Status,StatusID,PostDate")] CMSContent cMSContent)
        {
            if (ModelState.IsValid)
            {
                db.CMSContents.Add(cMSContent);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(cMSContent);
        }

        // GET: CMSContents/Edit/5
        [AuthorizeAdminRedirect(Roles = "Website")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CMSContent cMSContent = db.CMSContents.Find(id);
            if (cMSContent == null)
            {
                return HttpNotFound();
            }
            return View(cMSContent);
        }

        // POST: CMSContents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [AuthorizeAdminRedirect(Roles = "Website")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,SectionName,ViewName,ContentType,ContentReference,ContentValue,LastUpdated,AdminID,Status,StatusID,PostDate")] CMSContent cMSContent)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cMSContent).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cMSContent);
        }

        // POST: CMSContents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CMSSaveContentsTest(string SectionName, string ViewName, string ContentValue)
        {

            return Content(ContentValue);
            //return PartialView("_cmsContent");
        }


        // POST: CMSContents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [AuthorizeAdminRedirect(Roles = "Website")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CMSSaveContents([Bind(Include = "SectionName,ViewName,ContentValue")] CMSContentView cMSContentView)
        {
            if (ModelState.IsValid)
            {

                db.CMS_UpdateCMSContentSimple(cMSContentView.SectionName, cMSContentView.ViewName, cMSContentView.ContentValue);
            }
            else
            {
            }
            return Json(cMSContentView.ContentValue);
            //return PartialView("_cmsContent");
        }

        // POST: CMSContents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [AuthorizeAdminRedirect(Roles = "Website")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CMSSaveContentsPage([Bind(Include = "SectionName,ViewName,ContentValue")] CMSContentView cMSContentView)
        {
            if (ModelState.IsValid)
            {

                db.CMS_UpdateCMSContentSimple(cMSContentView.SectionName, cMSContentView.ViewName, cMSContentView.ContentValue);
            }
            else
            {
            }
            return Json(cMSContentView.ContentValue);
            //return PartialView("_cmsContent");
        }

        // GET: CMSContents/Edit/5
        [ChildActionOnly]
        public ActionResult CMSGetContents(string sectionName, string status, string viewName)
        {
            {

                var query = (from a in db.CMSContents
                            where a.StatusID == 1 
                            where a.SectionName == sectionName
                            select a).FirstOrDefault();

                var model = new CMSContentView
                {
                                SectionName = query.SectionName,
                                ContentValue = query.ContentValue,
                                Status = query.Status,
                                ViewName = query.ViewName
            };

                return PartialView("_CMSPartial", model);
            }
        }

        // POST: CMSContents/Publish/
        [AuthorizeAdminRedirect(Roles = "Website")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PublishContents([Bind(Include = "ViewName")] CMSContent cMSContent)
        {

            if (ModelState.IsValid)
            {
                db.CMS_UpdatePublishPage(cMSContent.ViewName);
             }

            return RedirectToAction(cMSContent.ViewName, "Site");
        }

        // POST: CMSContents/RollBackContents/
        [AuthorizeAdminRedirect(Roles = "Website")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RollBackContents([Bind(Include = "ViewName")] CMSContent cMSContent)
        {

            if (ModelState.IsValid)
            {
                db.CMS_UpDateCMSRollbackPage(cMSContent.ViewName);
            }

            return RedirectToAction(cMSContent.ViewName, "Site");
        }

        

        // GET: CMSContents/Delete/5
        [AuthorizeAdminRedirect(Roles = "Website")]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CMSContent cMSContent = db.CMSContents.Find(id);
            if (cMSContent == null)
            {
                return HttpNotFound();
            }
            return View(cMSContent);
        }

        // POST: CMSContents/Delete/5
        [AuthorizeAdminRedirect(Roles = "Website")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            CMSContent cMSContent = db.CMSContents.Find(id);
            db.CMSContents.Remove(cMSContent);
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
