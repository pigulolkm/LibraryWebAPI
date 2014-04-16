using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using LibraryWebAPI.Models;

namespace LibraryWebAPI.Controllers
{
    public class GCMController : ApiController
    {
        private LibraryEntities db = new LibraryEntities();

        // GET api/GCM
        public IEnumerable<GCM> GetGCMs()
        {
            return db.GCMs.AsEnumerable();
        }

        // GET api/GCM/5
        public GCM GetGCM(int id)
        {
            GCM gcm = db.GCMs.Find(id);
            if (gcm == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return gcm;
        }

        // PUT api/GCM/5
        public HttpResponseMessage PutGCM(int id, GCM gcm)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != gcm.Gcm_id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(gcm).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/GCM
        public HttpResponseMessage PostGCM(GCM gcm)
        {
            if (ModelState.IsValid)
            {
                gcm.Gcm_created_datetime = DateTime.Now;

                db.GCMs.Add(gcm);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, gcm.Gcm_id);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = gcm.Gcm_id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/GCM/5
        public HttpResponseMessage DeleteGCM(int id)
        {
            GCM gcm = db.GCMs.Find(id);
            if (gcm == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.GCMs.Remove(gcm);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, gcm.Gcm_id);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}