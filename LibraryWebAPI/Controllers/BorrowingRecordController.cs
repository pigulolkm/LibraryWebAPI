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
    public class BorrowingRecordController : ApiController
    {
        private LibraryEntities db = new LibraryEntities();

        // GET api/BorrowingRecord
        public IEnumerable<Borrowing_record> GetBorrowing_record()
        {
            return db.Borrowing_record.AsEnumerable();
        }

        // GET api/BorrowingRecord/5
        public Borrowing_record GetBorrowing_record(int id)
        {
            Borrowing_record borrowing_record = db.Borrowing_record.Find(id);
            if (borrowing_record == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return borrowing_record;
        }

        // PUT api/BorrowingRecord/5
        public HttpResponseMessage PutBorrowing_record(int id, Borrowing_record borrowing_record)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != borrowing_record.BR_id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(borrowing_record).State = EntityState.Modified;

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

        // POST api/BorrowingRecord
        public HttpResponseMessage PostBorrowing_record(Borrowing_record borrowing_record)
        {
            if (ModelState.IsValid)
            {
                db.Borrowing_record.Add(borrowing_record);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, borrowing_record);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = borrowing_record.BR_id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/BorrowingRecord/5
        public HttpResponseMessage DeleteBorrowing_record(int id)
        {
            Borrowing_record borrowing_record = db.Borrowing_record.Find(id);
            if (borrowing_record == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Borrowing_record.Remove(borrowing_record);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, borrowing_record);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}