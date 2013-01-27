using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Parts
{
    public class Financial
    {
        /// <summary>
        /// Permite calcular los intereses adeudados en el periodo de un mes
        /// </summary>
        /// <param name="anualRate"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static decimal MonthInterest(double anualRate, decimal principal)
        {
            return principal * (decimal)anualRate / 1200;
        }

        public static decimal QuarterlyInterest(double anualRate, decimal principal)
        {
            return principal * (decimal)anualRate / 400;
        }

        /// <summary>
        /// Calcula el pago que se deberá efectuar tomando en cuenta el interes
        /// compuesto
        /// </summary>
        /// <param name="anualRate">The rate charged per year on a Loan</param>
        /// <param name="principal"></param>
        /// <param name="paymentNumber"></param>
        /// <returns></returns>
        public static decimal MonthlyPayment(double anualRate, decimal principal, int paymentNumber)
        {
            var i = anualRate / 1200;
            return principal * (decimal)(i / (1 - Math.Pow(i + 1, -paymentNumber)));
        }

        public static decimal CalculateDefault(double rate, decimal principal, DateTime defaultDate, DateTime lastPayment)
        {
            var tiempo = defaultDate - lastPayment;
            return principal * (decimal)rate/100 * tiempo.Days /  365;
        }
    }
}
