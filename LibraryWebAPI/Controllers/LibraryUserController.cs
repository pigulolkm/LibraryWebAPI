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
        // GET api/LibraryUser/GetValidateToken
        // Return Array [{ token : True/False, borrowedAmount : {No.}, borrowingLimit : {No.} }]
        public Array GetValidateToken(String token, String email, String Lid)
        {
            object[] result = new object[] { new { result = "False" } };
            int LID = int.Parse(Lid);

            try
            {
                var libraryUser = db.LibraryUsers.Where(lb => lb.L_token.Equals(token) && lb.L_email.Equals(email) && lb.L_id == LID);
                bool valid = libraryUser.Any();

                if (valid)
                {
                    int bAmount = db.Borrowing_record.Where(br => br.L_id == LID && br.BR_returnedDate.Equals(null)).Count();
                    int bLimit = db.Rules.Select(r => r.Rule_borrowingLimit).Single();

                    // Update libraryUser last login time and token --- Start
                    LibraryUser libraryuser = libraryUser.SingleOrDefault();

                    DateTime currentTime = DateTime.Now;
                    string currentTimeMillis = currentTime.Millisecond.ToString();
                    string hashedToken = Crypto.Hash(currentTimeMillis + libraryuser.L_email);

                    libraryuser.L_lastLoginTime = currentTime;
                    libraryuser.L_token = hashedToken;

                    db.Entry(libraryuser).State = EntityState.Modified;
                    // Update libraryUser last login time and token --- End
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        // return "result": "NotFound"
                        result = new Object[] { new { result = HttpStatusCode.NotFound.ToString() } };
                    }

                    result = new object[] { new { result = "True", borrowedAmount = bAmount, borrowingLimit = bLimit } };
                }

            }
            catch (Exception e) { }
            return result;
        }

        // GET api/LibraryUser/GetValidateCard
        // Return Array [{ token : True/False, borrowedAmount : {No.}, borrowingLimit : {No.}, name : firstName + " " + lastName, LID = LID }]
        public Array GetValidateCard(String cardID)
        {
            object[] result = new object[] { new { result = "False" } };

            try
            {
                var libraryUser = db.LibraryUsers.Where(lb => lb.L_cardID.Equals(cardID));
                bool valid = libraryUser.Any();
                if (valid)
                {
                    LibraryUser libraryuser = libraryUser.SingleOrDefault();

                    int bAmount = db.Borrowing_record.Where(br => br.L_id == libraryuser.L_id && br.BR_returnedDate.Equals(null)).Count();
                    int bLimit = db.Rules.Select(r => r.Rule_borrowingLimit).Single();

                    // Update libraryUser last login time and token --- Start

                    DateTime currentTime = DateTime.Now;
                    string currentTimeMillis = currentTime.Millisecond.ToString();
                    string hashedToken = Crypto.Hash(currentTimeMillis + libraryuser.L_email);

                    libraryuser.L_lastLoginTime = currentTime;
                    libraryuser.L_token = hashedToken;

                    db.Entry(libraryuser).State = EntityState.Modified;
                    // Update libraryUser last login time and token --- End
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        // return "result": "NotFound"
                        result = new Object[] { new { result = HttpStatusCode.NotFound.ToString() } };
                    }

                    String firstName = libraryUser.Select(lb => lb.L_firstName).Single();
                    String lastName = libraryUser.Select(lb => lb.L_lastName).Single();

                    result = new object[] { new { result = "True", borrowedAmount = bAmount, borrowingLimit = bLimit, name = firstName + " " + lastName, LID = libraryuser.L_id } };
                }

            }
            catch (Exception e) { }
            return result;
        }

        // PUT api/LibraryUser/PutSignInLibraryUser
        [HttpPut]
        public Array SignInLibraryUser([FromBody] JObject json)
        {
            object[] result = new Object[]{new { result = "False" }};
            string password = json["pw"].ToString();
            string email = json["email"].ToString();
            String lastLoginTime;
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

                int bAmount = db.Borrowing_record.Where(br => br.L_id == libraryuser.L_id && br.BR_returnedDate.Equals(null)).Count();
                int bLimit = db.Rules.Select(r => r.Rule_borrowingLimit).Single();

                lastLoginTime = String.Format("{0:dd-MM-yyyy HH:mm:ss}", libraryuser.L_lastLoginTime);
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

                result = new Object[] { new { result = "True", token = token, name = libraryuser.L_lastName, LID = libraryuser.L_id, lastLoginTime = lastLoginTime, borrowedAmount = bAmount, borrowingLimit = bLimit } };
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

                try
                {
                    libraryuser.L_cardID =  DateTime.Now.Year.ToString() + "LS" + libraryuser.L_id.ToString();
                    db.Entry(libraryuser).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
                }

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