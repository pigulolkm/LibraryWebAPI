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
    public class LibraryUserController : ApiController
    {
        private LibraryEntities db = new LibraryEntities();

        // GET api/LibraryUser
        public IEnumerable<LibraryUser> GetLibraryUsers()
        {
            return db.LibraryUsers.AsEnumerable();
        }

        // GET api/LibraryUser/5
        public LibraryUser GetLibraryUser(int id)
        {
            LibraryUser libraryuser = db.LibraryUsers.Find(id);
            if (libraryuser == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return libraryuser;
        }

        // PUT api/LibraryUser/5
        public HttpResponseMessage PutLibraryUser(int id, LibraryUser libraryuser)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != libraryuser.L_id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(libraryuser).State = EntityState.Modified;

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

        // POST api/LibraryUser
        public HttpResponseMessage PostLibraryUser(LibraryUser libraryuser)
        {
            if (ModelState.IsValid)
            {
                db.LibraryUsers.Add(libraryuser);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, libraryuser);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = libraryuser.L_id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/LibraryUser/5
        public HttpResponseMessage DeleteLibraryUser(int id)
        {
            LibraryUser libraryuser = db.LibraryUsers.Find(id);
            if (libraryuser == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.LibraryUsers.Remove(libraryuser);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, libraryuser);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}