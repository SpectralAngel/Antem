using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antem.Models;
using Antem.Models.Exceptions;
using NUnit.Framework;

namespace Antem.Test.Models
{
    [TestFixture]
    class ContributionsAccountTest
    {
        ContributionsAccount account;
        Users user;

        [SetUp]
        public void Init()
        {
            user = new Users()
            {
                Username = "Test"
            };
            account = new ContributionsAccount();
            account.Deposit(2000, user);
        }

        [Test]
        public void Deposit()
        {
            account.Deposit(500, user);
            Assert.AreEqual(account.Balance, 2500);
        }

        [Test]
        public void MovementCreation()
        {
            var movement = account.Deposit(500, user);
            Assert.AreEqual(movement.Amount, 500);
            Assert.AreEqual(movement.Account, account);
            Assert.AreEqual(movement.User, user);
            Assert.LessOrEqual(movement.Day, DateTime.UtcNow);
        }

        [Test]
        public void Withdraw()
        {
            account.Liberate(DateTime.UtcNow);
            var movement = account.Withdraw(500, user);
            Assert.AreEqual(account.Balance, 1500);
        }

        [Test]
        [ExpectedException(typeof(CannotWithdrawException))]
        public void WithdrawBeforeLiberation()
        {
            var movement = account.Withdraw(500, user);
        }
    }
}
