using Antem.Models;
using Antem.Parts;
using Antem.Web.Models.UI;
using AutoMapper;
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
        IRepository<Member> repository;
        ExportFactory<IUnitOfWork> factory;
        //
        // GET: /Member/

        [ImportingConstructor]
        public MemberController(IRepository<Member> repo,
            ExportFactory<IUnitOfWork> factory)
        {
            repository = repo;
            this.factory = factory;
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
        public ActionResult Create(MemberEditModel member)
        {
            using (var unitOfWork = factory.CreateExport().Value)
            {
                try
                {
                    var newMember = Mapper.Map<MemberEditModel, Member>(member);
                    repository.Save(newMember);
                    unitOfWork.Commit();
                    return RedirectToAction("Index");
                }
                catch(Exception ex)
                {
                    unitOfWork.Rollback();
                    throw ex;
                }
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
        public ActionResult Edit(Member member)
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
    }
}
