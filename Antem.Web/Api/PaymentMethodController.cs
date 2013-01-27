using Antem.Models;
using Antem.Parts;
using Antem.Web.Filters;
using Antem.Web.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Antem.Web.Api
{
    [Transaction]
    public class PaymentMethodController : ApiController
    {
        IRepository<PaymentMethod> repository;

        [ImportingConstructor]
        public PaymentMethodController(IRepository<PaymentMethod> repository)
        {
            this.repository = repository;
        }

        // GET api/paymentmethod
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/paymentmethod/5
        public PaymentMethodViewModel Get(int id)
        {
            var paymentMethod = repository.Get(id);
            return Mapper.Map<PaymentMethodViewModel>(paymentMethod);
        }

        // POST api/paymentmethod
        public void Post([FromBody]string value)
        {
        }

        // PUT api/paymentmethod/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/paymentmethod/5
        public void Delete(int id)
        {
        }
    }
}
