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
        User user;
        decimal initialDeposit = 2000;
        decimal exchangeAmount = 500;
        decimal overWithdrawal = 3000;
        decimal balanceAfterWithdrawal;
        decimal balanceAfterDeposit;

        [SetUp]
        public void Init()
        {
            user = new User()
            {
                Username = "Test"
            };
            balanceAfterDeposit = initialDeposit + exchangeAmount;
            balanceAfterWithdrawal = initialDeposit - exchangeAmount;
            account = new ContributionsAccount();
            account.Deposit(initialDeposit, user);
        }

        [Test]
        public void Deposit()
        {
            account.Deposit(exchangeAmount, user);
            Assert.AreEqual(balanceAfterDeposit, account.Balance);
        }

        [Test]
        public void Withdraw()
        {
            account.Liberate(DateTime.UtcNow);
            account.Withdraw(exchangeAmount, user);
            Assert.AreEqual(balanceAfterWithdrawal, account.Balance);
        }

        [Test]
        [ExpectedException(typeof(CannotWithdrawException))]
        public void WithdrawBeforeLiberation()
        {
            account.Withdraw(exchangeAmount, user);
        }

        [Test]
        [ExpectedException(typeof(CannotWithdrawException))]
        public void WithdrawOverBalance()
        {
            account.Withdraw(overWithdrawal, user);
        }
    }
}
