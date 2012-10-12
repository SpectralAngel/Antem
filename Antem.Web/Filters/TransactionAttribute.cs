using Antem.Parts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Antem.Web.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class TransactionAttribute : ActionFilterAttribute
    {
        [Import]
        ExportFactory<IUnitOfWork> factory { get; set; }
        IUnitOfWork unitOfWork;

        public TransactionAttribute()
        {
            Order = 100;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            unitOfWork = factory.CreateExport().Value;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (unitOfWork.isActive)
            {
                if (filterContext.Exception == null)
                {
                    unitOfWork.Commit();
                }
                else
                {
                    unitOfWork.Rollback();
                }
            }
        }
    }
}
