using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antem.Parts;
using NUnit.Framework;

namespace Antem.Test.Models
{
    [TestFixture]
    public class FinancialTest
    {
        decimal principal = 20000;
        decimal expectedCuota = 1295.26M;
        int payments = 18;
        decimal interest = 333.33M;
        decimal defaultInterest = 394.52M;
        double anualRate = 20;
        double defaultRate = 24;
        DateTime defaultDate;
        DateTime lastPayment = DateTime.UtcNow;
        int defaultDays = 30;

        [Test]
        public void MonthInterest()
        {
            var result = Financial.MonthInterest(anualRate, principal);
            Assert.AreEqual(interest, Math.Round(result, 2));
        }

        [Test]
        public void MonthlyPayment()
        {
            var result = Financial.MonthlyPayment(anualRate, principal, payments);
            Assert.AreEqual(expectedCuota, Math.Round(result, 2));
        }

        [Test]
        public void CalculateDefault()
        {
            defaultDate = lastPayment.AddDays(defaultDays);
            var result = Financial.CalculateDefault(defaultRate, principal, defaultDate, lastPayment);
            Assert.AreEqual(defaultInterest, Math.Round(result, 2));
        }
    }
}
