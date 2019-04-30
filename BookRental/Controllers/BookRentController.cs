using BookRental.Models;
using BookRental.Utility;
using BookRental.ViewModel;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Net;

namespace BookRental.Controllers
{
    [Authorize]
    public class BookRentController : Controller
    {
        private ApplicationDbContext db;

        public BookRentController()
        {
            db = new ApplicationDbContext();
        }

        // GET: BookRent
        public ActionResult Index(int? pageNumber, string option = null, string search = null)
        {
            string userid = User.Identity.GetUserId();

            var model = from br in db.BookRental
                        join b in db.Books on br.BookId equals b.Id
                        join u in db.Users on br.UserId equals u.Id

                        select new BookRentalViewModel
                        {
                            BookId = b.Id,
                            RentalPrice = br.RentalPrice,
                            Price = b.Price,
                            Pages = b.Pages,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            BirthDate = u.Birthdate,
                            ScheduleEndDate = br.ScheduleEndDate,
                            Author = b.Author,
                            Availability = b.Availability,
                            DateAdded = b.DateAdded,
                            Description = b.Description,
                            Email = u.Email,
                            GenreId = b.GenreId,
                            Genre = db.Genres.Where(g => g.Id.Equals(b.GenreId)).FirstOrDefault(),
                            ISBN = b.ISBN,
                            ImageUrl = b.ImageUrl,
                            ProductDimensions = b.ProductDimensions,
                            PublicationDate = b.PublicationDate,
                            Publisher = b.Publisher,
                            RentalDuration = br.RentalDuration,
                            Status = br.Status.ToString(),
                            Title =  b.Title,
                            UserId = u.Id,
                            Id = br.Id,
                            StartDate = br.StartDate                           
                        };

            if (option == "email" && search.Length > 0)
            {
                model = model.Where(u => u.Email.Contains(search));
            }
            if (option == "name" && search.Length > 0)
            {
                model = model.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search));
            }
            if (option == "status" && search.Length > 0)
            {
                model = model.Where(u => u.Status.Contains(search));
            }

            if (!User.IsInRole(StaticDetails.AdminUserRole))
            {
                model = model.Where(u => u.UserId.Equals(userid));
            }

            return View(model.ToList().ToPagedList(pageNumber?? 1,5));
        }

        public ActionResult Create(string title = null, string isbn = null)
        {
            if (title != null && isbn != null)
            {
                BookRentalViewModel model = new BookRentalViewModel
                {
                    Title = title,
                    ISBN = isbn
                };
            }
            return View( new BookRentalViewModel());
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookRentalViewModel bookRent)
        {
            if (ModelState.IsValid)
            {
                var email = bookRent.Email;

                var userDetails = from u in db.Users
                                  where u.Email.Equals(email)
                                  select new { u.Id, u.FirstName, u.LastName, u.Birthdate };

                var ISBN = bookRent.ISBN;

                Book bookSelected = db.Books.Where(b => b.ISBN == ISBN).FirstOrDefault();

                var rentalDuration = bookRent.RentalDuration;

                var chargeRate = from u in db.Users
                                 join m in db.MembershipTypes on u.MembershipTypeId equals m.Id
                                 where u.Email.Equals(email)
                                 select new { m.ChargeRateOneMonth, m.ChargeRateSixMonth };

                var oneMonthRental = Convert.ToDouble(bookSelected.Price) * Convert.ToDouble(chargeRate.ToList()[0].ChargeRateOneMonth) / 100;
                var sixMonthRental = Convert.ToDouble(bookSelected.Price) * Convert.ToDouble(chargeRate.ToList()[0].ChargeRateSixMonth) / 100;

                double rentalPrice = 0;

                if (bookRent.RentalDuration == StaticDetails.SixMonthCount)
                {
                    rentalPrice = sixMonthRental;
                }
                else
                {
                    rentalPrice = oneMonthRental;
                }

                BookRent modelToAddToDb = new BookRent
                {
                    BookId = bookSelected.Id,
                    RentalPrice = rentalPrice,
                    ScheduleEndDate = bookRent.ScheduleEndDate,
                    RentalDuration = bookRent.RentalDuration,
                    Status = BookRent.StatusEnum.Approved,
                    UserId = userDetails.ToList()[0].Id
                };

                bookSelected.Availability -= 1;
                db.BookRental.Add(modelToAddToDb);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View();
        }
        
        [HttpPost]
        public ActionResult Reserve(BookRentalViewModel book)
        {
            var userid = User.Identity.GetUserId();
            Book bookToRent = db.Books.Find(book.BookId);
            double rentalPrice = 0;

            if (userid != null)
            {
                var chargeRate = from u in db.Users
                                 join m in db.MembershipTypes on u.MembershipTypeId equals m.Id
                                 where u.Id.Equals(userid)
                                 select new { m.ChargeRateOneMonth, m.ChargeRateSixMonth };

                if (book.RentalDuration == StaticDetails.SixMonthCount)
                {
                    rentalPrice = Convert.ToDouble(bookToRent.Price) * Convert.ToDouble(chargeRate.ToList()[0].ChargeRateSixMonth) / 100;
                }
                else
                {
                    rentalPrice = Convert.ToDouble(bookToRent.Price) * Convert.ToDouble(chargeRate.ToList()[0].ChargeRateOneMonth) / 100;
                }

                BookRent bookRent = new BookRent
                {
                    BookId = bookToRent.Id,
                    UserId = userid,
                    RentalDuration = book.RentalDuration,
                    RentalPrice = rentalPrice,
                    Status = BookRent.StatusEnum.Requested
                };

                db.BookRental.Add(bookRent);

                var bookInDb = db.Books.SingleOrDefault(c => c.Id == book.BookId);
                bookInDb.Availability -= 1;
                db.SaveChanges();

                return RedirectToAction("Index", "BookRent");

            }

            return View();
        }

        //GET Details
        public ActionResult Details(int? id)
        {            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(id);

            var model = getVMFromBookRent(bookRent);

            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }            

            return View(model);
        }

        //GET Decline
        public ActionResult Decline(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(id);

            var model = getVMFromBookRent(bookRent);

            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Decline(BookRentalViewModel model)
        {
            if (model.Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(model.Id);
            bookRent.Status = BookRent.StatusEnum.Rejected;

            Book bookInDB = db.Books.Find(bookRent.BookId);
            bookInDB.Availability += 1;

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        //GET Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(id);

            var model = getVMFromBookRent(bookRent);

            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfrmed(int Id)
        {
            if (Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(Id);

            var bookInDb = db.Books.Where(b => b.Id.Equals(bookRent.BookId)).FirstOrDefault();

            if (!bookRent.Status.ToString().Equals("Rented"))
            {
                bookInDb.Availability += 1;
            }
            db.BookRental.Remove(bookRent);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        //GET Approve
        public ActionResult Approve(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(id);

            var model = getVMFromBookRent(bookRent);

            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View("Approve", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(BookRentalViewModel model)
        {
            if (model.Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(model.Id);
            bookRent.Status = BookRent.StatusEnum.Approved;
            
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        //GET PickUp
        public ActionResult PickUp(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(id);

            var model = getVMFromBookRent(bookRent);

            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View("Approve", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PickUp(BookRentalViewModel model)
        {
            if (model.Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(model.Id);
            bookRent.Status = BookRent.StatusEnum.Rented;
            bookRent.StartDate = DateTime.Now;

            if (bookRent.RentalDuration == StaticDetails.SixMonthCount)
            {
                bookRent.ScheduleEndDate = DateTime.Now.AddMonths(Convert.ToInt32(StaticDetails.SixMonthCount));
            }
            else
            {
                bookRent.ScheduleEndDate = DateTime.Now.AddMonths(Convert.ToInt32(StaticDetails.OneMonthCount));
            }

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        //GET Return
        public ActionResult Return(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(id);

            var model = getVMFromBookRent(bookRent);

            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View("Approve", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Return(BookRentalViewModel model)
        {
            if (model.Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookRent bookRent = db.BookRental.Find(model.Id);
            bookRent.Status = BookRent.StatusEnum.Closed;
            bookRent.AdditionalCharge = model.AdditionalCharge;

            Book bookInDb = db.Books.Find(bookRent.BookId);
            bookInDb.Availability += 1;

            bookRent.ActualEndDate = DateTime.Now;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        private BookRentalViewModel getVMFromBookRent(BookRent bookRent)
        {
            Book bookSelected = db.Books.Where(b => b.Id == bookRent.BookId).FirstOrDefault();

            var userDetails = from u in db.Users
                              where u.Id.Equals(bookRent.UserId)
                              select new { u.Id, u.FirstName, u.LastName, u.Email };

            BookRentalViewModel model = new BookRentalViewModel
            {
                Id = bookRent.Id,
                BookId = bookSelected.Id,
                RentalPrice = bookRent.RentalPrice,
                Price = bookSelected.Price,
                Pages = bookSelected.Pages,
                FirstName = userDetails.ToList()[0].FirstName,
                LastName = userDetails.ToList()[0].LastName,
                Email = userDetails.ToList()[0].Email,
                UserId = userDetails.ToList()[0].Id,
                ScheduleEndDate = bookRent.ScheduleEndDate,
                Author = bookSelected.Author,
                StartDate = bookRent.StartDate,
                Availability = bookSelected.Availability,
                Description = bookSelected.Description,
                GenreId = bookSelected.GenreId,
                Genre = db.Genres.FirstOrDefault(g => g.Id.Equals(bookSelected.GenreId)),
                ISBN = bookSelected.ISBN,
                ImageUrl = bookSelected.ImageUrl,
                ProductDimensions = bookSelected.ProductDimensions,
                PublicationDate = bookSelected.PublicationDate,
                Publisher = bookSelected.Publisher,
                RentalDuration = bookRent.RentalDuration,
                Status = bookRent.Status.ToString(),
                Title = bookSelected.Title,
                AdditionalCharge = bookRent.AdditionalCharge
            };

            return model;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
        }
    }
}