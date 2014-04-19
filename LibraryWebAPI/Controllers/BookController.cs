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
    public class BookController : ApiController
    {
        private LibraryEntities db = new LibraryEntities();

        // GET api/Book
        public IEnumerable<Book> GetBooks()
        {
            return db.Books.AsEnumerable();
        }

        // GET api/Book/5
        public Book GetBook(int id)
        {
            Book book = db.Books.Find(id);
            if (book == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return book;
        }

        // GET api/Book/PostGetBookByKey?searchKey=...&searchOption=...
        public IEnumerable<Book> PostGetBookByKey(SearchBooks searchBooks)
        {
            var books = (IEnumerable<Book>)null;
            switch (searchBooks.searchOption)
            {
                case "Author": books = from b in db.Books where b.B_author.Contains(searchBooks.searchKey) select b;
                                        break;
                case "Title": books = from b in db.Books where b.B_title.Contains(searchBooks.searchKey) select b;
                                        break;
                case "Subject": books = from b in db.Books where b.B_subject.Contains(searchBooks.searchKey) select b;
                                        break;
                case "Publisher": books = from b in db.Books where b.B_publisher.Contains(searchBooks.searchKey) select b;
                                        break;
                case "ISBN": books = from b in db.Books where b.B_ISBN.Equals(searchBooks.searchKey) select b;
                                        break;
                case "ScanCode": books = from b in db.Books where b.B_ISBN.Equals(searchBooks.searchKey) select b;
                                        break;
            }

            return books.AsEnumerable();
        }

        // PUT api/Book/5
        public HttpResponseMessage PutBook(int id, Book book)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != book.B_id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(book).State = EntityState.Modified;

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

        // POST api/Book
        public HttpResponseMessage PostBook(Book book)
        {
            if (ModelState.IsValid)
            {
                db.Books.Add(book);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, book);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = book.B_id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/Book/5
        public HttpResponseMessage DeleteBook(int id)
        {
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Books.Remove(book);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, book);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}