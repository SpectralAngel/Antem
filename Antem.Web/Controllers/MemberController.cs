using Antem.Models;
using Antem.Parts;
using Antem.Web.Filters;
using Antem.Web.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Antem.Web.Controllers
{
    [Transaction]
    [Authorize]
    public class MemberController : Controller
    {
        IRepository<Member> repository;
        //
        // GET: /Member/
        public MemberController(IRepository<Member> repo)
        {
            repository = repo;
            AutoMapping.MemberMappings();
        }

        public ActionResult Index()
        {
            ViewBag.Count = repository.Count();
            return View();
        }

        //
        // GET: /Member/Details/5
        public ActionResult Details(int id)
        {
            var member = repository.Get(id);
            return View(member);
        }

        //
        // GET: /Member/Create
        public ActionResult Create()
        {
            ViewData["State"] = new SelectList(new List<State>(), "State", "State");
            ViewData["Town"] = new SelectList(new List<Town>(), "Town", "Town");
            return View();
        }

        //
        // POST: /Member/Create
        [HttpPost]
        public ActionResult Create(MemberViewModel member)
        {
            var newMember = Mapper.Map<MemberViewModel, Member>(member);
            repository.Save(newMember);
            return RedirectToAction("Index");
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
        public ActionResult Edit(MemberViewModel member)
        {
            try
            {
                var entity = repository.Get(member.Id);
                Mapper.Map<MemberViewModel, Member>(member, entity);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
