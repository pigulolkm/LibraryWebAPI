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
using System.Collections;
using Newtonsoft.Json;

namespace LibraryWebAPI.Controllers
{
    public class BorrowingRecordController : ApiController
    {
        private LibraryEntities db = new LibraryEntities();

        // GET api/BorrowingRecord
        public IEnumerable<Borrowing_record> GetBorrowingRecord()
        {
            return db.Borrowing_record.AsEnumerable();
        }

        // GET api/BorrowingRecord/5
        public Borrowing_record GetBorrowingRecord(int id)
        {
            Borrowing_record borrowing_record = db.Borrowing_record.Find(id);
            if (borrowing_record == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return borrowing_record;
        }

        // PUT api/BorrowingRecord/5
        public HttpResponseMessage PutBorrowingRecord(int id, Borrowing_record borrowing_record)
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
        public HttpResponseMessage PostBorrowingRecord(Borrowing_record[] borrowing_record)
        //public Object PostBorrowingRecord(Borrowing_record[] borrowing_record)
        {
            object result = new object { };
            if (ModelState.IsValid)
            {
                int days = db.Rules.Select(r => r.Rule_borrowingPeriod).Single();
                List<object> success = new List<object>();
                List<object> fail = new List<object>();
                
                
                foreach (Borrowing_record br in borrowing_record)
                {
                    var bookItem = db.Books.Where(b => b.B_id == br.B_id & b.B_status.Equals("Y"));
                    bool availalbe = bookItem.Any();
                    if (availalbe)
                    {
                        // Create borrowing record
                        br.BR_datetime = DateTime.Now;
                        br.BR_renewalTimes = 0;
                        br.BR_shouldReturnedDate = DateTime.Now.AddDays(days);

                        // Update book status
                        Book book = bookItem.Single();
                        book.B_status = "N";

                        db.Borrowing_record.Add(br);
                        
                        success.Add(book);
                    }
                    else
                    {
                        Book book = db.Books.Where(b => b.B_id == br.B_id).Single();
                        fail.Add(book);
                    }
                }

                try
                {
                    db.SaveChanges();

                    result = new {  Success = success.ToArray() , Fail = fail.ToArray() };
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                response.Content = new StringContent(JsonConvert.SerializeObject(result));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/BorrowingRecord/5
        public HttpResponseMessage DeleteBorrowingRecord(int id)
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