using Antem.Models;
using Antem.Parts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Antem.Web.Controllers
{
    public class MemberController : Controller
    {
        IRepository<Member> repository { get; set; }
        //
        // GET: /Member/

        [ImportingConstructor]
        public MemberController(IRepository<Member> repo)
        {
            repository = repo;
        }

        public ActionResult Index()
        {
            dynamic status = new ExpandoObject();
            status.Count = repository.Count();
            return View(status);
        }

        //
        // GET: /Member/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Member/Create

        public ActionResult Create()
        {
            var member = new Member();

            return View(member);
        }

        //
        // POST: /Member/Create

        [HttpPost]
        public ActionResult Create(Member member)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Member/Edit/5

        public ActionResult Edit(int id)
        {
            var member = repository.Get(id);
            return View(member);
        }

        //
        // POST: /Member/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Member/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Member/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
