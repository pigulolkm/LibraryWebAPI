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
    public class RulesController : ApiController
    {
        private LibraryEntities db = new LibraryEntities();

        // GET api/Rules
        public IEnumerable<Rules> GetRules()
        {
            return db.Rules.AsEnumerable();
        }

        // GET api/Rules/5
        public Rules GetRules(int id)
        {
            Rules rules = db.Rules.Find(id);
            if (rules == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return rules;
        }

        // PUT api/Rules/5
        public HttpResponseMessage PutRules(int id, Rules rules)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != rules.Rule_id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(rules).State = EntityState.Modified;

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

        // POST api/Rules
        public HttpResponseMessage PostRules(Rules rules)
        {
            if (ModelState.IsValid)
            {
                db.Rules.Add(rules);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, rules);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = rules.Rule_id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/Rules/5
        public HttpResponseMessage DeleteRules(int id)
        {
            Rules rules = db.Rules.Find(id);
            if (rules == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Rules.Remove(rules);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, rules);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}