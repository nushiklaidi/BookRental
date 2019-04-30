using BookRental.Models;
using BookRental.Utility;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using HttpPostAttribute = System.Web.Mvc.HttpPostAttribute;

namespace BookRental.Controllers
{
    [System.Web.Http.Authorize(Roles = StaticDetails.AdminUserRole)]
    public class GenreController : Controller
    {
        private ApplicationDbContext _Context;

        public GenreController()
        {
            _Context = new ApplicationDbContext();
        }

        // GET: Genre
        public ActionResult Index()
        {
            return View(_Context.Genres.ToList());
        }

        //Get Action
        public ActionResult Create()
        {
            return View();
        }

        //Post Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Genre genre)
        {
            if (ModelState.IsValid)
            {
                _Context.Genres.Add(genre);
                _Context.SaveChanges();

                return RedirectToAction("Index");
            }
            return View();
        }

        //Get Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Genre genre = _Context.Genres.Find(id);

            if (genre == null)
            {
                return HttpNotFound();
            }

            return View(genre);
        }

        //Get Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Genre genre = _Context.Genres.Find(id);

            if (genre == null)
            {
                return HttpNotFound();
            }

            return View(genre);
        }

        //Post Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Genre genre)
        {
            if (ModelState.IsValid)
            {               
                _Context.Entry(genre).State = EntityState.Modified;
                _Context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View();
        }

        //Get Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Genre genre = _Context.Genres.Find(id);

            if (genre == null)
            {
                return HttpNotFound();
            }

            return View(genre);
        }

        //Post Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            Genre genre = _Context.Genres.Find(id);
            _Context.Genres.Remove(genre);
            _Context.SaveChanges();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            _Context.Dispose();
        }
    }
}