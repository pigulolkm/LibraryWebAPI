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
using LibraryWebAPI.Utils;
using Newtonsoft.Json;

namespace LibraryWebAPI.Controllers
{
    public class ReservationController : ApiController
    {
        private LibraryEntities db = new LibraryEntities();

        // GET api/Reservation
        public IEnumerable<Reservation> GetReservations()
        {
            return db.Reservations.AsEnumerable();
        }

        // GET api/Reservation/5
        public Reservation GetReservation(int id)
        {
            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return reservation;
        }

        // PUT api/Reservation/5
        public HttpResponseMessage PutReservation(int id, Reservation reservation)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != reservation.R_id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(reservation).State = EntityState.Modified;

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

        // POST api/Reservation
        public HttpResponseMessage PostReservation(Reservation reservation)
        {
            object result = null;
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);
            if (ModelState.IsValid)
            {
                /*
                 * 1. check the Book is on the shelf
                 * 2. check the borrower is already borrowed this book
                 * 3. check is reservation of this book is already made
                 * 4. the result count < Util.Reservation_AmountForEachBook
                 * 5. check reservation limit of the user
                 * 6. create a reservation record
                 */

                // 1.
                Boolean bookOnTheShlef = (from b in db.Books
                                          where b.B_id == reservation.B_id &
                                          b.B_status != Util.BookStatus_ONTHESHELF
                                          select b).Any();

                if (bookOnTheShlef)
                {
                    // 2.
                    Boolean isBorrowerOfThisBook = (from br in db.Borrowing_record
                                                    where br.B_id == reservation.B_id &
                                                    br.L_id == reservation.L_id &
                                                    br.BR_returnedDate == null
                                                    select br).Any();

                    if (!isBorrowerOfThisBook)
                    {
                        // 3. 
                        Boolean isReservated = (from r in db.Reservations
                                                where r.B_id == reservation.B_id &
                                                r.L_id == reservation.L_id &
                                                r.R_isActivated == true &
                                                r.R_finishDatetime == null
                                                select r).Any();

                        if (!isReservated)
                        {

                            // 4.
                            int reservationCountOfBook = (from r in db.Reservations
                                                          where r.B_id == reservation.B_id &
                                                          r.R_isActivated == true &
                                                          r.R_finishDatetime == null
                                                          select r).Count();

                            if (reservationCountOfBook < Util.Reservation_AmountForEachBook)
                            {

                                int reservationCountOfUser = (from r in db.Reservations
                                                              where r.L_id == reservation.L_id &
                                                              r.R_isActivated == true &
                                                              r.R_finishDatetime == null
                                                              select r).Count();
                                // 5.
                                if (reservationCountOfUser < db.Rules.Select(rule => rule.Rule_reservationLimit).Single())
                                {
                                    // 6.
                                    reservation.R_datetime = DateTime.Now;
                                    reservation.R_isActivated = true;

                                    db.Reservations.Add(reservation);
                                    db.SaveChanges();

                                    result = new { Status = "True" };

                                    response = Request.CreateResponse(HttpStatusCode.Created);
                                }
                                else
                                {
                                    result = new { Status = "False", Message = "You reach the reservation limit" };
                                }
                            }
                            else
                            {
                                result = new { Status = "False", Message = "Reach the reservation limit of the book" };
                            }
                        }
                        else
                        {
                            result = new { Status = "False", Message = "You have already reserved this book" };
                        }
                    }
                    else
                    {
                        result = new { Status = "False", Message = "You have already borrowed this book" };
                    }
                }
                else
                {
                    result = new { Status = "False", Message = "The book cannot be reserved" };
                }
                response.Content = new StringContent(JsonConvert.SerializeObject(result));

                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/Reservation/5
        public HttpResponseMessage DeleteReservation(int id)
        {
            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Reservations.Remove(reservation);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, reservation);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}