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
using System.Web.Helpers;
using Newtonsoft.Json.Linq;

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

        // PUT api/LibraryUser/PutSignInLibraryUser
        [HttpPut]
        public Array SignInLibraryUser([FromBody] JObject json)
        {
            object[] result = new Object[]{new { result = "False" }};
            string password = json["pw"].ToString();
            string email = json["email"].ToString();
            // STEPS
            // 1. compare password
            // 2. create current time & token (hash(current time + email))
            // 3. update L_lastLoginTime & L_token
            // 4. return token to client

            // 1.
            var user = from l in db.LibraryUsers
                       where l.L_email.Equals(email) && l.L_password.Equals(password) 
                       select l;

            // user is not null & password is correct
            if (user.Any())
            {
                LibraryUser libraryuser = user.SingleOrDefault();
                // 2.
                DateTime currentTime = DateTime.Now;
                string currentTimeMillis = currentTime.Millisecond.ToString();
                string token = Crypto.Hash(currentTimeMillis + email);

                // 3.
                libraryuser.L_lastLoginTime = currentTime;
                libraryuser.L_token = token;

                db.Entry(libraryuser).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    // return "result": "NotFound"
                    result = new Object[] { new { result = HttpStatusCode.NotFound.ToString() } };
                }
                // 4.
                result = new Object[] { new { result = "True", token = token, name = libraryuser.L_lastName } };
            }
            return result;
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
                
                libraryuser.L_registerDatetime = DateTime.Now;
                libraryuser.L_isBan = false;
                libraryuser.L_accessRight = "100";

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