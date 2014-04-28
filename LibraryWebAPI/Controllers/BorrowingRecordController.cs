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
using LibraryWebAPI.Utils;

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

        // GET api/BorrowingRecord/{id}?token={token}
        public HttpResponseMessage GetBorrowingRecord(int id, String token)
        {
            bool valid = db.LibraryUsers.Where(lb => lb.L_id == id && lb.L_token.Equals(token)).Any();
            object result = new object { };

            if (valid)
            {
                var notReturnedRecords = from nbr in db.Borrowing_record
                                         join bk in db.Books on nbr.B_id equals bk.B_id
                                         where nbr.L_id == id && nbr.BR_returnedDate == null
                                         select new { 
                                             Bid = bk.B_id, 
                                             Title = bk.B_title, 
                                             Author = bk.B_author, 
                                             Publisher = bk.B_publisher,
                                             ReturnedDate = nbr.BR_returnedDate, 
                                             ShouldReturnedDate = nbr.BR_shouldReturnedDate 
                                         };

                

                var returnRecords = from br in db.Borrowing_record
                                    join bk in db.Books 
                                    on br.B_id equals bk.B_id
                                    where br.L_id == id && br.BR_returnedDate.HasValue
                                    orderby br.BR_returnedDate descending
                                    select new
                                    {
                                        Bid = bk.B_id,
                                        Title = bk.B_title,
                                        Author = bk.B_author,
                                        Publisher = bk.B_publisher,
                                        ReturnedDate = br.BR_returnedDate,
                                        ShouldReturnedDate = br.BR_shouldReturnedDate
                                    };

                result = new { BorrowingRecord = notReturnedRecords.ToArray().Concat(returnRecords.ToArray()),
                               BorrowedAmount = returnRecords.Count() + notReturnedRecords.Count(),
                               NonReturnedAmount = notReturnedRecords.Count()
                             };


                if (result == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }
            }
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(result));
            return response;
        }
        
        // PUT api/BorrowingRecord/5
        // Return Books
        // br -> { BR_id , B_id, L_id }
        public HttpResponseMessage PutBorrowingRecord(int id)
        {
            object result = new object { };
            DateTime now = DateTime.Now;
            double fine = 0;
            ReturnBooks returnBooks = new ReturnBooks();

            if (ModelState.IsValid)
            {
                var bookItem = db.Books.Where(b => b.B_id == id);
                Boolean bookExist = bookItem.Any();

                if (bookExist)
                {
                    Boolean BookIsBorrowed = bookItem.Where(b => b.B_status.Equals(Util.BookStatus_BORROWED)).Any();
                    Borrowing_record B_record = db.Borrowing_record.Where(br => br.B_id == id && br.BR_returnedDate == null).SingleOrDefault();

                    if (B_record != null && BookIsBorrowed)
                    {
                        // Out of Date
                        if (B_record.BR_shouldReturnedDate.Date < now.Date)
                        {
                            // Calculate the fine
                            TimeSpan outDateTime = now.Date.Subtract(B_record.BR_shouldReturnedDate.Date);
                            fine = outDateTime.Days * db.Rules.Select(r => r.Rule_outDateFine).Single();
                        }

                        // Update the borrowing record
                        B_record.BR_returnedDate = now;

                        Book book = bookItem.Single();

                        /// Check if reserved
                        var re =  db.Reservations.Where(r => r.B_id == id & r.R_isActivated == true & r.R_finishDatetime == null).OrderBy(r => r.R_datetime);
                        bool isReserved = re.Any();
                        if (isReserved)
                        { 
                            Reservation reservation = re.First();
                            GCM gcm = db.GCMs.Where(g => g.Gcm_userID == reservation.L_id).Single();

                            reservation.R_finishDatetime = DateTime.Now;
                            reservation.R_getBookDate = DateTime.Now.AddDays(Util.Reservation_getBookDays);

                            book.B_status = Util.BookStatus_RESERVED;

                            Util.sendNotificationMsg("Reservation remind : \n" + book.B_title + " can be borrowed before " + DateTime.Now.AddDays(Util.Reservation_getBookDays).ToShortDateString(), new String[] { gcm.Gcm_regID });
                        }
                        else
                        {
                            // Update Book Status for others to borrrow
                            book.B_status = Util.BookStatus_ONTHESHELF;
                        }
                        // Create ReturnBooks item for displaying
                        returnBooks = new ReturnBooks()
                        {
                            author = book.B_author,
                            title = book.B_title,
                            publisher = book.B_publisher,
                            publicationDate = book.B_publicationDate,
                            fine = fine,
                            isReserved = isReserved
                        };

                        result = new { Result = "True", ReturnBooks = returnBooks };
                    }
                    else
                    {
                        result = new { Result = "False", Message = "The book is already returned." };
                    }
                }
                else
                {
                    result = new { Result = "False", Message = "The book ID is invalid." };
                }

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
                }
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(JsonConvert.SerializeObject(result));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }  
        }

        // PUT api/PutReturnBooks/5
        // Renew Books
        /*
         *  {
         *      Result : True/False,
         *      Message : / RenewBooks : renewBooks
         *  }
        */
        public HttpResponseMessage PutBorrowingRecordRenewBooks(int id)
        {
            object result = new object { };

            if (ModelState.IsValid)
            {
                
                var bookItem = db.Books.Where(b => b.B_id == id);
                Boolean bookExist = bookItem.Any();
                // 1. Check record is exist 
                if (bookExist)
                {
                    
                    Boolean BookIsBorrowed = bookItem.Where(b => b.B_status.Equals(Util.BookStatus_BORROWED)).Any();
                    Borrowing_record B_record = db.Borrowing_record.Where(br => br.B_id == id && br.BR_returnedDate == null).SingleOrDefault();
                    // 2. Check record is valid (Returned or not).                     
                    if (B_record != null && BookIsBorrowed)
                    {
                        // 3. Check the renewing book after the limitRenewBookDay(e.g. 7 Days) of the borrowed book date
                        // 1 : larger than
                        // 0 : equal
                        // -1 : smaller than
                        int limitRenewBookDay = db.Rules.Select(r => r.Rule_limitRenewBookDay).FirstOrDefault();
                        DateTime allowRenewDate = B_record.BR_datetime.Value.Date.AddDays(limitRenewBookDay);
                        if (allowRenewDate.CompareTo(DateTime.Now.Date) < 1)
                        {
                            // 4. Check renewal time limit of the record
                            int renewalLimit = db.Rules.Select(r => r.Rule_renewalLimit).Single();
                            if (B_record.BR_renewalTimes < renewalLimit)
                            {
                                // 5. Update the renewal time and should returned date
                                B_record.BR_shouldReturnedDate = B_record.BR_shouldReturnedDate.AddDays(db.Rules.Select(r => r.Rule_borrowingPeriod).Single());
                                B_record.BR_renewalTimes += 1;

                                // 6. Create result 
                                result = new { Result = "True" };
                            }
                            else
                            {
                                result = new { Result = "False", Message = "The book has arrived the renewal limit." };
                            }
                        }
                        else
                        {
                            result = new { Result = "False", Message = "The book can be renewed on " + String.Format("{0:dd-MM-yyyy}", allowRenewDate)};
                        }
                    }
                    else
                    {
                        result = new { Result = "False", Message = "The book is already returned." };
                    }
                }
                else
                {
                    result = new { Result = "False", Message = "The book ID is invalid." };
                }

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
                }
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(JsonConvert.SerializeObject(result));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }  
        }

        // POST api/BorrowingRecord
        // Borrow Books
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
                    var bookItem = db.Books.Where(b => b.B_id == br.B_id);
                    bool availalbe = (from b in bookItem
                                      where b.B_status.Equals(Util.BookStatus_ONTHESHELF) ||
                                      b.B_status.Equals(Util.BookStatus_RESERVED)
                                      select b).Any();
         
                    if (availalbe)
                    {
                        // Create borrowing record
                        br.BR_datetime = DateTime.Now;
                        br.BR_renewalTimes = 0;
                        br.BR_shouldReturnedDate = DateTime.Now.AddDays(days);

                        // Update book status
                        Book book = bookItem.Single();
                        book.B_status = Util.BookStatus_BORROWED;

                        // Create BorrowBook item for displaying
                        BorrowBooks b = new BorrowBooks()
                        {
                            author = book.B_author,
                            title = book.B_title,
                            publisher = book.B_publisher,
                            publicationDate = book.B_publicationDate,
                            shouldReturnedDate = br.BR_shouldReturnedDate
                        };

                        db.Borrowing_record.Add(br);
                        
                        success.Add(b);
                    }
                    else
                    {
                        Book book = bookItem.Single();
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